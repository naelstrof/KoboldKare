// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System.Text;

namespace Photon.Utilities
{
	/// <summary>
	/// Very unchecked, and exposed alternative to BitArray for masks in the 65-128 bit range.
	/// Zero-based indexes. Specifically made for use with ring buffers.
	/// </summary>
	public struct FastBitMask128
	{
		private ulong seg1, seg2;
		private int bitcount, seg1bitcount, seg2bitcount;
		private ulong alltrue1, alltrue2;

		public ulong Seg1 { get { return seg1; } }
		public ulong Seg2 { get { return seg2; } }
		public ulong AllTrue1 { get { return alltrue1; } }
		public ulong AllTrue2 { get { return alltrue2; } }

		/// <summary>
		/// Changing the bitcount with this property recalculates the masks, and sets any unused bits in the backing fields to 0;
		/// </summary>
		public int BitCount
		{
			get
			{
				return bitcount;
			}

			set
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (value > 128)
				{
					Debug.LogError("Attempting to set " + typeof(FastBitMask128).Name + ".bitcount to " + value + ", but the max allowed is 128.");
					bitcount = 128;
				}
				else if (value < 0)
				{
					Debug.LogError("Attempting to set " + typeof(FastBitMask128).Name + ".bitcount to " + value + ", but the min allowed is 0.");
					bitcount = 0;
				}
				else
					bitcount = value;
#else
				bitcount = value;
#endif

				this.seg1bitcount = bitcount < 64 ? bitcount : 64;
				this.seg2bitcount = bitcount > 64 ? bitcount - 64 : 0;

				alltrue1 =
					(bitcount < 64) ? ((ulong)1 << bitcount) - 1 :
					ulong.MaxValue;
				alltrue2 =
					(bitcount == 128) ? ulong.MaxValue :
					(bitcount > 64) ? ((ulong)1 << (bitcount - 64)) - 1 :
					0;

				// Clear any old bits that are no longer in use - allows for very unchecked testing if we can assume they are all zero
				seg1 = seg1 & alltrue1;
				seg2 = seg2 & alltrue2;

			}
		}

		public FastBitMask128(int bitcount)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (bitcount > 127 || bitcount < 0)
				RangeError(bitcount);
#endif
			this.seg1 = 0;
			this.seg2 = 0;
			this.bitcount = bitcount;
			this.seg1bitcount = bitcount < 64 ? bitcount : 64;
			this.seg2bitcount = bitcount > 64 ? bitcount - 64 : 0;

			alltrue1 =
				(bitcount < 64) ? ((ulong)1 << bitcount) - 1 :
				ulong.MaxValue;
			alltrue2 =
				(bitcount == 128) ? ulong.MaxValue :
				(bitcount > 64) ? ((ulong)1 << (bitcount - 64)) - 1 :
				0;
		}

		public FastBitMask128(FastBitMask128 copyFrom)
		{
			this.seg1 = copyFrom.seg1;
			this.seg2 = copyFrom.seg2;
			this.bitcount = copyFrom.bitcount;
			this.seg1bitcount = copyFrom.seg1bitcount;
			this.seg2bitcount = copyFrom.seg2bitcount;
			this.alltrue1 = copyFrom.alltrue1;
			this.alltrue2 = copyFrom.alltrue2;
		}

		public bool this[int bit]
		{
			get
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (bit > 127 || bit < 0)
					RangeError(bit);
#endif
				if (bit < 64)
					return (seg1 & ((ulong)1 << bit)) != 0;
				else
					return (seg2 & ((ulong)1 << bit - 64)) != 0;
			}

			set
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (bit > 127 || bit < 0)
					RangeError(bit);
#endif
				if (value)
				{
					if (bit < 64)
						seg1 |= ((ulong)1 << bit);
					else
						seg2 |= ((ulong)1 << (bit - 64));
				}
				else
				{
					if (bit < 64)
						seg1 &= ~((ulong)1 << bit);
					else
						seg2 &= ~((ulong)1 << (bit - 64));
				}
			}
		}

		public bool Get(int bit)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (bit > 127 || bit < 0)
				RangeError(bit);
