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

//using Photon.Compression.Utilities;

namespace Photon.Compression
{

	/// <summary>
	/// A Utility class that gives all byte[], uint[] and ulong[] buffers bitpacking/serialization methods.
	/// </summary>
	public static class ArraySerializeExt
	{
		private const string bufferOverrunMsg = "Byte buffer length exceeded by write or read. Dataloss will occur. Likely due to a Read/Write mismatch.";

		#region Zero

		// byte[]
		/// <summary>
		/// Zero out all array values. Start and End values are inclusive.
		/// </summary>
		public static void Zero(this byte[] buffer, int startByte, int endByte)
		{
			for (int i = startByte; i <= endByte; ++i)
				buffer[i] = 0;
		}

		/// <summary>
		/// Zero out all array values. Start value is inclusive.
		/// </summary>
		public static void Zero(this byte[] buffer, int startByte)
		{
			int cnt = buffer.Length;
			for (int i = startByte; i < cnt; ++i)
				buffer[i] = 0;
		}

		/// <summary>
		/// Zero out all array values.
		/// </summary>
		public static void Zero(this byte[] buffer)
		{
			int cnt = buffer.Length;
			for (int i = 0; i < cnt; ++i)
				buffer[i] = 0;
		}

		// ushort[]
		/// <summary>
		/// Zero out all array values. Start and End values are inclusive.
		/// </summary>
		public static void Zero(this ushort[] buffer, int startByte, int endByte)
		{
			for (int i = startByte; i <= endByte; ++i)
				buffer[i] = 0;
		}

		/// <summary>
		/// Zero out all array values. Start value is inclusive.
		/// </summary>
		public static void Zero(this ushort[] buffer, int startByte)
		{
			int cnt = buffer.Length;
			for (int i = startByte; i < cnt; ++i)
				buffer[i] = 0;
		}

		/// <summary>
		/// Zero out all array values.
		/// </summary>
		public static void Zero(this ushort[] buffer)
		{
			int cnt = buffer.Length;
			for (int i = 0; i < cnt; ++i)
				buffer[i] = 0;
		}

		// uint[]
		/// <summary>
		/// Zero out all array values. Start and End values are inclusive.
		/// </summary>
		public static void Zero(this uint[] buffer, int startByte, int endByte)
		{
			for (int i = startByte; i <= endByte; ++i)
				buffer[i] = 0;
		}

		/// <summary>
		/// Zero out all array values. Start value is inclusive.
		/// </summary>
		public static void Zero(this uint[] buffer, int startByte)
		{
			int cnt = buffer.Length;
			for (int i = startByte; i < cnt; ++i)
				buffer[i] = 0;
		}

		/// <summary>
		/// Zero out all array values.
		/// </summary>
		public static void Zero(this uint[] buffer)
		{
			int cnt = buffer.Length;
			for (int i = 0; i < cnt; ++i)
				buffer[i] = 0;
		}

		// ulong[]
		/// <summary>
		/// Zero out all array values. Start and End values are inclusive.
		/// </summary>
		public static void Zero(this ulong[] buffer, int startByte, int endByte)
		{
			for (int i = startByte; i <= endByte; ++i)
				buffer[i] = 0;
		}

		/// <summary>
		/// Zero out all array values. Start value is inclusive.
		/// </summary>
		public static void Zero(this ulong[] buffer, int startByte)
		{
			int cnt = buffer.Length;
			for (int i = startByte; i < cnt; ++i)
				buffer[i] = 0;
		}

		/// <summary>
		/// Zero out all array values.
		/// </summary>
		public static void Zero(this ulong[] buffer)
		{
			int cnt = buffer.Length;
			for (int i = 0; i < cnt; ++i)
				buffer[i] = 0;
		}

		#endregion

		#region Read/Write Signed Value

