// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Compression;
using Photon.Pun.Simple.ContactGroups;
using System.Collections.Generic;

namespace Photon.Pun.Simple
{

    public interface IOnRootVitalBecameZero
    {
        void OnRootVitalBecameZero(Vital vital, IVitalsContactReactor causeOfDeath);
    }


    public class SyncVitals : SyncObject<SyncVitals.Frame>
        , IVitalsSystem
        , IOnSnapshot
        , IOnNetSerialize
        , IOnAuthorityChanged
        , IOnPostSimulate
        , IOnVitalValueChange
        , IOnCaptureState
        //, IDamageable
        , IUseKeyframes
        , IOnStateChange
    {
        public override int ApplyOrder { get { return ApplyOrderConstants.VITALS; } }

        //private List<IOnVitalChange> iOnVitalChange = new List<IOnVitalChange>();

        public override bool AllowReconstructionOfEmpty { get { return false; } }

        public byte SystemIndex { get; set; }

        #region Inspector Items

        public Vitals vitals = new Vitals();
        public Vitals Vitals { get { return vitals; } }

        [SerializeField]
        protected ContactGroupMaskSelector contactGroups = new ContactGroupMaskSelector();
        public IContactGroupMask ValidContactGroups { get { return contactGroups; } }

        [Tooltip("Vital triggers/pickups must have this as a valid mount type. When pickups will attach to this mount when picked up.")]
        public MountSelector defaultMounting = new MountSelector(0);
        public int ValidMountsMask { get { return (1 << defaultMounting.id); } }

        public Mount DefaultMount { get; set; }

        [Tooltip("When root vital <= zero, syncState.Despawn() will be called. This allows for a default handling of object 'death'.")]
        public bool autoDespawn = true;
        [Tooltip("When OnStateChange changes from ObjState.Despawned to any other state, vital values will be reset to their starting defaults.")]
        public bool resetOnSpawn = true;

        #endregion

        [System.NonSerialized]
        private VitalsData lastSentData;

        // Callbacks
        public List<IOnVitalsValueChange> OnVitalsValueChange = new List<IOnVitalsValueChange>(0);
        public List<IOnVitalsParamChange> OnVitalsParamChange = new List<IOnVitalsParamChange>(0);
        public List<IOnRootVitalBecameZero> OnRootVitalBecameZero = new List<IOnRootVitalBecameZero>(0);

        // Cached Items
        protected SyncState syncState;
        private Vital[] vitalArray;
        protected Vital rootVital;
        private int vitalsCount;
        protected int defaultMountingMask;

        // runtime states
        protected bool isPredicted;

        #region Frame

        public class Frame : FrameBase
        {
            public VitalsData vitalsData;

            public Frame() : base() { }

            public Frame(int frameId) : base(frameId) { }

            public Frame(int frameId, Vitals vitals) : base(frameId)
            {
                vitalsData = new VitalsData(vitals);
            }

            public override void CopyFrom(FrameBase sourceFrame)
            {
                base.CopyFrom(sourceFrame);

                var srcVitalsData = (sourceFrame as Frame).vitalsData;
                vitalsData.CopyFrom(srcVitalsData);
            }
        }

        protected override void PopulateFrames()
        {
            int frameCount = TickEngineSettings.frameCount;

            frames = new Frame[frameCount + 1];
            for (int i = 0; i <= frameCount; ++i)
                frames[i] = new Frame(i, vitals);
        }

        #endregion Frame

        public override void OnAwake()
        {
            base.OnAwake();

            this.transform.EnsureRootComponentExists<ContactManager, NetObject>();

            if (netObj)
                syncState = netObj.GetComponent<SyncState>();

            vitalArray = vitals.VitalArray;
            vitalsCount = vitals.vitalDefs.Count;
            rootVital = vitalArray[0];

            /// subscribe to callbacks to Vitals changes
            vitals.OnVitalValueChangeCallbacks.Add(this);

            lastSentData = new VitalsData(vitals);
            for (int i = 0; i < vitalsCount; ++i)
                vitalArray[i].ResetValues();

            defaultMountingMask = 1 << (defaultMounting.id);
        }

        public override void OnStart()
        {
            base.OnStart();

            var mountsLookup = GetComponent<MountsManager>();
            if (mountsLookup)
            {
                if (mountsLookup.mountIdLookup.ContainsKey(defaultMounting.id))
                    DefaultMount = mountsLookup.mountIdLookup[defaultMounting.id];
                else
                {
                    Debug.LogWarning("Sync Vitals has a Default Mount setting of "
                    + MountSettings.GetName(defaultMounting.id) +
                    " but no such mount is defined yet on GameObject: '" + name + "'. Root mount will be used as a failsafe.");

                    /// Invalid default mounting (doesn't exist)... warn and set to Root
                    defaultMounting.id = 0;
                    DefaultMount = mountsLookup.mountIdLookup[0];

                }
            }
        }

        public override void OnAuthorityChanged(bool isMine, bool controllerChanged)
        {
            base.OnAuthorityChanged(isMine, controllerChanged);
            OwnedIVitals.OnChangeAuthority(this, isMine, controllerChanged);
        }



        public Consumption TryTrigger(IContactReactor icontactReactor, ContactEvent contactEvent, int compatibleMounts)
        {
            var reactor = (icontactReactor as IVitalsContactReactor);

            if (ReferenceEquals(reactor, null))
                return Consumption.None;

            /// First test to see if the contacting and contacted are a groups match - if not return false.
            if (contactGroups != 0)
            {
                //var groups = (reactor as Component).GetComponent<ContactGroupAssign>();
                IContactGroupsAssign groups = contactEvent.contactTrigger.ContactGroupsAssign;

                int triggermask = ReferenceEquals(groups, null) ? 0 : groups.Mask;
                if ((contactGroups.Mask & triggermask) == 0)
                {
#if UNITY_EDITOR
                    Debug.Log(name + " SyncVitals.TryTrigger() ContactGroup Mismatch. Cannot pick up '" + (contactEvent.contactTrigger as Component).transform.root.name + "' because its has a non-matching ContactGroupAssign.");
#endif
                    return Consumption.None;
                }
            }

            /// If both are set to 0 (Root) then consider that a match, otherwise zero for one but not the other is a mismatch (for now)
            if ((compatibleMounts != defaultMountingMask) && (compatibleMounts & defaultMountingMask) == 0)
                return Consumption.None;

            Vital vital = vitals.GetVital(reactor.VitalNameType);
            if (vital == null)
            {
                return Consumption.None;
            }

            /// Apply changes resulting from the trigger. Return true if affected/consumed. This bool is used to inform whether the trigger should despawn/pickup.

            //double charge = vpr.Charge;
            Consumption consumed;

            double amountConsumed;
            {
                /// Apply to vital if vital has authority.
                if (IsMine)
                {
                    double discharge = reactor.DischargeValue(contactEvent.contactType);
                    amountConsumed = vitals.ApplyCharges(discharge, reactor.AllowOverload, reactor.Propagate);
                }
                /// Vital does not belong to us, but we want to know IF it would have been consumed for prediction purposes.
                else
                {
                    amountConsumed = vital.TestApplyChange(reactor, contactEvent);
                }
            }

            var consumable = icontactReactor as IVitalsConsumable;
            if (!ReferenceEquals(consumable, null))
                consumed = TestConsumption(amountConsumed, consumable, contactEvent);
            else
                consumed = Consumption.None;

            return consumed;
        }

        protected Consumption TestConsumption(double amountConsumed, IVitalsConsumable iva, ContactEvent contactEvent)
        {

            var consumption = iva.Consumption;
            var discharge = iva.DischargeValue(contactEvent.contactType);

            if (consumption == Consumption.None)
                return Consumption.None;

            if (consumption == Consumption.All)
            {
                if (amountConsumed != 0)
                {
                    iva.Charges = 0;
                    return Consumption.All;
                }
                return Consumption.None;
            }

            var consumed = amountConsumed == 0 ? Consumption.None : discharge == amountConsumed ? Consumption.All : Consumption.Partial;
            iva.Charges -= amountConsumed;
            return consumed;
        }

        public Mount TryPickup(IContactReactor reactor, ContactEvent contactEvent)
        {
            return DefaultMount;
        }

        public void OnPostSimulate(int frameId, int subFrameId, bool isNetTick)
        {
            if (isNetTick)
                vitals.Simulate();
        }

        public virtual void OnCaptureCurrentState(int frameId)
        {
            var framedatas = frames[frameId].vitalsData.datas;
            for (int i = 0; i < vitalsCount; ++i)
                framedatas[i] = vitalArray[i].VitalData;
        }

        #region Serialization

        public SerializationFlags OnNetSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
        {

            /// Don't transmit data if this component is disabled. Allows for muting components
            /// Simply by disabling them at the authority side.
            if (!enabled)
            {
                buffer.WriteBool(false, ref bitposition);
                return SerializationFlags.None;
            }

            Frame frame = frames[frameId];
            buffer.WriteBool(true, ref bitposition);

            bool isKeyframe = IsKeyframe(frameId);

            return vitals.Serialize(frame.vitalsData, lastSentData, buffer, ref bitposition, isKeyframe);

        }


        public SerializationFlags OnNetDeserialize(int originFrameId, byte[] buffer, ref int bitposition, FrameArrival arrival)
        {

            /// Needs to ignore any incoming updates that are the server/relay mirroring back what we sent
            var frame = (IsMine) ? offtickFrame : frames[originFrameId];

            /// If frame is empty, we are done here. Typically means object was disabled.
            if (!buffer.ReadBool(ref bitposition))
            {
                return SerializationFlags.None;
            }

            bool isKeyframe = IsKeyframe(originFrameId);
            var flags = vitals.Deserialize(frame.vitalsData, buffer, ref bitposition, isKeyframe);

            frame.content =
                (flags & SerializationFlags.IsComplete) != 0 ? FrameContents.Complete :
                (flags & SerializationFlags.HasContent) != 0 ? FrameContents.Partial :
                FrameContents.Empty;

            //Debug.LogWarning(frame.frameId + " <b>DES " + frame.vitalsData.datas[0].Value + "</b> " + frame.vitalsData.datas[0].Value);

            return flags;

        }

        #endregion Serialization


        /// <summary>
        /// Returns how much of the damage was consumed.
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        [System.Obsolete("Use vitals.ApplyCharges() instead")]
        public double ApplyDamage(double damage)
        {
            if (!IsMine)
                return damage;

            if (damage == 0)
                return damage;

            return vitals.ApplyCharges(damage, false, true);
        }

        public void OnVitalValueChange(Vital vital)
        {
            if (vital.VitalData.Value <= 0)
            {
                RootVitalBecameZero(vital);
            }

            for (int i = 0, cnt = OnVitalsValueChange.Count; i < cnt; ++i)
                OnVitalsValueChange[i].OnVitalValueChange(vital);
        }

        public void OnVitalParamChange(Vital vital)
        {
            Debug.LogError("Not implemented");
            for (int i = 0, cnt = OnVitalsParamChange.Count; i < cnt; ++i)
                OnVitalsParamChange[i].OnVitalParamChange(vital);
        }

        
        protected virtual void RootVitalBecameZero(Vital vital)
        {
            for (int i = 0, cnt = OnRootVitalBecameZero.Count; i < cnt; ++i)
                OnRootVitalBecameZero[i].OnRootVitalBecameZero(vital, null);

            if (autoDespawn)
                if (syncState)
                    if (ReferenceEquals(rootVital, vital))
                        syncState.Despawn(false);
        }

        private bool wasDespawned;

        public void OnStateChange(ObjState newState, ObjState previousState, Transform attachmentTransform, Mount attachTo = null, bool isReady = true)
        {
            /// Detect respawn (change from despawned to any other state currently) and reset values when that occurs.
            if (wasDespawned && newState != ObjState.Despawned)
            {
                for (int i = 0; i < vitalsCount; ++i)
                    vitalArray[i].ResetValues();

            }
            wasDespawned = newState == ObjState.Despawned;
        }

        //public override bool OnSnapshot(int prevFrameId, int snapFrameId, int targFrameId)
        //      {

        //          bool ready = base.OnSnapshot(prevFrameId, snapFrameId, targFrameId);

        //          if (!ready)
        //		return false;

        //          Debug.LogWarning(snapFrame.);
        //	vitals.Apply(snapFrame.vitalsData);
        //	return true;
        //}

        protected override void ApplySnapshot(Frame snapframe, Frame targframe, bool snapIsValid, bool targIsValid)
        {
            if (snapIsValid && snapframe.content >= FrameContents.Extrapolated)
            {
                //Debug.LogWarning(snap.frameId + " " + netObj.frameValidMask[snap.frameId] + "/" + snap.content + " <b>snap: " + snap.vitalsData.datas[0].Value + "</b> " + snap.vitalsData.datas);

                vitals.Apply(base.snapFrame.vitalsData);
            }
        }

        protected override void InterpolateFrame(Frame targframe, Frame startframe, Frame endframe, float t)
        {
            /// TODO: This isn't really an interpolate. Might want to try to make one.
            targframe.CopyFrom(startframe);
        }

        protected override void ExtrapolateFrame(Frame prevframe, Frame snapframe, Frame targframe)
        {
            var snapdatas = snapframe.vitalsData.datas;
            var targdatas = targframe.vitalsData.datas;

            for (int i = 0; i < vitalsCount; ++i)
                targdatas[i] = vitalArray[i].VitalDef.Extrapolate(snapdatas[i]);

            /// TODO: Maybe should be .Complete?
            targframe.content = FrameContents.Extrapolated;
        }


    }

//#if UNITY_EDITOR

//    [CustomEditor(typeof(SyncVitals))]
//    [CanEditMultipleObjects]
//    public class SyncVitalsEditor : ContactSystemHeaderEditor
//    {

//        protected override string HelpURL
//        {
//            get { return REFERENCE_DOCS_PATH + "subsystems#syncvitals_component"; }
//        }

//        protected override string TextTexturePath
//        {
//            get { return "Header/SyncVitalsText"; }
//        }
//    }

//#endif
}


