// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Runtime.InteropServices;
using UnityEngine;

namespace Photon.Compression
{

	/// <summary>
	/// A struct that allows Quaternion and Vector types to be treated as the same.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct Element : System.IEquatable<Element>
	{
		public enum VectorType { Vector3 = 1, Quaternion = 2 }

		[FieldOffset(0)]
		public VectorType vectorType;

		[FieldOffset(4)]
		public Vector3 v;

		[FieldOffset(4)]
		public Quaternion quat;

		#region Constructors

		public Element(Vector3 v) : this()
		{
			vectorType = VectorType.Vector3;
			this.v = v;
		}

		public Element(Quaternion quat) : this()
		{
			vectorType = VectorType.Quaternion;
			this.quat = quat;
		}

		#endregion

		#region Implicit/Explicit Casts

		public static explicit operator Quaternion(Element e)
		{
			if (e.vectorType == VectorType.Quaternion)
				return e.quat;
			else
			{
#if UNITY_EDITOR
				Debug.LogWarning("Casting a Element of type Vector to a Quaternion uses Quaternion.Euler(). Is this intentional? Cast to Vector3 and convert to Quaternion yourself to silence this warning.");
#endif
				return Quaternion.Euler(e.v);
			}

		}
		public static explicit operator Vector3(Element e)
		{
			if (e.vectorType == VectorType.Vector3)
				return e.v;
			else
			{
#if UNITY_EDITOR
				Debug.LogWarning("Casting a Element of type Quaternion to a Vector3 uses quaternion.eulerAngles. Is this intentional? Cast to Quaternion and convert to EulerAngles yourself to silence this warning.");
#endif
				return e.quat.eulerAngles;
			}
		}


		#endregion

		public static Element Slerp(Element a, Element b, float t)
		{
			if (a.vectorType == VectorType.Quaternion)
				return Quaternion.Slerp((Quaternion)a, (Quaternion)b, t);
			else
				return Vector3.Slerp((Vector3)a, (Vector3)b, t);
		}

		public static Element SlerpUnclamped(Element a, Element b, float t)
		{
			if (a.vectorType == VectorType.Quaternion)
				return Quaternion.SlerpUnclamped((Quaternion)a, (Quaternion)b, t);
			else
				return Vector3.SlerpUnclamped((Vector3)a, (Vector3)b, t);
		}

		#region IEquatable

		public static bool operator ==(Element a, Element b)
		{
			return a.vectorType == b.vectorType &&
				(a.vectorType == VectorType.Vector3) ?
					(
					a.v.x == b.v.x &&
					a.v.y == b.v.y &&
					a.v.z == b.v.z
					) : (
					a.quat.x == b.quat.x &&
					a.quat.y == b.quat.y &&
					a.quat.z == b.quat.z &&
					a.quat.w == b.quat.w
					);
		}

		public static bool operator !=(Element a, Element b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			return obj is Element && Equals((Element)obj);
		}

		public bool Equals(Element other)
		{
			return vectorType == other.vectorType &&
				(vectorType == VectorType.Vector3) ?
					(
					v.x == other.v.x &&
					v.y == other.v.y &&
					v.z == other.v.z
					) : (
					quat.x == other.quat.x &&
					quat.y == other.quat.y &&
					quat.z == other.quat.z &&
					quat.w == other.quat.w
					);
		}

		public static bool Equals(Vector3 a, Vector3 b)
		{
			return
					(
					a.x == b.x &&
					a.y == b.y &&
					a.z == b.z
					);
		}

		public static bool Equals(Quaternion a, Quaternion b)
		{
			return
					(
					a.x == b.x &&
					a.y == b.y &&
					a.z == b.z &&
					a.w == b.w
					);
		}

		#endregion

		public static implicit operator Element(Quaternion q) { return new Element(q); }
		public static implicit operator Element(Vector3 v) { return new Element(v); }

		public override string ToString()
		{
			return vectorType + " " + ((vectorType == VectorType.Quaternion) ? quat.ToString() : v.ToString());
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
