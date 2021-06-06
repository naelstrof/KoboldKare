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

using System;

namespace Photon.Compression
{
	/// <summary>
	/// Extension methods for writing bits to primitive buffers.
	/// </summary>
	public static class PrimitiveSerializeExt
	{
		const string overrunerror = "Write buffer overrun. writepos + bits exceeds target length. Data loss will occur.";

		#region	Inject ByteConverted

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ByteConverter value, ref ulong buffer, ref int bitposition, int bits)
		{
			((ulong)value).Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ByteConverter value, ref uint buffer, ref int bitposition, int bits)
		{
			((ulong)value).Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ByteConverter value, ref ushort buffer, ref int bitposition, int bits)
		{
			((ulong)value).Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ByteConverter value, ref byte buffer, ref int bitposition, int bits)
		{
			((ulong)value).Inject(ref buffer, ref bitposition, bits);
		}

		#endregion

		#region Signed Write/Inject/Extract

		/// <summary>
		/// Write a signed value into a buffer using zigzag.
		/// </summary>
		/// <returns>Returns the modified buffer with the injected value.</returns>
		public static ulong WriteSigned(this ulong buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			return buffer.Write(zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this long value, ref ulong buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this int value, ref ulong buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this short value, ref ulong buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this sbyte value, ref ulong buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Read a previously injected signed int back out of a buffer.
		/// </summary>
		/// <returns>Returns the restored signed value.</returns>
		public static int ReadSigned(this ulong buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.Read(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		/// <summary>
		/// Write a signed value into a buffer using zigzag.
		/// </summary>
		/// <returns>Returns the modified buffer with the injected value.</returns>
		public static uint WriteSigned(this uint buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			return buffer.Write(zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this long value, ref uint buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this int value, ref uint buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this short value, ref uint buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this sbyte value, ref uint buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Read a previously injected signed int back out of a buffer.
		/// </summary>
		/// <returns>Returns the restored signed value.</returns>
		public static int ReadSigned(this uint buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.Read(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		/// <summary>
		/// Write a signed value into a buffer using zigzag.
		/// </summary>
		/// <returns>Returns the modified buffer with the injected value.</returns>
		public static ushort WriteSigned(this ushort buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			return buffer.Write(zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this long value, ref ushort buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this int value, ref ushort buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this short value, ref ushort buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this sbyte value, ref ushort buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Read a previously injected signed int back out of a buffer.
		/// </summary>
		/// <returns>Returns the restored signed value.</returns>
		public static int ReadSigned(this ushort buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.Read(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		/// <summary>
		/// Write a signed value into a buffer using zigzag.
		/// </summary>
		/// <returns>Returns the modified buffer with the injected value.</returns>
		public static byte WriteSigned(this byte buffer, int value, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			return buffer.Write(zigzag, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this long value, ref byte buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this int value, ref byte buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this short value, ref byte buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write) a signed value into a buffer using zigzag. The buffer reference is modified.
		/// </summary>
		public static void InjectSigned(this sbyte value, ref byte buffer, ref int bitposition, int bits)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));
			zigzag.Inject(ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Read a previously injected signed int back out of a buffer.
		/// </summary>
		/// <returns>Returns the restored signed value.</returns>
		public static int ReadSigned(this byte buffer, ref int bitposition, int bits)
		{
			uint value = (uint)buffer.Read(ref bitposition, bits);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));
			return zagzig;
		}

		#endregion

		#region Boolean Write/Inject/Extract

		public static ulong WritetBool(this ulong buffer, bool value, ref int bitposition)
		{
			return Write(buffer, (ulong)(value ? 1 : 0), ref bitposition, 1);
		}
		public static uint WritetBool(this uint buffer, bool value, ref int bitposition)
		{
			return Write(buffer, (ulong)(value ? 1 : 0), ref bitposition, 1);
		}
		public static ushort WritetBool(this ushort buffer, bool value, ref int bitposition)
		{
			return Write(buffer, (ulong)(value ? 1 : 0), ref bitposition, 1);
		}
		public static byte WritetBool(this byte buffer, bool value, ref int bitposition)
		{
			return Write(buffer, (ulong)(value ? 1 : 0), ref bitposition, 1);
		}

		public static void Inject(this bool value, ref ulong buffer, ref int bitposition)
		{
			Inject((ulong)(value ? 1 : 0), ref buffer,  ref bitposition, 1);
		}
		public static void Inject(this bool value, ref uint buffer, ref int bitposition)
		{
			Inject((ulong)(value ? 1 : 0), ref buffer, ref bitposition, 1);
		}
		public static void Inject(this bool value, ref ushort buffer, ref int bitposition)
		{
			Inject((ulong)(value ? 1 : 0), ref buffer, ref bitposition, 1);
		}
		public static void Inject(this bool value, ref byte buffer, ref int bitposition)
		{
			Inject((ulong)(value ? 1 : 0), ref buffer, ref bitposition, 1);
		}

		public static bool ReadBool(this ulong buffer, ref int bitposition)
		{
			return (buffer.Read(ref bitposition, 1) == 0) ? false : true;
		}
		public static bool ReadtBool(this uint buffer, ref int bitposition)
		{
			return (buffer.Read(ref bitposition, 1) == 0) ? false : true;
		}
		public static bool ReadBool(this ushort buffer, ref int bitposition)
		{
			return (buffer.Read(ref bitposition, 1) == 0) ? false : true;
		}
		public static bool ReadBool(this byte buffer, ref int bitposition)
		{
			return (buffer.Read(ref bitposition, 1) == 0) ? false : true;
		}

		#endregion

		#region Primary Write

		/// <summary>
		/// Primary Write x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// WARNING: Unlike Inject, Write passes the buffer by reference, so you MUST use the return value as the new buffer value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		/// <returns>Returns the modified buffer.</returns>
		public static ulong Write(this ulong buffer, ulong value, ref int bitposition, int bits = 64)
		{
			System.Diagnostics.Debug.Assert(bitposition + bits <= 64, overrunerror);

			ulong offsetvalue = value << bitposition;
			ulong mask = ulong.MaxValue >> (64 - bits) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetvalue);

			bitposition += bits;
			return buffer;
		}

		/// <summary>
		/// Primary Write x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// WARNING: Unlike Inject, Write passes the buffer by reference, so you MUST use the return value as the new buffer value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		/// <returns>Returns the modified buffer.</returns>
		public static uint Write(this uint buffer, ulong value, ref int bitposition, int bits = 64)
		{
			System.Diagnostics.Debug.Assert(bitposition + bits <= 32, overrunerror);

			uint offsetvalue = (uint)value << bitposition;
			uint mask = uint.MaxValue >> (32 - bits) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetvalue);

			bitposition += bits;
			return buffer;
		}

		/// <summary>
		/// Primary Write x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// WARNING: Unlike Inject, Write passes the buffer by reference, so you MUST use the return value as the new buffer value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		/// <returns>Returns the modified buffer.</returns>
		public static ushort Write(this ushort buffer, ulong value, ref int bitposition, int bits = 64)
		{
			System.Diagnostics.Debug.Assert(bitposition + bits <= 16, overrunerror);

			uint offsetvalue = ((uint)value << bitposition);
			uint mask = ((uint)ushort.MaxValue >> (16 - bits) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetvalue);
			buffer = (ushort)_target;

			bitposition += bits;
			return buffer;
		}

		/// <summary>
		/// Primary Write x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// WARNING: Unlike Inject, Write passes the buffer by reference, so you MUST use the return value as the new buffer value.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		/// <returns>Returns the modified buffer.</returns>
		public static byte Write(this byte buffer, ulong value, ref int bitposition, int bits = 64)
		{
			System.Diagnostics.Debug.Assert(bitposition + bits <= 8, overrunerror);

			uint offsetvalue = ((uint)value << bitposition);
			uint mask = ((uint)byte.MaxValue >> (8 - bits) << bitposition);

			// Clear bits in buffer we need to write to, then write to them.
			uint _target = buffer & ~mask;
			_target |= (mask & offsetvalue);
			buffer = (byte)_target;

			bitposition += bits;
			return buffer;
		}

		#endregion

		#region Inject UInt64 Buffer

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong value, ref ulong buffer, ref int bitposition, int bits = 64)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);

		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong value, ref ulong buffer, int bitposition, int bits = 64)
		{
			System.Diagnostics.Debug.Assert(bitposition + bits <= 64, overrunerror);

			ulong offsetvalue = value << bitposition;
			ulong mask = ulong.MaxValue >> (64 - bits) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetvalue);
		}


		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint value, ref ulong buffer, ref int bitposition, int bits = 32)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);

		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint value, ref ulong buffer, int bitposition, int bits = 32)
		{
			System.Diagnostics.Debug.Assert(bitposition + bits <= 64, overrunerror);

			ulong offsetvalue = ((ulong)value << bitposition);
			ulong mask = ulong.MaxValue >> (64 - bits) << bitposition;

			// Clear bits in buffer we need to write to, then write to them.
			buffer &= ~mask;
			buffer |= (mask & offsetvalue);

		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort value, ref ulong buffer, ref int bitposition, int bits = 16)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);

		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort value, ref ulong buffer, int bitposition, int bits = 16)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte value, ref ulong buffer, ref int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte value, ref ulong buffer, int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// Negative numbers will not serialize properly. Use InjectUnsigned for signed values.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void InjectUnsigned(this long value, ref ulong buffer, ref int bitposition, int bits = 32)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// Negative numbers will not serialize properly. Use InjectUnsigned for signed values.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void InjectUnsigned(this int value, ref ulong buffer, ref int bitposition, int bits = 32)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// Negative numbers will not serialize properly. Use InjectUnsigned for signed values.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void InjectUnsigned(this short value, ref ulong buffer, ref int bitposition, int bits = 32)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// Negative numbers will not serialize properly. Use InjectUnsigned for signed values.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void InjectUnsigned(this sbyte value, ref ulong buffer, ref int bitposition, int bits = 32)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}

		#endregion

		#region Inject UInt32 Buffer

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong value, ref uint buffer, ref int bitposition, int bits = 64)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);

		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong value, ref uint buffer, int bitposition, int bits = 64)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);

		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint value, ref uint buffer, ref int bitposition, int bits = 32)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);

		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint value, ref uint buffer, int bitposition, int bits = 32)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);

		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort value, ref uint buffer, ref int bitposition, int bits = 16)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);

		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort value, ref uint buffer, int bitposition, int bits = 16)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);

		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte value, ref uint buffer, ref int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);

		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte value, ref uint buffer, int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);

		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// Negative numbers will not serialize properly. Use InjectUnsigned for signed values.
		/// </summary>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void InjectUnsigned(this long value, ref uint buffer, ref int bitposition, int bits = 64)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// Negative numbers will not serialize properly. Use InjectUnsigned for signed values.
		/// </summary>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void InjectUnsigned(this int value, ref uint buffer, ref int bitposition, int bits = 64)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// Negative numbers will not serialize properly. Use InjectUnsigned for signed values.
		/// </summary>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void InjectUnsigned(this short value, ref uint buffer, ref int bitposition, int bits = 64)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// Negative numbers will not serialize properly. Use InjectUnsigned for signed values.
		/// </summary>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void InjectUnsigned(this sbyte value, ref uint buffer, ref int bitposition, int bits = 64)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}

		#endregion

		#region Inject UInt16 Buffer

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong value, ref ushort buffer, ref int bitposition, int bits = 16)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong value, ref ushort buffer, int bitposition, int bits = 16)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint value, ref ushort buffer, ref int bitposition, int bits = 16)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint value, ref ushort buffer, int bitposition, int bits = 16)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort value, ref ushort buffer, ref int bitposition, int bits = 16)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort value, ref ushort buffer, int bitposition, int bits = 16)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte value, ref ushort buffer, ref int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte value, ref ushort buffer, int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}

		#endregion

		#region Inject UInt8 Buffer

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong value, ref byte buffer, ref int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ulong value, ref byte buffer, int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint value, ref byte buffer, ref int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this uint value, ref byte buffer, int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort value, ref byte buffer, ref int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this ushort value, ref byte buffer, int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte value, ref byte buffer, ref int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Write position. Writing will begin at this position in the buffer.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		public static void Inject(this byte value, ref byte buffer, int bitposition, int bits = 8)
		{
			buffer = Write(buffer, (ulong)value, ref bitposition, bits);
		}
		#endregion

		#region Obsolete Extract

		/// <summary>
		/// Extract (read/deserialize) a value from a source primitive (the buffer) by reading x bits starting at the bitposition, and return the reconstructed value. 
		/// </summary>
		/// <param name="value">Source primitive buffer to read from.</param>
		/// <param name="bits">Number of lower order bits to copy from source to return value.</param>
		/// <param name="bitposition">Auto-incremented reference to the value read bit pointer. Extraction starts at this point in value.</param>
		/// <returns>Downcast this ulong return value to the desired type.</returns>
		[System.Obsolete("Argument order changed")]
		public static ulong Extract(this ulong value, int bits, ref int bitposition)
		{
			return Extract(value, bits, ref bitposition);
		}

		#endregion

		#region Read - Uint64 Buffer

		/// <summary>
		/// Read a value from a source primitive (the buffer) by reading x bits starting at the bitposition, and return the reconstructed value. 
		/// </summary>
		/// <param name="value">Source primitive buffer to read from.</param>
		/// <param name="bits">Number of lower order bits to copy from source to return value.</param>
		/// <param name="bitposition">Auto-incremented reference to the value read bit pointer. Read starts at this point in value.</param>
		/// <returns>Downcast this ulong return value to the desired type.</returns>
		public static ulong Read(this ulong value, ref int bitposition, int bits)
		{
			ulong mask = (ulong.MaxValue >> (64 - bits));
			ulong fragment = (((ulong)value >> bitposition) & mask);

			bitposition += bits;
			return fragment;
		}
		[System.Obsolete("Use Read instead.")]
		public static ulong Extract(this ulong value, ref int bitposition, int bits)
		{
			ulong mask = (ulong.MaxValue >> (64 - bits));
			ulong fragment = (((ulong)value >> bitposition) & mask);

			bitposition += bits;
			return fragment;
		}

		/// <summary>
		/// Extract and return bits from value. 
		/// </summary>
		/// <param name="value">Source primitive.</param>
		/// <param name="bits">How many lower order bits to read.</param>
		/// <returns>Cast the return value to the desired type.</returns>
		[System.Obsolete("Always include the [ref int bitposition] argument. Extracting from position 0 would be better handled with a mask operation.")]
		public static ulong Extract(this ulong value, int bits)
		{
			ulong mask = (ulong.MaxValue >> (64 - bits));
			ulong fragment = ((ulong)value & mask);

			return fragment;
		}

		#endregion

		#region Read - Uint32 Buffer

		/// <summary>
		/// Read a value from a source primitive (the buffer) by reading x bits starting at the bitposition, and return the reconstructed value. 
		/// </summary>
		/// <param name="value">Source primitive buffer to read from.</param>
		/// <param name="bits">Number of lower order bits to copy from source to return value.</param>
		/// <param name="bitposition">Auto-incremented reference to the value read bit pointer. Read starts at this point in value.</param>
		/// <returns>Cast this uint return value to the desired type.</returns>
		public static uint Read(this uint value, ref int bitposition, int bits)
		{
			uint mask = (uint.MaxValue >> (32 - bits));
			uint fragment = (((uint)value >> bitposition) & mask);

			bitposition += bits;
			return fragment;
		}
		[System.Obsolete("Use Read instead.")]
		public static uint Extract(this uint value, ref int bitposition, int bits)
		{
			uint mask = (uint.MaxValue >> (32 - bits));
			uint fragment = (((uint)value >> bitposition) & mask);

			bitposition += bits;
			return fragment;
		}
		/// <summary>
		/// Extract and return bits from value. 
		/// </summary>
		/// <param name="value">Source primitive.</param>
		/// <param name="bits">How many lower order bits to read.</param>
		/// <returns>Cast the return value to the desired type.</returns>
		[System.Obsolete("Always include the [ref int bitposition] argument. Extracting from position 0 would be better handled with a mask operation.")]
		public static uint Extract(this uint value, int bits)
		{
			uint mask = (uint.MaxValue >> (32 - bits));
			uint fragment = ((uint)value & mask);

			return fragment;
		}

		#endregion

		#region Read - Uint16 Buffer

		/// <summary>
		/// Read a value from a source primitive (the buffer) by reading x bits starting at the bitposition, and return the reconstructed value. 
		/// </summary>
		/// <param name="value">Source primitive buffer to read from.</param>
		/// <param name="bits">Number of lower order bits to copy from source to return value.</param>
		/// <param name="bitposition">Auto-incremented reference to the value read bit pointer. Read starts at this point in value.</param>
		/// <returns>Cast this ushort return value to the desired type.</returns>
		public static uint Read(this ushort value, ref int bitposition, int bits)
		{
			uint mask = ((uint)ushort.MaxValue >> (16 - bits));
			uint fragment = (((uint)value >> bitposition) & mask);

			bitposition += bits;
			return fragment;
		}
		[System.Obsolete("Use Read instead.")]
		public static uint Extract(this ushort value, ref int bitposition, int bits)
		{
			uint mask = ((uint)ushort.MaxValue >> (16 - bits));
			uint fragment = (((uint)value >> bitposition) & mask);

			bitposition += bits;
			return fragment;
		}

		#endregion

		#region Read - Uint8 Buffer

		/// <summary>
		/// Read a value from a source primitive (the buffer) by reading x bits starting at the bitposition, and return the reconstructed value. 
		/// </summary>
		/// <param name="value">Source primitive buffer to read from.</param>
		/// <param name="bits">Number of lower order bits to copy from source to return value.</param>
		/// <param name="bitposition">Auto-incremented reference to the value read bit pointer. Read starts at this point in value.</param>
		/// <returns>Downcast this uint return value to the desired type.</returns>
		public static uint Read(this byte value, ref int bitposition, int bits)
		{
			uint mask = ((uint)byte.MaxValue >> (8 - bits));
			uint fragment = (((uint)value >> bitposition) & mask);

			bitposition += bits;
			return fragment;
		}
		[System.Obsolete("Use Read instead.")]
		public static uint Extract(this byte value, ref int bitposition, int bits)
		{
			uint mask = ((uint)byte.MaxValue >> (8 - bits));
			uint fragment = (((uint)value >> bitposition) & mask);

			bitposition += bits;
			return fragment;
		}
		/// <summary>
		/// Extract and return bits from value. 
		/// </summary>
		/// <param name="value">Source primitive.</param>
		/// <param name="bits">How many lower order bits to read.</param>
		/// <returns>Cast the return value to the desired type.</returns>
		[System.Obsolete("Always include the [ref int bitposition] argument. Extracting from position 0 would be better handled with a mask operation.")]
		public static byte Extract(this byte value, int bits)
		{
			uint mask = ((uint)byte.MaxValue >> (8 - bits));
			uint fragment = ((uint)value & mask);

			return (byte)fragment;
		}

		#endregion


		#region Float

		/// <summary>
		/// Inject (serialize/write) a float into a primitive buffer at the bitposition. No compression occurs, this is a full 32bit write.
		/// </summary>
		/// <param name="f">Float to compress and write.</param>
		/// <param name="buffer">Target buffer for write.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		public static void Inject(this float f, ref ulong buffer, ref int bitposition)
		{
			buffer = Write(buffer, ((ulong)(ByteConverter)f), ref bitposition, 32);
		}

		/// <summary>
		/// REad a float from a bitpacked primitive(value) starting at bitposition.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		/// <returns></returns>
		public static float ReadFloat(this ulong buffer, ref int bitposition)
		{
			return (ByteConverter)Read(buffer, ref bitposition, 32);
		}
		[System.Obsolete("Use Read instead.")]
		public static float ExtractFloat(this ulong buffer, ref int bitposition)
		{
			return (ByteConverter)Extract(buffer, ref bitposition, 32);
		}

		#endregion

		#region HalfFloat

		/// <summary>
		/// Inject (serialize/write) a compressed float into a primitive buffer at the bitposition.
		/// </summary>
		/// <param name="f">Float to compress and write.</param>
		/// <param name="buffer">Target buffer for write.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		public static ushort InjectAsHalfFloat(this float f, ref ulong buffer, ref int bitposition)
		{
			ushort c = HalfFloat.HalfUtilities.Pack(f);
			buffer = Write(buffer, c, ref bitposition, 16);
			return c;
		}
		/// <summary>
		/// Inject (serialize/write) a compressed float into a primitive buffer at the bitposition.
		/// </summary>
		/// <param name="f">Float to compress and write.</param>
		/// <param name="buffer">Target buffer for write.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		public static ushort InjectAsHalfFloat(this float f, ref uint buffer, ref int bitposition)
		{
			ushort c = HalfFloat.HalfUtilities.Pack(f);
			buffer = Write(buffer, c, ref bitposition, 16);
			return c;
		}

		/// <summary>
		/// Read a float from a bitpacked primitive(value) starting at bitposition.
		/// </summary>
		/// <param name="buffer">Source buffer to read from.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		/// <returns></returns>
		public static float ReadHalfFloat(this ulong buffer, ref int bitposition)
		{
			ushort c = (ushort)Read(buffer, ref bitposition, 16);
			return HalfFloat.HalfUtilities.Unpack(c);
		}
		[System.Obsolete("Use Read instead.")]
		public static float ExtractHalfFloat(this ulong buffer, ref int bitposition)
		{
			ushort c = (ushort)Extract(buffer, ref bitposition, 16);
			return HalfFloat.HalfUtilities.Unpack(c);
		}
		/// <summary>
		/// Read a float from a bitpacked primitive(value) starting at bitposition.
		/// </summary>
		/// <param name="buffer">Source buffer to read from.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer (in bits)</param>
		/// <returns></returns>
		public static float ReadHalfFloat(this uint buffer, ref int bitposition)
		{
			ushort c = (ushort)Read(buffer, ref bitposition, 16);
			return HalfFloat.HalfUtilities.Unpack(c);
		}
		[System.Obsolete("Use Read instead.")]
		public static float ExtractHalfFloat(this uint buffer, ref int bitposition)
		{
			ushort c = (ushort)Extract(buffer, ref bitposition, 16);
			return HalfFloat.HalfUtilities.Unpack(c);
		}

		#endregion


		#region Obsolete Inject

		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		[System.Obsolete("Argument order changed")]
		public static void Inject(this ulong value, ref uint buffer, int bits, ref int bitposition)
		{
			Inject(value, ref buffer, ref bitposition, bits);
		}
		/// <summary>
		/// Inject (write/serialize) x bits of source value into a target primitive (the buffer) starting at bitposition.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="buffer">Target of write.</param>
		/// <param name="bitposition">Auto-incremented write position. Writing will begin at this position in the buffer, and this value will have bits added to it.</param>
		/// <param name="bits">Number of lower order bits to copy from source to target buffer.</param>
		[System.Obsolete("Argument order changed")]
		public static void Inject(this ulong value, ref ulong buffer, int bits, ref int bitposition)
		{
			Inject(value, ref buffer, ref bitposition, bits);
		}
		#endregion


	}
}