#endif
			if (bit < 64)
				return (seg1 & ((ulong)1 << bit)) != 0;
			else
				return (seg2 & ((ulong)1 << bit - 64)) != 0;
		}

		public void Set(int bit, bool value)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (bit > 127 || bit < 0)
				RangeError(bit);
#endif
			if (value)
			{
				if (bit < 64)
					seg1 |= ((ulong)1 << bit);
				else
					seg2 |= ((ulong)1 << (bit - 64));
			}
			else
			{
				if (bit < 64)
					seg1 &= ~((ulong)1 << bit);
				else
					seg2 &= ~((ulong)1 << (bit - 64));
			}
		}

		public void SetTrue(int bit)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (bit > 127 || bit < 0)
				RangeError(bit);
#endif
			if (bit < 64)
				seg1 |= ((ulong)1 << bit);
			else
				seg2 |= ((ulong)1 << (bit - 64));
		}

		public void SetFalse(int bit)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (bit > 127 || bit < 0)
				RangeError(bit);
#endif
			if (bit < 64)
				seg1 &= ~((ulong)1 << bit);
			else
				seg2 &= ~((ulong)1 << (bit - 64));
		}

		public void SetAllTrue()
		{
			seg1 = alltrue1;
			seg2 = alltrue2;
		}

		public void SetAllFalse()
		{
			seg1 = 0;
			seg2 = 0;
		}

		/// <summary>
		/// All of the backing ulongs == 0
		/// </summary>
		public bool AllAreFalse { get { return bitcount != 0 && seg1 == 0 && seg2 == 0; } }
		public bool AllAreTrue { get { return bitcount == 0 || seg1 == alltrue1 && seg2 == alltrue2; } }

		public void OR(FastBitMask128 other)
		{
			seg1 |= other.seg1;
			seg2 |= other.seg2;
		}

		public void OR(FastBitMask128 other, int otherOffset)
		{

			if (otherOffset == 0)
			{
				seg1 |= other.seg1;
				seg2 |= other.seg2;
				return;
			}
			if (otherOffset == 64)
			{
				seg2 |= other.seg1;
				return;
			}
			if (otherOffset >= 128)
			{
				return;
			}
			if (otherOffset > 64)
			{
				seg2 |= (seg1 << (otherOffset - 64));
				return;
			}

			seg1 |= other.seg1 << otherOffset;
			seg2 |= other.seg1 >> (64 - otherOffset);
			seg2 |= other.seg2 << otherOffset;

		}

		public void AND(FastBitMask128 other)
		{
			seg1 &= other.seg1;
			seg2 &= other.seg2;
		}

		public void XOR(FastBitMask128 other)
		{
			seg1 ^= other.seg1;
			seg2 ^= other.seg2;
		}

		public static FastBitMask128 operator |(FastBitMask128 a, FastBitMask128 b)
		{
			return new FastBitMask128(a) { seg1 = a.seg1 | b.seg1, seg2 = a.seg2 | b.seg2 };
		}

		public static FastBitMask128 operator &(FastBitMask128 a, FastBitMask128 b)
		{
			return new FastBitMask128(a) { seg1 = a.seg1 & b.seg1, seg2 = a.seg2 & b.seg2 };
		}

		public static FastBitMask128 operator ^(FastBitMask128 a, FastBitMask128 b)
		{
			return new FastBitMask128(a) { seg1 = (a.seg1 ^ b.seg1) & a.alltrue1, seg2 = (a.seg2 ^ b.seg2) & a.alltrue2 };
		}

		public static FastBitMask128 operator !(FastBitMask128 a)
		{
			return new FastBitMask128(a) { seg1 = (~a.seg1) & a.alltrue1, seg2 = (~a.seg2) & a.alltrue2 };
		}

		/// <summary>
		/// Returns a FastMask with all bits flipped. Unused bits remain as zeros.
		/// </summary>
		/// <returns></returns>
		public FastBitMask128 NOT()
		{
			return new FastBitMask128(this) { seg1 = (~seg1 & alltrue1), seg2 = (~seg2 & alltrue2) }; 
		}

		//[UnityEditor.InitializeOnLoadMethod]
		//public static void Tester()
		//{
		//	var test = new FastBitMask128(64) { seg1 = ((ulong)1 << 63), seg2 = 2 };
		//	//test.SetAllTrue();
		//	Debug.Log(test.seg1 + ":" + test.seg2 + "  TestCNT: " + test.CountTrue() + " " + test.CountFalse());
		//}

		// TODO: these could be faster
		public int CountTrue()
		{
			int truecount;

			if (seg1 == 0)
				truecount = 0;
			else if (seg1 == alltrue1)
				truecount = seg1bitcount;
			else
			{
				truecount = 0;
				ulong scratch = seg1;

				while (scratch != 0)
				{
					if ((scratch & 1) == 1)
						truecount++;

					scratch = scratch >> 1;
				}
			}

			if (seg2 == 0)
				return truecount;
			else if (seg2 == alltrue2)
				return truecount + seg2bitcount;
			else
			{
				ulong scratch = seg2;

				while (scratch != 0)
				{
					if ((scratch & 1) == 1)
						truecount++;

					scratch = scratch >> 1;
				}

				return truecount;
			}
		}

		public int CountFalse()
		{
			int truecount;

			if (seg1 == 0)
				truecount = 0;
			else if (seg1 == alltrue1)
				truecount = seg1bitcount;
			else
			{
				truecount = 0;
				ulong scratch = seg1;

				while (scratch != 0)
				{
					if ((scratch & 1) == 1)
						truecount++;

					scratch = scratch >> 1;
				}
			}

			if (seg2 == 0)
				return bitcount - truecount;
			else if (seg2 == alltrue2)
				return bitcount - (truecount + seg2bitcount);
			else
			{
				ulong scratch = seg2;

				while (scratch != 0)
				{
					if ((scratch & 1) == 1)
						truecount++;

					scratch = scratch >> 1;
				}

				return bitcount - truecount;
			}
		}



		/// <summary>
		/// Non-inclusive clearning of X bits working back from start. Max count of 64.
		/// </summary>
		/// <param name=""></param>
		/// <param name="start"></param>
		/// <returns></returns>
		public void ClearBitsBefore(int start, int count)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (start > 127 || start < 0)
				RangeError(start);
