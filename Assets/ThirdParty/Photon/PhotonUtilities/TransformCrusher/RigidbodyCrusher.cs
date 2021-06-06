//// ---------------------------------------------------------------------------------------------
//// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
//// <author>developer@exitgames.com</author>
//// ---------------------------------------------------------------------------------------------

//using UnityEngine;
//using emotitron.Compression;

//namespace Photon.Compression
//{
//	/// <summary>
//	/// Future home of the RigidbodyCrusher - still being developed - don't use
//	/// </summary>
//	[System.Serializable]
//	public class RigidbodyCrusher : TransformCrusher, ICrusherCopy<RigidbodyCrusher>
//	{

//		[SerializeField] private ElementCrusher velCrusher;
//		[SerializeField] private ElementCrusher angCrusher;


//		/// <summary>
//		/// Sets the velocity crusher to the assigned reference, subscribes to callbacks, and reruns CacheValues().
//		/// </summary>
//		public ElementCrusher VelocityCrusher
//		{
//			get { return velCrusher; }
//			set
//			{
//				if (ReferenceEquals(velCrusher, value))
//					return;

//				if (velCrusher != null)
//					velCrusher.OnRecalculated -= OnCrusherChange;

//				velCrusher = value;

//				if (velCrusher != null)
//					velCrusher.OnRecalculated += OnCrusherChange;

//				CacheValues();
//			}
//		}

//		/// <summary>
//		/// Sets the angular velocity crusher to the assigned reference, subscribes to callbacks, and reruns CacheValues().
//		/// </summary>
//		public ElementCrusher AngVelocityCrusher
//		{
//			get { return angCrusher; }
//			set
//			{
//				if (ReferenceEquals(angCrusher, value))
//					return;

//				if (angCrusher != null)
//					angCrusher.OnRecalculated -= OnCrusherChange;

//				angCrusher = value;

//				if (angCrusher != null)
//					angCrusher.OnRecalculated += OnCrusherChange;

//				CacheValues();
//			}
//		}


//		protected override void ConstructDefault(bool isStatic = false)
//		{
//			Debug.Log("RB Crusher ConstructDefault ");
//			base.ConstructDefault(isStatic);

//			if (isStatic)
//			{
//				// Statics initialize all crushers as null.
//			}
//			else
//			{
//				VelocityCrusher = new ElementCrusher(TRSType.Position, false);
//				AngVelocityCrusher = new ElementCrusher(TRSType.Position, false);
				
//			}
//		}

//		public RigidbodyCrusher()
//		{
//			UnityEngine.Debug.Log("RigidbodyCrusher Construct ");
//			ConstructDefault(false);
//		}
//		/// <summary>
//		/// Default constructor for TransformCrusher.
//		/// </summary>
//		/// <param name="isStatic">Set this as true if this crusher is not meant to be serialized. Static crushers are created in code, and are not meant to be modified after creation.
//		/// This allows them to be indexed by their hashcodes and reused.
//		/// </param>
//		public RigidbodyCrusher(bool isStatic = false)
//		{
//			ConstructDefault(isStatic);
//		}

//		#region Cached compression values


//		[System.NonSerialized] private readonly int[] cached_vBits = new int[4];
//		[System.NonSerialized] private readonly int[] cached_aBits = new int[4];

//		public override void CacheValues()
//		{
//			base.CacheValues();
//			for (int i = 0; i < 4; ++i)
//			{
//				cached_vBits[i] = (velCrusher == null) ? 0 : velCrusher.Cached_TotalBits[i];
//				cached_aBits[i] = (angCrusher == null) ? 0 : angCrusher.Cached_TotalBits[i];
//				_cached_total[i] += cached_vBits[i] + cached_aBits[i];
//				cached_total = System.Array.AsReadOnly(_cached_total);
//			}

//		}

//		#endregion


//		#region Array Writers

//		public void Write(CompressedMatrixRB cmRB, byte[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			if (!cached)
//				CacheValues();



//			if (cached_pBits[(int)bcl] > 0)
//				posCrusher.Write(cmRB.cPos, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_rBits[(int)bcl] > 0)
//				rotCrusher.Write(cmRB.cRot, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_sBits[(int)bcl] > 0)
//				sclCrusher.Write(cmRB.cScl, buffer, ref bitposition, IncludedAxes.XYZ, bcl);

//			if (cached_vBits[(int)bcl] > 0)
//				velCrusher.Write(cmRB.cVel, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_aBits[(int)bcl] > 0)
//				angCrusher.Write(cmRB.cAng, buffer, ref bitposition, IncludedAxes.XYZ, bcl);

//		}

//		public void Write(CompressedMatrixRB cmRB, uint[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			if (!cached)
//				CacheValues();

//			if (cached_pBits[(int)bcl] > 0)
//				posCrusher.Write(cmRB.cPos, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_rBits[(int)bcl] > 0)
//				rotCrusher.Write(cmRB.cRot, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_sBits[(int)bcl] > 0)
//				sclCrusher.Write(cmRB.cScl, buffer, ref bitposition, IncludedAxes.XYZ, bcl);

