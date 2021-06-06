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

#if DEVELOPMENT_BUILD
#define UNITY_ASSERTIONS
#endif

namespace Photon.Compression
{
	/// <summary>
	/// Experimental packers, that counts number of used bits for serialization. Effective for values that hover close to zero.
	/// </summary>
	public static class ArrayPackBytesExt
	{
		#region Primary Write Packed

		/// <summary>
		/// EXPERIMENTAL: Primary UNSAFE Write Method.
		/// </summary>
		public unsafe static void WritePackedBytes(ulong* uPtr, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebytes = value.UsedByteCount();

			ArraySerializeUnsafe.Write(uPtr, (uint)(valuebytes), ref bitposition, (int)sizebits);
			ArraySerializeUnsafe.Write(uPtr, value, ref bitposition, valuebytes << 3);

			//UnityEngine.Debug.Log(value + " buff:" + buffer + "bytes " + bytes +
			//	" = [" + (int)sizebits + " : " + (valuebytes << 3) + "]  total bits: " + ((int)sizebits + (valuebytes << 3)));
		}

		/// <summary>
		/// EXPERIMENTAL: Primary Write Method.
		/// </summary>
		public static void WritePackedBytes(this ulong[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebytes = value.UsedByteCount();

			buffer.Write((uint)(valuebytes), ref bitposition, (int)sizebits);
			buffer.Write(value, ref bitposition, valuebytes << 3);

			//UnityEngine.Debug.Log(value + " buff:" + buffer + "bytes " + bytes +
			//	" = [" + (int)sizebits + " : " + (valuebytes << 3) + "]  total bits: " + ((int)sizebits + (valuebytes << 3)));
		}
		/// <summary>
		/// EXPERIMENTAL: Primary Write Method.
		/// </summary>
		public static void WritePackedBytes(this uint[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebytes = value.UsedByteCount();

			buffer.Write((uint)(valuebytes), ref bitposition, sizebits);
			buffer.Write(value, ref bitposition, valuebytes << 3);

			//UnityEngine.Debug.Log(value + " buff:" + buffer + "bytes " + bytes +
			//	" = [" + (int)sizebits + " : " + (valuebits << 3) + "]  total bits: " + ((int)sizebits + (valuebits << 3)));
		}
		/// <summary>
		/// EXPERIMENTAL: Primary Write Method.
		/// </summary>
		public static void WritePackedBytes(this byte[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebytes = value.UsedByteCount();

			buffer.Write((uint)(valuebytes), ref bitposition, sizebits);
			buffer.Write(value, ref bitposition, valuebytes << 3);

			//UnityEngine.Debug.Log(value + " buff:" + buffer + "bytes " + bytes +
			//	" = [" + (int)sizebits + " : " + (valuebits << 3) + "]  total bits: " + ((int)sizebits + (valuebits << 3)));
		}

		#endregion

		#region Primary Read Packed

		/// <summary>
		/// Primary UNSAFE Reader for PackedBytes.
		/// </summary>
		public unsafe static ulong ReadPackedBytes(ulong* uPtr, ref int bitposition, int bits)
		{
			if (bits == 0)
				return 0;

			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebits = (int)ArraySerializeUnsafe.Read(uPtr, ref bitposition, sizebits) << 3;
			return ArraySerializeUnsafe.Read(uPtr, ref bitposition, valuebits);
		}
		/// <summary>
		/// Primary Reader for PackedBytes.
		/// </summary>
		public static ulong ReadPackedBytes(this ulong[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
				return 0;

			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebits = (int)buffer.Read(ref bitposition, sizebits) << 3;
			return buffer.Read(ref bitposition, valuebits);
		}
		/// <summary>
		/// Primary Reader for PackedBytes.
		/// </summary>
		public static ulong ReadPackedBytes(this uint[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
				return 0;

			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebits = (int)buffer.Read(ref bitposition, sizebits) << 3;
			return buffer.Read(ref bitposition, valuebits);
		}
		/// <summary>
		/// Primary Reader for PackedBytes.
		/// </summary>
		public static ulong ReadPackedBytes(this byte[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
				return 0;

			int bytes = (bits + 7) >> 3;
			int sizebits = bytes.UsedBitCount();
			int valuebits = (int)buffer.Read(ref bitposition, sizebits) << 3;
			return buffer.Read(ref bitposition, valuebits);
		}

		#endregion

		#region Packed Signed

		// Unsafe

		/// <summary>
		/// EXPERIMENTAL: Primary UNSAFE Write signed value as PackedByte. 
		/// </summary>
		public unsafe static void WriteSignedPackedBytes(ulong* uPtr, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			WritePackedBytes(uPtr, zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// EXPERIMENTAL: Primary UNSAFE Read signed value from PackedByte. 
		/// </summary>
		public unsafe static int ReadSignedPackedBytes(ulong* uPtr, ref int bitposition, int bits)
		{
			uint value = (uint)ReadPackedBytes(uPtr, ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		// ulong[]

		/// <summary>
		/// EXPERIMENTAL: Primary Write signed value as PackedByte. 
		/// </summary>
		public static void WriteSignedPackedBytes(this ulong[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBytes(zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// EXPERIMENTAL: Read signed value from PackedByte. 
		/// </summary>
		public static int ReadSignedPackedBytes(this ulong[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.ReadPackedBytes(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		// uint[]

		/// <summary>
		/// EXPERIMENTAL: Primary Write signed value as PackedByte. 
		/// </summary>
		public static void WriteSignedPackedBytes(this uint[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBytes(zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// EXPERIMENTAL: Read signed value from PackedByte. 
		/// </summary>
		public static int ReadSignedPackedBytes(this uint[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.ReadPackedBytes(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		// byte[]

		/// <summary>
		/// EXPERIMENTAL: Primary Write signed value as PackedByte. 
		/// </summary>
		public static void WriteSignedPackedBytes(this byte[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.WritePackedBytes(zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// EXPERIMENTAL: Read signed value from PackedByte. 
		/// </summary>
		public static int ReadSignedPackedBytes(this byte[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.ReadPackedBytes(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		/// <summary>
		/// EXPERIMENTAL: Primary Write signed value as PackedByte. 
		/// </summary>
		public static void WriteSignedPackedBytes64(this byte[] buffer, long value, ref int bitposition, int bits)
		{
			ulong zig = (ulong)((value << 1) ^ (value >> 63));
			buffer.WritePackedBytes(zig, ref bitposition, bits);
		}
		/// <summary>
		/// EXPERIMENTAL: Read signed value from PackedByte. 
		/// </summary>
		public static long ReadSignedPackedBytes64(this byte[] buffer, ref int bitposition, int bits)
		{
			ulong zig = buffer.ReadPackedBytes(ref bitposition, bits);
			long zag = (long)((long)(zig >> 1) ^ (-(long)(zig & 1)));
			return zag;
		}

		#endregion
	}
}