		public static void WriteSigned(this byte[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.Write(zigzag, ref bitposition, bits);
		}
		public static void WriteSigned(this uint[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.Write(zigzag, ref bitposition, bits);
		}
		public static void WriteSigned(this ulong[] buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			buffer.Write(zigzag, ref bitposition, bits);
		}

		public static void WriteSigned(this byte[] buffer, long value, ref int bitposition, int bits)
		{
			ulong zigzag = (ulong)((value << 1) ^ (value >> 63));
			buffer.Write(zigzag, ref bitposition, bits);
		}
		public static void WriteSigned(this uint[] buffer, long value, ref int bitposition, int bits)
		{
			ulong zigzag = (ulong)((value << 1) ^ (value >> 63));
			buffer.Write(zigzag, ref bitposition, bits);
		}
		public static void WriteSigned(this ulong[] buffer, long value, ref int bitposition, int bits)
		{
			ulong zigzag = (ulong)((value << 1) ^ (value >> 63));
			buffer.Write(zigzag, ref bitposition, bits);
		}

		public static int ReadSigned(this byte[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.Read(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}
		public static int ReadSigned(this uint[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.Read(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}
		public static int ReadSigned(this ulong[] buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.Read(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		public static long ReadSigned64(this byte[] buffer, ref int bitposition, int bits)
		{
			ulong value = buffer.Read(ref bitposition, bits);
			long zagzig = ((long)(value >> 1) ^ (-(long)(value & 1)));
			return zagzig;
		}
		public static long ReadSigned64(this uint[] buffer, ref int bitposition, int bits)
		{
			ulong value = buffer.Read(ref bitposition, bits);
			long zagzig = ((long)(value >> 1) ^ (-(long)(value & 1)));
			return zagzig;
		}
		public static long ReadSigned64(this ulong[] buffer, ref int bitposition, int bits)
		{
			ulong value = buffer.Read(ref bitposition, bits);
			long zagzig = ((long)(value >> 1) ^ (-(long)(value & 1)));
			return zagzig;
		}

		#endregion

		#region Float Reader/Writer

		/// <summary>
		/// Converts a float to a 32bit uint with ByteConverter, then writes those 32 bits to the buffer.
		/// </summary>
		/// <param name="buffer">The array we are reading from.</param>
		/// <param name="value">The float value to write.</param>
		/// <param name="bitposition">The bit position in the array we start the read at. Will be incremented by 32 bits.</param>
		public static void WriteFloat(this byte[] buffer, float value, ref int bitposition)
		{
			Write(buffer, ((ByteConverter)value).uint32, ref bitposition, 32);
		}

		/// <summary>
		/// Reads a uint32 from the buffer, and converts that back to a float with a ByteConverter cast. If performance is a concern, you can call the primary (ByteConverter)byte[].Read())
		/// </summary>
		/// <param name="buffer">The array we are reading from.</param>
		/// <param name="bitposition">The bit position in the array we start the read at. Will be incremented by 32 bits.</param>
		public static float ReadFloat(this byte[] buffer, ref int bitposition)
		{
			return ((ByteConverter)Read(buffer, ref bitposition, 32));
		}

		#endregion

		#region Primary Append

		/// <summary>
		/// Faster Primary Write method specifically specifically for sequential writes. 
		/// Doesn't preserve existing data past the write point in exchnage for a faster write.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="value"></param>
		/// <param name="bitposition"></param>
		/// <param name="bits"></param>
		public static void Append(this byte[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 8;
			const int MODULUS = MAXBITS - 1;
			int offset = (bitposition & MODULUS); // this is just a modulus
			int index = bitposition >> 3;

			ulong offsetmask = ((1UL << offset) - 1);

			ulong comp = buffer[index] & offsetmask;
			ulong result = comp | (value << offset);

			buffer[index] = (byte)result;

			offset = MAXBITS - offset;
			while (offset < bits)
			{
				index++;
				buffer[index] = (byte)((value >> offset));
				offset = offset + MAXBITS;
			}
			bitposition += bits;
		}

		public static void Append(this uint[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 32;
			const int MODULUS = MAXBITS - 1;
			int offset = bitposition & MODULUS; // this is just a modulus
			int index = bitposition >> 5;

			ulong offsetmask = ((1UL << offset) - 1);

			ulong comp = buffer[index] & offsetmask;
			ulong result = comp | (value << offset);

			buffer[index] = (uint)result;

			offset = MAXBITS - offset;
			while (offset < bits)
			{
				index++;
				buffer[index] = (uint)((value >> offset));
				offset = offset + MAXBITS;
			}
			bitposition += bits;
		}
		/// <summary>
		/// Faster Primary Write method specifically specifically for sequential writes. 
		/// Doesn't preserve existing data past the write point in exchnage for a faster write.
		/// </summary>
		public static void Append(this uint[] buffer, uint value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 32;
			const int MODULUS = MAXBITS - 1;
			int offset = bitposition & MODULUS; // this is just a modulus
			int index = bitposition >> 5;

			ulong offsetmask = ((1UL << offset) - 1);
			ulong comp = buffer[index] & offsetmask;
			ulong result = comp | ((ulong)value << offset);

			buffer[index] = (uint)result;
			buffer[index + 1] = (uint)(result >> MAXBITS);

			bitposition += bits;
		}

		public static void Append(this ulong[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = bitposition & MODULUS; // this is just a modulus
			int index = bitposition >> 6;

			ulong offsetmask = ((1UL << offset) - 1);
			ulong comp = buffer[index] & offsetmask;
			ulong result = comp | (value << offset);

			buffer[index] = result;
			buffer[index + 1] = value >> (MAXBITS - offset);

			bitposition += bits;
		}

		#endregion

		#region Primary Writers

		/// <summary>
		/// This is the primary byte[].Write() method. All other byte[].Write methods lead to this one, so when performance matters, cast using (ByteConverter)value and use this method.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="value"></param>
		/// <param name="bitposition"></param>
		/// <param name="bits"></param>
		/// <returns></returns>
		public static void Write(this byte[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 8;
			const int MODULUS = MAXBITS - 1;
			int offset = bitposition & MODULUS;
			int index = bitposition >> 3;
			int totalpush = offset + bits;

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong offsetmask = mask << offset;
			ulong offsetcomp = value << offset;

			buffer[index] = (byte)((buffer[index] & ~offsetmask) | (offsetcomp & offsetmask));

			offset = MAXBITS - offset;
			totalpush = totalpush - MAXBITS;

			// These are complete overwrites of the array element, so no masking is required
			while (totalpush > MAXBITS)
			{
				index++;
				offsetcomp = value >> offset;
				buffer[index] = (byte)offsetcomp;
				offset += MAXBITS;
				totalpush = totalpush - MAXBITS;
			}

			// remaning partial write needs masking
			if (totalpush > 0)
			{
				index++;

				offsetmask = mask >> offset;
				offsetcomp = value >> offset;
				buffer[index] = (byte)((buffer[index] & ~offsetmask) | (offsetcomp & offsetmask));
			}
			bitposition += bits;
		}

		public static void Write(this uint[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 32;
			const int MODULUS = MAXBITS - 1;
			int offset = bitposition & MODULUS;
			int index = bitposition >> 5;
			int totalpush = offset + bits;

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong offsetmask = mask << offset;
			ulong offsetval = value << offset;

			buffer[index] = (uint)((buffer[index] & ~offsetmask) | (offsetval & offsetmask));

			offset = MAXBITS - offset;
			totalpush = totalpush - MAXBITS;

			while (totalpush > MAXBITS)
			{
				index++;
				offsetmask = mask >> offset;
				offsetval = value >> offset;
				buffer[index] = (uint)((buffer[index] & ~offsetmask) | (offsetval & offsetmask));
				offset += MAXBITS;
				totalpush = totalpush - MAXBITS;
			}
			bitposition += bits;
		}

		public static void Write(this ulong[] buffer, ulong value, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = bitposition & MODULUS;
			int index = bitposition >> 6;
			int totalpush = offset + bits;

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong offsetmask = mask << offset;
			ulong offsetval = value << offset;

			buffer[index] = (buffer[index] & ~offsetmask) | (offsetval & offsetmask);

			offset = MAXBITS - offset;
			totalpush = totalpush - MAXBITS;

			while (totalpush > MAXBITS)
			{
				index++;
				offsetmask = mask >> offset;
				offsetval = value >> offset;
				buffer[index] = (buffer[index] & ~offsetmask) | (offsetval & offsetmask);
				offset += MAXBITS;
				totalpush = totalpush - MAXBITS;
			}
			bitposition += bits;
		}

		#endregion

		#region Secondary Writers

		public static void WriteBool(this ulong[] buffer, bool b, ref int bitposition)
		{
			buffer.Write((ulong)(b ? 1 : 0), ref bitposition, 1);
		}
		public static void WriteBool(this uint[] buffer, bool b, ref int bitposition)
		{
			buffer.Write((ulong)(b ? 1 : 0), ref bitposition, 1);
		}
		public static void WriteBool(this byte[] buffer, bool b, ref int bitposition)
		{
			buffer.Write((ulong)(b ? 1 : 0), ref bitposition, 1);
		}

		#endregion

		#region Primary Readers

		/// <summary>
		/// This is the Primary byte[].Read() method. All other byte[].ReadXXX() methods lead here. For maximum performance use this for all Read() calls and cast accordingly.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		/// <returns>UInt64 read value. Cast this to the intended type.</returns>
		public static ulong Read(this byte[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
				return 0;

			const int MAXBITS = 8;
			const int MODULUS = MAXBITS - 1;
			int offset = bitposition & MODULUS; // this is just a modulus
			int index = bitposition >> 3;

#if UNITY_EDITOR || DEVELOPEMENT_BUILD
			if ((bitposition + bits) > (buffer.Length << 3))
				UnityEngine.Debug.LogError(bufferOverrunMsg);
#endif

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong value = (ulong)buffer[index] >> offset;
			offset = MAXBITS - offset;
			while (offset < bits)
			{
				index++;
				value |= (ulong)buffer[index] << offset;
				offset += MAXBITS;
			}

			bitposition += bits;
			return value & mask;
		}

		/// <summary>
		/// This is the Primary uint[].Read() method. All other uint[].ReadXXX methods lead here. For maximum performance use this for all Read() calls and cast accordingly.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		/// <returns>UInt64 read value. Cast this to the intended type.</returns>
		public static ulong Read(this uint[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
				return 0;

			const int MAXBITS = 32;
			const int MODULUS = MAXBITS - 1;
			int offset = bitposition & MODULUS; // this is just a modulus
			int index = bitposition >> 5;

#if UNITY_EDITOR || DEVELOPEMENT_BUILD
			if ((bitposition + bits) > (buffer.Length << 3))
				UnityEngine.Debug.LogError(bufferOverrunMsg);
#endif

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong value = (ulong)buffer[index] >> offset;
			offset = MAXBITS - offset;
			while (offset < bits)
			{
				index++;
				value |= (ulong)buffer[index] << offset;
				offset += MAXBITS;
			}
			bitposition += bits;
			return value & mask;
		}

		/// <summary>
		/// This is the Primary ulong[].Read() method. All other ulong[].ReadXXX methods lead here. For maximum performance use this for all Read() calls and cast accordingly.
		/// </summary>
		/// <param name="buffer">The array we are deserializing from.</param>
		/// <param name="bitposition">The position in the array (in bits) where we will begin reading.</param>
		/// <param name="bits">The number of bits to read.</param>
		/// <returns>UInt64 read value. Cast this to the intended type.</returns>
		public static ulong Read(this ulong[] buffer, ref int bitposition, int bits)
		{
			if (bits == 0)
				return 0;

			const int MAXBITS = 64;
			const int MODULUS = MAXBITS - 1;
			int offset = bitposition & MODULUS; // this is just a modulus
			int index = bitposition >> 6;

#if UNITY_EDITOR || DEVELOPEMENT_BUILD
			if ((bitposition + bits) > (buffer.Length << 3))
				UnityEngine.Debug.LogError(bufferOverrunMsg);
#endif

			ulong mask = ulong.MaxValue >> (64 - bits);
			ulong value = (ulong)buffer[index] >> offset;
			offset = MAXBITS - offset;
			while (offset < bits)
			{
				index++;
				value |= (ulong)buffer[index] << offset;
				offset += MAXBITS;
			}

			bitposition += bits;
			return value & mask;
		}

		#endregion

		#region Secondary Readers
		
		// Ulong
		[System.Obsolete("Just use Read(), it return a ulong already.")]
		public static ulong ReadUInt64(this byte[] buffer, ref int bitposition, int bits = 64)
		{
			return Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Just use Read(), it return a ulong already.")]
		public static ulong ReadUInt64(this uint[] buffer, ref int bitposition, int bits = 64)
		{
			return Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Just use Read(), it return a ulong already.")]
		public static ulong ReadUInt64(this ulong[] buffer, ref int bitposition, int bits = 64)
		{
			return Read(buffer, ref bitposition, bits);
		}

		// UInt
		public static uint ReadUInt32(this byte[] buffer, ref int bitposition, int bits = 32)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}

		public static uint ReadUInt32(this uint[] buffer, ref int bitposition, int bits = 32)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}

		public static uint ReadUInt32(this ulong[] buffer, ref int bitposition, int bits = 32)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}

		// UShort
		public static ushort ReadUInt16(this byte[] buffer, ref int bitposition, int bits = 16)
		{
			return (ushort)Read(buffer, ref bitposition, bits);
		}

		public static ushort ReadUInt16(this uint[] buffer, ref int bitposition, int bits = 16)
		{
			return (ushort)Read(buffer, ref bitposition, bits);
		}

		public static ushort ReadUInt16(this ulong[] buffer, ref int bitposition, int bits = 16)
		{
			return (ushort)Read(buffer, ref bitposition, bits);
		}

		//Byte
		public static byte ReadByte(this byte[] buffer, ref int bitposition, int bits = 8)
		{
			return (byte)Read(buffer, ref bitposition, bits);
		}

		public static byte ReadByte(this uint[] buffer, ref int bitposition, int bits = 32)
		{
			return (byte)Read(buffer, ref bitposition, bits);
		}

		public static byte ReadByte(this ulong[] buffer, ref int bitposition, int bits)
		{
			return (byte)Read(buffer, ref bitposition, bits);
		}

		// Bool
		public static bool ReadBool(this ulong[] buffer, ref int bitposition)
		{
			return Read(buffer, ref bitposition, 1) == 1 ? true : false;
		}
		public static bool ReadBool(this uint[] buffer, ref int bitposition)
		{
			return Read(buffer, ref bitposition, 1) == 1 ? true : false;
		}
		public static bool ReadBool(this byte[] buffer, ref int bitposition)
		{
			return Read(buffer, ref bitposition, 1) == 1 ? true : false;
		}

		// Char
		public static char ReadChar(this ulong[] buffer, ref int bitposition)
		{
			return (char)Read(buffer, ref bitposition, 16);
		}
		public static char ReadChar(this uint[] buffer, ref int bitposition)
		{
			return (char)Read(buffer, ref bitposition, 16);
		}
		public static char ReadChar(this byte[] buffer, ref int bitposition)
		{
			return (char)Read(buffer, ref bitposition, 16);
		}

		#endregion

		#region ReadOut Safe

		/// <summary>
		/// Read the contents of one array buffer to another. This safe version doesn't use Unsafe, and may be up to 3x slower than ReadArrayOutUnsafe().
		/// </summary>
		/// <param name="source">Source array to copy from.</param>
		/// <param name="srcStartPos">Bit position in source to start reading frome.</param>
		/// <param name="target">Target array</param>
		/// <param name="bitposition">Current write position for target. Will start writing at this bitposition. Value gets incremented.</param>
		/// <param name="bits">Number of bits to readout. Typically the current write position of the source buffer.</param>
		public static void ReadOutSafe(this ulong[] source, int srcStartPos, byte[] target, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			int readpos = srcStartPos;
			int remaining = bits;

			// TODO: Add len checks

			while (remaining > 0)
			{
				int cnt = remaining > 64 ? 64 : remaining;
				ulong val = source.Read(ref readpos, cnt);
				target.Write(val, ref bitposition, cnt);

				remaining -= cnt;
			}
		}

		/// <summary>
		/// Read the contents of one array buffer to another. This safe version doesn't use Unsafe, and may be up to 3x slower than ReadArrayOutUnsafe().
		/// </summary>
		/// <param name="source">Source array to copy from.</param>
		/// <param name="srcStartPos">Bit position in source to start reading frome.</param>
		/// <param name="target">Target array</param>
		/// <param name="bitposition">Current write position for target. Will start writing at this bitposition. Value gets incremented.</param>
		/// <param name="bits">Number of bits to readout. Typically the current write position of the source buffer.</param>
		public static void ReadOutSafe(this ulong[] source, int srcStartPos, ulong[] target, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			int readpos = srcStartPos;
			int remaining = bits;

			// TODO: Add len checks

			while (remaining > 0)
			{
				int cnt = remaining > 64 ? 64 : remaining;
				ulong val = source.Read(ref readpos, cnt);
				target.Write(val, ref bitposition, cnt);

				remaining -= cnt;
			}
		}

		/// <summary>
		/// Read the contents of one array buffer to another. This safe version doesn't use Unsafe, and may be up to 3x slower than ReadArrayOutUnsafe().
		/// </summary>
		/// <param name="source">Source array to copy from.</param>
		/// <param name="srcStartPos">Bit position in source to start reading frome.</param>
		/// <param name="target">Target array</param>
		/// <param name="bitposition">Current write position for target. Will start writing at this bitposition. Value gets incremented.</param>
		/// <param name="bits">Number of bits to readout. Typically the current write position of the source buffer.</param>
		public static void ReadOutSafe(this byte[] source, int srcStartPos, ulong[] target, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			int readpos = srcStartPos;
			int remaining = bits;

			// TODO: Add len checks

			while (remaining > 0)
			{
				int cnt = remaining > 8 ? 8 : remaining;
				ulong val = source.Read(ref readpos, cnt);
				target.Write(val, ref bitposition, cnt);

				remaining -= cnt;
			}
		}

		/// <summary>
		/// Read the contents of one array buffer to another. This safe version doesn't use Unsafe, and may be up to 3x slower than ReadArrayOutUnsafe().
		/// </summary>
		/// <param name="source">Source array to copy from.</param>
		/// <param name="srcStartPos">Bit position in source to start reading frome.</param>
		/// <param name="target">Target array</param>
		/// <param name="bitposition">Current write position for target. Will start writing at this bitposition. Value gets incremented.</param>
		/// <param name="bits">Number of bits to readout. Typically the current write position of the source buffer.</param>
		public static void ReadOutSafe(this byte[] source, int srcStartPos, byte[] target, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;

			int readpos = srcStartPos;
			int remaining = bits;

			// TODO: Add len checks

			while (remaining > 0)
			{
				int cnt = remaining > 8 ? 8 : remaining;
				ulong val = source.Read(ref readpos, cnt);
				target.Write(val, ref bitposition, cnt);

				remaining -= cnt;
			}
		}

		#endregion

		#region Virtual Indexers

		/// <summary>
		/// Treats buffer as ulong[] and returns the index value of that virtual ulong[]
		/// </summary>
		/// <param name="index">The index of the virtual ulong[]</param>
		public static ulong IndexAsUInt64(this byte[] buffer, int index)
		{
			int i = index << 3;
			return (ulong)(
				(ulong)buffer[i] |
				(ulong)buffer[i + 1] << 8 |
				(ulong)buffer[i + 2] << 16 |
				(ulong)buffer[i + 3] << 24 |
				(ulong)buffer[i + 4] << 32 |
				(ulong)buffer[i + 5] << 40 |
				(ulong)buffer[i + 6] << 48 |
				(ulong)buffer[i + 7] << 56);
		}
		/// <summary>
		/// Treats buffer as ulong[] and returns the index value of that virtual ulong[]
		/// </summary>
		/// <param name="index">The index of the virtual ulong[]</param>
		public static ulong IndexAsUInt64(this uint[] buffer, int index)
		{
			int i = index << 1;
			return (ulong)(
				(ulong)buffer[i] |
				(ulong)buffer[i + 1] << 32);
		}

		/// <summary>
		/// Treats buffer as uint[] and returns the index value of that virtual uint[]
		/// </summary>
		/// <param name="index">The index of the virtual uint[]</param>
		public static uint IndexAsUInt32(this byte[] buffer, int index)
		{
			int i = index << 3;
			return (uint)(
				(uint)buffer[i] |
				(uint)buffer[i + 1] << 8 |
				(uint)buffer[i + 2] << 16 |
				(uint)buffer[i + 3] << 24);
		}
		/// <summary>
		/// Treats buffer as uint[] and returns the index value of that virtual uint[]
		/// </summary>
		/// <param name="index">The index of the virtual uint[]</param>
		public static uint IndexAsUInt32(this ulong[] buffer, int index)
		{
			const int MODULUS = 1;
			int i = index >> 1;
			int offset = (index & MODULUS) << 5; // modulus * 8
			ulong element = buffer[i];
			return (byte)((element >> offset));
		}
		/// <summary>
		/// Treats buffer as byte[] and returns the index value of that virtual byte[]
		/// </summary>
		/// <param name="index">The index of the virtual byte[]</param>
		public static byte IndexAsUInt8(this ulong[] buffer, int index)
		{
			const int MODULUS = 7;
			int i = index >> 3;
			int offset = (index & MODULUS) << 3; // modulus * 8
			ulong element = buffer[i];
			return (byte)((element >> offset));
		}
		/// <summary>
		/// Treats buffer as byte[] and returns the index value of that virtual byte[]
		/// </summary>
		/// <param name="index">The index of the virtual byte[]</param>
		public static byte IndexAsUInt8(this uint[] buffer, int index)
		{
			const int MODULUS = 3;
			int i = index >> 3;
			int offset = (index & MODULUS) << 3; // modulus * 8
			ulong element = buffer[i];
			return (byte)((element >> offset));
		}

		#endregion

		#region Obsolete Writers

		[System.Obsolete("Argument order has changed.")]
		public static byte[] Write(this byte[] buffer, ulong value, int bits, ref int bitposition)
		{
			Write(buffer, value, ref bitposition, bits);
			return buffer;
		}
		[System.Obsolete("Argument order has changed.")]
		public static uint[] Write(this uint[] buffer, ulong value, int bits, ref int bitposition)
		{
			Write(buffer, value, ref bitposition, bits);
			return buffer;
		}
		[System.Obsolete("Argument order has changed.")]
		public static ulong[] Write(this ulong[] buffer, ulong value, int bits, ref int bitposition)
		{
			Write(buffer, value, ref bitposition, bits);
			return buffer;
		}
		[System.Obsolete("Argument order has changed.")]
		public static byte[] Write(this byte[] buffer, float value, ref int bitposition)
		{
			Write(buffer, ((ByteConverter)value).uint32, ref bitposition, 32);
			return buffer;
		}
		[System.Obsolete("Argument order has changed.")]
		public static float Read(this byte[] buffer, ref int bitposition)
		{
			return Read(buffer, ref bitposition, 32);
		}

		#endregion

		#region Obsolete Readers

		[System.Obsolete("Argument order has changed.")]
		public static ulong Read(this byte[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}
		[System.Obsolete("Argument order has changed.")]
		public static ulong Read(this uint[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}
		[System.Obsolete("Argument order has changed.")]
		public static ulong Read(this ulong[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static byte ReadUInt8(this ulong[] buffer, int bits, ref int bitposition)
		{
			return (byte)Read(buffer, ref bitposition, bits);
		}
		[System.Obsolete("Argument order has changed.")]
		public static uint ReadUInt32(this ulong[] buffer, int bits, ref int bitposition)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}
		[System.Obsolete("Argument order has changed.")]
		public static ulong ReadUInt64(this ulong[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static byte ReadUInt8(this uint[] buffer, int bits, ref int bitposition)
		{
			return (byte)Read(buffer, ref bitposition, bits);
		}
		[System.Obsolete("Argument order has changed.")]
		public static uint ReadUInt32(this uint[] buffer, int bits, ref int bitposition)
		{
			return (uint)Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static ulong ReadUInt64(this uint[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static byte ReadUInt8(this byte[] buffer, int bits, ref int bitposition)
		{
			return (byte)Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static uint ReadUInt32(this byte[] buffer, int bits, ref int bitposition)
		{
			return (byte)Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Argument order has changed.")]
		public static ulong ReadUInt64(this byte[] buffer, int bits, ref int bitposition)
		{
			return Read(buffer, ref bitposition, bits);
		}

		[System.Obsolete("Instead use ReadOutUnsafe. They are much faster.")]
		public static byte[] Write(this byte[] buffer, byte[] srcbuffer, ref int readpos, ref int writepos, int bits)
		{
			while (bits > 0)
			{
				int fragbits = (bits > 64) ? 64 : bits;
				ulong frag = srcbuffer.Read(ref readpos, fragbits);
				buffer.Write(frag, ref writepos, fragbits);
				bits -= fragbits;
			}

			return buffer;
		}

		[System.Obsolete("Do not use unless you have removed ArraySerializerUnsafe, this is for benchmarking comparisons only.")]
		public static void ReadArrayOutSafe(this ulong[] source, int srcStartPos, byte[] target, ref int bitposition, int bits)
		{
			if (bits == 0)
				return;
			int readpos = srcStartPos;
			int remaining = bits;

			// TODO: Add len checks

			while (remaining > 0)
			{
				int cnt = remaining > 64 ? 64 : remaining;
				ulong val = source.Read(ref readpos, cnt);
				target.Write(val, ref bitposition, cnt);

				remaining -= cnt;
			}
		}

		#endregion

	}
}