//			if (cached_vBits[(int)bcl] > 0)
//				velCrusher.Write(cmRB.cVel, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_aBits[(int)bcl] > 0)
//				angCrusher.Write(cmRB.cAng, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//		}

//		public void Write(CompressedMatrixRB cmRB, ulong[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			if (!cached)
//				CacheValues();

//			if (cached_pBits[(int)bcl] > 0)
//				posCrusher.Write(cmRB.cPos, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_rBits[(int)bcl] > 0)
//				rotCrusher.Write(cmRB.cRot, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_sBits[(int)bcl] > 0)
//				sclCrusher.Write(cmRB.cScl, buffer, ref bitposition, IncludedAxes.XYZ, bcl);


//			if (cached_vBits[(int)bcl] > 0)
//				velCrusher.Write(cmRB.cVel, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_aBits[(int)bcl] > 0)
//				angCrusher.Write(cmRB.cAng, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//		}

//		#endregion

//		#region Array Readers

//		//[System.Obsolete()]
//		public new MatrixRB ReadAndDecompress(ulong[] array, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			int bitposition = 0;
//			return ReadAndDecompress(array, ref bitposition, bcl);
//		}

//		public new MatrixRB ReadAndDecompress(uint[] array, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			int bitposition = 0;
//			return ReadAndDecompress(array, ref bitposition, bcl);
//		}

//		public new MatrixRB ReadAndDecompress(byte[] array, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			int bitposition = 0;
//			return ReadAndDecompress(array, ref bitposition, bcl);
//		}

//		// Skips intermediate step of creating a compressedMatrx
//		public void ReadAndDecompress(MatrixRB nonalloc, ulong[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			Read(CompressedMatrixRB.reusable, array, ref bitposition, bcl);
//			Decompress(nonalloc, CompressedMatrixRB.reusable);
//		}
//		public void ReadAndDecompress(MatrixRB nonalloc, uint[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			Read(CompressedMatrixRB.reusable, array, ref bitposition, bcl);
//			Decompress(nonalloc, CompressedMatrixRB.reusable);
//		}
//		public void ReadAndDecompress(MatrixRB nonalloc, byte[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			Read(CompressedMatrixRB.reusable, array, ref bitposition, bcl);
//			Decompress(nonalloc, CompressedMatrixRB.reusable);
//		}

//		public new MatrixRB ReadAndDecompress(ulong[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			ReadAndDecompress(MatrixRB.reusable, array, ref bitposition, bcl);
//			return MatrixRB.reusable;
//		}

//		public new MatrixRB ReadAndDecompress(uint[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			ReadAndDecompress(MatrixRB.reusable, array, ref bitposition, bcl);
//			return MatrixRB.reusable;
//		}

//		public new MatrixRB ReadAndDecompress(byte[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			ReadAndDecompress(MatrixRB.reusable, array, ref bitposition, bcl);
//			return MatrixRB.reusable;
//		}

//		// UNTESTED
//		public void Read(CompressedMatrixRB nonalloc, byte[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			if (!cached)
//				CacheValues();

//			nonalloc.crusher = this;
//			if (cached_pBits[(int)bcl] > 0)
//				posCrusher.Read(nonalloc.cPos, array, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_rBits[(int)bcl] > 0)
//				rotCrusher.Read(nonalloc.cRot, array, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_sBits[(int)bcl] > 0)
//				sclCrusher.Read(nonalloc.cScl, array, ref bitposition, IncludedAxes.XYZ, bcl);

//			if (cached_vBits[(int)bcl] > 0)
//				velCrusher.Read(nonalloc.cVel, array, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_aBits[(int)bcl] > 0)
//				angCrusher.Read(nonalloc.cAng, array, ref bitposition, IncludedAxes.XYZ, bcl);
//		}
//		// UNTESTED
//		//[System.Obsolete("Bitstream is slated to be removed. Use the Array and Primitive serializers instead.")]
//		public new CompressedMatrixRB Read(ulong[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			Read(CompressedMatrixRB.reusable, array, ref bitposition, bcl);
//			return CompressedMatrixRB.reusable;
//		}
//		public new CompressedMatrixRB Read(uint[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			Read(CompressedMatrixRB.reusable, array, ref bitposition, bcl);
//			return CompressedMatrixRB.reusable;
//		}
//		public new CompressedMatrixRB Read(byte[] array, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			Read(CompressedMatrixRB.reusable, array, ref bitposition, bcl);
//			return CompressedMatrixRB.reusable;
//		}

//		public new CompressedMatrixRB Read(ulong[] array, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			int bitposition = 0;
//			Read(CompressedMatrixRB.reusable, array, ref bitposition, bcl);
//			return CompressedMatrixRB.reusable;
//		}
//		public new CompressedMatrixRB Read(uint[] array, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			int bitposition = 0;
//			Read(CompressedMatrixRB.reusable, array, ref bitposition, bcl);
//			return CompressedMatrixRB.reusable;
//		}
//		public new CompressedMatrixRB Read(byte[] array, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			int bitposition = 0;
//			Read(CompressedMatrixRB.reusable, array, ref bitposition, bcl);
//			return CompressedMatrixRB.reusable;
//		}

