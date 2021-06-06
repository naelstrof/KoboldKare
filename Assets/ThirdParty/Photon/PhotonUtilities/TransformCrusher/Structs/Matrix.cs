// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using emotitron.Compression;
using Photon.Utilities;
using UnityEngine;

namespace Photon.Compression
{
	/// <summary>
	/// A class that holds TRS (Position / Rotation / Scale) values as well as a reference to the crusher that was used to
	/// restore it, and the RotationType enum to indicate if this is using Quaternion or Eulers for rotation.
	/// </summary>
	public class Matrix
	{
		public TransformCrusher crusher;

		public Vector3 position;
		public Element rotation;
		public Vector3 scale;

		public static Matrix reusable = new Matrix();

		// Constructor
		public Matrix()
		{
		}
		// Constructor
		public Matrix(TransformCrusher crusher)
		{
			this.crusher = crusher;
		}
		// Constructor
		public Matrix(TransformCrusher crusher, Vector3 position, Element rotation, Vector3 scale)
		{
			this.crusher = crusher;
			this.position = position;
			this.scale = scale;
			this.rotation = rotation;
		}

		// Constructor
		public Matrix(TransformCrusher crusher, Transform transform)
		{
			this.crusher = crusher;

			bool lclpos = (crusher == null || crusher.PosCrusher == null || crusher.PosCrusher.local);
			this.position = (lclpos) ? transform.localPosition : transform.position;

			// Not sure the idea option for scale... lossy or local.
			bool lclscl = (crusher == null || crusher.SclCrusher == null || crusher.SclCrusher.local);
			this.scale = (lclscl) ? transform.localScale : transform.lossyScale;

			bool lclrot = (crusher == null || crusher.RotCrusher == null || crusher.RotCrusher.local);
			if (crusher != null && crusher.RotCrusher != null && crusher.RotCrusher.TRSType == TRSType.Quaternion)
				this.rotation = (lclrot) ? transform.localRotation : transform.rotation;
			else
				this.rotation = (lclrot) ? transform.localEulerAngles : transform.eulerAngles;
		}

		public void Set(TransformCrusher crusher, Vector3 position, Element rotation, Vector3 scale)
		{
			this.crusher = crusher;
			this.position = position;
			this.scale = scale;
			this.rotation = rotation;
		}

		[System.Obsolete("Use Capture() instead. Set was confusing with other usage.")]
		public void Set(TransformCrusher crusher, Transform transform) { Capture(crusher, transform); }

		/// <summary>
		/// Set Matrix values to the current values of the supplied Rigidbody. Also set the crusher ref.
		/// </summary>
		public void Capture(TransformCrusher crusher, Transform transform)
		{
			this.crusher = crusher;
			this.position = transform.position;

			// Not sure the idea option for scale... lossy or local.
			this.scale = transform.localScale;

			if (crusher != null && crusher.RotCrusher != null && crusher.RotCrusher.TRSType == TRSType.Quaternion)
				this.rotation = transform.rotation;
			else
				this.rotation = transform.eulerAngles;
		}

		[System.Obsolete("Use Capture() instead. Set was confusing with other usage.")]
		public void Set(Transform transform) { Capture(transform); }

		/// <summary>
		/// Set Matrix values to the current values of the supplied Transform
		/// </summary>
		public void Capture(Transform transform)
		{
			this.position = transform.position;

			// Not sure the idea option for scale... lossy or local.
			this.scale = transform.localScale;

			if (crusher != null && crusher.RotCrusher != null && crusher.RotCrusher.TRSType == TRSType.Quaternion)
				this.rotation = transform.rotation;
			else
				this.rotation = transform.eulerAngles;
		}


		[System.Obsolete("Use Capture() instead. Set was confusing with other usage.")]
		public void Set(TransformCrusher crusher, Rigidbody rb) { Capture(crusher, rb); }

		/// <summary>
		/// Set Matrix values to the current values of the supplied Rigidbody. Also set the crusher ref.
		/// </summary>
		public void Capture(TransformCrusher crusher, Rigidbody rb)
		{
			this.crusher = crusher;
			this.position = rb.position;

			// Not sure the ideal option for scale... lossy or local.
			this.scale = rb.transform.localScale;

			if (crusher != null && crusher.RotCrusher != null && crusher.RotCrusher.TRSType == TRSType.Quaternion)
				this.rotation = rb.rotation;
			else
				this.rotation = rb.rotation.eulerAngles;
		}

