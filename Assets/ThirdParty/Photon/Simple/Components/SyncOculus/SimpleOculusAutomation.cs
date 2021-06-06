// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if OCULUS


using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using Photon.Pun.Simple.Assists;
#endif



namespace Photon.Pun.Simple
{

    public class SimpleOculusAutomation : NetComponent
	{

		protected List<Camera> cameras = new List<Camera>();

#if UNITY_EDITOR
		protected override void Reset()
		{
			base.Reset();

			var trackingSpace = transform.RecursiveFind("TrackingSpace");
			if(trackingSpace)
				trackingSpace.Add3DHandsRot().transform.Add3DHandsPos();

			var test = trackingSpace.AddDefaultSyncTransform3DRigidbody();

			var leftHandAnchor = transform.RecursiveFind("LeftHandAnchor");
			if (leftHandAnchor)
				leftHandAnchor.Add3DHandsRot().transform.Add3DHandsPos();

			var rightHandAnchor = transform.RecursiveFind("RightHandAnchor");
			if (rightHandAnchor)
				rightHandAnchor.Add3DHandsRot().transform.Add3DHandsPos();

		}
#endif

		private void OnEnable()
		{
			PhotonNetwork.AddCallbackTarget(this);
		}

		private void OnDisable()
		{
			PhotonNetwork.RemoveCallbackTarget(this);
		}


		public override void OnAwake()
		{
			base.OnAwake();
			transform.GetNestedComponentsInChildren(cameras);
		}

		public override void OnStart()
		{
			base.OnStart();

			foreach (Camera cam in cameras)
			{
				//if (cam.name == "CenterEyeAnchor")
				//cam.enabled = false;
				//else
				cam.gameObject.SetActive(IsMine);
			}
		}

		public override void OnAuthorityChanged(bool isMine, bool asServer)
		{
			base.OnAuthorityChanged(isMine, asServer);

			foreach (Camera cam in cameras)
			{
				//if (cam.name == "CenterEyeAnchor")
				//	cam.enabled = false;
				//else
				cam.gameObject.SetActive(IsMine);
			}
		}

		public override void OnJoinedRoom()
		{
			base.OnJoinedRoom();

			foreach (Camera cam in cameras)
			{
				//if (cam.name == "CenterEyeAnchor")
				//	cam.enabled = false;
				//else
				cam.gameObject.SetActive(IsMine);
			}
		}

	}

#if UNITY_EDITOR
    [CustomEditor(typeof(SimpleOculusAutomation))]
	[CanEditMultipleObjects]
	public class SimpleOculusAutomationEditor : HeaderEditorBase // 
	{
		protected override string Instructions
		{
			get
			{
				return "Automatically disables of camera objects based in IsMine == false. Provides Automation options for adding Sync components.";
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUI.Button(EditorGUILayout.GetControlRect(), "Auto Add Syncs"))
				OculusAssist.ConvertOculus();
		}
	}
#endif
}

#endif