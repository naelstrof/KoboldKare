// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR
#if PUN_2_OR_NEWER

using UnityEngine;
using UnityEditor;
using emotitron.Utilities.Networking;
using System.Collections.Generic;
using System;

using Photon.Pun;

namespace Photon.Pun.Simple.Assists
{

    public static class NetObjectAssists
    {

        [MenuItem(AssistHelpers.CONVERT_TO_FOLDER + "Player", false, AssistHelpers.CONVERTTO_PRIORITY)]
        public static void ConvertToPlayer()
        {
            var selection = ConvertToBasicNetObject(null);

            if (selection == null)
                return;

            selection.transform.EnsureComponentExists<SyncAnimator, Animator>(null, null, true);

            selection.EnsureComponentExists<SyncTransform>();

            if (selection.GetComponent<Animator>())
                selection.EnsureComponentExists<SyncAnimator>();

            VitalsAssists.AddVitalsSystem();

            /// Quality of life components
            selection.EnsureComponentExists<AutoOwnerComponentEnable>();

            selection.EnsureComponentExists<ContactTrigger>();

            /// Inventory system
            InventorySystemAssists.AddInventorySystem();

            StateAssists.AddSystem(selection, true);

            selection.EnsureComponentExists<AutoDestroyUnspawned>();
        }

        [MenuItem(AssistHelpers.CONVERT_TO_FOLDER + "Platform", false, AssistHelpers.CONVERTTO_PRIORITY)]
        public static void ConvertToPlatform()
        {
            var go = GetSelectedGameObject();

            if (!go)
                return;

            if (go.transform.lossyScale != Vector3.one)
            {
                if (go.transform.parent && go.transform.parent.lossyScale != Vector3.one)
                {
                    Debug.LogWarning("Aborted Convert To Platform. Parent object has a scale other than " + Vector3.one + " which would distort any object that mounts to it.");
                    return;
                }

                Debug.LogWarning("Selected object as a scale other than " + Vector3.one + ". Creating a parent object for the platform and making the selected object a child.");
                var par = new GameObject("Platform");
                par.transform.position = go.transform.position;
                par.transform.rotation = go.transform.rotation;
                par.transform.parent = go.transform.parent;
                go.transform.parent = par.transform;

                go = par;
            }

            go.EnsureComponentExists<NetObject>(true);
            go.EnsureComponentExists<Mount>();
            var mover = go.EnsureComponentExists<SyncNodeMover>();
            mover.posDef.includeAxes = AxisMask.Y;
            mover.StartNode.Pos = new Vector3(0, -2f, 0);
            mover.EndNode.Pos = new Vector3(0, 2f, 0);
            mover.oscillatePeriod = 3;

            mover.rotDef.includeAxes = AxisMask.None;
            mover.sclDef.includeAxes = AxisMask.None;

            Selection.activeGameObject = go;
        }



        #region Vitals System

        #endregion


        [MenuItem(AssistHelpers.ADD_TO_OBJ_FOLDER + "Auto Mount Hitscan", false, AssistHelpers.PRIORITY)]
        public static AutoMountHitscan AddAutoMountHitscan()
        {
            GameObject par = AssistHelpers.GetSelectedGameObject();

            if (!par)
                return null;

            GameObject go = new GameObject("AutoMount");
            go.transform.eulerAngles = new Vector3(90f, 0, 0);
            go.transform.parent = par.transform;
            go.transform.localPosition = new Vector3(0, 0, 0);
            Selection.activeGameObject = go;

            go.EnsureComponentExists<OnStateChangeToggle>(false);

            return go.EnsureComponentExists<AutoMountHitscan>();
        }


        public static GameObject ConvertToBasicNetObject(GameObject selection = null, OwnershipOption ownershipOption = OwnershipOption.Fixed)
        {
            if (selection == null)
                selection = Selection.activeGameObject;

            if (selection == null)
            {
                Debug.LogWarning("No selected GameObject. Creating a dummy Player/NPC.");
                selection = new GameObject("Empty Player");
                selection.CreateChildStatePlaceholders(Space_XD.SPACE_3D, Dynamics.Variable, 2);
                //return null;
            }

#if PUN_2_OR_NEWER
            var pv = selection.EnsureComponentExists<PhotonView>();
            pv.OwnershipTransfer = ownershipOption;
#endif
            selection.EnsureComponentExists<NetObject>();

            return selection;
        }

