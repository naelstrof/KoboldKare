// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if GHOST_WORLD
using Photon.Pun.Simple.GhostWorlds;
#endif

namespace Photon.Pun.Simple.ContactGroups
{

	public class ContactGroupAssign : MonoBehaviour
#if GHOST_WORLD
        , ICopyToGhost
#endif
		, IContactGroupsAssign
		, IContactGroupMask
	{
		public ContactGroupMaskSelector contactGroups;

        [Tooltip("Will add a ContactGroupAssign to any children that have colliders and no ContactGroupAssign of their own. ")]
        [SerializeField]
        protected bool applyToChildren = true;
        public bool ApplyToChildren { get { return applyToChildren; } }

        // cached
        public int Mask { get { return contactGroups.Mask; } }
		
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(ContactGroupAssign))]
	[CanEditMultipleObjects]
	public class ContactGroupAssignEditor : ContactGroupHeaderEditor
    {
		protected override string Instructions { get {  return "Assigns colliders of this object (and any children) to a Contact Group. Contact Groups allows collider specific handling (such as critical hits) of collisions, raycast, or overlap hits. " +
                    "This is an alternative to Unity layers, which have the problem of needing to work for both physics and rendering."; } }

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();

			ContactGroupSettings.Single.DrawGui(target, true, false, false);
		}
	}

#endif

	
}
