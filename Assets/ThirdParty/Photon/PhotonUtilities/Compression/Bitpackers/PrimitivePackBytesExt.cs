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
	public static class PrimitivePackBytesExt
	{

		#region Primary Write / Inject Packed

		/// <summary>
		/// EXPERIMENTAL: Primary Write Method.
		/// </summary>
		public static ulong WritePackedBytes(this ulong buffer, ulong value, ref int bitposition, int bits)
		{
			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebytes = value.UsedByteCount();

			buffer = buffer.Write((uint)(valuebytes), ref bitposition, (int)sizebits);
			buffer = buffer.Write(value, ref bitposition, valuebytes << 3);

			//UnityEngine.Debug.Log(value + " buff:" + buffer + "bytes " + bytes + 
			//	" = [" + (int)sizebits  + " : " + (valuebytes << 3) + "]  total bits: " + ((int)sizebits + (valuebytes << 3)));

			return buffer;
		}
		/// <summary>
		/// EXPERIMENTAL: Primary Write Method.
		/// </summary>
		public static uint WritePackedBytes(this uint buffer, uint value, ref int bitposition, int bits)
		{
			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebytes = value.UsedByteCount();

			buffer = buffer.Write((uint)(valuebytes), ref bitposition, sizebits);
			buffer = buffer.Write(value, ref bitposition, valuebytes << 3);

			//UnityEngine.Debug.Log(value + " buff:" + buffer + "bytes " + bytes + 
			//	" = [" + (int)sizebits + " : " + (valuebytes << 3) + "]  total bits: " + ((int)sizebits + (valuebytes << 3)));

			return buffer;
		}

		/// <summary>
		/// EXPERIMENTAL: Primary Write Method.
		/// </summary>
		public static void InjectPackedBytes(this  ulong value, ref ulong buffer,ref int bitposition, int bits)
		{
			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebytes = value.UsedByteCount();

			buffer = buffer.Write((uint)(valuebytes), ref bitposition, (int)sizebits);
			buffer = buffer.Write(value, ref bitposition, valuebytes << 3);

			//UnityEngine.Debug.Log(value + " buff:" + buffer + "bytes " + bytes +
			//	" = [" + (int)sizebits + " : " + (valuebytes << 3) + "]  total bits: " + ((int)sizebits + (valuebytes << 3)));
		}
		/// <summary>
		/// EXPERIMENTAL: Primary Write Method.
		/// </summary>
		public static void InjectPackedBytes(this uint value, ref uint buffer, ref int bitposition, int bits)
		{
			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebytes = value.UsedByteCount();

			buffer = buffer.Write((uint)(valuebytes), ref bitposition, sizebits);
			buffer = buffer.Write(value, ref bitposition, valuebytes << 3);

			//UnityEngine.Debug.Log(value + " buff:" + buffer + "bytes " + bytes +
			//	" = [" + (int)sizebits + " : " + (valuebytes << 3) + "]  total bits: " + ((int)sizebits + (valuebytes << 3)));
		}

		#endregion

		#region Primary Read Packed

		/// <summary>
		/// Primary Reader for PackedBytes.
		/// </summary>
		public static ulong ReadPackedBytes(this ulong buffer, ref int bitposition, int bits)
		{
			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebytes = (int)buffer.Read(ref bitposition, sizebits);
			return buffer.Read(ref bitposition, valuebytes << 3);
		}
		/// <summary>
		/// Primary Reader for PackedBytes.
		/// </summary>
		public static uint ReadPackedBytes(this uint buffer, ref int bitposition, int bits)
		{
			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebytes = (int)buffer.Read(ref bitposition, sizebits);
			return buffer.Read(ref bitposition, valuebytes << 3);
		}

		#endregion

		#region Packed Signed

		/// <summary>
		/// EXPERIMENTAL: Primary Write signed value as PackedByte. 
		/// </summary>
		public static ulong WriteSignedPackedBytes(this ulong buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			return buffer.WritePackedBytes(zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// EXPERIMENTAL: Read signed value from PackedByte. 
		/// </summary>
		public static int ReadSignedPackedBytes(this ulong buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.ReadPackedBytes(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}
		#endregion
	}
}

