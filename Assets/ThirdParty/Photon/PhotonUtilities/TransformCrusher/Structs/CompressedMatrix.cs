// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using emotitron.Compression;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Text;
#endif

namespace Photon.Compression
{
	/// <summary>
	/// This class contains CompressedElement classes for the compressed Position, Rotation and Scale,
	/// and exposes methods for comparing, copying, serialiing and deserializing the entire collection in one call.
	/// </summary>
	public class CompressedMatrix : IEquatable<CompressedMatrix>
	{
		public CompressedElement cPos = new CompressedElement();
		public CompressedElement cRot = new CompressedElement();
		public CompressedElement cScl = new CompressedElement();

		public TransformCrusher crusher;

		public static CompressedMatrix reusable = new CompressedMatrix();

		#region Constructors

		// Constructor
		public CompressedMatrix()
		{
		}

		public CompressedMatrix(TransformCrusher crusher)
		{
			this.crusher = crusher;
		}
		// Constructor
		public CompressedMatrix(TransformCrusher crusher, CompressedElement cPos, CompressedElement cRot, CompressedElement cScl)
		{
			this.crusher = crusher;
			this.cPos = cPos;
			this.cRot = cRot;
			this.cScl = cScl;
		}

		// Constructor
		public CompressedMatrix(TransformCrusher crusher, ref CompressedElement cPos, ref CompressedElement cRot, ref CompressedElement cScl, int pBits, int rBits, int sBits)
		{
			this.crusher = crusher;
			this.cPos = cPos;
			this.cRot = cRot;
			this.cScl = cScl;
		}

		#endregion

		public void CopyTo(CompressedMatrix copyTarget)
		{
			cPos.CopyTo(copyTarget.cPos);
			cRot.CopyTo(copyTarget.cRot);
			cScl.CopyTo(copyTarget.cScl);
		}
		public void CopyFrom(CompressedMatrix copySource)
		{
			cPos.CopyFrom(copySource.cPos);
			cRot.CopyFrom(copySource.cRot);
			cScl.CopyFrom(copySource.cScl);
		}

		public void Clear()
		{
			crusher = null;
			cPos.Clear();
			cRot.Clear();
			cScl.Clear();
		}

		#region AsArray

		protected static readonly ulong[] reusableArray64 = new ulong[6];
		protected static readonly uint[] reusableArray32 = new uint[12];
		protected static readonly byte[] reusableArray8 = new byte[24];

		// 64
		/// <summary>
		/// Serializes the CompressedMatrix into an array.
		/// <para>WARNING: The returned array is recycled - so the values are subject to change. Use contents immediately.</para>
		/// <para>If you want to store the returned value, supply a nonalloc array as an argument.</para>
		/// </summary>
		public ulong[] AsArray64(BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			crusher.Write(this, reusableArray64, ref bitposition, bcl);
			reusableArray64.Zero((bitposition + 63) >> 6);
			return reusableArray64;
		}

		/// <summary>
		/// Serializes the CompressedMatrix into supplied nonalloc array.
		/// <para>NOTE: Contents of the nonalloc array will be overwritten.</para>
		/// </summary>
		public void AsArray64(ulong[] nonalloc, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			crusher.Write(this, nonalloc, ref bitposition, bcl);
			nonalloc.Zero((bitposition + 63) >> 6);
		}

		// 32
		/// <summary>
		/// Serializes the CompressedMatrix into an array.
		/// <para>WARNING: The returned array is recycled - so the values are subject to change. Use contents immediately.</para>
		/// <para>If you want to store the returned value, supply a nonalloc array as an argument.</para>
		/// </summary>
		public uint[] AsArray32(BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			crusher.Write(this, reusableArray32, ref bitposition, bcl);
			reusableArray32.Zero((bitposition + 31) >> 5);
			return reusableArray32;
		}

