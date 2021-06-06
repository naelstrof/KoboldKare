// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

using Photon.Compression;

#if UNITY_EDITOR
using Photon.Utilities;
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
    /// TODO: this class can become non-abstract if I finish it out and see reason for it.
    /// <summary>
    /// The generic base class for any VitalTrigger derived class.
    /// </summary>
    /// <typeparam name="TFrame"></typeparam>
    public class SyncContact : SyncObject<SyncContact.Frame>
        , ISyncContact
        , IOnSnapshot
        , IOnNetSerialize
        , IOnAuthorityChanged
        , IOnCaptureState
        , ISerializationOptional

    {
#if UNITY_EDITOR
        [EnumMask(true)]
#endif
        //public ContactType triggerOn = ContactType.Enter | ContactType.Hitscan;

        //[System.NonSerialized] public List<IOnContact> OnTriggerCallbacks = new List<IOnContact>();

        protected Frame currentState = new Frame();

        protected IContactTrigger contactTrigger;

        protected Rigidbody rb;
        protected Rigidbody2D rb2d;
        protected bool _hasRigidbody;
        public bool HasRigidbody { get { return _hasRigidbody; } }

        public GameObject VisiblePickupObj
        {
            get
            {
                return gameObject;
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            serializeThis = true;
        }
#endif

        #region Frame

        public struct ContactRecord
        {
            public ContactType contactType;
            public int contactSystemViewID;
            public byte contactSystemIndex;

            public ContactRecord(int contactSystemViewID, byte contactSystemIndex, ContactType contactType)
            {
                this.contactSystemViewID = contactSystemViewID;
                this.contactSystemIndex = contactSystemIndex;
                this.contactType = contactType;
            }

            public override string ToString()
            {
                return contactType + " view: " + contactSystemViewID + " index:" + contactSystemIndex;
            }
        }

        public class Frame : FrameBase
        {
            public List<ContactRecord> contactRecords = new List<ContactRecord>(1);

            public Frame() : base() { }

            public Frame(int frameId) : base(frameId)
            {

            }

            // triggers are events, not states, so they don't extrapolate.
            public override void CopyFrom(FrameBase sourceFrame)
            {
                base.CopyFrom(sourceFrame);
                this.content = FrameContents.Empty;
                this.contactRecords.Clear();
            }

            public static Frame Construct(int frameId)
            {
                return new Frame(frameId);
            }

            public override void Clear()
            {
                base.Clear();
                contactRecords.Clear();
            }
        }

        #endregion Frame

        #region Startup

        public override void OnAwake()
        {
            base.OnAwake();

            contactTrigger = GetComponent<IContactTrigger>();

            rb = GetComponentInParent<Rigidbody>();
            rb2d = GetComponentInParent<Rigidbody2D>();
            _hasRigidbody = rb || rb2d;

            //transform.GetNestedComponentsInChildren<IOnContact, NetObject>(this.OnTriggerCallbacks);
        }

        #endregion Startup

        #region Triggers

        protected Queue<ContactEvent> queuedContactEvents = new Queue<ContactEvent>();

        #region OnEnter

        // Step #1

        public virtual void SyncContactEvent(ContactEvent contactEvent)
        {
            if (!IsMine)
                return;

            //// TODO: this allows undefined contacts to trigger if triggerOn is also undefined. May not be desired.
            //if (triggerOn != 0 && (triggerOn & contactType) == 0)
            //    return;

            //Debug.Log(name + " OnContactEvent <b>ENQUEUE</b> " + " " + contactEvent.contactSystem.GetType().Name + "  trigon:" + triggerOn + "/" + contactType);

            EnqueueEvent(contactEvent);

        }

        protected virtual bool EnqueueEvent(ContactEvent contactEvent)
        {
            ///// TODO: need to put a consumption validity test here
            //IContactSystem ics = contactEvent.contactSystem;
            //if (ReferenceEquals(ics, null))
            //    return false;

            queuedContactEvents.Enqueue(contactEvent);

            return true;
        }

        #endregion

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frameId"></param>
        /// <param name="amActingAuthority"></param>
        /// <param name="realm"></param>
        public virtual void OnCaptureCurrentState(int frameId)
        {
            Frame frame = frames[frameId];

            ContactEvent contactEvent;

            frame.content = FrameContents.Empty;

            while (queuedContactEvents.Count > 0)
            {

                contactEvent = queuedContactEvents.Dequeue();

                var consumed = Contact(contactEvent);

                //Debug.Log(name + " fid:" + frameId + " CAP " + " " + contactEvent.contactType);

                //Debug.Log(name + " <b>OnCapture:</b> " + consumed);
                if (consumed != Consumption.None)
                {
                    frame.content = FrameContents.Complete;
                    var tes = frame.contactRecords;
                    var te = new ContactRecord( 
                        contactEvent.contactSystem.ViewID, 
                        contactEvent.contactSystem.SystemIndex,
                        contactEvent.contactType
                    );
                    tes.Add(te);

                    //Consume(frame, contactEvent, consumed);
                    //OnTriggerSuccess(frame, contactEvent, consumed);
                }

                // Stop testing if has been consumed.
                if (consumed == Consumption.All)
                    break;
            }

            queuedContactEvents.Clear();

        }

        //protected virtual void OnTriggerSuccess(Frame frame, ContactEvent contactEvent, Consumption consumed)
        //{

        //}

        /// <summary>
        /// Attempt a trigger. Returns true if a triggerEvent results in a valid collision.
        /// </summary>
        protected virtual Consumption Contact(ContactEvent contactEvent)
        {
            //Debug.Log(name + " CONTACTS " + this.OnTriggerCallbacks.Count);

            return contactTrigger.ContactCallbacks(contactEvent);


            //for (int i = 0, cnt = this.OnTriggerCallbacks.Count; i < cnt; ++i)
            //{
            //    Consumption consumed = this.OnTriggerCallbacks[i].OnContact(contactEvent);
            //    if (consumed != Consumption.None)
            //        return consumed;
            //}

            //return Consumption.None;
        }

        protected virtual void Consume(Frame frame, ContactEvent contactEvent, Consumption consumed)
        {

        }

        #region Serialization


        public virtual SerializationFlags OnNetSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
        {

            //if (!IsMine)
            //{
            //    /// attached bool
            //    buffer.WriteBool(false, ref bitposition);
            //    return SerializationFlags.None;
            //}

            Frame frame = frames[frameId];

            SerializationFlags flags;

            var tes = frame.contactRecords;

            /// pickup event

            if (frame.content == FrameContents.Complete)
            {
                for (int i = 0, cnt = tes.Count; i < cnt; ++i)
                {
                    var te = tes[i];
                    
                    //Debug.LogError("fid: " + frameId + " Send Pickup " + te.contactSystemViewID + " " + te.contactType + " " + te.contactSystemIndex);

                    /// attached bool
                    buffer.Write(1, ref bitposition, 1);
                    buffer.WritePackedBytes((uint)te.contactSystemViewID, ref bitposition, 32);
                    // TODO: this requires a proper bitcount, not 8
                    buffer.WritePackedBits(te.contactSystemIndex, ref bitposition, 8);

                    // Convert the mask into an index, since only one bit should ever be true.
                    var contactType = te.contactType;
                    int maskindex = contactType == ContactType.Enter ? 0 : contactType == ContactType.Hitscan ? 3 : contactType == ContactType.Stay ? 1 : 2;
                    buffer.Write((uint)maskindex, ref bitposition, 2);
                }
                buffer.Write(0, ref bitposition, 1);
                flags = SerializationFlags.HasContent | SerializationFlags.ForceReliable;
            }
            else
            {
                /// attached bool
                buffer.WriteBool(false, ref bitposition);
                flags = SerializationFlags.None;
            }

            return flags;
        }

        public SerializationFlags OnNetDeserialize(int originFrameId, byte[] buffer, ref int bitposition, FrameArrival arrival)
        {
            Frame frame = frames[originFrameId];

            // Get first content bool to see if we have any contacts at all.
            bool hasContent = buffer.ReadBool(ref bitposition);
            if (hasContent)
            {
                /// if pickup event bool
                do
                {
                    var tes = frame.contactRecords;

                    var te = new ContactRecord(
                        (int)buffer.ReadPackedBytes(ref bitposition, 32),
                        (byte)buffer.ReadPackedBits(ref bitposition, 8),
                        (ContactType)(1 << (int)buffer.Read(ref bitposition, 2))
                        );

                    //Debug.LogError(name + " DES: " + te);
                    tes.Add(te);

                }
                // Loop for all contact records.
                while (buffer.ReadBool(ref bitposition));

                frame.content = FrameContents.Complete;
                return SerializationFlags.HasContent /*| SerializationFlags.ForceReliable*/;
            }

            // No contact records
            frame.content = FrameContents.Empty;
            return SerializationFlags.None;
        }

        #endregion Serialization

        protected override void ApplySnapshot(Frame snapframe, Frame targframe, bool snapIsValid, bool targIsValid)
        {
            base.ApplySnapshot(snapframe, targframe, snapIsValid, targIsValid);

            if (snapframe.content == FrameContents.Complete)
            {
                List<ContactRecord> contacts = snapframe.contactRecords;

                for (int i = 0, cnt = contacts.Count; i < cnt; ++i)
                {
                    ContactRecord contact = contacts[i];
                    var pv = PhotonNetwork.GetPhotonView(contact.contactSystemViewID);

                    if (pv && pv.IsMine)
                    {
                        var cm = pv.GetComponent<ContactManager>();
                        if (cm)
                        {
                            var currentAttachedICS = cm.GetContacting(contact.contactSystemIndex);

                            // Retry the trigger and see if this will consume it, if so run pickup to apply it.
                            var contactevent = new ContactEvent(currentAttachedICS, contactTrigger, contact.contactType);

                            var consumed = Contact(contactevent);
                            if (consumed != Consumption.None)
                                Consume(snapframe, contactevent, consumed);
                        }
                    }
                }
            }
        }

       

        #region Utilities

        // TODO: move this to a utility for use elsewhere
        protected static int ConvertMaskToIndex(int mask)
        {
            int bits = 0;
            if (mask > 32767)
            {
                mask >>= 16;
                bits += 16;
            }

            if (mask > 127)
            {
                mask >>= 8;
                bits += 8;
            }

            if (mask > 7)
            {
                mask >>= 4;
                bits += 4;
            }

            if (mask > 1)
            {
                mask >>= 2;
                bits += 2;
            }

            if (mask > 0)
            {
                bits++;
            }

            return bits;
        }

        public static int ConvertIndexToMask(int index)
        {
            if (index == 0)
                return 0;

            return 1 << (index - 1);
        }

        #endregion

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SyncContact))]
    [CanEditMultipleObjects]
    public class SyncContactEditor : SyncObjectEditor
    {
        protected override string Instructions
        {
            get
            {
                return "Operates in tandem with a " + typeof(IContactTrigger).Name + " component to handle " + typeof(IOnContactEvent).Name + " events, and defer them for serialization." +
                    "This allows unowned " + typeof(IContactSystem).Name + "s to react to "+ typeof(IOnContactEvent).Name +"s on their controlling client.\n";
            }
        }

        protected override string TextTexturePath
        {
            get
            {
                return "Header/SyncContactText";
            }
        }
        //public override void OnInspectorGUI()
        //{
        //	base.OnInspectorGUI();

        //	var _target = (target as SyncPickup);

        //	//ListFoundInterfaces(_target.gameObject, _target.onTriggerCallbacks);
        //}

        //protected override void OnInspectorGUIInjectMiddle()
        //{
        //	base.OnInspectorGUIInjectMiddle();
        //	EditorGUILayout.LabelField("Generates OnTrigger()", richLabel);
        //}
    }

#endif

}
