// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Simple.Internal;

using UnityEditor;

namespace Photon.Pun.Simple
{
	[CustomEditor(typeof(SyncVitals))]
	[CanEditMultipleObjects]
	public class SyncVitalsEditor : SyncObjectEditor
	{
        protected override string HelpURL
        {
            get { return SimpleDocsURLS.SUBSYS_PATH + "#syncvitals_component"; }
        }

        protected override string TextTexturePath
        {
            get { return "Header/SyncVitalsText"; }
        }

        protected override string Instructions
		{
			get
			{
				return "Collection of Vital types used for handling more complex health systems, accounting for layers of vitals. The default creates a base Health, with a Shield and Armor layer. " +
					"Vitals reactors can affect these vitals.";
			}
		}

		private readonly static List<Rigidbody> reusableRBList = new List<Rigidbody>();
		private readonly static List<Rigidbody2D> reusableRB2DList = new List<Rigidbody2D>();


		protected override void OnInspectorGUIInjectMiddle()
		{
			base.OnInspectorGUIInjectMiddle();

			EditorGUI.BeginChangeCheck();
;
			SyncVitals sshealth = target as SyncVitals;

#if PUN_2_OR_NEWER
            sshealth.transform.root.transform.GetNestedComponentsInChildren<Rigidbody, NetObject>(reusableRBList);
			sshealth.transform.root.transform.GetNestedComponentsInChildren<Rigidbody2D, NetObject>(reusableRB2DList);
#endif

            int rbCount = reusableRBList.Count;
			int rb2dCount = reusableRB2DList.Count;

			bool isRigidBody = rbCount > 0 || rb2dCount > 0;
			bool isOnRigidbody = (rbCount > 0 && sshealth.GetComponent<Rigidbody>()) || (rb2dCount > 0 && sshealth.GetComponent<Rigidbody2D>());

			if (isRigidBody)
			{
				/// SSH is on root of an RB
				if (isOnRigidbody)
				{
					int colliderCount = sshealth.transform.CountChildCollider(false, true);
					if (colliderCount < 1)
						EditorGUILayout.HelpBox("Cannot locate any non-trigger Collider/Collider2D on this GameObject. Will not be able to detect RB collisions.", MessageType.Warning);
				}
				/// SSH is on an RB, but not the root level
				else
				{
					EditorGUILayout.HelpBox(sshealth.GetType().Name +
						" must be on same child as a RigidBody to detect Rigidbody collisions.", MessageType.Warning);
				}
			}

			if (!isOnRigidbody)
			{
				int triggerCount = sshealth.transform.CountChildCollider(true, true);

				if (triggerCount < 1)
					EditorGUILayout.HelpBox("Cannot locate a Collider/Collider2D on this object and/or children. Hitscans and triggers will not work.", MessageType.Warning);

				if (triggerCount > 1)
					EditorGUILayout.HelpBox(triggerCount + " colliders were found on this object and/or children. " +
						"More than one may cause multiple trigger events.", MessageType.Warning);
			}

			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}
	}
}