		/// <summary>
		/// Serializes the CompressedMatrix into supplied nonalloc array.
		/// <para>NOTE: Contents of the nonalloc array will be overwritten.</para>
		/// </summary>
		public void AsArray32(uint[] nonalloc, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			crusher.Write(this, nonalloc, ref bitposition, bcl);
			nonalloc.Zero((bitposition + 31) >> 5);
		}

		// 8
		/// <summary>
		/// Serializes the CompressedMatrix into an array.
		/// <para>WARNING: The returned array is recycled - so the values are subject to change. Use contents immediately.</para>
		/// <para>If you want to store the returned value, supply a nonalloc array as an argument.</para>
		/// </summary>
		public byte[] AsArray8(BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			crusher.Write(this, reusableArray64, ref bitposition, bcl);
			reusableArray8.Zero((bitposition + 7) >> 3);
			return reusableArray8;
		}

		/// <summary>
		/// Serializes the CompressedMatrix into supplied nonalloc array.
		/// <para>NOTE: Contents of the nonalloc array will be overwritten.</para>
		/// </summary>
		public void AsArray8(byte[] nonalloc, BitCullingLevel bcl = BitCullingLevel.NoCulling)
		{
			int bitposition = 0;
			crusher.Write(this, nonalloc, ref bitposition, bcl);
			nonalloc.Zero((bitposition + 7) >> 3);
		}

		#endregion

		#region Implicit/Explicit Casts

		public static explicit operator ulong(CompressedMatrix cm)
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if (cm.crusher.TallyBits() > 64)
				Debug.LogError("Cast of CompressedMatrix to ulong only works if the total bits of all crushers is >= 64 bits.");
#endif
			ulong buffer = 0;
			int bitposition = 0;
			cm.crusher.Write(cm, ref buffer, ref bitposition);
			return buffer;
		}

		public static explicit operator uint(CompressedMatrix cm)
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if (cm.crusher.TallyBits() > 32)
				Debug.LogError("Cast of CompressedMatrix to uint only works if the total bits of all crushers is >= 32 bits.");
#endif
			ulong buffer = 0;
			int bitposition = 0;
			cm.crusher.Write(cm, ref buffer, ref bitposition);
			return (uint)buffer;
		}

		public static explicit operator ushort(CompressedMatrix cm)
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if (cm.crusher.TallyBits() > 16)
				Debug.LogError("Cast of CompressedMatrix to ushort only works if the total bits of all crushers is >= 16 bits.");
#endif
			ulong buffer = 0;
			int bitposition = 0;
			cm.crusher.Write(cm, ref buffer, ref bitposition);
			return (ushort)buffer;
		}

		public static explicit operator byte(CompressedMatrix cm)
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if (cm.crusher.TallyBits() > 8)
				Debug.LogError("Implicit cast of CompressedMatrix to byte only works if the total bits of all crushers is >= 8 bits.");