//		public void Read(CompressedMatrixRB nonalloc, ulong[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			if (!cached)
//				CacheValues();

//			nonalloc.crusher = this;
//			if (cached_pBits[(int)bcl] > 0)
//				posCrusher.Read(nonalloc.cPos, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_rBits[(int)bcl] > 0)
//				rotCrusher.Read(nonalloc.cRot, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_sBits[(int)bcl] > 0)
//				sclCrusher.Read(nonalloc.cScl, buffer, ref bitposition, IncludedAxes.XYZ, bcl);


//			if (cached_vBits[(int)bcl] > 0)
//				velCrusher.Read(nonalloc.cVel, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_aBits[(int)bcl] > 0)
//				angCrusher.Read(nonalloc.cAng, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//		}

//		public void Read(CompressedMatrixRB nonalloc, uint[] buffer, ref int bitposition, BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			if (!cached)
//				CacheValues();

//			nonalloc.crusher = this;
//			if (cached_pBits[(int)bcl] > 0)
//				posCrusher.Read(nonalloc.cPos, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_rBits[(int)bcl] > 0)
//				rotCrusher.Read(nonalloc.cRot, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_sBits[(int)bcl] > 0)
//				sclCrusher.Read(nonalloc.cScl, buffer, ref bitposition, IncludedAxes.XYZ, bcl);


//			if (cached_vBits[(int)bcl] > 0)
//				velCrusher.Read(nonalloc.cVel, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//			if (cached_aBits[(int)bcl] > 0)
//				angCrusher.Read(nonalloc.cAng, buffer, ref bitposition, IncludedAxes.XYZ, bcl);
//		}


//		#endregion



//		/// <summary>
//		/// Get the total number of bits this Transform is set to write.
//		/// </summary>
//		public new int TallyBits(BitCullingLevel bcl = BitCullingLevel.NoCulling)
//		{
//			int tally = base.TallyBits(bcl);
//			int v = velCrusher != null ? velCrusher.TallyBits(bcl) : 0;
//			int a = angCrusher != null ? angCrusher.TallyBits(bcl) : 0;
//			return tally + v + a;
//		}

//		public void CopyFrom(RigidbodyCrusher source)
//		{
//			posCrusher.CopyFrom(source.posCrusher);
//			rotCrusher.CopyFrom(source.rotCrusher);
//			sclCrusher.CopyFrom(source.sclCrusher);
//			velCrusher.CopyFrom(source.velCrusher);
//			angCrusher.CopyFrom(source.angCrusher);

//			CacheValues();
//		}

//		public override bool Equals(object obj)
//		{
//			return Equals(obj as RigidbodyCrusher);
//		}

//		public bool Equals(RigidbodyCrusher other)
//		{
//			return !ReferenceEquals(other, null) &&
//				//EqualityComparer<Transform>.Default.Equals(defaultTransform, other.defaultTransform) &&
//				posCrusher.Equals(other.posCrusher) &&
//				rotCrusher.Equals(other.rotCrusher) &&
//				sclCrusher.Equals(other.sclCrusher) &&
//				velCrusher.Equals(other.velCrusher) &&
//				angCrusher.Equals(other.angCrusher);
//			//(posCrusher == null ? other.posCrusher == null : (other.posCrusher != null && posCrusher.Equals(other.posCrusher))) &&
//			//(rotCrusher == null ? other.rotCrusher == null : (other.rotCrusher != null && rotCrusher.Equals(other.rotCrusher))) &&
//			//(sclCrusher == null ? other.sclCrusher == null : (other.sclCrusher != null && sclCrusher.Equals(other.sclCrusher))) &&
//			//(velCrusher == null ? other.velCrusher == null : (other.velCrusher != null && velCrusher.Equals(other.velCrusher))) &&
//			//(angCrusher == null ? other.angCrusher == null : (other.angCrusher != null && angCrusher.Equals(other.angCrusher)));
//		}

//		public override int GetHashCode()
//		{
//			var hashCode = -453726296;
//			//hashCode = hashCode * -1521134295 + EqualityComparer<Transform>.Default.GetHashCode(defaultTransform);
//			hashCode = hashCode * -1521134295 + ((posCrusher == null) ? 0 : posCrusher.GetHashCode());
//			hashCode = hashCode * -1521134295 + ((rotCrusher == null) ? 0 : rotCrusher.GetHashCode());
//			hashCode = hashCode * -1521134295 + ((sclCrusher == null) ? 0 : sclCrusher.GetHashCode());
//			return hashCode;
//		}


//		public static bool operator ==(RigidbodyCrusher crusher1, RigidbodyCrusher crusher2)
//		{
//			if (ReferenceEquals(crusher1, null))
//				return false;

//			return crusher1.Equals(crusher2);
//		}

//		public static bool operator !=(RigidbodyCrusher crusher1, RigidbodyCrusher crusher2)
//		{
//			if (ReferenceEquals(crusher1, null))
//				return true;

//			return !(crusher1.Equals(crusher2));
//		}
//	}
//}

