// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;
using Photon.Compression.Internal;
using Photon.Utilities;

namespace Photon.Compression
{
	
	public class SyncRangedIntAttribute : SyncVarBaseAttribute
		, IPackByte, IPackSByte
		, IPackUInt16, IPackInt16
		, IPackUInt32, IPackInt32
		, IPackUInt64, IPackInt64
		, IPackSingle, IPackDouble
	{
		private int min, max;
		private readonly int smallest, biggest;
		private readonly IndicatorBits indicatorBits;
		
		
		/// <summary>
		/// Network this field as an Integer type that will remain between the range values. Any values outside of these will be clamped.
		/// By knowing this range, values can automatically be bitpacked to the smallest possible value.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public SyncRangedIntAttribute(int min, int max, IndicatorBits indicatorBits = IndicatorBits.None, KeyRate keyRate = KeyRate.UseDefault)
		{
			this.min = min;
			this.max = max;
			this.indicatorBits = indicatorBits;
			this.keyRate = keyRate;

			if (min < max)
			{
				smallest = min;
				biggest = max;
			}
			else
			{
				smallest = max;
				biggest = min;
			}
			uint range = (uint)(biggest - smallest);
			bitCount = range.GetBitsForMaxValue();
        }

        public override int GetMaxBits(Type fieldType)
		{
			switch (indicatorBits)
			{
				case IndicatorBits.IsZero:
					return bitCount + 1;

				case IndicatorBits.IsZeroMidMinMax:
					return bitCount + 2;

				default:
					return bitCount;
			}
		}

		#region Packer/Unpackers

		// 8 Bits

		private SerializationFlags Write(int value, int prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{

			int clamped = (value > biggest) ? biggest : (value < smallest) ? smallest : value;

			if (!IsForced(frameId, clamped, prevValue, writeFlags))
			{
				return SerializationFlags.None;
			}

			if (indicatorBits == IndicatorBits.IsZero)
			{
				if (clamped == 0)
				{
					buffer.Write(0, ref bitposition, 1);
					return SerializationFlags.IsComplete;
				}
				buffer.Write(1, ref bitposition, 1);
			}

			else if (indicatorBits == IndicatorBits.IsZeroMidMinMax)
			{
				if (clamped == 0)
				{
					buffer.Write(0, ref bitposition, 1);
					return SerializationFlags.IsComplete;
				}
				else if (clamped == min)
				{
					buffer.Write(1, ref bitposition, 1);
					return SerializationFlags.IsComplete;
				}
				else if (clamped == max)
				{
					buffer.Write(3, ref bitposition, 1);
					return SerializationFlags.IsComplete;
				}
				buffer.Write(3, ref bitposition, 1);
			}
            
            if (bitCount < 16)
                buffer.Write((ulong)(clamped - smallest), ref bitposition, bitCount);
            else
                buffer.WritePackedBytes((ulong)(clamped - smallest), ref bitposition, bitCount);

            return SerializationFlags.IsComplete;
		}

		private int Read(byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{

			if (indicatorBits == IndicatorBits.IsZero)
			{
				if (buffer.Read(ref bitposition, 1) == 0)
					return 0;
			}
			else if (indicatorBits == IndicatorBits.IsZeroMidMinMax)
			{
				ulong indicator = buffer.Read(ref bitposition, 2);
				switch (indicator)
				{
					case 0:
						return 0;
					case 1:
						return min;
					case 2:
						return max;
				}
			}
            
            if (bitCount < 16)
                return (int)buffer.Read(ref bitposition, bitCount) + smallest;
            else
                return (int)((long)buffer.ReadPackedBytes(ref bitposition, bitCount) + smallest);

        }

        public SerializationFlags Pack(ref Byte value, Byte prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue, writeFlags))
				return SerializationFlags.None;

			return Write(value, (int)prevValue, buffer, ref bitposition, frameId, writeFlags);
		}
		public SerializationFlags Unpack(ref Byte value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (Byte)Read(buffer, ref bitposition, frameId, writeFlags);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Pack(ref SByte value, SByte prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue, writeFlags))
				return SerializationFlags.None;

			return Write(value, (int)prevValue, buffer, ref bitposition, frameId, writeFlags);
		}
		public SerializationFlags Unpack(ref SByte value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (SByte)Read(buffer, ref bitposition, frameId, writeFlags);
			return SerializationFlags.IsComplete;
		}

