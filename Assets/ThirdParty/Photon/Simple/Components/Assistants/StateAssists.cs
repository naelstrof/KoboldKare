// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR
#if PUN_2_OR_NEWER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

namespace Photon.Pun.Simple.Assists
{
	public static class StateAssists
	{

		public static SystemPresence GetSystemPresence(this GameObject go, params MonoBehaviour[] depends)
		{
			var netobj = go.transform.GetParentComponent<NetObject>();

			var comp = go.transform.GetNestedComponentInParents<SyncState, NetObject>();

			if (comp)
			{
				if (!netobj)
					return SystemPresence.Incomplete;

				/// Syncvar is on not NetObject root - destroy it.
				if (netobj.gameObject != comp.gameObject)
				{
					Object.DestroyImmediate(comp);
					return SystemPresence.Absent;
				}

				/// We have the SyncState Selected
				if (comp.gameObject == go)
				{
					if (go.GetComponent<SyncSpawnTimer>())
						return SystemPresence.Complete;
					else
						return SystemPresence.Partial;
				}
				else
					return SystemPresence.Nested;

			}
			return SystemPresence.Absent;
		}

		public static void AddSystem(this GameObject go, bool add, params MonoBehaviour[] depends)
		{

			if (add)
			{
				var netobj = go.transform.GetParentComponent<NetObject>();
				if (netobj)
					go = netobj.gameObject;

				var ss = go.EnsureComponentExists<SyncState>();
				ss.autoOwnerChange = false;

				go.EnsureComponentOnNestedChildren<OnStateChangeToggle>(false);
				go.EnsureComponentExists<OnStateChangeKinematic>();
				go.EnsureComponentExists<SyncSpawnTimer>();
			}
			else
			{
				go.DestroyComponentOnNestedChildren<OnStateChangeToggle>();

				var kin = go.GetComponent<OnStateChangeKinematic>();
				if (kin)
					Object.DestroyImmediate(kin);

				var sst = go.GetComponent<SyncSpawnTimer>();
				if (sst)
					Object.DestroyImmediate(sst);

				var ss = go.GetComponent<SyncState>();
				if (ss)
					Object.DestroyImmediate(ss);
			}
			

		}
	}

}

#endif
#endif