		[System.Obsolete("Use Capture() instead. Set was confusing with other usage.")]
		public void Set(Rigidbody rb) { Capture(rb); }

		/// <summary>
		/// Set Matrix values to the current values of the supplied Rigidbody
		/// </summary>
		public void Capture(Rigidbody rb)
		{
			this.position = rb.position;

			if (crusher != null && crusher.RotCrusher != null && crusher.RotCrusher.TRSType == TRSType.Quaternion)
				this.rotation = rb.rotation;
			else
				this.rotation = rb.rotation.eulerAngles;

			// Not sure the ideal option for scale... lossy or local.
			this.scale = rb.transform.localScale;

		}

		public void Clear()
		{
			this.crusher = null;
		}

		/// <summary>
		/// Compress this matrix using the crusher it was previously created with.
		/// </summary>
		/// <returns></returns>
		public void Compress(CompressedMatrix nonalloc)
		{
			crusher.Compress(nonalloc, this);
		}

		/// <summary>
		/// Apply this TRS Matrix to the default transform, using the crusher that created this TRS Matrix. Unused Axes will be left unchanged.
		/// </summary>
		[System.Obsolete("Supply the transform to Apply to. Default Transform has been deprecated to allow shared TransformCrushers.")]
		public void Apply()
		{
			crusher.Apply(this);
		}

		/// <summary>
		/// Apply this TRS Matrix to the supplied transform, using the crusher that created this TRS Matrix. Unused Axes will be left unchanged.
		/// </summary>
		public void Apply(Transform t)
		{
			if (crusher == null)
			{
				Debug.LogError("No crusher defined for this matrix. This matrix has not yet had a value assigned to it most likely, but you are trying to apply it to a transform.");
				return;
			}
			crusher.Apply(t, this);
		}

		[System.Obsolete("Apply for Rigidbody has been replaced with Move and Set, to indicate usage of MovePosition/Rotation vs rb.position/rotation.")]
		public void Apply(Rigidbody rb)
		{
			if (crusher == null)
			{
				Debug.LogError("No crusher defined for this matrix. This matrix has not yet had a value assigned to it most likely, but you are trying to apply it to a transform.");
				return;
			}
			crusher.Apply(rb, this);
		}

		/// <summary>
		/// Lerp the position, rotation and scale of two Matrix objects, writing the results to the target Matrix.
		/// </summary>
		/// <param name="target">Result target.</param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="t"></param>
		/// <returns>Returns a reference to the supplied target matrix.</returns>
		public static Matrix Lerp(Matrix target, Matrix start, Matrix end, float t)
		{
			var crusher = end.crusher;
			target.crusher = crusher;

			target.position = Vector3.Lerp(start.position, end.position, t);

			if (crusher != null && crusher.RotCrusher != null)
			{
				if (crusher.RotCrusher.TRSType == TRSType.Quaternion)
					target.rotation = Quaternion.Slerp((Quaternion)start.rotation, (Quaternion)end.rotation, t);
				else
				{
					var srot = (Vector3)start.rotation;
					var erot = (Vector3)end.rotation;
					var ydelta = srot.y - erot.y;
					var zdelta = srot.z - erot.z;


					Vector3 unfucked = new Vector3(
						erot.x,
						ydelta > 180 ? erot.y + 360 : ydelta < -180 ? erot.y - 360 : erot.y,
						zdelta > 180 ? erot.z + 360 : zdelta < -180 ? erot.z - 360 : erot.z
						);

					target.rotation = Vector3.Lerp(srot, (Vector3)unfucked, t);
				}
			}
			else
				target.rotation = end.rotation;

			target.scale = Vector3.Lerp(start.scale, end.scale, t);

			return target;
		}