#endif
			ulong buffer = 0;
			int bitposition = 0;
			cm.crusher.Write(cm, ref buffer, ref bitposition);
			return (byte)buffer;
		}

		public static explicit operator ulong[] (CompressedMatrix cm)
		{
			return cm.AsArray64();
		}
		public static explicit operator uint[] (CompressedMatrix cm)
		{
			return cm.AsArray32();
		}
		public static explicit operator byte[] (CompressedMatrix cm)
		{
			return cm.AsArray8();
		}

		#endregion


		/// <summary>
		/// Decompress this CompressedMatrix into the supplied nonalloc Matrix class.
		/// </summary>
		/// <param name="nonalloc">The target for the uncompressed TRS.</param>
		public void Decompress(Matrix nonalloc)
		{
			if (crusher != null)
				crusher.Decompress(nonalloc, this);
			else
				nonalloc.Clear();
		}

		/// <summary>
		/// Decompress this CompressedMatrix into a recycled Matrix class.
		/// <para>WARNING: No Matrix is provided in this overload, so a reusable internal Matrix ref is used. Use immediately.</para>
		/// </summary>
		public Matrix Decompress()
		{
			crusher.Decompress(Matrix.reusable, this);
			return Matrix.reusable;
		}

		[System.Obsolete("Supply the transform to Compress. Default Transform has been deprecated to allow shared TransformCrushers.")]
		public void Apply()
		{
			if (crusher != null)
				crusher.Apply(this);
		}
		/// <summary>
		/// Convenience method. For the most direct call use crusher.Apply(transform, compressedmatrix) instead.
		/// </summary>
		public void Apply(Transform t)
		{
			if (crusher != null)
				crusher.Apply(t, this);
		}

		/// <summary>
		/// Convenience method. For the most direct call use crusher.Apply(rigidbody, compressedmatrix) instead.
		/// </summary>
		[System.Obsolete("Apply for Rigidbody has been replaced with Move and Set, to indicate usage of MovePosition/Rotation vs rb.position/rotation.")]
		public void Apply(Rigidbody rb)
		{
			if (crusher != null)
				crusher.Apply(rb, this);
		}
		/// <summary>
		/// Convenience method. For the most direct call use crusher.Set(rigidbody, compressedmatrix) instead.
		/// </summary>
		public void Set(Rigidbody rb)
		{
			if (crusher != null)
				crusher.Set(rb, this);
		}
		/// <summary>
		/// Convenience method. For the most direct call use crusher.Move(rigidbody, compressedmatrix) instead.
		/// </summary>
		public void Move(Rigidbody rb)
		{
			if (crusher != null)
				crusher.Move(rb, this);
		}

		public static bool operator ==(CompressedMatrix a, CompressedMatrix b)
		{
			if (ReferenceEquals(a, null))
				return false;

			return a.Equals(b);
		}
		public static bool operator !=(CompressedMatrix a, CompressedMatrix b)
		{
			if (ReferenceEquals(a, null))
				return true;

			return !a.Equals(b);
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		public static StringBuilder strb = new StringBuilder();

		public StringBuilder ToString(StringBuilder strb)
		{
			strb.Append("cpos: ").Append(cPos).Append(" crot: ").Append(cRot).Append(" csl: ").Append(cScl);
			return strb;
		}
		public override string ToString()
		{
			strb.Length = 0;
			strb.Append("cpos: ").Append(cPos).Append(" crot: ").Append(cRot).Append(" csl: ").Append(cScl);
			return "cpos: " + cPos + " crot: " + cRot + " srot " + cScl;
		}
#endif

		public override bool Equals(object obj)
		{
			return Equals(obj as CompressedMatrix);
		}

		/// <summary>
		/// Compare the values of this CompressedMatrix with the values of another.
		/// </summary>
		/// <param name="other"></param>
		/// <returns>True if the values match, false if not.</returns>
		public bool Equals(CompressedMatrix other)
		{
			return
				!ReferenceEquals(other, null) &&
				cPos.Equals(other.cPos) &&
				cRot.Equals(other.cRot) &&
				cScl.Equals(other.cScl);
		}

		public override int GetHashCode()
		{
			var hashCode = 94804922;
			hashCode = hashCode * -1521134295 + cPos.GetHashCode();
			hashCode = hashCode * -1521134295 + cRot.GetHashCode();
			hashCode = hashCode * -1521134295 + cScl.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<TransformCrusher>.Default.GetHashCode(crusher);
			return hashCode;
		}
	}

#if UNITY_EDITOR || DEVELOPMENT_BUILD

	public static class CompressedMatrixExt
	{
		public static StringBuilder AppendSB(this StringBuilder strb, CompressedMatrix cm)
		{

			strb.Append(" cPos: ").AppendSB(cm.cPos).Append(" cRot: ").AppendSB(cm.cRot).Append(" cScl: ").AppendSB(cm.cScl);
			return strb;
		}
	}
#endif

}


