// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------


namespace Photon.Pun.Simple
{
	public class VitalsData
	{
		public Vitals vitals;
		public VitalData[] datas;

		public VitalsData(Vitals vitals)
		{
			this.vitals = vitals;
			datas = new VitalData[vitals.VitalArray.Length];
		}

		public void CopyFrom(VitalsData source)
		{
			var srcdatas = source.datas;
			for (int i = 0, cnt = srcdatas.Length; i < cnt; ++i)
				datas[i] = srcdatas[i];
		}
	}

	public struct VitalData
	{
		// Value and IntValue should always change together. Changing either will change both.
		private double _value;
		public double Value { get { return _value; } set { _value = value; /*intValue = (int)System.Math.Round(value);*/ } }
		//private int intValue;
		//public int IntValue { get { return intValue; } set { intValue = value; _value = value; } }

		public int ticksUntilRegen;
		public int ticksUntilDecay;

		//public float Value { get { return value; } set { this.value = value; } }
		//public int TicksUntilDecay { get { return ticksUntilDecay; } set { ticksUntilDecay = value; } }
		//public int TicksUntilRegen { get { return ticksUntilRegen; } set { ticksUntilRegen = value; } }

		public VitalData(double value, int ticksUntilDecay, int ticksUntilRegen)
		{
			this._value = value;
			//this.intValue = (int)value;
			this.ticksUntilDecay = ticksUntilDecay;
			this.ticksUntilRegen = ticksUntilRegen;
		}

		public override string ToString()
		{
			return _value + /*":" + intValue +*/ " ticksUntilDecay: " + ticksUntilDecay + " ticksUntilRegen: " + ticksUntilRegen;
		}
	}

}

