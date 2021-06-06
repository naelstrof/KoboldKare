using System;

namespace Photon.Compression
{
	public static class ArraySegmentExt
	{
		public static ArraySegment<byte> ExtractArraySegment(byte[] buffer, ref int bitposition)
		{
			return new ArraySegment<byte>(buffer, 0, (bitposition + 7) >> 3);
		}

		public static ArraySegment<ushort> ExtractArraySegment(ushort[] buffer, ref int bitposition)
		{
			return new ArraySegment<ushort>(buffer, 0, (bitposition + 15) >> 4);
		}

		public static ArraySegment<uint> ExtractArraySegment(uint[] buffer, ref int bitposition)
		{
			return new ArraySegment<uint>(buffer, 0, (bitposition + 31) >> 5);
		}

		public static ArraySegment<ulong> ExtractArraySegment(ulong[] buffer, ref int bitposition)
		{
			return new ArraySegment<ulong>(buffer, 0, (bitposition + 63) >> 6);
		}

		public static void Append(this ArraySegment<byte> buffer, ulong value, ref int bitposition, int bits)
		{
			int offset = buffer.Offset << 3;
			bitposition += offset;
			buffer.Array.Append(value, ref bitposition, bits);
			bitposition -= offset;
		}
		public static void Append(this ArraySegment<uint> buffer, ulong value, ref int bitposition, int bits)
		{
			int offset = buffer.Offset << 5;
			bitposition += offset;
			buffer.Array.Append(value, ref bitposition, bits);
			bitposition -= offset;
		}
		public static void Append(this ArraySegment<ulong> buffer, ulong value, ref int bitposition, int bits)
		{
			int offset = buffer.Offset << 6;
			bitposition += offset;
			buffer.Array.Append(value, ref bitposition, bits);
			bitposition -= offset;
		}

		public static void Write(this ArraySegment<byte> buffer, ulong value, ref int bitposition, int bits)
		{
			int offset = buffer.Offset << 3;
			bitposition += offset;
			buffer.Array.Write(value, ref bitposition, bits);
			bitposition -= offset;
		}
		public static void Write(this ArraySegment<uint> buffer, ulong value, ref int bitposition, int bits)
		{
			int offset = buffer.Offset << 5;
			bitposition += offset;
			buffer.Array.Write(value, ref bitposition, bits);
			bitposition -= offset;
		}
		public static void Write(this ArraySegment<ulong> buffer, ulong value, ref int bitposition, int bits)
		{
			int offset = buffer.Offset << 6;
			bitposition += offset;
			buffer.Array.Write(value, ref bitposition, bits);
			bitposition -= offset;
		}


		public static ulong Read(this ArraySegment<byte> buffer, ref int bitposition, int bits)
		{
			int offset = buffer.Offset << 3;
			bitposition += offset;
			ulong value = buffer.Array.Read(ref bitposition, bits);
			bitposition -= offset;
			return value;
		}
		public static ulong Read(this ArraySegment<uint> buffer, ref int bitposition, int bits)
		{
			int offset = buffer.Offset << 5;
			bitposition += offset;
			ulong value = buffer.Array.Read(ref bitposition, bits);
			bitposition -= offset;
			return value;
		}
		public static ulong Read(this ArraySegment<ulong> buffer, ref int bitposition, int bits)
		{
			int offset = buffer.Offset << 6;
			bitposition += offset;
			ulong value = buffer.Array.Read(ref bitposition, bits);
			bitposition -= offset;
			return value;
		}

		public static void ReadOutSafe(this ArraySegment<byte> source, int srcStartPos, byte[] target, ref int bitposition, int bits)
		{
			int offset = source.Offset << 3;
			srcStartPos += offset;
			source.Array.ReadOutSafe(srcStartPos, target, ref bitposition, bits);
		}
		public static void ReadOutSafe(this ArraySegment<byte> source, int srcStartPos, ulong[] target, ref int bitposition, int bits)
		{
			int offset = source.Offset << 3;
			srcStartPos += offset;
			source.Array.ReadOutSafe(srcStartPos, target, ref bitposition, bits);
		}
		public static void ReadOutSafe(this ArraySegment<ulong> source, int srcStartPos, byte[] target, ref int bitposition, int bits)
		{
			int offset = source.Offset << 6;
			srcStartPos += offset;
			source.Array.ReadOutSafe(srcStartPos, target, ref bitposition, bits);
		}
		public static void ReadOutSafe(this ArraySegment<ulong> source, int srcStartPos, ulong[] target, ref int bitposition, int bits)
		{
			int offset = source.Offset << 6;
			srcStartPos += offset;
			source.Array.ReadOutSafe(srcStartPos, target, ref bitposition, bits);
		}
	}
}

