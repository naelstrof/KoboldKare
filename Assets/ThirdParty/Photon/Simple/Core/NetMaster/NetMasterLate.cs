// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
    [HelpURL(Internal.SimpleDocsURLS.OVERVIEW_PATH)]
    public class NetMasterLate : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance of the NetMaster. "There can be only one."
        /// </summary>
        public static NetMasterLate single;

        private void Awake()
        {
            if (single && single != this)
            {
                /// If a singleton already exists, destroy the old one - TODO: Not sure about this behavior yet. Allows for settings changes with scene changes.
                Destroy(single);
            }

            single = this;

            DontDestroyOnLoad(this);
        }

        private void FixedUpdate()
        {
            if (!TickEngineSettings.single.enableTickEngine)
                return;

            /// Disable Simple if no NetObjects exist.
            if (NetObject.activeControlledNetObjs.Count == 0 && NetObject.activeUncontrolledNetObjs.Count == 0)
                return;

            NetMasterCallbacks.OnPreSimulateCallbacks(NetMaster.CurrentFrameId, NetMaster.CurrentSubFrameId);

        }

        private void Update()
        {
            if (!TickEngineSettings.single.enableTickEngine)
                return;

            /// Disable Simple if no NetObjects exist.
            if (NetObject.activeControlledNetObjs.Count == 0 && NetObject.activeUncontrolledNetObjs.Count == 0)
                return;

            NetMasterCallbacks.OnPostUpdateCallbacks();
        }

        private void LateUpdate()
        {
            if (!TickEngineSettings.single.enableTickEngine)
                return;

            /// Disable Simple if no NetObjects exist.
            if (NetObject.activeControlledNetObjs.Count == 0 && NetObject.activeUncontrolledNetObjs.Count == 0)
                return;

            NetMasterCallbacks.OnPostLateUpdateCallbacks();
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(NetMasterLate))]
    public class NetMasterLateEditor : NetCoreHeaderEditor
    {
        protected override string TextTexturePath
        {
            get
            {
                return "Header/NetMasterText";
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            const string desc = "Late Timing singleton used by all Simple components. " +
                "Effectively a lightweight networking specific Update Manager. " +
                "This component will be added automatically at runtime if one does not exist in your scene. " +
                "NetMasterLate is set to execute on the latest Script Execution timing, " +
                "ensuring its Fixed/Late/Update callbacks occur after all other scene components.";

            EditorGUILayout.LabelField(desc, new GUIStyle("HelpBox") { wordWrap = true, alignment = TextAnchor.UpperLeft });
        }
    }

#endif
}
