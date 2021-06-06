//// ---------------------------------------------------------------------------------------------
//// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
//// <author>developer@exitgames.com</author>
//// ---------------------------------------------------------------------------------------------

//using UnityEngine;
//using emotitron.Compression;

//namespace Photon.Compression
//{
//	/// <summary>
//	/// Future home of the MatrixRB - in development - don't use
//	/// </summary>
//	public class MatrixRB : Matrix
//	{
//		public Vector3 velocity;
//		public Vector3 angularVelocity;

//		public new static readonly MatrixRB reusable = new MatrixRB();


//		// Constructor
//		public MatrixRB()
//		{
//		}
//		// Constructor
//		public MatrixRB(TransformCrusher crusher)
//		{
//			this.crusher = crusher;
//		}
//		// Constructor
//		public MatrixRB(TransformCrusher crusher, Vector3 position, Element rotation, Vector3 scale)
//		{
//			this.crusher = crusher;
//			this.position = position;
//			this.scale = scale;
//			this.rotation = rotation;
//		}

//		// Constructor
//		public MatrixRB(TransformCrusher crusher, Transform transform)
//		{
//			this.crusher = crusher;
//			this.position = transform.position;

//			// Not sure the idea option for scale... lossy or local.
//			this.scale = transform.localScale;

//			var rotcrusher = crusher.RotCrusher;
//			if (crusher != null && rotcrusher != null && rotcrusher.TRSType == TRSType.Quaternion)
//				this.rotation = transform.rotation;
//			else
//				this.rotation = transform.eulerAngles;
//		}


//	}
//}

