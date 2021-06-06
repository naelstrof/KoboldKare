// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using Photon.Pun.Simple.Assists;
using UnityEditor;
#endif


namespace Photon.Pun.Simple
{

    public class VitalsContactReactor : ContactReactorBase
        , IOnContactEvent
        , IVitalsContactReactor
        , IOnStateChange
    //, IOnContact
    // , IOnPickup
    {

        #region Presets

#if UNITY_EDITOR
        //public Enum Preset { HealthPickup, DamageZone, WeaponMelee, WeaponCannon, WeaponScan }

        public VitalsContactReactor UsePreset(Preset preset)
        {
            switch (preset)
            {
                case Preset.HealthPickup:
                    {
                        PresetPickup();
                        triggerOn = ContactType.Enter | ContactType.Hitscan;
                        break;
                    }

                case Preset.RechargeZone:
                    {
                        PresetRechargeZone();
                        triggerOn = ContactType.Enter | ContactType.Stay;
                        break;
                    }
                case Preset.DamageZone:
                    {
                        PresetDamageZone();
                        triggerOn = ContactType.Enter | ContactType.Stay;
                        consumeDespawn = Consumption.None;
                        break;
                    }

                case Preset.WeaponMelee:
                    {
                        PresetWeapon();
                        triggerOn = ContactType.Enter | ContactType.Stay;
                        break;
                    }

                case Preset.WeaponCannon:
                    {
                        PresetWeapon();
                        triggerOn = ContactType.Enter;
                        break;
                    }

                case Preset.WeaponScan:
                    {
                        PresetWeapon();
                        triggerOn = ContactType.Hitscan;
                        break;
                    }
            }
            return this;
        }

        private void PresetWeapon()
        {
            vitalNameType = new VitalNameType(VitalType.None);
            allowOverload = false;
            dischargeOnEnter = -20;
            dischargePerSec = -20;
            dischargeOnExit = -20;
            dischargeOnScan = -20;
            propagate = true;
            useCharges = false;
            isPickup = false;
            consumeDespawn = Consumption.None;
        }

        private void PresetRechargeZone()
        {
            vitalNameType = new VitalNameType(VitalType.Health);
            allowOverload = true;
            dischargeOnEnter = 20;
            dischargePerSec = 20;
            dischargeOnExit = 20;
            dischargeOnScan = 20;
            useCharges = false;
            isPickup = false;
            consumeDespawn = Consumption.None;
        }

        private void PresetDamageZone()
        {
            vitalNameType = new VitalNameType(VitalType.None);
            allowOverload = false;
            dischargeOnEnter = -20;
            dischargePerSec = -20;
            dischargeOnExit = -20;
            dischargeOnScan = -20;
            propagate = true;
            useCharges = false;
            isPickup = false;
            consumeDespawn = Consumption.None;
        }

        private void PresetPickup()
        {
            vitalNameType = new VitalNameType(VitalType.Health);
            allowOverload = true;
            dischargeOnEnter = 20;
            dischargePerSec = 20;
            dischargeOnExit = 20;
            dischargeOnScan = 20;
            initialCharges = 100;
            useCharges = false;
            isPickup = true;
            consumeDespawn = Consumption.None;
        }

        private void PresetWell()
        {
            vitalNameType = new VitalNameType(VitalType.Health);
            allowOverload = false;
            dischargeOnEnter = 20;
            dischargePerSec = 20;
            dischargeOnExit = 20;
            dischargeOnScan = 20;
            useCharges = true;
            isPickup = false;
            consumeDespawn = Consumption.All;
        }

#endif
        #endregion

        #region Inspector

        [SerializeField]
        [HideInInspector]
        protected VitalNameType vitalNameType = new VitalNameType(VitalType.Health);
        virtual public VitalNameType VitalNameType { get { return new VitalNameType(VitalType.None); } }

        //[HideInInspector] public ContactType triggerOn = ContactType.Enter | ContactType.Stay | ContactType.Exit | ContactType.Hitscan;

        [HideInInspector] public double dischargeOnEnter = 20;
        [HideInInspector] public double dischargeOnExit = 20;
        [HideInInspector] public double dischargeOnScan = 20;
        [SerializeField]
        [HideInInspector] protected double dischargePerSec = 20;

        public double DischargePerSec
        {
            get
            {
                return dischargePerSec;
            }
            internal set
            {
                valuePerFixed = value * Time.fixedDeltaTime;
                dischargePerSec = value;
            }
        }

        // TODO: make this an enum, and allow it to propagate up or down the stack.
        [Tooltip("Unconsumed values (remainders) should be passed through to the next vital in the stack of vitals.")]
        public bool propagate = false;
        public virtual bool Propagate
        {
            get { return propagate; }
            set { propagate = value; }
        }

        public bool allowOverload = false;
        public virtual bool AllowOverload
        {
            get { return allowOverload; }
            set { allowOverload = value; }
        }

        [SerializeField]
        protected bool isPickup = true;
        public override bool IsPickup { get { return isPickup; } }

        #region Charges

#if UNITY_EDITOR
        [Internal.HideNextX(2, false)]
        public bool useCharges;

        [Utilities.DisableField]
        public double _charges = 50;
#else
        public bool useCharges;
        public double _charges = 50;
#endif

        [Tooltip("The Charges value that will be set on initialization, and whenever this object respawns.")]
        [SerializeField] protected double initialCharges = 50;

        public virtual double Charges
        {
            get { return _charges; }
            //set
            //{
            //    if (_charges == value)
            //        return;

            //    // Clamp incoming value to zero
            //    var clamped = (initialCharges >= 0) ? System.Math.Max(value, 0) : System.Math.Min(value, 0);

            //    _charges = clamped;

            //    if (useCharges && clamped != initialCharges)
            //    {
            //        Consumption consumed = (clamped == 0) ? Consumption.All : Consumption.Partial;

            //        Consume(consumed);
            //    }
            //}
        }

        public virtual Consumption ConsumeCharges(double amount)
        {
            if (amount == 0)
                return Consumption.None;

            // Clamp incoming value to zero
            double unclamped = _charges - amount;
            var clamped = ((initialCharges >= 0) ? System.Math.Max(unclamped, 0) : System.Math.Min(unclamped, 0));

            _charges = clamped;

            if (/*useCharges && */clamped != initialCharges)
            {
                return (clamped == 0) ? Consumption.All : Consumption.Partial;
            }
            return Consumption.None;
        }

        protected virtual void Consume(Consumption consumed)
        {
            if (consumed == Consumption.None)
                return;

            // Despawn if we have consumed enough to trigger consumeDespawn
            if (consumeDespawn != Consumption.None && syncState != null)
            {
                // If all was consumed, trigger despawn in both cases
                if (consumed == Consumption.All)
                    syncState.Despawn(false);

                // consumed == partial - despawn if consumeDespawn also is set to partial.
                else if (consumeDespawn == Consumption.Partial)
                {
                    syncState.Despawn(false);
                }
            }
        }

        #endregion

        public Consumption consumeDespawn = Consumption.None;

        #endregion

        // cached
        protected double valuePerFixed;

        public override void OnAwakeInitialize(bool isNetObject)
        {
            base.OnAwakeInitialize(isNetObject);

            valuePerFixed = dischargePerSec * Time.fixedDeltaTime;
        }

        protected override Consumption ProcessContactEvent(ContactEvent contactEvent)
        {
            //Debug.Log("Process " + contactEvent + " --  " + (contactEvent.contactSystem as IVitalsSystem));

            //double consumed;

            var system = (contactEvent.contactSystem as IVitalsSystem);
            if (system == null)
                return Consumption.None;

            double value = GetValueForTriggerType(contactEvent.contactType);

            //if (vitalNameType.type == VitalType.None)
            //{
            //    consumed = system.Vitals.ApplyCharges(vitalNameType, value, allowOverload, propagate);
            //}
            //else
            //{
            //    Vital vital = system.Vitals.GetVital(vitalNameType);

            //    if (ReferenceEquals(vital, null))
            //    {
            //        Debug.LogWarning("No matching Vital found.");
            //        return Consumption.None;
            //    }

            //    //consumed = vital.ApplyChange(value, this);
            //}

            double consumed = system.Vitals.ApplyCharges(vitalNameType, value, allowOverload, propagate);

            Consumption consumption;

            if (useCharges)
            {
                consumption = ConsumeCharges(consumed);
            }
            else if (consumed != 0)
            {
                if (consumed == value)
                {
                    consumption = Consumption.All;
                }
                else
                {
                    consumption = Consumption.Partial;
                }

                Consume(consumption);
            }
            else
            {
                //Debug.LogWarning("Reactor not consumed.");
                return Consumption.None;
            }


            ///// TEST - Notify other component of pickup condition
            //var onContact = transform.GetComponent<IOnPickup>();
            //Debug.Log("POST 2 " + (onContact != null));

            //if (onContact != null)
            //    syncState.HardMount(onContact.OnPickup(contactEvent));

            if (isPickup && consumption != Consumption.None)
            {
                Mount mount = system.TryPickup(this, contactEvent);
                if (mount)
                    syncState.HardMount(mount);
            }

            return consumption;
        }

        public void OnStateChange(ObjState newState, ObjState previousState, Transform attachmentTransform, Mount attachTo = null, bool isReady = true)
        {
            if (previousState == ObjState.Despawned && (newState & ObjState.Visible) != 0)
                _charges = initialCharges;
        }


        public double DischargeValue(ContactType contactType = ContactType.Undefined)
        {
            double discharge;

            switch (contactType)
            {
                case ContactType.Enter:
                    discharge = dischargeOnEnter;
                    break;

                case ContactType.Stay:
                    discharge = dischargePerSec;
                    break;

                case ContactType.Exit:
                    discharge = dischargeOnExit;
                    break;

                case ContactType.Hitscan:
                    discharge = dischargeOnScan;
                    break;

                default:
                    discharge = 0;
                    break;
            }

            if (useCharges)
            {
                if (discharge >= 0)
                    return System.Math.Min(discharge, _charges);
                else
                    return System.Math.Max(discharge, _charges);
            }

            return discharge;
        }

        protected double GetValueForTriggerType(ContactType collideType)
        {

            double value;
            switch (collideType)
            {
                case ContactType.Enter:
                    value = dischargeOnEnter; break;

                case ContactType.Stay:
                    value = valuePerFixed; break;

                case ContactType.Exit:
                    value = dischargeOnExit; break;

                case ContactType.Hitscan:
                    value = dischargeOnScan; break;

                default:
                    value = 0; break;
            }
            return value;
        }


    }

#if UNITY_EDITOR

