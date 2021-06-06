// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR
#if PUN_2_OR_NEWER

using UnityEngine;
using UnityEditor;
using Photon.Pun.Simple.ContactGroups;

namespace Photon.Pun.Simple.Assists
{

    public static class PickupAssists
    {

        public const string PICKUP_FOLDER = AssistHelpers.ADD_TO_SCENE_TXT + "Pickup/";

        [MenuItem(PICKUP_FOLDER + "Item: Static", false, AssistHelpers.PRIORITY)]
        public static void CreateItemPickup3DStatic()
        {
            GameObject selection = new GameObject("Pickup Item");
            ConvertToItemPickup(selection, Space_XD.SPACE_3D, Dynamics.Static);
        }

        [MenuItem(PICKUP_FOLDER + "Item: Dynamic", false, AssistHelpers.PRIORITY)]
        public static void CreateItemPickup3DDynamic()
        {
            GameObject selection = new GameObject("Pickup Item");
            ConvertToItemPickup(selection, Space_XD.SPACE_3D, Dynamics.Variable);
        }

        [MenuItem(PICKUP_FOLDER + "Vital: Static", false, AssistHelpers.PRIORITY)]
        public static void CreateVitalPickup3DStatic()
        {
            GameObject selection = new GameObject("Pickup Vital");
            ConvertToVitalPickup(selection, Space_XD.SPACE_3D, Dynamics.Static, Preset.HealthPickup);
        }

        [MenuItem(PICKUP_FOLDER + "Vital: Dynamic", false, AssistHelpers.PRIORITY)]
        public static void CreateVitalPickup3DVDynamic()
        {
            GameObject selection = new GameObject("Pickup Vital");
            ConvertToVitalPickup(selection, Space_XD.SPACE_3D, Dynamics.Variable, Preset.HealthPickup);
        }

        //[MenuItem("GameObject/Simple/Convert To/Pickup : Item", false, -100)]
        public static void ConvertToItemPickup(GameObject selection, Space_XD space, Dynamics dynamics)
        {
            selection = ConvertToPickup(selection, space, dynamics, typeof(BasicInventory));

            selection.EnsureComponentExists<InventoryContactReactors>();

            var sst = selection.EnsureComponentExists<SyncSpawnTimer>();
            sst.despawnEnable = false;

            Selection.activeGameObject = selection;
        }

        //[MenuItem("GameObject/Simple/Convert To/Pickup : Vital", false, -100)]
        public static void ConvertToVitalPickup(GameObject selection, Space_XD space, Dynamics dynamics, Preset preset)
        {
            selection = ConvertToPickup(selection, space, dynamics, typeof(SyncVitals));

            //selection.EnsureComponentExists<VitalsContactReactors>();
            selection.EnsureComponentExists<VitalsContactReactor>().UsePreset(preset);

            var sst = selection.EnsureComponentExists<SyncSpawnTimer>();
            sst.despawnEnable = true;
            sst.despawnOn = ObjState.Mounted;

            Selection.activeGameObject = selection;
        }


        /// <summary>
        /// Add the core components needed for all Pickup types, and add toggles to existing children.
        /// </summary>
        public static GameObject ConvertToPickup(GameObject selection, Space_XD space, Dynamics dynamics, params System.Type[] allowedSystems)
        {
            selection = NetObjectAssists.ConvertToBasicNetObject(selection, Photon.Pun.OwnershipOption.Takeover);

            selection.EnsureComponentExists<ContactTrigger>();

            selection.EnsureComponentExists<SyncContact>();
            var ss = selection.EnsureComponentExists<SyncState>();
            ss.mountableTo.mask = MountSettings.AllTrueMask;
            selection.EnsureComponentExists<SyncOwner>();

            if (dynamics != Dynamics.Static)
                selection.EnsureComponentExists<OnStateChangeKinematic>();

            /// Add OnStateChangeToggle to existing children before creating placeholder children
            selection.EnsureComponentOnNestedChildren<OnStateChangeToggle>(false);

            /// Add ContactGroups, and set to default
            var hga = selection.EnsureComponentExists<ContactGroupAssign>();
            hga.contactGroups.Mask = 0;

            if (dynamics != Dynamics.Static)
            {
                selection.AddRigidbody(space);
                var st = selection.EnsureComponentExists<SyncTransform>();
                st.transformCrusher.SclCrusher.Enabled = false;

            }

            selection.CreateChildStatePlaceholders(space, dynamics, 1.5f);

            return selection;
        }
    }
}
#endif
#endif