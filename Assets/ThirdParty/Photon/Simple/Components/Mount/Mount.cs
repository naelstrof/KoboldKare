// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
    public class Mount : NetComponent
        , IOnPreQuit
        , IOnPreNetDestroy
    {
        public const string ROOT_MOUNT_NAME = "Root";

        [Tooltip("A Mount component can be associated with more than one mount name. The first root will always include 'Root'.")]
        [SerializeField] [HideInInspector] public MountSelector mountType = new MountSelector(1);

        [SerializeField] [HideInInspector] public int componentIndex;

        [System.NonSerialized]
        public List<IMountable> mountedObjs = new List<IMountable>();

        [System.NonSerialized]
        public MountsManager mountsLookup;

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            mountsLookup = MountsManager.EstablishMounts(gameObject);
            //UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(this, (index == 0));
        }
#endif
        public override void OnAwake()
        {
            base.OnAwake();
            mountsLookup = netObj.GetComponent<MountsManager>();
        }

        public void OnPreQuit()
        {
            DismountAll();
        }

        public void OnPreNetDestroy(NetObject rootNetObj)
        {
            if (rootNetObj == netObj)
                DismountAll();
        }

        //private void OnDestroy()
        //{
        //	//DismountAll();
        //}

        public void DismountAll()
        {
            for (int i = mountedObjs.Count - 1; i >= 0; --i)
            {
                if (mountedObjs[i] as Component)
                    mountedObjs[i].ImmediateUnmount();
            }
        }

        /// <summary>
        /// Removes mountable from the list of objects attached to the current mount, and adds to the list of the new mount.
        /// </summary>
        /// <param name="newMount"></param>
        public static void ChangeMounting(IMountable mountable, Mount prevMount, Mount newMount)
        {

            //Debug.Log("Change Mounting " + (prevMount ? prevMount.photonView.OwnerActorNr.ToString() : "null") + " -> " + (newMount ? newMount.photonView.OwnerActorNr.ToString() : "null"));

            if (!ReferenceEquals(prevMount, null))
            {
                prevMount.mountedObjs.Remove(mountable);
            }

            if (!ReferenceEquals(newMount, null))
            {
                var mountedObjs = newMount.mountedObjs;

                if (!mountedObjs.Contains(mountable))
                    mountedObjs.Add(mountable);
            }
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(Mount))]
    [CanEditMultipleObjects]
    public class MountPickupEditor : MountSystemHeaderEditor
    {
        protected override string Instructions
        {
            get
            {
                return "Define equipment mounting points, which can be referenced by index or name.";
            }
        }

        protected override string TextTexturePath
        {
            get { return "Header/MountText"; }
        }

        static Mount thismount;

        public override void OnEnable()
        {
            base.OnEnable();
        }


        private static HashSet<int> usedIndexes = new HashSet<int>();
        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();

#if PUN_2_OR_NEWER

            thismount = target as Mount;

            var netObj = thismount.transform.GetParentComponent<NetObject>();

            if (netObj == null)
            {
                Debug.LogWarning(thismount.name + " Mount is on a non-NetObject.");
                return;
            }

            usedIndexes.Clear();

            MountsManager mountslookup = netObj.GetComponent<MountsManager>();
            if (!mountslookup)
            {
                mountslookup = netObj.gameObject.AddComponent<MountsManager>();
            }

            mountslookup.CollectMounts();

            //EditorGUI.BeginChangeCheck();

            //EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(thismount.componentIndex == 0);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mountType"));
            EditorGUI.EndDisabledGroup();

            MountsManager.DrawAllMountMappings(thismount, mountslookup);

            EditorGUILayout.Space();
            MountSettings.Single.DrawGui(target, true, false, false, false);

#endif
        }
    }

#endif
}

