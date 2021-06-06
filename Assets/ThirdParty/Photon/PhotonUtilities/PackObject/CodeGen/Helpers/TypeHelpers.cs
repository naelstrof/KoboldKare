// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Photon.Compression.Internal
{
	public static class TypeHelpers
	{

		#region Nested Loop Check

		private static HashSet<Type> nestCheck = new HashSet<Type>();
		private static int safetycounter;

		public static bool CheckForNestedLoop(this Type type, HashSet<Type> hashset = null)
		{
			if (hashset == null)
			{
				hashset = nestCheck;
				safetycounter = 0;
				hashset.Clear();
			}

			/// Only check if this is a PackObj, since we are looking for nested pack objects.
			var attrs = type.GetCustomAttributes(typeof(PackObjectAttribute), false);
			if (attrs.Length == 0)
				return false;

			if (hashset.Contains(type))
				return true;

			safetycounter++;

			if (safetycounter > 20)
			{
				Debug.Log("Ouch. Stuck trying to test for loops. " + type.Name);
				return true;
			}

			var fields = type.GetFields();

			foreach (var field in fields)
			{
				//if (field.FieldType == type)
				//	return true;

				/// Recurse into nested PackObjects to check for recursion
				if (field.GetCustomAttributes(typeof(PackObjectAttribute), false).Length != 0)
				{
					hashset.Add(type);
					Debug.Log("Loop found " + type.Name + " " + field.FieldType + " " + field.GetCustomAttributes(typeof(PackObjectAttribute), false).Length);
					if (CheckForNestedLoop(field.FieldType, hashset))
						return true;
					hashset.Remove(type);
				}
			}
			return false;
		}

		#endregion

		#region IsManaged

		public static bool IsUnManaged(this Type t)
		{
			if (t.IsPrimitive || t.IsPointer || t.IsEnum)
				return true;

			//Debug.Log("<b>Testing For UnManaged</b> " + t.Name + " " + t.IsValueType);
			if (!t.IsValueType)
				return false;

			if (t.IsGenericType)
				return false;

			if (t.IsValueType)
			{
				var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				foreach (var f in fields)
					if (!IsUnManaged(f.FieldType))
						return false;
			}
			/// Unmanaged type... insta fail
			else
				return false;

			return true;
		}

		#endregion

		#region Type To Hash

		private static HashSet<Type> endlessLoopCheck = new HashSet<Type>();

		public static long TypeToHash64(this Type type, HashSet<Type> endlessCheck = null)
		{

			/// Only process if this is a PackObj
			var attrs = type.GetCustomAttributes(typeof(PackObjectAttribute), false);
			if (attrs.Length == 0)
				return 0;


			var attr = attrs[0] as PackObjectAttribute;

			if (endlessCheck == null)
			{
				endlessCheck = endlessLoopCheck;
				endlessCheck.Clear();
			}
			else if (endlessCheck.Contains(type))
			{
				Debug.LogWarning(type.Name + " has an endless recursive field loop.");
				return 0;
			}

			endlessCheck.Add(type);
			var hashes = new List<long>();

			hashes.Add(type.FullName.GetHashCode());

			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			bool includeAllPublic = attr.defaultInclusion == DefaultPackInclusion.AllPublic;

			foreach (var f in fields)
				hashes.Add(HashField(f, includeAllPublic));

			HashAttribute(attr, hashes);

			/// Sort and add the hashes
			hashes.Sort();
			long hash = 504981047;
			foreach (var h in hashes) hash = hash * -1521134295 + h;
			return hash;
		}

		private static long HashField(FieldInfo field, bool includeAllPublic)
		{
			var hashes = new List<long>();

			/// Hash nested PackObjects
			long objhash = TypeToHash64(field.FieldType, endlessLoopCheck);
			if (objhash != 0)
			{
				hashes.Add(objhash);
				return objhash;
			}

			/// Hash PackAttribs and callbacks
			var attrs = field.GetCustomAttributes(typeof(SyncVarBaseAttribute), false);

			/// This field isn't being packed.
			if (attrs.Length == 0 && !includeAllPublic)
				return 0;

			hashes.Add(field.FieldType.FullName.GetHashCode());
			hashes.Add(field.Name.GetHashCode());

			/// If the attribute existed, hash it
			if (attrs.Length != 0)
			{
				var attr = attrs[0] as SyncVarBaseAttribute;

				HashAttribute(attr, hashes);
				HashCallback(field, attr.applyCallback, hashes);
				HashCallback(field, attr.snapshotCallback, hashes);
			}

			var fields = field.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (var f in fields)
			{
				
				/// Not a nested object - treat as a regular field.
				hashes.Add(HashField(f, includeAllPublic));

			}

			long hash = 504981047;
			hashes.Sort();
			foreach (var h in hashes)
				hash = hash * -1521134295 + h;

			return hash;
		}

		private static void HashAttribute(Attribute attr, List<long> hashes)
		{
			hashes.Add(attr.GetType().FullName.GetHashCode());

			foreach (var a in attr.GetType().GetFields())
			{
				//Debug.Log("Hashing attr " + a.Name + " " /*+ a.GetValue(attr)*/);
				var obj = (a.GetValue(attr));
				if (obj != null)
					hashes.Add(obj.GetHashCode());
			}
		}

		public static MethodInfo HashCallback(this FieldInfo field, string methodname, List<long> hashes)
		{
			Type objType = field.ReflectedType;

			if (methodname == null)
				return null;

			/// Callbacks aren't attributed, so can't easily find them - so finding it by hand here.
			var methinfo = field.ReflectedType.GetMethod(methodname);
			var parms = (methinfo == null) ? null : methinfo.GetParameters();

			var ftype = field.FieldType;


			if (methinfo == null)
				goto BadCallback;

			if (!methinfo.IsPublic)
				goto BadCallback;

			if (methinfo.IsStatic)
				goto BadCallback;

			if (parms.Length != 2)
				goto BadCallback;

			if (parms[0].ParameterType == ftype && parms[1].ParameterType == ftype)
			{
				if (hashes != null)
				foreach (var prm in methinfo.GetParameters())
					hashes.Add(prm.ParameterType.FullName.GetHashCode());

				return methinfo;
			}

			return null;

			BadCallback:

			bool doesntExist = methinfo == null;
			bool isStatic = doesntExist ? false : methinfo.IsStatic;
			bool isPrivate = doesntExist ? false : methinfo.IsPrivate;

			string staticString = isStatic ? "<color=red>static</color> " : "";
			string accessString = isPrivate ? "<color=red>private </color>" : "public ";
			string typename = ftype.Name;
			string parmstring = "";

			if (parms != null)
			for (int i = 0; i < parms.Length; ++i)
				parmstring += (i == 0 ? "" : ", ") + parms[i].ParameterType.Name;

			Debug.LogWarning("<b>Invalid Callback</b> <i>'" + accessString + staticString + methodname + "(" + parmstring + ")'</i> on <b>" + objType.Name + "</b>" +
				". Check that method with this name exists (spelling?) and method has format: <i>'public " + methodname + "(" + typename + ", " + typename + ")'</i>");

			return null;
		}

		#endregion
	}
}

#endif