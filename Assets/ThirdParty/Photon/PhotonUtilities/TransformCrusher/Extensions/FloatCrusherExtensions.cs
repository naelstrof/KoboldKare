// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using emotitron.Compression;

namespace Photon.Compression
{
	public static class FloatCrusherExtensions
	{

		#region Primitive Buffer Writers

		/// <summary>
		/// Compress (encode) a float value using this FloatCrusher, then Inject() that compressed value into the
		/// supplied buffer starting at the indicated bitposition.
		/// </summary>
		/// <param name="f">Float to be compressed and serialized</param>
		/// <param name="buffer">Target primitive buffer to serialize into.</param>
		/// <param name="bitposition">The auto-incremented position in the array (in bits) where we will begin reading.</param>
		/// <param name="bcl"></param>
		/// <returns>Returns the compressed uint that was serialized.</returns>
		public static CompressedFloat Write(this FloatCrusher fc, float f, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			CompressedFloat c = fc.Compress(f);
			c.cvalue.Inject(ref buffer, ref bitposition, bits);
			return c;
		}
		/// <summary>
		/// Compress (encode) a float value using this FloatCrusher, then Inject() that compressed value into the
		/// supplied buffer starting at the indicated bitposition.
		/// </summary>
		/// <param name="f">Float to be compressed and serialized</param>
		/// <param name="buffer">Target primitive buffer to serialize into.</param>
		/// <param name="bitposition">The auto-incremented position in the array (in bits) where we will begin reading.</param>
		/// <param name="bcl"></param>
		/// <returns>Returns the compressed uint that was serialized.</returns>
		public static CompressedFloat Write(this FloatCrusher fc, float f, ref uint buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			CompressedFloat c = fc.Compress(f);
			c.cvalue.Inject(ref buffer, ref bitposition, bits);
			return c;
		}
		/// <summary>
		/// Compress (encode) a float value using this FloatCrusher, then Inject() that compressed value into the
		/// supplied buffer starting at the indicated bitposition.
		/// </summary>
		/// <param name="f">Float to be compressed and serialized</param>
		/// <param name="buffer">Target primitive buffer to serialize into.</param>
		/// <param name="bitposition">The auto-incremented position in the array (in bits) where we will begin reading.</param>
		/// <param name="bcl"></param>
		/// <returns>Returns the compressed uint that was serialized.</returns>
		public static CompressedFloat Write(this FloatCrusher fc, float f, ref ushort buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			CompressedFloat c = fc.Compress(f);
			c.cvalue.Inject(ref buffer, ref bitposition, bits);
			return c;
		}
		/// <summary>
		/// Compress (encode) a float value using this FloatCrusher, then Inject() that compressed value into the
		/// supplied buffer starting at the indicated bitposition.
		/// </summary>
		/// <param name="f">Float to be compressed and serialized</param>
		/// <param name="buffer">Target primitive buffer to serialize into.</param>
		/// <param name="bitposition">The auto-incremented position in the array (in bits) where we will begin reading.</param>
		/// <param name="bcl"></param>
		/// <returns>Returns the compressed uint that was serialized.</returns>
		public static CompressedFloat Write(this FloatCrusher fc, float f, ref byte buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			CompressedFloat c = fc.Compress(f);
			c.cvalue.Inject(ref buffer, ref bitposition, bits);
			return c;
		}

		/// <summary>
		/// Inject() a previously compressed value into the supplied ulong buffer starting at the indicated bitposition.
		/// </summary>
		/// <param name="c">Compressed value to be written</param>
		/// <param name="buffer">Where the bits will be written</param>
		/// <param name="bitposition">The position in the buffer to start writing at. Ref value will be increased by the bits used.</param>
		/// <param name="bcl"></param>
		/// <returns>Returns the compressed uint that was serialized.</returns>
		public static CompressedFloat Write(this FloatCrusher fc, CompressedFloat c, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			c.cvalue.Inject(ref buffer, ref bitposition, fc._bits[(int)bcl]);
			return c;
		}

		#endregion

		#region ULong Primitive Buffer Readers

