// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;
using Photon.Compression.Internal;

namespace Photon.Compression
{
	public class SyncHalfFloatAttribute : SyncVarBaseAttribute
        , IPackSingle
        , IPackDouble
	{

		private readonly IndicatorBit indicatorBit;

		// Constructor
		public SyncHalfFloatAttribute(IndicatorBit indicatorBit = IndicatorBit.None, KeyRate keyRate = KeyRate.UseDefault)
		{
			this.indicatorBit = indicatorBit;
			this.keyRate = keyRate;
		}

		public override int GetMaxBits(Type fieldType)
		{
			return 16 + (indicatorBit == IndicatorBit.None ? 0 : 1);
		}

		// Single
		public SerializationFlags Pack(ref Single value, Single prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			ushort cval = HalfFloat.HalfUtilities.Pack(value);

			if (!IsForced(frameId, value, prevValue, writeFlags))
			{
				if (cval == HalfFloat.HalfUtilities.Pack(prevValue))
					return SerializationFlags.None;
			}

			if (indicatorBit == IndicatorBit.IsZero)
			{
				if (value == 0)
				{
					buffer.Write(1, ref bitposition, 1);
					return SerializationFlags.IsComplete;
				}
				buffer.Write(0, ref bitposition, 1);
			}

			buffer.Write(cval, ref bitposition, 16);

			return SerializationFlags.IsComplete;
		}
		public SerializationFlags Unpack(ref Single value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{

			if (indicatorBit == IndicatorBit.IsZero)
			{
				if (buffer.Read(ref bitposition, 1) == 0)
				{
					value = 0;
					return SerializationFlags.None;
				}
			}

			var cval = (ushort)buffer.Read(ref bitposition, 16);
			value = HalfFloat.HalfUtilities.Unpack(cval);

			return SerializationFlags.IsComplete;
		}


		// Double
		public SerializationFlags Pack(ref Double value, Double prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			float fval = (float)value;
			return Pack(ref fval, (float)prevValue, buffer, ref bitposition, frameId, writeFlags);

		}
		public SerializationFlags Unpack(ref Double value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			float fval = 0;

			SerializationFlags flag = Unpack(ref fval, buffer, ref bitposition, frameId, writeFlags);
			value = fval;
			return flag;
		}
	}

}
