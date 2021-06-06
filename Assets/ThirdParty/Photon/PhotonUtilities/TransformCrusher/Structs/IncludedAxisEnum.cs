// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Compression
{
	public enum IncludedAxes { None = 0, X = 1, Y = 2, XY = 3, Z = 4, XZ = 5, YZ = 6, XYZ = 7, Uniform = 15 }

	public static class IncludeAxisExtensions
	{
		//public static bool IsX(this IncludedAxes ia) { return (((int)ia & 1) != 0); }
		//public static bool IsY(this IncludedAxes ia) { return (((int)ia & 2) != 0); }
		//public static bool IsZ(this IncludedAxes ia) { return (((int)ia & 4) != 0); }

		//public static bool IsXYZ(this IncludedAxes ia, int axesId)
		//{
		//	return ((int)ia & (1 << axesId)) != 0;
		//}
		
		/// <summary>
		/// Only factors in used axes
		/// </summary>
		public static float SqrMagnitude(this Vector3 v, IncludedAxes ia)
		{
			return
			((((int)ia & 1) != 0) ? v.x * v.x : 0) +
			((((int)ia & 2) != 0) ? v.y * v.y : 0) +
			((((int)ia & 4) != 0) ? v.z * v.z : 0);
		}

		/// <summary>
		/// Only factors in used axes
		/// </summary>
		public static float Magnitude(this Vector3 v, IncludedAxes ia)
		{
			return Mathf.Sqrt(
			((((int)ia & 1) != 0) ? v.x * v.x : 0) +
			((((int)ia & 2) != 0) ? v.y * v.y : 0) +
			((((int)ia & 4) != 0) ? v.z * v.z : 0));

		}

		/// <summary>
		/// Lerp extension that only applies the lerp to axis indicated by the XYZ. Other axis return the same axis value as start.
		/// </summary>
		public static Vector3 Lerp(this GameObject go, Vector3 start, Vector3 end, IncludedAxes ia, float t, bool localPosition = false)
		{
			// the lerped position, not accounting for any axis that are not included
			Vector3 rawLerpedPos = Vector3.Lerp(start, end, t);

			// for non-included axis, use the current postion
			return new Vector3(
				(((int)ia & 1) != 0) ? rawLerpedPos[0] : ((localPosition) ? go.transform.localPosition[0] : go.transform.position[0]),
				(((int)ia & 2) != 0) ? rawLerpedPos[1] : ((localPosition) ? go.transform.localPosition[1] : go.transform.position[1]),
				(((int)ia & 4) != 0) ? rawLerpedPos[2] : ((localPosition) ? go.transform.localPosition[2] : go.transform.position[2]));
		}

		/// <summary>
		/// Set position applying ONLY the values indicated as included, non-included axis use the current axis of the gameobject.
		/// </summary>
		public static void SetPosition(this GameObject go, Vector3 pos, IncludedAxes ia, bool localPosition = false)
		{
			Vector3 newpos = new Vector3(
				(((int)ia & 1) != 0) ? pos[0] : (localPosition) ? go.transform.localPosition[0] : go.transform.position[0],
				(((int)ia & 2) != 0) ? pos[1] : (localPosition) ? go.transform.localPosition[1] : go.transform.position[1],
				(((int)ia & 4) != 0) ? pos[2] : (localPosition) ? go.transform.localPosition[2] : go.transform.position[2]);

			if (!localPosition)
				go.transform.position = newpos;
			else
				go.transform.localPosition = newpos;
		}
	}

	// Attribute that lets me flag SendCull to use the custom drawer and be a multiselect enum
	public class XYZSwitchMaskAttribute : PropertyAttribute
	{
		public XYZSwitchMaskAttribute() { }
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(XYZSwitchMaskAttribute))]
	public class XYZSwitchMaskAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			float left = r.xMin + EditorGUIUtility.labelWidth;
			float fwidth = Mathf.Min(r.width - EditorGUIUtility.labelWidth, 120);
			float third = fwidth * .333f;

			int value = property.intValue;
			bool x = (value & 1) != 0;
			bool y = (value & 2) != 0;
			bool z = (value & 4) != 0;

			EditorGUI.LabelField(r, label);

			EditorGUI.LabelField(new Rect(left, r.yMin, third, r.height), "X");
			x = EditorGUI.Toggle(new Rect(left + 16, r.yMin, third, r.height), x);

			EditorGUI.LabelField(new Rect(left + third, r.yMin, third, r.height), "Y");
			y = EditorGUI.Toggle(new Rect(left + 16 + third, r.yMin, third, r.height), y);

			EditorGUI.LabelField(new Rect(left + third * 2, r.yMin, third, r.height), "Z");
			z =EditorGUI.Toggle(new Rect(left + 16 + third * 2, r.yMin, third, r.height), z);

			property.intValue = (x ? 1 : 0) + (y ? 2 : 0) + (z ? 4 : 0);
		}
	}
#endif

}


