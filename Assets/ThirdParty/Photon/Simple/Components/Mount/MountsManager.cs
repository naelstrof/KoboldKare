// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

using Photon.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]

    public class MountsManager : MonoBehaviour
        , IOnAwake
        , IOnPreQuit
    {
        [System.NonSerialized] public Dictionary<int, Mount> mountIdLookup = new Dictionary<int, Mount>();
        [System.NonSerialized] public List<Mount> indexedMounts = new List<Mount>();
        [System.NonSerialized] public int bitsForMountId;

        #region Startup / Shutdown

#if UNITY_EDITOR
        private void Reset()
        {
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(this, false);

            EstablishMounts(gameObject);
        }

        /// <summary>
        /// Ensure this gameobject has a NetObject with a MountsLookup component, as well as a "Root" Mount. Adds these if needed.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static MountsManager EstablishMounts(GameObject go)
        {
            /// Must have a NetObj
            NetObject netObj = go.GetComponent<NetObject>();

            if (!netObj)
                netObj = go.GetComponentInParent<NetObject>();

            if (!netObj)
                netObj = go.transform.root.gameObject.AddComponent<NetObject>();

#if PUN_2_OR_NEWER
            MountsManager mountsLookup = netObj.transform.GetNestedComponentInChildren<MountsManager, NetObject>(true);
#else
            MountsManager mountsLookup = null;
#endif
            /// Remove Lookup if its somehow on a child
            if (mountsLookup && mountsLookup.gameObject != go)
            {
                Object.DestroyImmediate(mountsLookup);
            }

            /// Add Lookup if its still missing
            if (!mountsLookup)
                mountsLookup = netObj.gameObject.AddComponent<MountsManager>();

            /// Create or correct the root mount
            var rootMount = netObj.gameObject.GetComponent<Mount>();
            if (rootMount == null)
            {
                rootMount = netObj.gameObject.gameObject.AddComponent<Mount>();
                rootMount.mountType = new MountSelector(0);
            }
            else if (rootMount.mountType.id != 0)
            {
                rootMount.mountType.id = 0;
            }

            mountsLookup.CollectMounts();

            return mountsLookup;
        }

#endif

        public void OnAwake()
        {
            CollectMounts();
            bitsForMountId = (indexedMounts.Count == 0) ? 0 : (indexedMounts.Count - 1).GetBitsForMaxValue();
        }

        public void OnPreQuit()
        {
            UnmountAll();
        }

        /// <summary>
        /// Adds all child mounts to the lookup collections.
        /// </summary>
        /// <returns></returns>
        public List<Mount> CollectMounts(bool force = false)
        {

            indexedMounts.Clear();
            mountIdLookup.Clear();

#if PUN_2_OR_NEWER
            /// Get all mounts and index them
            transform.GetNestedComponentsInChildren<Mount, NetObject>(indexedMounts);
#endif

            for (int i = 0, cnt = indexedMounts.Count; i < cnt; ++i)
            {
                var mount = indexedMounts[i];
                mount.componentIndex = i;

                int mountTypeId = mount.mountType.id;
                if (!mountIdLookup.ContainsKey(mountTypeId))
                    mountIdLookup.Add(mountTypeId, mount);
            }

            return indexedMounts;
        }

        #endregion Startup/Shutdown

        public Mount GetMount(int mountId)
        {
            Mount mount;
            mountIdLookup.TryGetValue(mountId, out mount);
            return mount;
        }

        /// <summary>
        /// Force all mounted objects to dismount.
        /// </summary>
        public void UnmountAll()
        {
            for (int i = 0, cnt = indexedMounts.Count; i < cnt; i++)
            {
                indexedMounts[i].DismountAll();
            }
        }

#if UNITY_EDITOR

        public static GUIStyle mountlabelstyle;
        public static GUIStyle mountHiliteStyle;
        public static GUIStyle mountNoliteStyle;

        protected static void EnsureStylesExists()
        {

            if (mountlabelstyle == null)
                mountlabelstyle = new GUIStyle("Label") { margin = new RectOffset(0, 0, 0, 0), alignment = TextAnchor.UpperLeft };

            /// Get style for "This" mount row

#if UNITY_2019_3_OR_NEWER
			GUIStyle basestyle = (GUIStyle)"FoldOutPreDrop";
#else
            GUIStyle basestyle = (GUIStyle)"WarningOverlay";
#endif
            if (ReferenceEquals(mountHiliteStyle, null))
            {
                mountHiliteStyle = new GUIStyle(basestyle)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    normal = ((GUIStyle)"PR Ping").normal
                };
            }

            /// Get style for "Not This" mount rows
            if (ReferenceEquals(mountNoliteStyle, null))
            {
                mountNoliteStyle = new GUIStyle(basestyle)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    normal = ((GUIStyle)"Label").normal
                };
            }
        }

        public static void DrawAllMountMappings(Mount thismount, MountsManager mountsLookup)
        {
            EnsureStylesExists();

            int usedmask = 0;

            int cnt = mountsLookup.indexedMounts.Count;

            EditorGUILayout.LabelField("All Mounts On NetObject", (GUIStyle)"BoldLabel");

            /// Massive warning if too many mounts are on the object, and the last ones will not get serialized correctly.
            if (mountsLookup.indexedMounts.Count > MountSettings.mountTypeCount)
                EditorGUILayout.HelpBox("NetObject has too many mounts. Mount count is limited to the count value in MountSettings.", MessageType.Error);

            if (cnt > 0)
            {
                EditorGUILayout.BeginVertical((GUIStyle)"HelpBox");


                for (int i = 0; i < mountsLookup.indexedMounts.Count; ++i)
                {

                    SerializedObject mount = new SerializedObject(mountsLookup.indexedMounts[i]);


                    var mountType = mount.FindProperty("mountType");
                    var index = mountType.FindPropertyRelative("id");

                    /// Make sure Root is selected for the first mount, and only the first mount
                    if (i == 0)
                    {
                        if (index.intValue != 0)
                        {
                            index.intValue = 0;
                            mount.ApplyModifiedProperties();
                        }
                    }
                    /// Switch off Root for all other mounts
                    else if (index.intValue == 0)
                    {
                        index.intValue = i;
                        mount.ApplyModifiedProperties();
                        Debug.LogWarning("Only the first root Mount on a NetOjbect can be set to mountType 'Root'");
                    }

                    int idx = index.intValue;

                    /// Watch for repeats
                    int idxmask = 1 << idx;
                    bool isAlreadyUsed = (idxmask & usedmask) != 0;
                    usedmask |= idxmask;

                    bool isthis = (ReferenceEquals(mount.targetObject, thismount));
                    const int CNT_WIDTH = 30;
                    const int LABL_PAD = 2;

                    /// Start drawing the row
                    EditorGUILayout.BeginHorizontal(isthis ? mountHiliteStyle : mountNoliteStyle);

                    /// Get the indented rect
                    Rect indexrect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(CNT_WIDTH), GUILayout.MinWidth(CNT_WIDTH));
                    indexrect.xMin += 8;

                    /// Warning Icon
                    if (isAlreadyUsed)
                        EditorGUI.LabelField(new Rect(indexrect) { xMin = indexrect.xMin - 3 }, new GUIContent(EditorGUIUtility.FindTexture("CollabError"), "More than one Mount has the same compatible mount selection. Only the first will be used."));
                    else
                    {
                        /// Index
                        EditorGUI.BeginDisabledGroup(!isthis);
                        EditorGUI.LabelField(indexrect, i.ToString(), mountlabelstyle);
                        EditorGUI.EndDisabledGroup();
                    }

                    /// Mount Selector
                    float selectorWidth = EditorGUIUtility.labelWidth - (CNT_WIDTH + LABL_PAD + 8);
                    EditorGUI.BeginDisabledGroup(i == 0);
                    EditorGUILayout.PropertyField(mountType, GUIContent.none, GUILayout.MaxWidth(selectorWidth), GUILayout.MinWidth(selectorWidth));
                    EditorGUI.EndDisabledGroup();


                    if (isthis)
                    {
                        /// Object
                        Rect thisrect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(40));
                        thisrect.xMin += 16;
                        EditorGUI.LabelField(thisrect, "This", mountlabelstyle);
                    }
                    else
                    {
                        EditorGUI.BeginDisabledGroup(!isthis);
                        EditorGUILayout.ObjectField(mountsLookup.indexedMounts[i], typeof(Mount), false/*, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - cntwidth), GUILayout.MinWidth(EditorGUIUtility.labelWidth - cntwidth)*/);
                        EditorGUI.EndDisabledGroup();
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }
        }

#endif
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MountsManager))]
    [CanEditMultipleObjects]
    public class MountsManagerEditor : MountSystemHeaderEditor
    {
        protected override bool UseThinHeader { get { return true; } }

        protected override string Instructions
        {
            get
            {
                return "Manager/Collection/Lookup for Mounts.";
            }
        }


        protected override string BackTexturePath
        {
            get
            {
                return "Header/GreenBack";
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var _target = target as MountsManager;
            _target.CollectMounts();
        }

    }

#endif
}

