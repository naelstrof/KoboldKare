// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Utilities;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Compression
{
#if UNITY_EDITOR
    [HelpURL(HELP_URL)]
#endif

    public class PackObjectSettings : SettingsScriptableObject<PackObjectSettings>
    {

#if UNITY_EDITOR

        public const string HELP_URL = @"https://doc.photonengine.com/en-us/pun/current/gameplay/simple/simplepackobjects";
        public override string HelpURL { get { return HELP_URL; } }
        public override string SettingsDescription
        {
            get
            {
                return "(BETA) PackObject attributes can be added to classes (and structs if unsafe is enabled), and will include these classes in code generation of compression extension methods." +
                    "These extensions are used to create SyncVars, but can also be used by developers as well.";
            }
        }
#endif

        [Header("Code Generation")]

        [Tooltip("Enables the auto generation of codegen for PackObjects / PackAttributes. Disable this if you would like to suspend codegen. Existing codegen will remain, unless it produces errors.")]
        public bool autoGenerate = true;

        [Tooltip("Automatically deletes codegen if it produces any compile errors. Typically you will want to leave this enabled. Disable to see the actual errors being generated.")]
        public bool deleteBadCode = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Bootstrap()
        {
            var single = Single;
        }


#if UNITY_EDITOR


        [MenuItem("Window/Photon Unity Networking/PackObject (SyncVars) Settings", false , 108)]
        private static void SelectInstance()
        {
            Single.SelectThisInstance();
        }

        public override bool DrawGui(Object target, bool asFoldout, bool includeScriptField, bool initializeAsOpen = true, bool asWindow = false)
		{
			bool expanded = base.DrawGui(target, asFoldout, includeScriptField, initializeAsOpen, asWindow);

            if (expanded)
            {
                EditorGUILayout.LabelField(Internal.TypeCatalogue.single.catalogue.ToString(), new GUIStyle("HelpBox") { padding = new RectOffset(6, 6, 6, 6) });

                EditorGUILayout.GetControlRect(false, 4);

                if (GUI.Button(EditorGUILayout.GetControlRect(), "Delete All Generated Code"))
                {
                    Compression.Internal.TypeCatalogue.DeleteAllPackCodeGen();
                }

                if (GUI.Button(EditorGUILayout.GetControlRect(), "Regenerate All Code"))
                {
                    Compression.Internal.TypeCatalogue.RebuildSNSCodegen();
                }
            }

			return expanded;
		}

#endif

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(PackObjectSettings))]
    [CanEditMultipleObjects]
    public class PackObjectSettingsEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            PackObjectSettings.Single.DrawGui(target, false, false, true);
        }
    }
#endif

}