    [CustomEditor(typeof(VitalsContactReactor))]
    [CanEditMultipleObjects]
    public class VitalsContactReactorEditor : ContactReactorsBaseEditor
    {
        protected override string Instructions
        {
            get
            {
                return "Responds to " + typeof(IOnContactEvent).Name + " callbacks, and applies value change to indicated Vital.";
            }
        }

        protected override string HelpURL { get { return Internal.SimpleDocsURLS.SUBSYS_PATH  + "#vitalscontactreactor_component"; } }

        protected override string TextTexturePath
        {
            get { return "Header/VitalsReactorText"; }
        }

        const int PAD = 4;

        protected override void OnInspectorGUIInjectMiddle()
        {
            base.OnInspectorGUIInjectMiddle();

            var _target = target as VitalsContactReactor;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("vitalNameType"));

            ContactType oldTriggerOn = _target.triggerOn;
            ContactType newTriggerOn = ContactType.Undefined;

            EditorGUILayout.BeginVertical(new GUIStyle("OL EntryBackEven") { padding = new RectOffset(0, PAD, PAD, PAD), margin = new RectOffset(0, 0, 4, 4) });
            {
                newTriggerOn |= DrawTrigger("dischargeOnEnter", "On Enter", oldTriggerOn, ContactType.Enter);
                newTriggerOn |= DrawTrigger("dischargePerSec", "On Stay", oldTriggerOn, ContactType.Stay);
                newTriggerOn |= DrawTrigger("dischargeOnExit", "On Exit", oldTriggerOn, ContactType.Exit);
                newTriggerOn |= DrawTrigger("dischargeOnScan", "On Scan", oldTriggerOn, ContactType.Hitscan);
            }
            EditorGUILayout.EndVertical();

