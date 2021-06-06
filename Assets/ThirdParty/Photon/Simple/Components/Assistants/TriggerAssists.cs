// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR
#if PUN_2_OR_NEWER

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Photon.Pun;

namespace Photon.Pun.Simple.Assists
{

    public static class TriggerAssists
    {

        [MenuItem(AssistHelpers.CONVERT_TO_FOLDER + "Zone: Vital Recharge", false, AssistHelpers.CONVERTTO_PRIORITY)]
        public static void ConvertToVitalRechargeZone()
        {
            ConvertToZone(Preset.RechargeZone);
        }

        [MenuItem(AssistHelpers.CONVERT_TO_FOLDER + "Zone: Vital Damage", false, AssistHelpers.CONVERTTO_PRIORITY)]
        public static void ConvertToDamageZone()
        {
            ConvertToZone(Preset.DamageZone);
        }


        private static void ConvertToZone(Preset preset)
        {
            var selection = NetObjectAssists.GetSelectedGameObject();
            if (selection != null)
                if (!selection.CheckReparentable())
                    return;

            selection.EnsureComponentExists<ContactTrigger>().UsePreset(preset);
            selection.EnsureComponentExists<VitalsContactReactor>().UsePreset(preset);

            selection.SetAllCollidersAsTriggger(true);
        }

        static List<Collider> colliders = new List<Collider>();
        static List<Collider2D> colliders2D = new List<Collider2D>();

        public static void SetAllCollidersAsTriggger(this GameObject selection, bool isTrigger = true)
        {
            if (ReferenceEquals(selection, null))
                return;

            selection.transform.GetNestedComponentsInChildren<Collider, NetObject>(colliders);
            for (int i = 0; i < colliders.Count; ++i)
                colliders[i].isTrigger = true;

            selection.transform.GetNestedComponentsInChildren<Collider2D, NetObject>(colliders2D);
            for (int i = 0; i < colliders2D.Count; ++i)
                colliders2D[i].isTrigger = true;
        }
    }
}

#endif
#endif
