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
	public struct FastBitMask64
	{
		public ulong bitmask;
		private int bitcount;
		private ulong alltrue;

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
				if (value > 64)
				{
					Debug.LogError("Attempting to set " + typeof(FastBitMask64).Name + ".bitcount to " + value + ", but the max allowed is 64.");
					bitcount = 64;
				}
				else if (value < 0)
				{
					Debug.LogError("Attempting to set " + typeof(FastBitMask64).Name + ".bitcount to " + value + ", but the min allowed is 0.");
					bitcount = 0;
				}
				else
					bitcount = value;
#else
				bitcount = value;

#endif
				alltrue =
					(bitcount < 64) ? ((ulong)1 << bitcount) - 1 :
					ulong.MaxValue;

				// Clear any old bits that are no longer in use - allows for very unchecked testing if we can assume they are all zero
				bitmask = bitmask & alltrue;
			}
		}

		public FastBitMask64(int bitcount)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (bitcount > 63 || bitcount < 0)
				RangeError(bitcount);
#endif
			this.bitmask = 0;
			this.bitcount = bitcount;

			alltrue =
				(bitcount < 64) ? ((ulong)1 << bitcount) - 1 :
				ulong.MaxValue;
		}

		private FastBitMask64(FastBitMask64 copyFrom)
		{
			this.bitmask = copyFrom.bitmask;
			this.bitcount = copyFrom.bitcount;
			this.alltrue = copyFrom.alltrue;
		}

		public bool this[int bit]
		{
			get
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (bit > 63 || bit < 0)
					RangeError(bit);
#endif
				return (bitmask & ((ulong)1 << bit)) != 0;
			}

			set
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (bit > 63 || bit < 0)
					RangeError(bit);
#endif
				if (value)
				{
					bitmask |= ((ulong)1 << bit);
				}
				else
				{
					bitmask &= ~((ulong)1 << bit);
				}
			}
		}

		public bool Get(int bit)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (bit > 63 || bit < 0)
				RangeError(bit);
#endif
			return (bitmask & ((ulong)1 << bit)) != 0;
		}

		public void Set(int bit, bool value)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (bit > 63 || bit < 0)
				RangeError(bit);
#endif
			if (value)
			{
				//bitmask |= singlemasks[bit];
				bitmask |= ((ulong)1 << bit);
			}
			else
			{
				bitmask &= ~((ulong)1 << bit);
			}
		}

		public void SetTrue(int bit)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (bit > 63 || bit < 0)
				RangeError(bit);
#endif
			bitmask |= ((ulong)1 << bit);
		}

		public void SetFalse(int bit)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (bit > 63 || bit < 0)
				RangeError(bit);
#endif
			bitmask &= ~((ulong)1 << bit);
		}

		public void SetAllTrue()
		{
			bitmask = alltrue;
		}

		public void SetAllFalse()
		{
			bitmask = 0;
		}

		/// <summary>
		/// All of the backing ulongs == 0
		/// </summary>
		public bool AllAreFalse { get { return bitcount != 0 && bitmask == 0; } }
		public bool AllAreTrue { get { return bitcount == 0 || bitmask == alltrue; } }

		public void OR(FastBitMask64 other)
		{
			bitmask |= other.bitmask;
		}

		public void AND(FastBitMask64 other)
		{
			bitmask &= other.bitmask;
		}

		public void XOR(FastBitMask64 other)
		{
			bitmask ^= other.bitmask;
		}


		// TODO: these could be faster
		public int CountTrue()
		{

			int truecount;

			if (bitmask == 0)
				truecount = 0;
			else if (bitmask == alltrue)
				truecount = bitcount;
			else
			{
				truecount = 0;
				ulong scratch = bitmask;

				while (scratch != 0)
				{
					if ((scratch & 1) == 1)
						truecount++;

					scratch = scratch >> 1;
				}
			}
			return truecount;
		}

		// TODO: these could be faster
		public int CountFalse()
		{
			if (bitmask == 0)
				return bitcount;

			if (bitmask == alltrue)
				return 0;

			int falsecount = 0;

			for (int i = 0, cnt = bitcount; i < cnt; ++i)
				if ((bitmask & ((ulong)1 << i)) == 0)
					falsecount++;

			return falsecount;
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
			if (start > 63 || start < 0)
				RangeError(start);
#endif
			ulong mask = count == 64 ? ulong.MaxValue : ((ulong)1 << count) - 1;
			ulong mask1, mask2;
			int offset = start - count;


			if (offset >= 0)
			{
				mask1 = mask << offset;
				mask2 = mask >> (bitcount - offset);
			}
			else
			{
				mask1 = mask >> -offset;
				mask2 = mask << (bitcount + offset);
			}

			bitmask &= ~mask1 & ~mask2 & alltrue;

		}
		/// <summary>
		/// Inclusively get relative distance to most future true bit in the range.
		/// </summary>
		public int CountValidRange(int start, int lookahead)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (start > 63 || start < 0)
				RangeError(start);
#endif
			/// Start with the farthest, and work backwards until we find a true.
			for (int i = lookahead; i >= 0; --i)
			{
				int b = start + i;
				if (b >= bitcount)
					b -= bitcount;

				if ((bitmask & ((ulong)1 << b)) != 0)
					return i + 1;
			}

			return 0;
		}

		public void Copy(FastBitMask64 other)
		{
			bitcount = other.bitcount;
			bitmask = other.bitmask;
			alltrue = other.alltrue;
		}

		public bool Compare(FastBitMask64 other)
		{
			return
				bitcount == other.bitcount &&
				bitmask == other.bitmask;
		}


#if UNITY_EDITOR || DEVELOPMENT_BUILD

		private static void RangeError(int bit)
		{
			Debug.LogError("Value of " + bit + " is out of the valid index range (0-63) for " + typeof(FastBitMask64).Name + ".");
		}
#endif
	}

	public static class FastBitMask64Ext
	{

#if DEBUG || UNITY_EDITOR || DEVELOPMENT_BUILD


		private static StringBuilder str = new StringBuilder(512);

		public static StringBuilder PrintMask(this FastBitMask64 ba, int greenbit = -1, int redbit = -1)
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

		public static StringBuilder PrintMask(this FastBitMask64 ba, int greenbit = -1, bool[] redbits = null)
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

		public static StringBuilder PrintMask(this FastBitMask64 ba, StringBuilder[] colorbits = null)
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
#else
        

#endif

	}


}