		//UNTESTED
		/// <summary>
		/// Reads (deserialize) a compressed value from the buffer, decmpress, and return a restored float.
		/// </summary>
		/// <param name="buffer">Source buffer to read from.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer. Will begin reading bits from this position.</param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static float ReadAndDecompress(this FloatCrusher fc, ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			uint c = (uint)buffer.Read(ref bitposition, bits);
			return fc.Decompress(c);
		}


		//UNTESTED
		/// <summary>
		/// Reads (deserialize) a compressed value from the buffer, and returns a CompressedValue.
		/// </summary>
		/// <param name="buffer">Source buffer to read from.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer. Will begin reading bits from this position.</param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Read(this FloatCrusher fc, ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			uint c = (uint)buffer.Read(ref bitposition, bits);
			return new CompressedFloat(fc, c);
		}
		/// <summary>
		/// Reads (deserialize) a compressed value from the buffer, and returns a CompressedValue.
		/// </summary>
		/// <param name="buffer">Source buffer to read from.</param>
		/// <param name="bitposition">Auto-incremented read position for the buffer. Will begin reading bits from this position.</param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		[System.Obsolete("No reason for buffer to be a ref")]
		public static CompressedFloat Read(this FloatCrusher fc, ref ulong buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			uint mask = fc.masks[(int)bcl];
			uint c = (uint)((buffer >> bitposition) & mask);

			bitposition += bits;
			return new CompressedFloat(fc, c);
		}

		#endregion


		#region Array Buffer Writers

		/// <summary>
		/// Serialize a compressed value to an array buffer using this FloatCrusher.
		/// </summary>
		/// <param name="c">CompressedValue</param>
		/// <param name="buffer"></param>
		/// <param name="bitposition"></param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Write(this FloatCrusher fc, CompressedFloat c, byte[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			buffer.Write(c.cvalue, ref bitposition, bits);
			return c;
		}

		/// <summary>
		/// Serialize a compressed value to an array buffer using this FloatCrusher.
		/// </summary>
		/// <param name="c">CompressedValue</param>
		/// <param name="buffer"></param>
		/// <param name="bitposition"></param>
		/// <param name="bcl"></param>
		/// <returns></returns>

		public static CompressedFloat Write(this FloatCrusher fc, CompressedFloat c, uint[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			buffer.Write(c.cvalue, ref bitposition, bits);
			return c;
		}
		/// <summary>
		/// Serialize a compressed value to an array buffer using this FloatCrusher.
		/// </summary>
		/// <param name="c">CompressedValue</param>
		/// <param name="buffer"></param>
		/// <param name="bitposition"></param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Write(this FloatCrusher fc, CompressedFloat c, ulong[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			buffer.Write(c.cvalue, ref bitposition, bits);
			return c;
		}

		/// <summary>
		/// Serialize a compressed value to an array buffer using this FloatCrusher.
		/// </summary>
		/// <param name="c">CompressedValue</param>
		/// <param name="buffer"></param>
		/// <param name="bitposition"></param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Write(this FloatCrusher fc, uint c, byte[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			buffer.Write(c, ref bitposition, bits);
			return new CompressedFloat(fc, c);
		}

		/// <summary>
		/// Serialize a compressed value to an array buffer using this FloatCrusher.
		/// </summary>
		/// <param name="c">CompressedValue</param>
		/// <param name="buffer"></param>
		/// <param name="bitposition"></param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Write(this FloatCrusher fc, uint c, uint[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			buffer.Write(c, ref bitposition, bits);
			return new CompressedFloat(fc, c);
		}

		/// <summary>
		/// Serialize a compressed value to an array buffer using this FloatCrusher.
		/// </summary>
		/// <param name="c">CompressedValue</param>
		/// <param name="buffer"></param>
		/// <param name="bitposition"></param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Write(this FloatCrusher fc, uint c, ulong[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			buffer.Write(c, ref bitposition, bits);
			return new CompressedFloat(fc, c);
		}

		/// <summary>
		/// Serialize an uncompressed value to an array buffer using this FloatCrusher.
		/// </summary>
		/// <param name="f">Uncompressed float</param>
		/// <param name="buffer"></param>
		/// <param name="bitposition"></param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Write(this FloatCrusher fc, float f, byte[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			uint c = fc.Compress(f);
			int bits = fc._bits[(int)bcl];
			buffer.Write(c, ref bitposition, bits);
			return new CompressedFloat(fc, c);
		}
		/// <summary>
		/// Serialize an uncompressed value to an array buffer using this FloatCrusher.
		/// </summary>
		/// <param name="f">Uncompressed float</param>
		/// <param name="buffer"></param>
		/// <param name="bitposition"></param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Write(this FloatCrusher fc, float f, uint[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			uint c = fc.Compress(f);
			buffer.Write(c, ref bitposition, bits);
			return new CompressedFloat(fc, c);
		}

		/// <summary>
		/// Serialize an uncompressed value to an array buffer using this FloatCrusher.
		/// </summary>
		/// <param name="f">Uncompressed float</param>
		/// <param name="buffer"></param>
		/// <param name="bitposition"></param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Write(this FloatCrusher fc, float f, ulong[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			uint c = fc.Compress(f);
			buffer.Write(c, ref bitposition, bits);
			return new CompressedFloat(fc, c);
		}

		#endregion

		#region Array Buffer Readers

		/// <summary>
		/// Deserializes a CompressedValue from an array buffer.
		/// </summary>
		/// <param name="buffer">Source array buffer.</param>
		/// <param name="bitposition">Auto-incremented read position for buffer.</param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Read(this FloatCrusher fc, byte[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			return new CompressedFloat(fc, buffer.ReadUInt32(ref bitposition, bits));
		}
		/// <summary>
		/// Deserializes a CompressedValue from an array buffer.
		/// </summary>
		/// <param name="buffer">Source array buffer.</param>
		/// <param name="bitposition">Auto-incremented read position for buffer.</param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Read(this FloatCrusher fc, uint[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			return new CompressedFloat(fc, buffer.ReadUInt32(ref bitposition, bits));
		}
		/// <summary>
		/// Deserializes a CompressedValue from an array buffer.
		/// </summary>
		/// <param name="buffer">Source array buffer.</param>
		/// <param name="bitposition">Auto-incremented read position for buffer.</param>
		/// <param name="bcl"></param>
		/// <returns></returns>
		public static CompressedFloat Read(this FloatCrusher fc, ulong[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bits = fc._bits[(int)bcl];
			return new CompressedFloat(fc, buffer.ReadUInt32(ref bitposition, bits));
		}

		/// <summary>
		/// Deserializes a compressed (encoded) float out of an array from the indicated bitpostion, and Decode() it.
		/// </summary>
		/// <param name="buffer">Source array buffer.</param>
		/// <param name="bitposition">The auto-incremented position in the array (in bits) where we will begin reading.</param>
		/// <returns>Restored float value.</returns>
		public static float ReadAndDecompress(this FloatCrusher fc, byte[] buffer, ref int bitposition)
		{
			uint c = buffer.ReadUInt32(ref bitposition, fc._bits[0]);
			return fc.Decompress(c);
		}
		/// <summary>
		/// Deserializes a compressed (encoded) float out of an array from the indicated bitpostion, and Decode() it.
		/// </summary>
		/// <param name="buffer">Source array buffer.</param>
		/// <param name="bitposition">The auto-incremented position in the array (in bits) where we will begin reading.</param>
		/// <returns>Restored float value.</returns>
		public static float ReadAndDecompress(this FloatCrusher fc, uint[] buffer, ref int bitposition)
		{
			uint c = buffer.ReadUInt32(ref bitposition, fc._bits[0]);
			return fc.Decompress(c);
		}
		/// <summary>
		/// Deserializes a compressed (encoded) float out of an array from the indicated bitpostion, and Decode() it.
		/// </summary>
		/// <param name="buffer">Source array buffer.</param>
		/// <param name="bitposition">The auto-incremented position in the array (in bits) where we will begin reading.</param>
		/// <returns>Restored float value.</returns>
		public static float ReadAndDecompress(this FloatCrusher fc, ulong[] buffer, ref int bitposition)
		{
			uint c = buffer.ReadUInt32(ref bitposition, fc._bits[0]);
			return fc.Decompress(c);
		}

		#endregion
	}
}