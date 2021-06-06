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

namespace Photon.Compression
{
	/// <summary>
	/// Experimental packers, that counts number of used bits for serialization. Effective for values that hover close to zero.
	/// </summary>
	public static class PrimitivePackBitsExt
	{
		#region Primary Inject/Write Packed

		/// <summary>
		/// EXPERIMENTAL: Primary WritePacked Method
		/// </summary>
		public static ulong WritePackedBits(this ulong buffer, uint value, ref int bitposition, int bits)
		{
			int countbits = BitCounter.UsedBitCount((uint)bits);
			int cnt = value.UsedBitCount();

			buffer = buffer.Write((uint)(cnt), ref bitposition, (int)countbits);
			buffer = buffer.Write(value, ref bitposition, cnt);

			//UnityEngine.Debug.Log(value + " = ones : " + cnt + " / " + (int)countbits + "  total bits: " + ((int)countbits + cnt));
			return buffer;
		}
		/// <summary>
		/// EXPERIMENTAL: Primary WritePacked Method
		/// </summary>
		public static uint WritePackedBits(this uint buffer, ushort value, ref int bitposition, int bits)
		{
			int countbits = BitCounter.UsedBitCount((uint)bits);
			int cnt = value.UsedBitCount();

			buffer = buffer.Write((uint)(cnt), ref bitposition, (int)countbits);
			buffer = buffer.Write(value, ref bitposition, cnt);

			//UnityEngine.Debug.Log(value + " = ones : " + cnt + " / " + (int)countbits + "  total bits: " + ((int)countbits + cnt));
			return buffer;
		}
		/// <summary>
		/// EXPERIMENTAL: Primary WritePacked Method
		/// </summary>
		public static ushort WritePackedBits(this ushort buffer, byte value, ref int bitposition, int bits)
		{
			int countbits = BitCounter.UsedBitCount((uint)bits);
			int cnt = value.UsedBitCount();

			buffer = buffer.Write((uint)(cnt), ref bitposition, (int)countbits);
			buffer = buffer.Write(value, ref bitposition, cnt);

			//UnityEngine.Debug.Log(value + " = ones : " + cnt + " / " + (int)countbits + "  total bits: " + ((int)countbits + cnt));
			return buffer;
		}

		#endregion
		
		#region Primary Read Packed

		/// <summary>
		/// Primary Reader for PackedBits.
		/// </summary>
		public static ulong ReadPackedBits(this ulong buffer, ref int bitposition, int bits)
		{
			var packsize = BitCounter.UsedBitCount(bits);
			int cnt = (int)buffer.Read(ref bitposition, (int)packsize);
			return buffer.Read(ref bitposition, cnt);
		}
		/// <summary>
		/// Primary Reader for PackedBits.
		/// </summary>
		public static ulong ReadPackedBits(this uint buffer, ref int bitposition, int bits)
		{
			var packsize = BitCounter.UsedBitCount(bits);
			int cnt = (int)buffer.Read(ref bitposition, (int)packsize);
			return buffer.Read(ref bitposition, cnt);
		}
		/// <summary>
		/// Primary Reader for PackedBits.
		/// </summary>
		public static ulong ReadPackedBits(this ushort buffer, ref int bitposition, int bits)
		{
			var packsize = BitCounter.UsedBitCount(bits);
			int cnt = (int)buffer.Read(ref bitposition, (int)packsize);
			return buffer.Read(ref bitposition, cnt);
		}

		#endregion

		#region Packed Signed

		// Primary Writers
		/// <summary>
		/// EXPERIMENTAL: Primary Write packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static ulong WriteSignedPackedBits(this ulong buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer = buffer.WritePackedBits(zigzag, ref bitposition, bits);
			return buffer;
		}
		/// <summary>
		/// EXPERIMENTAL: Primary Write packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static uint WriteSignedPackedBits(this uint buffer, short value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer = buffer.WritePackedBits((ushort)zigzag, ref bitposition, bits);
			return buffer;
		}
		/// <summary>
		/// EXPERIMENTAL: Primary Write packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static ushort WriteSignedPackedBits(this ushort buffer, sbyte value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer = buffer.WritePackedBits((byte)zigzag, ref bitposition, bits);
			return buffer;
		}

		// Primary Readers

		/// <summary>
		/// EXPERIMENTAL: Primary Read packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static int ReadSignedPackedBits(this ulong buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return (int)zagzig;
		}

		/// <summary>
		/// EXPERIMENTAL: Primary Read packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static short ReadSignedPackedBits(this uint buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return (short)zagzig;
		}

		/// <summary>
		/// EXPERIMENTAL: Primary Read packed signed value. ZigZag is employed to move the sign to the rightmost position.
		/// Packed values work best for serializing fields that have a large possible range, but are mostly hover closer to zero in value.
		/// </summary>
		public static sbyte ReadSignedPackedBits(this ushort buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.ReadPackedBits(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return (sbyte)zagzig;
		}

		#endregion

	}
}

