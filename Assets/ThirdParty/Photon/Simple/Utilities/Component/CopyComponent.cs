// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
	public static class CopyComponent
	{

		public static Component ComponentCopy(this Component original, GameObject destination)
		{
			System.Type type = original.GetType();
			Component copy = destination.AddComponent(type);
			// Copied fields can be restricted with BindingFlags
			FieldInfo[] fields = type.GetFields();
			foreach (System.Reflection.FieldInfo field in fields)
			{
				field.SetValue(copy, field.GetValue(original));
			}
			return copy;
		}

		public static T GetCopyOf<T>(this T comp, T other) where T : Component
		{
			if (comp == null || other == null)
				return comp;

			Type type = comp.GetType();
			if (type != other.GetType()) return null; // type mis-match
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
			PropertyInfo[] pinfos = type.GetProperties(flags);
			foreach (var pinfo in pinfos)
			{
				if (pinfo.CanWrite)
				{
					try
					{
						/// Only set value if we are not dealing with an obsolete method (otherwise... enjoy log warnings)
						bool isobsolete = Attribute.IsDefined(pinfo, typeof(System.ObsoleteAttribute));
						if (!isobsolete)
							pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
					}
					catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
				}
			}
			FieldInfo[] finfos = type.GetFields(flags);
			foreach (var finfo in finfos)
			{
				finfo.SetValue(comp, finfo.GetValue(other));
			}
			return comp as T;
		}

		public static T AddColliderCopy<T>(this GameObject go, T toAdd) where T : Collider
		{
			T copy = go.AddComponent(toAdd.GetType()).GetCopyOf(toAdd) as T;
			copy.sharedMaterial = toAdd.sharedMaterial;
			// Not sure why this isn't copying.
			copy.isTrigger = toAdd.isTrigger;
			return copy;
		}
	}
}