		/// <summary>
		/// Unclamped Lerp the position, rotation and scale of two Matrix objects, writing the results to the target Matrix.
		/// </summary>
		/// <param name="target">Result target.</param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="t"></param>
		/// <returns>Returns a reference to the supplied target matrix.</returns>
		public static Matrix LerpUnclamped(Matrix target, Matrix start, Matrix end, float t)
		{
			var crusher = end.crusher;

			target.crusher = crusher;

			target.position = Vector3.LerpUnclamped(start.position, end.position, t);

			if (crusher != null && crusher.RotCrusher != null)
			{
				if (crusher.RotCrusher.TRSType == TRSType.Quaternion)
					target.rotation = Quaternion.SlerpUnclamped((Quaternion)start.rotation, (Quaternion)end.rotation, t);
				else
				{
					var srot = (Vector3)start.rotation;
					var erot = (Vector3)end.rotation;
					var ydelta = srot.y - erot.y;
					var zdelta = srot.z - erot.z;

					Vector3 unfucked = new Vector3(
						erot.x,
						ydelta > 180 ? erot.y + 360 : ydelta < -180 ? erot.y - 360 : erot.y,
						zdelta > 180 ? erot.z + 360 : zdelta < -180 ? erot.z - 360 : erot.z
						);

					target.rotation = Vector3.LerpUnclamped(srot, (Vector3)unfucked, t);
				}
			}
			else
				target.rotation = end.rotation;

			target.scale = Vector3.LerpUnclamped(start.scale, end.scale, t);

			return target;
		}

		public static Matrix CatmullRomLerpUnclamped(Matrix target, Matrix pre, Matrix start, Matrix end, Matrix post, float t)
		{
			var crusher = end.crusher;

			target.crusher = crusher;

			target.position = CatmulRom.CatmullRomLerp(pre.position, start.position, end.position, post.position, t);

			if (crusher != null && crusher.RotCrusher != null)
			{
				if (crusher.RotCrusher.TRSType == TRSType.Quaternion)
					target.rotation = Quaternion.SlerpUnclamped((Quaternion)start.rotation, (Quaternion)end.rotation, t);
				else
				{
					var srot = (Vector3)start.rotation;
					var erot = (Vector3)end.rotation;
					var ydelta = srot.y - erot.y;
					var zdelta = srot.z - erot.z;

					Vector3 unfucked = new Vector3(
						erot.x,
						ydelta > 180 ? erot.y + 360 : ydelta < -180 ? erot.y - 360 : erot.y,
						zdelta > 180 ? erot.z + 360 : zdelta < -180 ? erot.z - 360 : erot.z
						);

					target.rotation = Vector3.LerpUnclamped(srot, (Vector3)unfucked, t);
				}
			}
			else
				target.rotation = end.rotation;


			target.scale = CatmulRom.CatmullRomLerp(pre.scale, start.scale, end.scale, post.scale, t);

			return target;
		}

		public static Matrix CatmullRomLerpUnclamped(Matrix target, Matrix pre, Matrix start, Matrix end, float t)
		{
			var crusher = end.crusher;

			target.crusher = crusher;

			target.position = CatmulRom.CatmullRomLerp(pre.position, start.position, end.position, t);

			if (crusher != null && crusher.RotCrusher != null)
			{
				if (crusher.RotCrusher.TRSType == TRSType.Quaternion)
					target.rotation = Quaternion.SlerpUnclamped((Quaternion)start.rotation, (Quaternion)end.rotation, t);
				else
				{
					var srot = (Vector3)start.rotation;
					var erot = (Vector3)end.rotation;
					var ydelta = srot.y - erot.y;
					var zdelta = srot.z - erot.z;

					Vector3 unfucked = new Vector3(
						erot.x,
						ydelta > 180 ? erot.y + 360 : ydelta < -180 ? erot.y - 360 : erot.y,
						zdelta > 180 ? erot.z + 360 : zdelta < -180 ? erot.z - 360 : erot.z
						);

					target.rotation = Vector3.LerpUnclamped(srot, (Vector3)unfucked, t);
				}
			}
			else
				target.rotation = end.rotation;

			target.scale = CatmulRom.CatmullRomLerp(pre.scale, start.scale, end.scale, t);

			return target;
		}


		public override string ToString()
		{
			return "MATRIX pos: " + position + " rot: " + rotation + " scale: " + scale + "  rottype: " + rotation.vectorType;
		}

	}


	public static class MatrixExtensions
	{
		public static void CopyFrom(this Matrix target, Matrix src)
		{
			target.crusher = src.crusher;
			target.position = src.position;
			target.rotation = src.rotation;
			target.scale = src.scale;
		}
	}
}

