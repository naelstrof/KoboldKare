// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR
#if PUN_2_OR_NEWER

using UnityEngine;
using Photon.Pun;

namespace Photon.Pun.Simple.Assists
{
	public static class MountAssists
	{


		public static SystemPresence GetSystemPresence(this GameObject go, params MonoBehaviour[] depends)
		{

            var netobj = go.transform.GetParentComponent<NetObject>();
			var ml = netobj ? netobj.transform.GetNestedComponentInChildren<MountsManager, NetObject>(true) : null;

            var m = go.GetComponent<Mount>();

			/// Destroy an ML that is not paired with a netobj
			if (ml)
				if (!netobj || ml.gameObject != netobj.gameObject)
					Object.DestroyImmediate(ml);

			//if (m)
			//	var ml = MountsLookup.EstablishMounts(go);

			//var m = go.transform.GetNestedComponentInParents<Mount>();
			//if (!m)
			//	m = go.transform.GetNestedComponentInChildren<Mount>();

			///// Missing NetObj is an immediate fail.
			//if (!netobj)
			//{
			//	/// Without an NetObj, we shouldn't have an ML
			//	if (ml)
			//		Object.DestroyImmediate(ml);

			//	if (m)
			//		return SystemPresence.Incomplete;
			//	else
			//		return SystemPresence.Missing;
			//}

			///// We have a NetObj and we have a mount - Make sure our Lookup on the NetObj.
			//else
			//{
			//	/// Destroy any Lookup on on the NetObj root.
			//	if (ml && ml.gameObject != netobj)
			//	{
			//		Object.DestroyImmediate(ml);
			//	}
			//	/// Make sure we have a MountsLookup on the root if we have a mount.
			//	if (m)
			//		ml = netobj.gameObject.EnsureComponentExists<MountsLookup>();
			//}

			///// If we have a mount and no Lookup, add a Lookup
			//if (!ml && m)
			//	ml = netobj ? netobj.gameObject.AddComponent<MountsLookup>() : go.AddComponent<MountsLookup>();

			if (m)
			{
				if (!ml || !netobj)
					return SystemPresence.Incomplete;
				else
					return SystemPresence.Complete;
			}
			else
			{
				if (ml)
					return
						SystemPresence.Nested;
			}
			
			return SystemPresence.Absent;
		}

		public static void AddSystem(this GameObject go, bool add, params MonoBehaviour[] depends)
		{
			var netobj = go.transform.GetParentComponent<NetObject>();

			if (add)
			{
				/// Make sure ML is with NetObject root
				if (netobj)
					netobj.gameObject.EnsureComponentExists<MountsManager>();
				else
					go.EnsureComponentExists<MountsManager>();

				go.EnsureComponentExists<Mount>();
			}
			else
			{
				var mount = go.GetComponent<Mount>();
				if (mount)
					Object.DestroyImmediate(mount);

				if (netobj.gameObject == go)
				{
					var ml = go.GetComponent<MountsManager>();
					if (ml)
						Object.DestroyImmediate(ml);
				}
			}
			

		}
	}
}

#endif
#endif