		// 16 bits

		public SerializationFlags Pack(ref UInt16 value, UInt16 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue, writeFlags))
				return SerializationFlags.None;

			return Write(value, (int)prevValue, buffer, ref bitposition, frameId, writeFlags);
		}
		public SerializationFlags Unpack(ref UInt16 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (UInt16)Read(buffer, ref bitposition, frameId, writeFlags);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Pack(ref Int16 value, Int16 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue, writeFlags))
				return SerializationFlags.None;

			return Write(value, (int)prevValue, buffer, ref bitposition, frameId, writeFlags);
		}
		public SerializationFlags Unpack(ref Int16 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (Int16)Read(buffer, ref bitposition, frameId, writeFlags);
			return SerializationFlags.IsComplete;
		}

		// 32 bits

		public SerializationFlags Pack(ref UInt32 value, UInt32 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue, writeFlags))
				return SerializationFlags.None;

			return Write((int)value, (int)prevValue, buffer, ref bitposition, frameId, writeFlags);
		}
		public SerializationFlags Unpack(ref UInt32 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (UInt32)Read(buffer, ref bitposition, frameId, writeFlags);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Pack(ref Int32 value, Int32 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue, writeFlags))
				return SerializationFlags.None;

			return Write(value, (int)prevValue, buffer, ref bitposition, frameId, writeFlags);
		}
		public SerializationFlags Unpack(ref Int32 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (Int32)Read(buffer, ref bitposition, frameId, writeFlags);
			return SerializationFlags.IsComplete;
		}

		// 32 bits

		public SerializationFlags Pack(ref UInt64 value, UInt64 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue, writeFlags))
				return SerializationFlags.None;

			return Write((int)value, (int)prevValue, buffer, ref bitposition, frameId, writeFlags);
		}
		public SerializationFlags Unpack(ref UInt64 value,byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (UInt64)Read(buffer, ref bitposition, frameId, writeFlags);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Pack(ref Int64 value, Int64 prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			if (!IsForced(frameId, value, prevValue, writeFlags))
				return SerializationFlags.None;

			return Write((int)value, (int)prevValue, buffer, ref bitposition, frameId, writeFlags);
		}
		public SerializationFlags Unpack(ref Int64 value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (Int64)Read(buffer, ref bitposition, frameId, writeFlags);
			return SerializationFlags.IsComplete;
		}

		// float

		public SerializationFlags Pack(ref Single value, Single prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			int rounded = (int)Math.Round(value);
            int prevrounded = (int)Math.Round(prevValue);

			if (!IsForced(frameId, writeFlags) && rounded == prevrounded)
				return SerializationFlags.None;

			return Write(rounded, prevrounded, buffer, ref bitposition, frameId, writeFlags);
		}
		public SerializationFlags Unpack(ref Single value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (Single)Read(buffer, ref bitposition, frameId, writeFlags);
			return SerializationFlags.IsComplete;
		}

		public SerializationFlags Pack(ref Double value, Double prevValue, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			int rounded = (int)Math.Round(value);
            int prevrounded = (int)Math.Round(prevValue);

			if (!IsForced(frameId, writeFlags) && rounded == prevrounded)
				return SerializationFlags.None;

			return Write(rounded, prevrounded, buffer, ref bitposition, frameId, writeFlags);
		}
		public SerializationFlags Unpack(ref Double value, byte[] buffer, ref int bitposition, int frameId, SerializationFlags writeFlags)
		{
			value = (Double)Read(buffer, ref bitposition, frameId, writeFlags);
			return SerializationFlags.IsComplete;
		}

		#endregion
		
	}
}
