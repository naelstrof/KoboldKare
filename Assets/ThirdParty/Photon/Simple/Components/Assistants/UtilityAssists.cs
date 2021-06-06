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
	public static class UtilityAssists
	{
		public static SystemPresence GetRootSystemPresence<T>(this GameObject go, params MonoBehaviour[] depends) where T : MonoBehaviour
		{
			var netobj = go.transform.GetParentComponent<NetObject>();

			var comp = netobj ? netobj.transform.GetNestedComponentInChildren<T, NetObject>(true) : go.GetComponent<T>();

			if (comp)
			{
				if (!netobj)
				{
					Object.DestroyImmediate(comp);
					return SystemPresence.Absent;
				}

				if (comp.gameObject.gameObject == go)
					return SystemPresence.Complete;
				else
					return SystemPresence.Nested;
			}

			return SystemPresence.Absent;
		}

		public static void AddRootSystem<T>(this GameObject go, bool add, params MonoBehaviour[] depends) where T : MonoBehaviour
		{
			var netobj = go.transform.GetParentComponent<NetObject>();

			if (add)
			{
				if (netobj)
					netobj.gameObject.EnsureComponentExists<T>();
				else
					go.EnsureComponentExists<T>();
			}
			else
			{
				var comp = netobj ? netobj.gameObject.GetComponentInChildren<T>() : go.GetComponent<T>();
				if (comp)
					Object.DestroyImmediate(comp);
			}
		}

		//public static SystemPresence GetSystemPresence<T>(this GameObject go, params MonoBehaviour[] rootdependencies) where T : MonoBehaviour
		//{

		//}

		//public static void AddSystem<T>(this GameObject go, bool add, params MonoBehaviour[] rootdependencies) where T : MonoBehaviour
		//{
		//	var netobj = go.transform.GetParentNetObject();

		//	if (add)
		//	{

		//		for (int i = 0; i < rootdependencies.Length; ++i)
		//			if (netobj.gameObject.EnsureComponentExists<>)

		//		if (netobj)
		//			netobj.gameObject.EnsureComponentExists<T>();
		//		else
		//			go.EnsureComponentExists<T>();
		//	}
		//	else
		//	{
		//		var comp = netobj ? netobj.gameObject.GetComponentInChildren<T>() : go.GetComponent<T>();
		//		if (comp)
		//			Object.DestroyImmediate(comp);
		//	}
		//}

	}
}

#endif
#endif