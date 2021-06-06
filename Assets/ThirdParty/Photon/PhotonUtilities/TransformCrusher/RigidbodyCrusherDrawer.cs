//// ---------------------------------------------------------------------------------------------
//// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
//// <author>developer@exitgames.com</author>
//// ---------------------------------------------------------------------------------------------

//#if UNITY_EDITOR

//using UnityEngine;
//using UnityEditor;

//namespace Photon.Compression
//{

//	[CustomPropertyDrawer(typeof(RigidbodyCrusher))]
//	[CanEditMultipleObjects]

//	public class RigidbodyCrusherDrawer : TransformCrusherDrawer
//	{
//		public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label)
//		{
//			base.OnGUI(r, prop, label);
//			SerializedProperty vel = prop.FindPropertyRelative("velCrusher");
//			SerializedProperty ang = prop.FindPropertyRelative("angCrusher");
//			float vh = EditorGUI.GetPropertyHeight(vel);
//			float ah = EditorGUI.GetPropertyHeight(ang);

//			EditorGUI.PropertyField(new Rect(r.xMin, currentline, r.width, vh), vel);
//			currentline += vh;
//			EditorGUI.PropertyField(new Rect(r.xMin, currentline, r.width, ah), ang);
//		}

//		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//		{
//			SerializedProperty vel = property.FindPropertyRelative("velCrusher");
//			SerializedProperty ang = property.FindPropertyRelative("angCrusher");
//			float vh = EditorGUI.GetPropertyHeight(vel);
//			float ah = EditorGUI.GetPropertyHeight(ang);

//			return base.GetPropertyHeight(property, label) + vh + ah;
//		}
//	}

//}
//#endif
