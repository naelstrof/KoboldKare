// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
	[CustomEditor(typeof(SyncTransform))]
	[CanEditMultipleObjects]
	public class SyncTransformEditor : SyncObjectEditor
	{
		protected override string Instructions
		{
			get
			{
				return "Add this component to sync the transform. The GameObject be part of a net entity (object with a PhotonView). " +
                    "If photonView.IsMine is true, this broadcasts transform changes to other players and applies them. Be sure to disable all controller code if IsMine is false, to avoid conflicts.";
			}
		}

		protected override string HelpURL
		{
			get
			{
				return Internal.SimpleDocsURLS.SYNCCOMPS_PATH + "#synctransform_component";
			}
		}

		protected override string TextTexturePath
		{
			get
			{
				return "Header/SyncTransformText";
			}
		}

		//public override void OnInspectorGUI()
		//{
		//	base.OnInspectorGUI();
		//}

		protected override void OnInspectorGUIInjectMiddle()
		{
			base.OnInspectorGUIInjectMiddle();

			/// Warn that a component may be playing with settings
			var iautosync = (target as Component).GetComponent<ITransformController>();
			if (!ReferenceEquals(iautosync, null))
			{
				if (iautosync.AutoSync)
				{
					EditorGUILayout.HelpBox((iautosync as Component).GetType().Name + " has AutoSync enabled, and is managing some crusher settings.", MessageType.Info);
				}
			}
		}
	}
}