            _target.triggerOn = newTriggerOn;


            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private ContactType DrawTrigger(string fieldname, string label, ContactType triggerOn, ContactType type)
        {
            var sp = serializedObject.FindProperty(fieldname);

            bool wasEnabled = (triggerOn & type) != 0;

            var r = EditorGUILayout.GetControlRect();

            bool nowEnabled = EditorGUI.ToggleLeft(new Rect(r) { xMin = r.xMin + PAD, xMax = EditorGUIUtility.labelWidth }, " ", wasEnabled);

            EditorGUI.BeginDisabledGroup(!nowEnabled);
            {
                EditorGUI.LabelField(new Rect(r) { xMin = r.xMin + 20 + PAD, xMax = EditorGUIUtility.labelWidth }, label);

                r.xMin += EditorGUIUtility.labelWidth;

                if (nowEnabled)
                    EditorGUI.PropertyField(new Rect(r) { xMin = r.xMin - 2 }, sp, GUIContent.none);
            }
            EditorGUI.EndDisabledGroup();

            if (wasEnabled != nowEnabled)
            {
                Undo.RecordObject(target, "Change " + label + " Toggle");
            }

            return nowEnabled ? type : ContactType.Undefined;
        }

        protected override void SyncStateWarnings()
        {
            if ((target as VitalsContactReactor).IsPickup)
                base.SyncStateWarnings();
        }
    }
#endif

}
