// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Photon.Compression.Internal
{

	[System.Serializable]
	[AttributeUsage(AttributeTargets.Field)]
	public abstract class SyncVarBaseAttribute : Attribute
	{

		/// <summary>
		/// Indicates how the Pack syncvar should be treated. 'State' maintains the value until changed. 'Trigger' resets the value to new() after capture/send.
		/// </summary>
		public SyncAs syncAs = SyncAs.Auto;

		public KeyRate keyRate;

		/// <summary>
		/// By default the callback is called After the value is changed. This behavior can be changed by setting the CallbackSetValue.
		/// </summary>
		public string applyCallback;
		/// <summary>
		/// Called every net tick before SyncObjects.OnSnapshot and before PackObjs.applyCallback is run. Use this if you intend to handle your own interpolation/tween operations. 
		/// Snap value is not be applied to the syncvar yet.
		/// </summary>
		public string snapshotCallback;
		/// <summary>
		/// Will automatically set the value where there is a callback defined. Disable if you would like to set the local value manually in your callback.
		/// </summary>
		public SetValueTiming setValueTiming = SetValueTiming.AfterCallback;

		/// <summary>
		/// Types that have interpolation methods defined for them, will automatically interpolate/Lerp/Tween to smooth movement in Update() between ticks.
		/// </summary>
		public bool interpolate = false;

		///// <summary>
		///// Types that support extrapolation will extrapolate by this much each tick on buffer under-runs. 0 = none. 1 = endless.
		///// </summary>
		//public float Extrapolation = .5f;

		public int bitCount = -1;

		public virtual void Initialize(Type primitiveType)
		{
			if (bitCount > -1)
				return;

			bitCount = GetDefaultBitCount(primitiveType);
		}

#if UNITY_EDITOR

        private static StringBuilder sb = new StringBuilder();
		public virtual string GetFieldDeclareCodeGen(Type fieldType, string fulltypename, string fname)
		{
			sb.Length = 0;
			if (fieldType == typeof(StringBuilder))
				return sb.Append("public System.Text.StringBuilder ").Append(fname).Append(" = new System.Text.StringBuilder();").ToString();

			else
			{
				if (fieldType.IsValueType)
					return sb.Append("public ").Append(fulltypename).Append(" ").Append(fname).Append(";").ToString();
				else
					return sb.Append("public ").Append(fulltypename).Append(" ").Append(fname).Append(" = new ").Append(fulltypename).Append("();").ToString();
			}
		}

		public virtual string GetCaptureCodeGen(Type fieldType, string fieldName, string s, string t)
		{
			return GetCopyCodeGen(fieldType, fieldName, s, t);
		}

		public virtual string GetCopyCodeGen(Type fieldType, string fieldName, string s, string t)
		{
			sb.Length = 0;

			if (fieldType == typeof(StringBuilder))
				return sb.Append(t).Append(".").Append(fieldName).Append(".Length = 0; ").Append(t).Append(".myString.Append(").Append(s).Append(".").Append(fieldName).Append(");").ToString();

            bool isList = fieldType.IsGenericType && fieldType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
            // TODO: This should become unneeded... moved to PacklList
            if (isList)
				return t + "." + fieldName + ".Clear(); " + t + "." + fieldName + ".AddRange(" + s + "." + fieldName + ");";
			// TODO: Dictionary copy would go here

			return sb.Append(t).Append(".").Append(fieldName).Append(" = ").Append(s).Append(".").Append(fieldName).Append(";").ToString();
		}

		public virtual string GetInterpolateCode(Type fieldType, string fname, string s, string e, string t)
		{
			if (
				fieldType == typeof(Single) || fieldType == typeof(Double) ||
				fieldType == typeof(SByte) || fieldType == typeof(Byte) ||
				fieldType == typeof(Int16) || fieldType == typeof(UInt16) ||
				fieldType == typeof(Int32) || fieldType == typeof(UInt32) ||
				fieldType == typeof(Int64) || fieldType == typeof(UInt64)
				)
				return t + "." + fname + " = (" + fieldType.FullName + ")((" + e + "." + fname + " - " + s + "." + fname + ") * time) + " + s + "." + fname + ";";
			else if (fieldType == typeof(Vector3))
				return t + "." + fname + " = Vector3.LerpUnclamped(" + s + "." + fname + ", " + e + "." + fname + ", time);";
			else if (fieldType == typeof(Vector2))
				return t + "." + fname + " = Vector2.LerpUnclamped(" + s + "." + fname + ", " + e + "." + fname + ", time);";
			
			return null;
		}
#endif

		public virtual int GetDefaultBitCount(Type fieldType)
		{
			if (fieldType == typeof(Byte) || fieldType == typeof(SByte))
				return 8;

			if (fieldType == typeof(UInt16) || fieldType == typeof(Int16) || fieldType == typeof(Char))
				return 16;

			if (fieldType == typeof(UInt32) || fieldType == typeof(Int32) || fieldType == typeof(Single))
				return 32;

			if (fieldType == typeof(UInt64) || fieldType == typeof(Int64) || fieldType == typeof(Double))
				return 64;

			if (fieldType == typeof(Boolean))
				return 1;

			if (fieldType == typeof(Vector3))
				return 32;

			if (fieldType == typeof(Vector2))
				return 32;

			return 0;
		}

		/// <summary>
		/// Maximum possible bits that can be used during bitpacking. These are used to determine the byte[] size for any PackObjects.
		/// Override this if the PackableAttribute will always use less than these values. Doing so will result in some memory savings.
		/// </summary>
		/// <param name="fieldType"></param>
		/// <returns></returns>
		public virtual int GetMaxBits(Type fieldType)
		{

			if (fieldType == typeof(Byte) || fieldType == typeof(SByte))
				return 8;

			if (fieldType == typeof(UInt16) || fieldType == typeof(Int16) || fieldType == typeof(Char))
				return 16;

			if (fieldType == typeof(UInt32) || fieldType == typeof(Int32) || fieldType == typeof(Single))
				return 32;

			if (fieldType == typeof(UInt64) || fieldType == typeof(Int64) || fieldType == typeof(Double))
				return 64;

			if (fieldType == typeof(Boolean))
				return 1;

			if (fieldType == typeof(Vector3))
				return 96;

			if (fieldType == typeof(Vector2))
				return 64;

            bool isList = fieldType.IsGenericType && fieldType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));

            if (isList)
			{
				Debug.LogWarning("Can't get max bits needed for List<> types, as they are variable. " + fieldType.Name);
				return 256 * 8;
			}

			Debug.LogWarning("Can't get bits needed for unsupported types. " + fieldType.Name);
			return 256 * 8;
		}



		public bool IsKeyframe(int frameId)
		{
			/// always true if 1
			if ((int)keyRate == 1)
				return true;

			/// always false if 0
			if (keyRate == 0)
				return false;

			if (frameId % (int)keyRate == 0)
				return true;

			return false;
		}

		//public bool IsForced(int frameId, SerializationFlags writeFlags)
		//{
		//    return false;
		//}

		public bool IsForced(int frameId, SerializationFlags writeFlags)
		{
			if (syncAs == SyncAs.Trigger)
			{
				/// TODO: this will create garbage for ref types.
				return false;
			}

			/// always true if 1
			if (keyRate == KeyRate.Every)
				return true;

			/// always true if forced
			if ((writeFlags & SerializationFlags.Force) != 0)
				return true;


			/// if no keyframes are used, still force on new connections
			if (keyRate == 0)
			{
				if ((writeFlags & SerializationFlags.NewConnection) != 0)
					return true;
			}

			if (keyRate != KeyRate.Never && frameId % (int)keyRate == 0)
				return true;

			return false;
		}


		public bool IsForcedClass<T>(int frameId, T value, T prevValue, SerializationFlags writeFlags) where T : class
		{
			if (syncAs == SyncAs.Trigger)
			{
				Debug.LogError("Reference type " + typeof(T).Name + " cannot be set to SyncAs.Trigger. This PackAttribute setting only applies to structs.");
				/// TODO: this will create garbage for ref types.
				return true;
			}

			/// always true if 1
			if (keyRate == KeyRate.Every)
				return true;

			/// always true if forced
			if ((writeFlags & SerializationFlags.Force) != 0)
				return true;


			/// if no keyframes are used, still force on new connections
			if (keyRate == 0)
			{
				if ((writeFlags & SerializationFlags.NewConnection) != 0)
					return true;
			}

			if (keyRate != KeyRate.Never && frameId % (int)keyRate == 0)
				return true;

			if (!value.Equals(prevValue))
				return true;

			return false;
		}

		public bool IsForced<T>(int frameId, T value, T prevValue, SerializationFlags writeFlags) where T : struct
		{
            if (syncAs == SyncAs.Trigger)
            {
				/// TODO: this will create garbage for ref types.
                return !value.Equals(new T());
            }

			/// always true if 1
			if (keyRate == KeyRate.Every)
				return true;

			/// always true if forced
			if ((writeFlags & SerializationFlags.Force) != 0)
				return true;


			/// if no keyframes are used, still force on new connections
			if (keyRate == 0)
			{
				if ((writeFlags & SerializationFlags.NewConnection) != 0)
					return true;
			}

			if (keyRate != KeyRate.Never && frameId % (int)keyRate == 0)
				return true;

			if (!value.Equals(prevValue))
				return true;

			return false;
		}
	}

}