        public static SystemPresence GetSystemPresence(this GameObject go, params MonoBehaviour[] depends)
        {
#if PUN_2_OR_NEWER

            var netObj = go.transform.GetParentComponent<NetObject>();


            PhotonView pv = go.transform.GetParentComponent<PhotonView>();

            if (pv && netObj)
            {
                if (pv.gameObject == go)
                    return SystemPresence.Complete;
                else
                    return SystemPresence.Nested;
            }

            else if (pv || netObj)
                return SystemPresence.Partial;
            else
                return SystemPresence.Absent;
#else
			return 0;
#endif


        }
        public static void AddSystem(this GameObject go, bool add, params MonoBehaviour[] depends)
        {

#if PUN_2_OR_NEWER

            if (add)
            {
                go.EnsureComponentExists<PhotonView>();
                go.EnsureComponentExists<NetObject>();
            }

            else
            {

                var no = go.transform.GetParentComponent<NetObject>();
                if (no)
                {
                    var ml = go.GetComponent<MountsManager>();
                    if (ml)
                        GameObject.DestroyImmediate(ml);

                    GameObject.DestroyImmediate(no);
                }

                var pv = go.transform.GetParentComponent<PhotonView>();
                if (pv)
                    GameObject.DestroyImmediate(pv);
#endif

            }
        }

        public static GameObject GetSelectedGameObject()
        {
            var selection = Selection.activeGameObject;

            if (selection == null)
            {
                Debug.LogWarning("No selected GameObject.");
                return null;
            }

            return selection;
        }

        public static GameObject GetSelectedReparentableGameObject()
        {
            var selection = GetSelectedGameObject();
            if (!selection)
                return null;

            return selection.CheckReparentable() ? selection : null;
        }

        public static bool CheckReparentable(this GameObject go)
        {
#if UNITY_2018_3_OR_NEWER
			if (PrefabUtility.IsPartOfPrefabAsset(go))
#else
            if (PrefabUtility.GetPrefabType(go) == PrefabType.Prefab)
#endif
            {
                Debug.LogWarning("Cannot add/reparent GameObjects on a Prefab Source Object. Make an instance of this prefab in the current scene and run this assistant on that, then save the changes to the prefab, or drag the scene instance into a Resource folder.");
                return false;
            }

            return true;
        }

        public static Transform RecursiveFind(this Transform t, string name)
        {
            for (int i = 0; i < t.childCount; ++i)
            {
                var child = t.GetChild(i);
                if (/*child.name.Length == name.Length && */child.name == name)
                {
                    return child;
                }
                else
                {
                    var found = RecursiveFind(child, name);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds missing Component T to any transform that currently has Component TIfExists.
        /// </summary>
        public static void EnsureComponentExists<T, TIfExists>(this Transform t, List<Component> created, List<Component> found, bool recursive) where TIfExists : Component where T : Component
        {
            if (created != null)
                created.Clear();

            if (found != null)
                found.Clear();

            var existing = t.GetComponent<T>();

            if (!existing && t.GetComponent<TIfExists>())
            {
                existing = t.gameObject.AddComponent<T>();
                if (created != null)
                    created.Add(existing);
            }

            if (existing && found != null)
                found.Add(existing);

            if (recursive)
                for (int i = 0; i < t.childCount; ++i)
                    EnsureComponentExists<T, TIfExists>(t.GetChild(i), created, found, true);

        }

        /// <summary>
        /// Adds missing Component T to any transform that currently has Component TIfExists. Slower version, but accepts strings for soft references to types.
        /// </summary>
        public static void EnsureComponentExists(this Transform t, List<Component> created, List<Component> found, string addComponent, string ifCompnoent, bool recursive)
        {

            Type aComp = Type.GetType(addComponent);
            Type iComp = Type.GetType(ifCompnoent);

            if (created != null)
                created.Clear();

            if (found != null)
                found.Clear();

            var existing = t.GetComponent(aComp);

            if (!existing && t.GetComponent(iComp))
            {
                existing = t.gameObject.AddComponent(aComp);
                if (created != null)
                    created.Add(existing);
            }

            if (existing && created != null)
                found.Add(existing);

            if (recursive)
                for (int i = 0; i < t.childCount; ++i)
                    EnsureComponentExists(t.GetChild(i), created, found, aComp, iComp, true);

        }

        /// <summary>
        /// Internal method for recursing once Type has been resolved.
        /// </summary>
        public static void EnsureComponentExists(this Transform t, List<Component> created, List<Component> found, Type aComp, Type iComp, bool recursive)
        {

            var existing = t.GetComponent(aComp);

            if (!existing && t.GetComponent(iComp))
            {
                existing = t.gameObject.AddComponent(aComp);
                if (created != null)
                    created.Add(existing);
            }

            if (existing && created != null)
                found.Add(existing);

            if (recursive)
                for (int i = 0; i < t.childCount; ++i)
                    EnsureComponentExists(t.GetChild(i), created, found, aComp, iComp, true);
        }

        public static bool HasComponent(this Transform t, string simpleName, string asmQualifiedName)
        {
            Type SimpleCapsuleWithStickMovementType = Type.GetType(simpleName);

            if (SimpleCapsuleWithStickMovementType == null)
                SimpleCapsuleWithStickMovementType = Type.GetType(asmQualifiedName);

            if (SimpleCapsuleWithStickMovementType != null)
                if (t.GetComponent(SimpleCapsuleWithStickMovementType))
                    return true;

            return false;
        }
    }
}

#endif

#endif
