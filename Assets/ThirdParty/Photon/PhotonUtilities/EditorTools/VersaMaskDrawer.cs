// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Photon.Utilities
{

	[CanEditMultipleObjects]
	public abstract class VersaMaskDrawer : PropertyDrawer
	{
		protected static GUIContent reuseGC = new GUIContent();
		protected abstract bool FirstIsZero { get; }
		protected virtual bool ShowMaskBits { get { return true; } }

        protected virtual string[] GetStringNames(SerializedProperty property)
        {
            var maskattr = attribute as VersaMaskAttribute;
            if (maskattr.castTo != null)
                return System.Enum.GetNames(maskattr.castTo);
            else
                return property.enumDisplayNames;
        }

		protected const float PAD = 4;
		protected const float LINE_SPACING = 18;
		protected const float BOX_INDENT = 0; //16 - PAD;

		protected static SerializedProperty currentProperty;
		protected int maskValue;

		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			currentProperty = property;

			bool usefoldout = UseFoldout(label);

			if (usefoldout)
			{

                property.isExpanded = EditorGUI.Toggle(new Rect(r) { xMin = r.xMin, height = LINE_SPACING, width = EditorGUIUtility.labelWidth }, property.isExpanded, (GUIStyle)"Foldout");
			}


			label = EditorGUI.BeginProperty(r, label, property);

			/// For extended drawer types, the mask field needs to be named mask
			var mask = property.FindPropertyRelative("mask");

			/// ELSE If this drawer is being used as an attribute, then the property itself is the enum mask.
			if (mask == null)
				mask = property;

			maskValue = mask.intValue;

			int tempmask;
			Rect br = new Rect(r) { xMin = r.xMin + BOX_INDENT };
			Rect ir = new Rect(br) { height = LINE_SPACING };

			Rect labelRect = new Rect(r) { xMin = usefoldout ? r.xMin + 14 : r.xMin, height = LINE_SPACING };

            var stringNames = GetStringNames(property);
			/// Remove Zero value from the array if need be.
			var namearray = new string[FirstIsZero ? stringNames.Length - 1 : stringNames.Length];
			int len = namearray.Length;
			for (int i = 0; i < len; i++)
				namearray[i] = stringNames[FirstIsZero ? (i + 1) : i];

			if (usefoldout && property.isExpanded)
			{
				tempmask = 0;

				EditorGUI.LabelField(new Rect(br) { yMin = br.yMin + LINE_SPACING }, "", (GUIStyle)"HelpBox");
				ir.xMin += PAD;
				ir.y += PAD;

				string drawmask = "";

				for (int i = 0; i < len; ++i)
				{
                    ir.y += LINE_SPACING;

					int offsetbit = 1 << i;
					EditorGUI.LabelField(ir, new GUIContent(namearray[i]));
					if (EditorGUI.Toggle(new Rect(ir) { xMin = r.xMin }, " ", ((mask.intValue & offsetbit) != 0)))
						{
						tempmask |= offsetbit;
						if (ShowMaskBits)
							drawmask = "1" + drawmask;
					}
					else if (ShowMaskBits)
						drawmask = "0" + drawmask;
                }

				reuseGC.text = (ShowMaskBits) ?( "[" + drawmask + "]") : "";
				EditorGUI.LabelField(labelRect, label, (GUIStyle)"label");
                EditorGUI.LabelField(labelRect, new GUIContent(" "), reuseGC);
            }
			else
			{
				tempmask = EditorGUI.MaskField(r, usefoldout ? " " : "", mask.intValue, namearray);

				if (usefoldout)
					EditorGUI.LabelField(new Rect(r) { xMin = r.xMin + 14 }, label, (GUIStyle)"label");
			}

			if (tempmask != mask.intValue)
			{
				Undo.RecordObject(property.serializedObject.targetObject, "Change Mask Selection");
				mask.intValue = tempmask;
				maskValue = tempmask;
				property.serializedObject.ApplyModifiedProperties();
			}

			EditorGUI.EndProperty();
		}

		protected bool UseFoldout(GUIContent label)
		{
			return label.text != null && label.text != "";
		}

        //protected void EnsureHasEnumtype()
        //{
        //    /// Set the attribute enum type if it wasn't set by user in attribute arguments.
        //    if (attribute == null)
        //    {
        //        Debug.LogWarning("Null Attribute");
        //        return;
        //    }
        //    var attr = attribute as VersaMaskAttribute;
        //    var type = attr.castTo;
        //    if (type == null)
        //        attr.castTo = fieldInfo.FieldType;
        //}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			currentProperty = property;

			bool expanded = (property.isExpanded && UseFoldout(label));

            if (expanded)
            {
                var stringNames = GetStringNames(property);
                return LINE_SPACING * (stringNames.Length + (FirstIsZero ? 0 : 1)) + PAD * 2;
            }
            else 
                return base.GetPropertyHeight(property, label);
		}
	}

}
#endif
