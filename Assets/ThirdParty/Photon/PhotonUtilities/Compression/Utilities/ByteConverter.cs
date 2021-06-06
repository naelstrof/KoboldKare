/*
* The MIT License (MIT)
* 
* 
* Copyright (c) 2012-2013 Fredrik Holmstrom (fredrik.johan.holmstrom@gmail.com)
* Extended 2018-2019 Davin Carten [emotitron] (davincarten@gmail.com)
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

using System.Runtime.InteropServices;
using System;

/// <summary>
/// Temporariliy using the Utilities namespace to avoid collisions with the DLL still in use by NST
/// </summary>
namespace Photon.Compression
{

	[StructLayout(LayoutKind.Explicit)]
	public struct ByteConverter
	{

		[FieldOffset(0)]
		public Single float32;
		[FieldOffset(0)]
		public Double float64;
		[FieldOffset(0)]
		public SByte int8;
		[FieldOffset(0)]
		public Int16 int16;
		[FieldOffset(0)]
		public UInt16 uint16;
		[FieldOffset(0)]
		public Char character;
		[FieldOffset(0)]
		public Int32 int32;
		[FieldOffset(0)]
		public UInt32 uint32;
		[FieldOffset(0)]
		public Int64 int64;
		[FieldOffset(0)]
		public UInt64 uint64;

		[FieldOffset(0)]
		public Byte byte0;
		[FieldOffset(1)]
		public Byte byte1;
		[FieldOffset(2)]
		public Byte byte2;
		[FieldOffset(3)]
		public Byte byte3;
		[FieldOffset(4)]
		public Byte byte4;
		[FieldOffset(5)]
		public Byte byte5;
		[FieldOffset(6)]
		public Byte byte6;
		[FieldOffset(7)]
		public Byte byte7;

		/// <summary>
		/// The upper 4 bytes of this 8 byte struct, returned as a uint.
		/// </summary>
		[FieldOffset(4)]
		public uint uint16_B;

		public void GetBytes(byte[] target, int count = 4)
		{
			for (int i = 0; i < count; ++i)
				target[i] = this[i];
		}

		/// <summary>
		/// A Byte indexer.
		/// </summary>
		/// <param name="index">Byte index.</param>
		/// <returns>Value of the byte at given index.</returns>
		public Byte this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return byte0;
					case 1: return byte1;
					case 2: return byte2;
					case 3: return byte3;
					case 4: return byte4;
					case 5: return byte5;
					case 6: return byte6;
					case 7: return byte7;
					default:
						System.Diagnostics.Debug.Assert((index >= 8), "Index value of " + index + " is invalid. ByteConverter this[] indexer must be a value between 0 and 7.");
						return 0;
				}
			}
		}

		#region Implicit To ByteConverter

		public static implicit operator ByteConverter(byte[] bytes)
		{
			ByteConverter bc = default(ByteConverter);

			int len = bytes.Length;

			bc.byte0 = bytes[0];
			if (len > 0) bc.byte1 = bytes[1];
			if (len > 1) bc.byte2 = bytes[2];
			if (len > 2) bc.byte3 = bytes[3];
			if (len > 3) bc.byte4 = bytes[4];
			if (len > 4) bc.byte5 = bytes[5];
			if (len > 5) bc.byte6 = bytes[3];
			if (len > 6) bc.byte7 = bytes[7];

			return bc;
		}

		public static implicit operator ByteConverter(Byte val)
		{
			ByteConverter bc = default(ByteConverter);
			bc.byte0 = val;
			return bc;
		}

		public static implicit operator ByteConverter(SByte val)
		{
			ByteConverter bc = default(ByteConverter);
			bc.int8 = val;
			return bc;
		}

		public static implicit operator ByteConverter(Char val)
		{
			ByteConverter bc = default(ByteConverter);
			bc.character = val;
			return bc;
		}

		public static implicit operator ByteConverter(UInt32 val)
		{
			ByteConverter bc = default(ByteConverter);
			bc.uint32 = val;
			return bc;
		}

		public static implicit operator ByteConverter(Int32 val)
		{
			ByteConverter bc = default(ByteConverter);
			bc.int32 = val;
			return bc;
		}

		public static implicit operator ByteConverter(UInt64 val)
		{
			ByteConverter bc = default(ByteConverter);
			bc.uint64 = val;
			return bc;
		}

		public static implicit operator ByteConverter(Int64 val)
		{
			ByteConverter bc = default(ByteConverter);
			bc.int64 = val;
			return bc;
		}

		public static implicit operator ByteConverter(Single val)
		{
			ByteConverter bc = default(ByteConverter);
			bc.float32 = val;
			return bc;
		}

		public static implicit operator ByteConverter(Double val)
		{
			ByteConverter bc = default(ByteConverter);
			bc.float64 = val;
			return bc;
		}

		public static implicit operator ByteConverter(Boolean val)
		{
			ByteConverter bc = default(ByteConverter);
			bc.int32 = val ? 1 : 0;
			return bc;
		}

		public void ExtractByteArray(byte[] targetArray)
		{
			int len = targetArray.Length;

			targetArray[0] = byte0;
			if (len > 0) targetArray[1] = byte1;
			if (len > 1) targetArray[2] = byte2;
			if (len > 2) targetArray[3] = byte3;
			if (len > 3) targetArray[4] = byte4;
			if (len > 4) targetArray[5] = byte5;
			if (len > 5) targetArray[6] = byte6;
			if (len > 6) targetArray[7] = byte7;
		}

		#endregion

		#region Implicit From ByteConverter

		public static implicit operator Byte(ByteConverter bc) { return bc.byte0; }
		public static implicit operator SByte(ByteConverter bc) { return bc.int8; }
		public static implicit operator Char(ByteConverter bc) { return bc.character; }
		public static implicit operator UInt16(ByteConverter bc) { return bc.uint16; }
		public static implicit operator Int16(ByteConverter bc) { return bc.int16; }
		public static implicit operator UInt32(ByteConverter bc) { return bc.uint32; }
		public static implicit operator Int32(ByteConverter bc) { return bc.int32; }
		public static implicit operator UInt64(ByteConverter bc) { return bc.uint64; }
		public static implicit operator Int64(ByteConverter bc) { return bc.int64; }
		public static implicit operator Single(ByteConverter bc) { return bc.float32; }
		public static implicit operator Double(ByteConverter bc) { return bc.float64; }
		public static implicit operator Boolean(ByteConverter bc) { return bc.int32 != 0; }

		#endregion
	}
}