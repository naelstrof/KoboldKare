// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

namespace Photon.Pun.Simple
{
	public class MountSwitcher : NetComponent
		, IOnPreUpdate
		, IOnPreSimulate
	{
		public KeyCode keycode = KeyCode.M;
		public MountSelector mount = new MountSelector(1);

		protected bool triggered;

		protected SyncState syncState;

		public override void OnAwake()
		{
			base.OnAwake();

			if (netObj)
				syncState = netObj.GetComponent<SyncState>();

			if (!GetComponent<SyncState>())
			{
				Debug.LogWarning(GetType().Name + " on '" + transform.parent.name + "/" + name + "' needs to be on the root of NetObject with component " + typeof(SyncState).Name + ". Disabling.");
				netObj.RemoveInterfaces(this);
			}
		}

		public void OnPreUpdate()
		{
			if (Input.GetKeyDown(keycode))
				triggered = true;
		}

		public void OnPreSimulate(int frameId, int subFrameId)
		{
			//Debug.Log("<color=blue>" + pv.IsMine + "</color>");

			if (triggered)
			{

				triggered = false;

				var currMount = syncState.CurrentMount;

				if (ReferenceEquals(currMount, null))
					return;

				if (!currMount.IsMine)
					return;

				Debug.Log("Try change to mount : " + currMount + " : " + currMount.IsMine + " : " + mount.id);

				syncState.ChangeMount(mount.id);
			}
		}

	}
}

