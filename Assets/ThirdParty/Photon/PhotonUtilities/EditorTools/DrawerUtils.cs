
#if UNITY_EDITOR

using System;
using UnityEditor;
using System.Collections;
using System.Reflection;

namespace Photon.Utilities
{
	public class DrawerUtils
	{
		#region Methods for finding target of Drawer 

		/// <summary>
		/// Hacky methods used by property drawer to find the actual target object.
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		public static object GetParent(SerializedProperty prop)
		{
			var path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split('.');

			for (int i = 0; i < elements.Length - 1; i++)
			{
				if (elements[i].Contains("["))
				{
					var elementName = elements[i].Substring(0, elements[i].IndexOf("["));
					var index = Convert.ToInt32(elements[i].Substring(elements[i].IndexOf("[")).Replace("[", "").Replace("]", ""));
					obj = GetValue(obj, elementName, index);
				}
				else
				{
					obj = GetValue(obj, elements[i]);
				}
			}
			return obj;
		}

		private static object GetValue(object source, string name)
		{
			if (source == null)
				return null;
			var type = source.GetType();
			var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (f == null)
			{
				var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p == null)
					return null;
				return p.GetValue(source, null);
			}
			return f.GetValue(source);
		}

		private static object GetValue(object source, string name, int index)
		{
			var enumerable = GetValue(source, name) as IEnumerable;
			var enm = enumerable.GetEnumerator();
			while (index-- >= 0)
				enm.MoveNext();
			return enm.Current;
		}

		#endregion
	}
}

#endif