#endif

			ulong mask = count == 64 ? ulong.MaxValue : ((ulong)1 << count) - 1;
			ulong mask1, mask2;
			int offset = start - count;

			if (bitcount > 64)
			{
				if (offset >= 0)
				{
					mask1 = mask << offset;
					mask2 = mask >> (seg2bitcount - offset);
				}
				else
				{
					/// Account for possiblity of wrapping back around to seg 1
					ulong wrapmask = mask << (bitcount + offset);
					mask1 = (mask >> -offset) | wrapmask;
					mask2 = mask << (seg2bitcount + offset);
				}

				seg1 &= ~mask1;
				seg2 &= (~mask2 & alltrue2);
			}

			/// if we are only using the first ulong (64 or less bits)
			else
			{
				if (offset >= 0)
				{
					mask1 = mask << offset;
					mask2 = mask >> (seg1bitcount - offset);
				}
				else
				{
					mask1 = mask >> -offset;
					mask2 = mask << (seg1bitcount + offset);
				}

				seg1 &= ~mask1 & ~mask2 & alltrue1;
			}

		}
		/// <summary>
		/// Inclusively get relative distance to most future true bit in the range.
		/// </summary>
		public int CountValidRange(int start, int lookahead)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (start > 127 || start < 0)
				RangeError(start);
