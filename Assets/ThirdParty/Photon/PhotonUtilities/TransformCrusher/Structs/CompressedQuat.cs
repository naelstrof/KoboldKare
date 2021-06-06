// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

namespace Photon.Compression
{

	public struct CompressedQuat
	{
		public readonly QuatCrusher crusher;
		public readonly ulong cvalue;

		public CompressedQuat(QuatCrusher crusher, ulong cvalue) : this()
		{
			this.crusher = crusher;
			this.cvalue = cvalue;
		}

		public CompressedQuat(QuatCrusher crusher, uint cvalue) : this()
		{
			this.crusher = crusher;
			this.cvalue = cvalue;
		}

		public CompressedQuat(QuatCrusher crusher, ushort cvalue) : this()
		{
			this.crusher = crusher;
			this.cvalue = cvalue;
		}

		public CompressedQuat(QuatCrusher crusher, byte cvalue) : this()
		{
			this.crusher = crusher;
			this.cvalue = cvalue;
		}

		public static implicit operator ulong(CompressedQuat cv) { return cv.cvalue; }
		public static explicit operator uint(CompressedQuat cv) { return (uint)cv.cvalue; }
		public static explicit operator ushort(CompressedQuat cv) { return (ushort)cv.cvalue; }
		public static explicit operator byte(CompressedQuat cv) { return (byte)cv.cvalue; }
		//public static implicit operator PackedValue(CompressedValue cv) { return new PackedValue(cv.cvalue, cv.bits); }
		//[System.Obsolete("PackedValue is slated to be removed.")]
		//public static implicit operator PackedValue(CompressedQuat cv) { return new PackedValue(cv.cvalue, cv.crusher.Bits); }

		public Quaternion Decompress()
		{
			return crusher.Decompress(cvalue);
		}


		public override string ToString()
		{
			return "[CompressedQuat: " + cvalue + " bits: " + crusher + "] ";
		}
	}
}