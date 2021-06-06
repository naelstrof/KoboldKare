// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Simple
{
    [System.Serializable]
    public class Vital/* : IVital*/
    {
        [SerializeField]
        private VitalDefinition vitalDef;
        public VitalDefinition VitalDef { get { return vitalDef; } }

        [System.NonSerialized]
        private VitalData vitalData;
        public VitalData VitalData { get { return vitalData; } private set { vitalData = value; } }

        public double Value
        {
            get { return vitalData.Value; }
            set
            {
                if (value == double.NegativeInfinity)
                    return;

                double prev = vitalData.Value;
                double clamped = System.Math.Max(System.Math.Min(value, vitalDef.MaxValue), 0);
                vitalData.Value = clamped;

                /// Only send out OnVitalChange notices on whole number changes - so we are using intvalue for our check, rather than value
                if (prev != clamped)
                {
                    for (int i = 0; i < OnValueChangeCallbacks.Count; ++i)
                        OnValueChangeCallbacks[i].OnVitalValueChange(this);
                }
            }
        }

        public int TicksUntilDecay { get { return vitalData.ticksUntilDecay; } set { vitalData.ticksUntilDecay = value; } }
        public int TicksUntilRegen { get { return vitalData.ticksUntilRegen; } set { vitalData.ticksUntilRegen = value; } }

        public Vital(VitalDefinition vitalDef)
        {
            this.vitalDef = vitalDef;
        }

        public void Initialize(float tickDuration)
        {
            vitalDef.Initialize(tickDuration);
            ResetValues();
        }

        public void ResetValues()
        {
            vitalData = vitalDef.GetDefaultData();

            /// Notify UI and other subscribed components of change
            for (int i = 0; i < OnValueChangeCallbacks.Count; ++i)
                OnValueChangeCallbacks[i].OnVitalValueChange(this);

        }

        #region Outgoing OnChange Callbacks

        public List<IOnVitalValueChange> OnValueChangeCallbacks = new List<IOnVitalValueChange>(0);
        public List<IOnVitalParamChange> OnParamChangeCallbacks = new List<IOnVitalParamChange>(0);

        public void AddIOnVitalChange(IOnVitalChange cb)
        {
            var vc = cb as IOnVitalValueChange;
            if (!ReferenceEquals(null, vc))
                OnValueChangeCallbacks.Add(vc);


            var pc = cb as IOnVitalParamChange;
            if (!ReferenceEquals(null, pc))
                OnParamChangeCallbacks.Add(pc);
        }
        public void RemoveIOnVitalChange(IOnVitalChange cb)
        {
            var vc = cb as IOnVitalValueChange;
            if (!ReferenceEquals(null, vc))
                OnValueChangeCallbacks.Remove(vc);

            var pc = cb as IOnVitalParamChange;
            if (!ReferenceEquals(null, pc))
                OnParamChangeCallbacks.Remove(pc);
        }

        #endregion

        public void Apply(VitalData vdata)
        {
            Value = vdata.Value;
            vitalData.ticksUntilDecay = vdata.ticksUntilDecay;
            vitalData.ticksUntilRegen = vdata.ticksUntilRegen;
        }

        /// <summary>
        /// Apply damage to vital, accounting for absorption. Returns consumed amount.
        /// </summary>
        public double ApplyCharges(double amt, bool allowOverload, bool ignoreAbsorbtion)
        {
            double absorbed = ignoreAbsorbtion ? amt : amt * vitalDef.Absorbtion;
            double holdval = Value;
            double resultVal = holdval + absorbed;

            if (allowOverload == false)
                resultVal = System.Math.Min(resultVal, vitalDef.FullValue);

            Value = resultVal;
            double consumed = Value - holdval;

            return consumed;
        }

        /// <summary>
        /// Is this vital currently at its max (can its value NOT be increased)
        /// </summary>
        /// <param name="allowOverload">Whether to check against Max value or Full value</param>
        /// <returns></returns>
        public bool IsFull(bool allowOverload)
        {
            return Value >= ((allowOverload) ? vitalDef.MaxValue : vitalDef.FullValue);
        }

        // <summary>
        /// Apply a change to this vital, and return how much was consumed.
        /// </summary>
		public double ApplyChange(IVitalsContactReactor iVitalsAffector, ContactEvent contectEvent)
        {
            double discharge = iVitalsAffector.DischargeValue(contectEvent.contactType);
            return ApplyChange(discharge, iVitalsAffector);
        }

        /// <summary>
        /// Apply a change to this vital, and return how much was consumed.
        /// </summary>
        public double ApplyChange(double amount, IVitalsContactReactor reactor = null)
        {
            double oldval = vitalData.Value;
            double newval = vitalData.Value + amount;

            if (!ReferenceEquals(reactor, null) && reactor.AllowOverload)
            {
                double maxValue = vitalDef.MaxValue;

                if (oldval >= maxValue)
                    return 0;

                if (newval > maxValue)
                    newval = maxValue;
            }
            else
            {
                double fullValue = vitalDef.FullValue;

                if (oldval >= fullValue)
                    return 0;

                if (newval > fullValue)
                    newval = fullValue;
            }

            Value = newval;

            double diff = newval - oldval;

            ///// reset regen/decay based on gain/loss
            //if (diff > 0)
            //    DisruptDecay();
            //else if (diff < 0)
            //    DisruptRegen();

            return diff;
        }

        public void DisruptRegen()
        {
            vitalData.ticksUntilRegen = vitalDef.RegenDelayInTicks;
        }

        public void DisruptDecay()
        {
            vitalData.ticksUntilDecay = vitalDef.DecayDelayInTicks;
        }

        public double TestApplyChange(IVitalsContactReactor iVitalsAffector, ContactEvent contactEvent)
        {
            double charge = iVitalsAffector.DischargeValue(contactEvent.contactType);
            return TestApplyChange(charge, iVitalsAffector);
        }
        /// <summary>
        /// Get the results of ApplyChange without actually applying them.
        /// </summary>
        public double TestApplyChange(double charge, IVitalsContactReactor iVitalsAffector)
        {
            double oldval = vitalData.Value;
            double newval = vitalData.Value + charge;

            if (!ReferenceEquals(iVitalsAffector, null) && iVitalsAffector.AllowOverload)
            {
                double maxValue = vitalDef.MaxValue;

                if (oldval >= maxValue)
                    return 0;

                if (newval > maxValue)
                    newval = maxValue;
            }
            else
            {
                double fullValue = vitalDef.MaxValue;

                if (oldval >= fullValue)
                    return 0;

                if (newval > fullValue)
                    newval = fullValue;
            }

            double diff = newval - oldval;

            return diff;
        }


        /// <summary>
        /// Apply Regeneration and Decay to current state
        /// </summary>
        public void Simulate()
        {
            /// Regeneration
            if (vitalData.ticksUntilRegen > 0)
                vitalData.ticksUntilRegen--;
            else if (vitalData.Value < vitalDef.FullValue)
                Value = System.Math.Min(vitalData.Value + vitalDef.RegenPerTick, vitalDef.FullValue);

            /// Overload decay
            if (vitalData.ticksUntilDecay > 0)
                vitalData.ticksUntilDecay--;
            else if (vitalData.Value > vitalDef.FullValue)
                Value = System.Math.Max(vitalData.Value - vitalDef.DecayPerTick, vitalDef.FullValue);


        }
    }
}

