// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Utilities
{
    /// <summary>
    /// Attribute that indicates an enum should render as multi-select mask drop list in the inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
	public class EnumMaskAttribute : PropertyAttribute
	{
		public bool definesZero;
        public Type castTo;

		public EnumMaskAttribute(bool definesZero = false, Type castTo = null)
		{
            this.castTo = castTo;
			this.definesZero = definesZero;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(EnumMaskAttribute))]
	public class EnumMaskAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
            string[] names;
            var maskattr = attribute as EnumMaskAttribute;
            if (maskattr.castTo != null)
                names = Enum.GetNames(maskattr.castTo);
            else
			    names = property.enumDisplayNames;

			if (maskattr.definesZero)
			{
				string[] truncated = new string[names.Length - 1];
				Array.Copy(names, 1, truncated, 0, truncated.Length);
				names = truncated;
			}

			//_property.intValue = System.Convert.ToInt32(EditorGUI.EnumMaskPopup(_position, _label, (SendCullMask)_property.intValue));
			property.intValue = EditorGUI.MaskField(r, label, property.intValue, names);

        }
    }
#endif
}