#endif
			int len = bitcount;

			/// Start with the farthest, and work backwards until we find a true.
			for (int i = lookahead; i >= 0; --i)
			{
				int b = start + i;
				if (b >= len)
					b -= len;

				if (b < 64)
				{
					if ((seg1 & ((ulong)1 << b)) != 0)
						return i + 1;
				}
				else
				{
					if ((seg2 & ((ulong)1 << b - 64)) != 0)
						return i + 1;
				}
			}

			return 0;
		}

		public void Copy(FastBitMask128 other)
		{
			bitcount = other.bitcount;
			seg1bitcount = other.seg1bitcount;
			seg2bitcount = other.seg2bitcount;
			seg1 = other.seg1;
			seg2 = other.seg2;
			alltrue1 = other.alltrue1;
			alltrue2 = other.alltrue2;
		}

		public bool Compare(FastBitMask128 other)
		{
			return
				bitcount == other.bitcount &&
				seg1 == other.seg1 &&
				seg2 == other.seg2;
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD

		private static void RangeError(int bit)
		{
			Debug.LogError("Value of " + bit + " is out of the valid index range (0-127) for " + typeof(FastBitMask128).Name + ".");
		}
#endif
	}

	public static class FastBitMaskExt
	{

#if DEBUG || UNITY_EDITOR || DEVELOPMENT_BUILD



		private static StringBuilder str = new StringBuilder(512);

		public static StringBuilder PrintMask(this FastBitMask128 ba, int greenbit = -1, int redbit = -1)
		{
			str.Length = 0;
			str.Append("[");
			for (int i = ba.BitCount - 1; i >= 0; --i)
			{

				if (i == greenbit)
					str.Append("<color=green>").Append(ba[i] ? 1 : 0).Append("</color>");
				else if (i == redbit)
                    str.Append("<color=red>").Append(ba[i] ? 1 : 0).Append("</color>");
                else
                    str.Append(ba[i] ? "1" : "<color=#0f0f0f>0</color>");

				if (i % 32 == 0)
					str.Append((i == 0) ? "]" : "] [");
				else if (i % 8 == 0 && i != 0)
					str.Append(":");
			}

			return str;
		}

		public static StringBuilder PrintMask(this FastBitMask128 ba, int greenbit = -1, bool[] redbits = null)
		{
			str.Length = 0;
			str.Append("[");
			for (int i = ba.BitCount - 1; i >= 0; --i)
			{
				if (i == greenbit)
                    str.Append("<color=green>").Append(ba[i] ? 1 : 0).Append("</color>");
                else if (redbits != null && i < redbits.Length && redbits[i])
                    str.Append("<color=red>").Append(ba[i] ? 1 : 0).Append("</color>");
                else
					str.Append(ba[i] ? "1" : "<color=#0f0f0f>0</color>");

				if (i % 32 == 0)
					str.Append((i == 0) ? "]" : "] [");
				else if (i % 8 == 0 && i != 0)
					str.Append(":");
			}

			return str;
		}

		public static StringBuilder PrintMask(this FastBitMask128 ba, StringBuilder[] colorbits = null)
		{
			str.Length = 0;
			str.Append("[");
			for (int i = ba.BitCount - 1; i >= 0; --i)
			{

				if (colorbits != null && i < colorbits.Length && colorbits[i] != null && colorbits[i].ToString() != "")
					str.Append("<b><color=" + colorbits[i].ToString() + ">" + (ba[i] ? 1 : 0) + "</color></b>");
				else
					str.Append(ba[i] ? "1" : "<color=#0f0f0f>0</color>");

				if (i % 32 == 0)
					str.Append((i == 0) ? "]" : "] [");
				else if (i % 8 == 0 && i != 0)
                    str.Append(":");
            }

			return str;
		}

#endif

	}


}

