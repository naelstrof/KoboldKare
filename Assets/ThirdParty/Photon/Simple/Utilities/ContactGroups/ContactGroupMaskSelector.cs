// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Photon.Utilities;
#endif

namespace Photon.Pun.Simple.ContactGroups
{
	[System.Serializable]
	public struct ContactGroupMaskSelector : IContactGroupMask
    {
		/// <summary>
		/// Default is an index of 0 and a layermask of 0
		/// </summary>
		[SerializeField] private int mask;
		public int Mask { get {return mask;} set { mask = value; } }

#if UNITY_EDITOR
		public bool expanded;
#endif

		public ContactGroupMaskSelector(int mask)
		{
			this.mask = mask;
#if UNITY_EDITOR
			expanded = true;
#endif
	}

	public static implicit operator int(ContactGroupMaskSelector selector)
		{
			return selector.mask;
		}

		public static implicit operator ContactGroupMaskSelector(int mask)
		{
			return new ContactGroupMaskSelector(mask);
		}

	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(ContactGroupMaskSelector))]
	[CanEditMultipleObjects]
	public class ContactGroupMaskSelectorDrawer : VersaMaskDrawer
    {
		protected override bool FirstIsZero
		{
			get
			{
				return true;
			}
		}
		protected override string[] GetStringNames(SerializedProperty property)
		{
			return ContactGroupSettings.Single.contactGroupTags.ToArray();
		}
	}

#endif
}

