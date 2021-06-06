// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace emotitron.Utilities.GUIUtilities
{
	/// <summary>
	/// Attribute that will hide a field if it does not implement the indicated interface. NOTE: Cannot be combined with other attributes.
	/// </summary>
	//[AttributeUsage(AttributeTargets.Field)]
	public class ShowIfInterfaceAttribute : PropertyAttribute
	{
		public readonly Type type;
		public readonly string tooltip;
		public readonly float min, max;

		public ShowIfInterfaceAttribute(Type type, string tooltip)
		{
			this.type = type;
			this.tooltip = tooltip;
		}

		public ShowIfInterfaceAttribute(Type type, string tooltip, float min, float max)
		{
			this.type = type;
			this.tooltip = tooltip;
			this.min = min;
			this.max = max;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(ShowIfInterfaceAttribute))]
	public class ShowIfInterfaceDrawer : PropertyDrawer
	{
		static GUIContent templabel = new GUIContent();

		public override void OnGUI(Rect r, SerializedProperty p, GUIContent label)
		{
			ShowIfInterfaceAttribute attr = attribute as ShowIfInterfaceAttribute;
			
			templabel.text = label.text;
			templabel.tooltip = attr.tooltip;

			Type interfaceType = attr.type;
			Type targetType = p.serializedObject.targetObject.GetType();

			if (interfaceType.IsAssignableFrom(targetType))
			{

				switch (p.propertyType)
				{
					case SerializedPropertyType.Float:
						EditorGUI.Slider(r, p,  attr.min, attr.max, templabel);
						break;

					case SerializedPropertyType.Integer:
						EditorGUI.IntSlider(r, p, (int)attr.min, (int)attr.max, templabel);
						break;
					
					default:
					EditorGUI.PropertyField(r, p, templabel);
						break;

				}

			}
		}

		public override float GetPropertyHeight(SerializedProperty p, GUIContent label)
		{
			ShowIfInterfaceAttribute attr = attribute as ShowIfInterfaceAttribute;
			Type interfaceType = attr.type;
			Type targetType = p.serializedObject.targetObject.GetType();

			bool containsInterface = (interfaceType.IsAssignableFrom(targetType));
			return containsInterface ? base.GetPropertyHeight(p, label) : 0;
		}
	}
#endif
}

