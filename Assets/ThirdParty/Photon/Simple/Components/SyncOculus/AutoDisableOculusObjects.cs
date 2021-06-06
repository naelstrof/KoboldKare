// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if OCULUS


using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Simple
{
	public class AutoDisableOculusObjects : NetComponent
	{

		protected List<Camera> cams = new List<Camera>();

		protected List<Behaviour> behaviours = new List<Behaviour>();

		protected static List<Behaviour> temp = new List<Behaviour>();

		public override void OnAwake()
		{
			base.OnAwake();

			transform.GetNestedComponentsInChildren<Camera>(cams);

			transform.GetNestedComponentsInChildren<Behaviour>(temp);

			System.Type teleportSupportType = System.Type.GetType("TeleportSupport");

			foreach (var comp in temp)
			{
				var nspace = comp.GetType().Namespace;
				var n = comp.GetType().Name;

				if (teleportSupportType.IsAssignableFrom(comp.GetType()))
				{

				}
				//else if (comp is Camera)
				//{

				//}
				else if (nspace != null && nspace.StartsWith("OVR"))
				{

				}
				else if (comp.GetType().Name.StartsWith("OVR"))
				{

				}
				else continue;

				behaviours.Add(comp);
			}
		}


		public override void OnJoinedRoom()
		{
			base.OnJoinedRoom();

			if (!IsMine)
			{
				Debug.Log("Disabling Join");
				foreach (Camera cam in cams)
					cam.gameObject.SetActive(false);

				foreach (var obj in behaviours)
					obj.enabled = false;
			}
		}

		public override void OnAuthorityChanged(bool isMine, bool asServer)
		{
			base.OnAuthorityChanged(isMine, asServer);

			if (!isMine)
			{
				Debug.Log("Disabling Authority");
				foreach (Camera cam in cams)
					cam.gameObject.SetActive(false);

				foreach (var obj in behaviours)
					obj.enabled = false;
			}
		}

	
	}

}

#endif