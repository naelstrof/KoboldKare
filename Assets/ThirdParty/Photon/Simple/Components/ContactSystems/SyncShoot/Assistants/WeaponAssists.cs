// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Photon.Pun.Simple.Assists
{
	public static class WeaponAssists
	{
		public const string REMOTECONTACT_FOLDER = AssistHelpers.ADD_TO_OBJ_FOLDER + "Remote Contact/";

		#region Assist Menu

		[MenuItem(REMOTECONTACT_FOLDER + "Contact Scan", false, AssistHelpers.PRIORITY)]
        public static void AddContactScan()
		{
            var go = AddWeaponPlaceholder("Contact Scan", AssistColor.Cyan, PrimitiveType.Cube);
            go.EnsureComponentExists<SyncContactScan>().UsePreset(Preset.ContactScan);
            go.EnsureComponentExists<ContactTrigger>().UsePreset(Preset.ContactScan);
            go.EnsureComponentExists<SyncContact>();
        }

        [MenuItem(REMOTECONTACT_FOLDER + "Contact Cannon", false, AssistHelpers.PRIORITY)]
        public static void AddContactCannon()
        {
            //AddWeapon<SyncCannon>("Net Projectile Launcher", PrimitiveType.Cylinder);

            var go = AddWeaponPlaceholder("Contact Cannon", AssistColor.Cyan, PrimitiveType.Cylinder);
            go.EnsureComponentExists<SyncCannon>().triggerKey = KeyCode.G;
            go.EnsureComponentExists<ContactTrigger>().UsePreset(Preset.WeaponCannon);
            go.EnsureComponentExists<SyncContact>();
            go.EnsureComponentExists<VitalsContactReactor>().UsePreset(Preset.WeaponCannon);
        }

        [MenuItem(REMOTECONTACT_FOLDER + "Damage Scan", false, AssistHelpers.PRIORITY)]
        public static void AddDamagescan()
        {
            var go = AddWeaponPlaceholder("Damage Scan", AssistColor.Red, PrimitiveType.Cube);
            go.EnsureComponentExists<SyncContactScan>().UsePreset(Preset.WeaponScan);
            go.EnsureComponentExists<ContactTrigger>().UsePreset(Preset.WeaponScan);
            go.EnsureComponentExists<SyncContact>();
            go.EnsureComponentExists<VitalsContactReactor>().UsePreset(Preset.WeaponScan);
        }

        [MenuItem(REMOTECONTACT_FOLDER + "Damage Cannon", false, AssistHelpers.PRIORITY)]
        public static void AddDamageCannon()
		{
			//AddWeapon<SyncCannon>("Net Projectile Launcher", PrimitiveType.Cylinder);

            var go = AddWeaponPlaceholder("Damage Cannon", AssistColor.Red, PrimitiveType.Cylinder);
            go.EnsureComponentExists<SyncCannon>().triggerKey = KeyCode.F;
            go.EnsureComponentExists<ContactTrigger>().UsePreset(Preset.WeaponCannon);
            go.EnsureComponentExists<SyncContact>();
            go.EnsureComponentExists<VitalsContactReactor>().UsePreset(Preset.WeaponCannon);
        }



        #endregion

        public static GameObject AddWeaponPlaceholder(string name, AssistColor color, PrimitiveType primitive = PrimitiveType.Cube)
        {
            var selection = Selection.activeGameObject;

            if (selection == null)
            {
                Debug.LogWarning("No selected GameObject. Cannot add " + name + ".");
                return null;
            }

            var go = selection.transform.CreateEmptyChildGameObject(name);
            var prim = go.CreateNewPrimitiveAsChild(primitive, AssistHelpers.ColliderType.None, "Model Placeholder", .5f, color);
            prim.transform.localEulerAngles = new Vector3(90, 0, 0);
            if (primitive == PrimitiveType.Cylinder)
                prim.transform.localScale = new Vector3(.2f, .2f, .2f);


            /// Make sure we have a visibility toggle
            if (!go.GetComponentInParent<OnStateChangeToggle>())
                go.AddComponent<OnStateChangeToggle>();

            Selection.activeObject = go;

            return go;
        }


    }
}
#endif

