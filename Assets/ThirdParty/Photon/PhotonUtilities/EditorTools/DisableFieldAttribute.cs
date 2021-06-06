// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Photon.Utilities
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class DisableFieldAttribute : PropertyAttribute
    {

    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DisableFieldAttribute))]
    public class DisableFieldAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(r, property);
            EditorGUI.EndDisabledGroup();
            
        }
    }
#endif
}


