//Copyright 2019, Da// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Utilities
{
	public static class NameTypeUtils
	{
		/// <summary>
		/// Gets the enum value for the type name. If none exists returns 1-Custom.
		/// </summary>
		public static int GetVitalTypeForName(string name, string[] enumNames)
		{
			for (int i = 0; i < enumNames.Length; ++i)
				if (name == enumNames[i])
					return i;

			return 1;
		}
	}

#if UNITY_EDITOR

	
	[CanEditMultipleObjects]
	public abstract class NameTypeDrawer : PropertyDrawer
	{
		protected static GUIContent reusableGC = new GUIContent();

		protected virtual string LabelName { get { return "Type"; } }
		protected virtual string NameTypeFieldname { get { return "type"; } }
		protected virtual string NameFieldname { get { return "name"; } }
		protected virtual string HashFieldname { get { return "hash"; } }
		protected abstract string[] EnumNames { get; }

		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			Rect er = new Rect(r) { xMin = r.xMin + EditorGUIUtility.labelWidth };
			float labelWidth = EditorGUIUtility.labelWidth;

			var type = property.FindPropertyRelative(NameTypeFieldname);
			var name = property.FindPropertyRelative(NameFieldname);
			var hash = property.FindPropertyRelative(HashFieldname);

			// Label
			GUIContent _label;
			if (label.tooltip == null || label.tooltip == "")
			{
				reusableGC.text = LabelName;
				reusableGC.tooltip = "The enum or name value in this collection to target.";
				_label = reusableGC;

			}else
				_label = label;

			EditorGUI.LabelField(r, _label);

#if UNITY_2019_3_OR_NEWER
			const int ENUMWIDTH = 64;
#else
			const int ENUMWIDTH = 59;
#endif
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(new Rect(er) { x = er.x - ENUMWIDTH - 2, width = ENUMWIDTH }, type, GUIContent.none);
			if (EditorGUI.EndChangeCheck())
			{
				name.stringValue = EnumNames[type.enumValueIndex];
				hash.intValue = name.stringValue.GetHashCode();
				property.serializedObject.ApplyModifiedProperties();
			}

			// Name Field
			er.xMin = r.xMin + labelWidth;
			er.xMax = r.xMax;

			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(new Rect(er) {/* xMin = er.xMin + 64,*/ height = er.height - 1 }, name, GUIContent.none);
			if (EditorGUI.EndChangeCheck())
			{
				type.enumValueIndex = NameTypeUtils.GetVitalTypeForName(name.stringValue, EnumNames);
				hash.intValue = name.stringValue.GetHashCode();
				property.serializedObject.ApplyModifiedProperties();
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label);
		}
	}

#endif
}
