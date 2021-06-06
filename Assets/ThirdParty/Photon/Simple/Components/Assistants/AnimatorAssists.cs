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
    public static class AnimatorAssists
    {

        public static SystemPresence GetSystemPresence(this GameObject go, params MonoBehaviour[] depends)
        {

#if PUN_2_OR_NEWER
            var netobj = go.transform.GetParentComponent<NetObject>();
#endif

            var comp = go.GetComponent<SyncAnimator>();

            if (comp)
            {
                if (!netobj)
                    return SystemPresence.Incomplete;

                if (comp.gameObject.gameObject == go)
                    return SystemPresence.Complete;
            }

            return SystemPresence.Absent;

        }

        public static void AddSystem(this GameObject go, bool add, params MonoBehaviour[] depends)
        {
            if (add)
            {
                go.AddComponent<SyncAnimator>();
            }
            else
            {
                var sa = go.GetComponent<SyncAnimator>();
                if (sa)
                    Object.DestroyImmediate(sa);
            }
        }
    }
}

#endif
#endif