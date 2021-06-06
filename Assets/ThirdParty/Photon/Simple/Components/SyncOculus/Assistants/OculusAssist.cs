
#if OCULUS
#if UNITY_EDITOR

using Photon.Pun.UtilityScripts;
using System;
using UnityEditor;
using UnityEngine;

namespace Photon.Pun.Simple.Assists
{
	public static class OculusAssist
	{



		[MenuItem(AssistHelpers.CONVERT_TO_FOLDER + "Oculus", false, -999)]
		public static void ConvertOculus()
		{
			var selection = NetObjectAssists.ConvertToBasicNetObject(null);

			if (selection == null)
				return;

			var t = selection.transform;

			/// Add root transform sync
			if (HasOculusController(t))
				selection.transform.Add3dPosOnly().transform.Add3dEulerOnly();

			selection.EnsureComponentExists<AutoDisableOculusObjects>();

			/// Tracking Space
			var trackingSpace = t.RecursiveFind("TrackingSpace");
			trackingSpace.Add3dEulerOnly();

			/// Hands
			var leftHandAnchor = t.RecursiveFind("LeftHandAnchor");
			var rghtHandAnchor = t.RecursiveFind("RightHandAnchor");
			leftHandAnchor.Add3DHandsPos().transform.Add3DHandsRot();
			rghtHandAnchor.Add3DHandsPos().transform.Add3DHandsRot();

			/// OVRCameraRig
			var cameraRig = t.RecursiveFind("OVRCameraRig");
			if (cameraRig)
				cameraRig.gameObject.EnsureComponentExists<AutoOwnerComponentEnable>();

			/// Add all SyncAnimator
			t.EnsureComponentExists<SyncAnimator, Animator>(null, null, true);

			selection.EnsureComponentExists<AutoDestroyUnspawned>();

			//Type ovrManagerType = Type.GetType("OVRManager");
			//if (ovrManagerType == null)
			//	ovrManagerType = Type.GetType("OVRManager, Oculus.VR, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

			/// Move OVRManager off of this object and onto a non-net object in the scene.
			//if (ovrManagerType != null)
			{
				var manager = t.GetComponentInChildren<OVRManager>();
				if (manager)
				{
					var founds = UnityEngine.Object.FindObjectsOfType<OVRManager>();
					UnityEngine.Object existing = null;
					foreach (var found in founds)
						if (found != manager)
						{
							Debug.Log("OVRManager exists in scene, removing from '" + selection.name + "'");
							existing = found;
							break;
						}
					if (founds == null)
					{
						Debug.Log("OVRManager moved from " + selection.name + "' to scene.");
						manager.ComponentCopy(new GameObject("OVRManager"));
						GameObject.DestroyImmediate(manager);
					}
				}
			}

			selection.EnsureComponentExists<SimpleOculusAutomation>();


			Type ovrGrabberType = Type.GetType("OVRGrabber");
			if (ovrGrabberType == null)
				ovrGrabberType = Type.GetType("OVRGrabber, Oculus.VR, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

			var foundGrabbers = selection.transform.GetNestedComponentsInChildren<OVRGrabber, NetObject>(null);

			foreach (OVRGrabber grabber in foundGrabbers)
			{
				// Oculus was kind enough to make all of these vars protected and private... so we get to do all of this reflection trash.
				Type type = grabber.GetType();
				var gripTransformFieldInfo = type.GetField("m_gripTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

				Transform grip = (gripTransformFieldInfo.GetValue(grabber) as Transform);

				Mount mount;
				if (grip)
					mount = grip.gameObject.EnsureComponentExists<Mount>();
				else
					mount = grabber.gameObject.EnsureComponentExists<Mount>();

				mount.mountType = MountSettings.GetOrCreate(grabber.name);
			}
		}

		private static bool HasOculusController(Transform t)
		{

			/// TODO: add other known root movement components for Oculus here as I find them
			if (t.HasComponent("SimpleCapsuleWithStickMovement", "SimpleCapsuleWithStickMovement, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"))
				return true;

			return false;
		}


	}
}

#endif

#endif