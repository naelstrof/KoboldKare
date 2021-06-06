// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple.ContactGroups
{
	[System.Serializable]
	public struct ContactGroupSelector : IContactGroupMask
	{
		/// <summary>
		/// Default is an index of 0 and a layermask of 0
		/// </summary>
		public int index;

		// TODO: this really should be cached if possible.
		/// <summary>
		/// Default is an index of 0 and a layermask of 0
		/// </summary>
		public int Mask { get { return (index == 0) ? 0 : ((int)1 << (index - 1)); } }
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(ContactGroupSelector))]
	[CanEditMultipleObjects]
	public class ContactGroupSelectorDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
		{
			var index = property.FindPropertyRelative("index");
			int newindex = EditorGUI.Popup(r, "Contact Group", index.intValue, ContactGroupSettings.Single.contactGroupTags.ToArray());

			if (newindex != index.intValue)
			{
				Undo.RecordObject(property.serializedObject.targetObject, "Change ContactGroup Selection");
				index.intValue = newindex;
			}
		}
	}

#endif
}

