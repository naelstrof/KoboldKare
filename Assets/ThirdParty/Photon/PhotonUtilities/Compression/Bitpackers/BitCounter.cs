/*
* The MIT License (MIT)
* 
* Copyright (c) 2018-2019 Davin Carten (emotitron) (davincarten@gmail.com)
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System.Collections.Generic;

namespace Photon.Compression
{

	public enum PackedBitsSize { UInt8 = 4, UInt16 = 5, UInt32 = 6, UInt64 = 7 }
	public enum PackedBytesSize { UInt8 = 1, UInt16 = 2, UInt32 = 3, UInt64 = 4 }

	/// <summary>
	/// Experimental packers, that counts number of used bits for serialization. Effective for values that hover close to zero.
	/// </summary>
	public static class BitCounter
	{
		#region Count Used Bit Utils

		public static readonly int[] bitPatternToLog2 = new int[128] {
			0, // change to 1 if you want bitSize(0) = 1
			48, -1, -1, 31, -1, 15, 51, -1, 63, 5, -1, -1, -1, 19, -1,
			23, 28, -1, -1, -1, 40, 36, 46, -1, 13, -1, -1, -1, 34, -1, 58,
			-1, 60, 2, 43, 55, -1, -1, -1, 50, 62, 4, -1, 18, 27, -1, 39,
			45, -1, -1, 33, 57, -1, 1, 54, -1, 49, -1, 17, -1, -1, 32, -1,
			53, -1, 16, -1, -1, 52, -1, -1, -1, 64, 6, 7, 8, -1, 9, -1,
			-1, -1, 20, 10, -1, -1, 24, -1, 29, -1, -1, 21, -1, 11, -1, -1,
			41, -1, 25, 37, -1, 47, -1, 30, 14, -1, -1, -1, -1, 22, -1, -1,
			35, 12, -1, -1, -1, 59, 42, -1, -1, 61, 3, 26, 38, 44, -1, 56
		};
		public const ulong MULTIPLICATOR = 0x6c04f118e9966f6bUL;

		/// <summary>
		/// Number of bits used (ie. position of the first non-zero bit from left to right).
		/// </summary>
		public static int UsedBitCount(this ulong val)
		{
			val |= val >> 1;
			val |= val >> 2;
			val |= val >> 4;
			val |= val >> 8;
			val |= val >> 16;
			val |= val >> 32;
			return bitPatternToLog2[(ulong)(val * MULTIPLICATOR) >> 57];
		}

		/// <summary>
		/// Number of bits used (ie. position of the first non-zero bit from left to right).
		/// </summary>
		public static int UsedBitCount(this uint val)
		{
			val |= val >> 1;
			val |= val >> 2;
			val |= val >> 4;
			val |= val >> 8;
			val |= val >> 16;
			//v |= v >> 32;
			return bitPatternToLog2[(ulong)(val * MULTIPLICATOR) >> 57];
		}

		/// <summary>
		/// Number of bits used (ie. position of the first non-zero bit from left to right).
		/// </summary>
		public static int UsedBitCount(this int val)
		{
			val |= val >> 1;
			val |= val >> 2;
			val |= val >> 4;
			val |= val >> 8;
			val |= val >> 16;
			//v |= v >> 32;
			return bitPatternToLog2[((ulong)val * MULTIPLICATOR) >> 57];
		}

		/// <summary>
		/// Number of bits used (ie. position of the first non-zero bit from left to right).
		/// </summary>
		public static int UsedBitCount(this ushort val)
		{
			uint v = val;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			//val |= val >> 16;
			//v |= v >> 32;
			return bitPatternToLog2[(ulong)(v * MULTIPLICATOR) >> 57];
		}

		/// <summary>
		/// Number of bits used (ie. position of the first non-zero bit from left to right).
		/// </summary>
		public static int UsedBitCount(this byte val)
		{
			uint v = val;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			//v |= v >> 8;
			//v |= v >> 16;
			//v |= v >> 32;
			return bitPatternToLog2[(ulong)(v * MULTIPLICATOR) >> 57];
		}

		#endregion

		#region Count Used Bytes Utils

		public static int UsedByteCount(this ulong val)
		{
			if (val == 0)
				return 0;

			if ((val & 0x000000FF00000000) != 0)
			{
				if ((val & 0x00FF000000000000) != 0)
				{
					if ((val & 0xFF00000000000000) != 0)
						return 8;
					else
						return 7;
				}
				else
				{
					if ((val & 0x0000FF0000000000) != 0)
						return 6;
					else
						return 5;
				}
			}
			else
			{
				if ((val & 0x0000000000FF0000) != 0)
				{
					if ((val & 0x00000000FF000000) != 0)
						return 4;
					else
						return 3;
				}
				else
				{
					if ((val & 0x000000000000FF00) != 0)
						return 2;
					else
						return 1;
				}
			}
		}

		public static int UsedByteCount(this uint val)
		{
			if (val == 0)
				return 0;

			if ((val & 0x00FF0000) != 0)
			{
				if ((val & 0xFF000000) != 0)
					return 4;
				return 3;
			}
			else
			{
				if ((val & 0x0000FF00) != 0)
					return 2;
				else
					return 1;
			}
		}

		public static int UsedByteCount(this ushort val)
		{
			if (val == 0)
				return 0;

			if ((val & 0xFF00) != 0)
				return 2;
			else
				return 1;
		}
		#endregion

		public static int CountTrueBits(this int val, int range = 32)
		{

			int truecount = 0;
			for (int i = 0; i < range; ++i)
			{
				int tmp = val >> i;

				if (tmp == 0)
				{
					return truecount;
				}

				if ((tmp & 1) != 0)
				{
					truecount++;
				}
			}

			return truecount;
		}

		private static List<int> reusableList = new List<int>(32);
		public static int CountTrueBits(this int val, out int[] mountTypeIndex, int range = 32)
		{
			reusableList.Clear();

			int truecount = 0;
			for (int i = 0; i < range; ++i)
			{
				int tmp = val >> i;

				if (tmp == 0)
				{
					mountTypeIndex = reusableList.ToArray();
					return truecount;
				}

				if ((tmp & 1) != 0)
				{
					truecount++;
					reusableList.Add(i);
				}
			}

			mountTypeIndex = reusableList.ToArray();
			return truecount;
		}

        public static int CountTrueBits(this uint val, out int[] mountTypeIndex, int range = 32)
        {
            reusableList.Clear();

            int truecount = 0;
            for (int i = 0; i < range; ++i)
            {
                uint tmp = val >> i;

                if (tmp == 0)
                {
                    mountTypeIndex = reusableList.ToArray();
                    return truecount;
                }

                if ((tmp & 1) != 0)
                {
                    truecount++;
                    reusableList.Add(i);
                }
            }

            mountTypeIndex = reusableList.ToArray();
            return truecount;
        }
    }


}

