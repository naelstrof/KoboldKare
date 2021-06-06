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

namespace Photon.Pun.Simple.ContactGroups
{

#if UNITY_EDITOR
	[HelpURL(Internal.SimpleDocsURLS.CONTACT_SYSTEM)]
#endif

	public class ContactGroupSettings : SettingsScriptableObject<ContactGroupSettings>
	{
		public static bool initialized;
		public const string DEF_NAME = "Default";

		[HideInInspector]
		public List<string> contactGroupTags = new List<string>(2) { DEF_NAME, "Critical" };
		public Dictionary<string, int> rewindLayerTagToId = new Dictionary<string, int>();

		[System.NonSerialized]
		public static int bitsForMask;

#if UNITY_EDITOR
		public override string SettingsName { get { return "Contact Group Settings"; } }
#endif

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void Bootstrap()
		{
			var single = Single;
		}

		public override void Initialize()
		{
			single = this;
			base.Initialize();

			if (initialized)
				return;

			initialized = true;

			bitsForMask = contactGroupTags.Count - 1;

			// populate the lookup dictionary
			for (int i = 0; i < contactGroupTags.Count; i++)
				if (rewindLayerTagToId.ContainsKey(contactGroupTags[i]))
				{
					Debug.LogError("The tag '" + contactGroupTags[i] + "' is used more than once in '" + GetType().Name + "'. Repeats will be discarded, which will likely break some parts of rewind until they are removed.");
				}
				else
				{
					rewindLayerTagToId.Add(contactGroupTags[i], i);
				}

			//XDebug.Log(!XDebug.logInfo ? null : ("Initialized ContactGroupMasterSettings - Total Layer Tags Count: " + contactGroupTags.Count));
		}

		/// <summary>
		/// Supplied a previous index and contactGroup name, and will return the index of the best guess in the current list of layer tags. First checks for name,
		/// then if the previous int still exists, if none of the above returns 0;
		/// </summary>
		/// <returns></returns>
		[System.Obsolete("Left over from NST, likely not useful any more.")]
		public static int FindClosestMatch(string n, int id)
		{
			var hgs = Single;

			if (hgs.contactGroupTags.Contains(n))
				return hgs.contactGroupTags.IndexOf(n);
			if (id < hgs.contactGroupTags.Count)
				return id;

			return 0;
		}
#if UNITY_EDITOR
		//private bool expanded = true;

		public override string HelpURL { get { return Internal.SimpleDocsURLS.CONTACT_SYSTEM; } }

		public static string instructions = "These tags are used by " + typeof(ContactGroupAssign).Name + " to assign colliders to contact groups, for uses like head shots and critical hits.";

        [MenuItem("Window/Photon Unity Networking/Contact Group Settings", false, 206)]
        private static void SelectInstance()
        {
            Single.SelectThisInstance();
        }

        int prevContactGroupCount = -1;
		string summaryString;

		public override bool DrawGui(Object target, bool asFoldout, bool includeScriptField, bool initializeAsOpen = true, bool asWindow = false)
		{
			

			bool isExpanded = base.DrawGui(target, asFoldout, includeScriptField, initializeAsOpen, asWindow);
			//bool isHierarchyMode = EditorGUIUtility.hierarchyMode;

			if (!asFoldout || isExpanded)
			{
				EditorGUI.BeginChangeCheck();

				SerializedObject soTarget = new SerializedObject(Single);
				
				SerializedProperty tags = soTarget.FindProperty("contactGroupTags");

				EditorGUILayout.PrefixLabel("Defined Groups:");

				EditorUtils.DrawEditableList(tags, true, "Group");

				EditorGUILayout.HelpBox(instructions, MessageType.None);

				int contactGroupCount = contactGroupTags.Count;

				if (prevContactGroupCount != contactGroupCount)
					summaryString = (contactGroupTags.Count - 1) + " bits per hit used for hitmasks.";

				EditorGUILayout.HelpBox(summaryString, MessageType.None);

				prevContactGroupCount = contactGroupCount;

				/// Save changes
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(Single, "Modify ContactGroupSettings ");
					soTarget.ApplyModifiedProperties();

					EditorUtility.SetDirty(this);
					AssetDatabase.SaveAssets();
				}
				
			}
			return isExpanded;
		}

		public static void DrawLinkToSettings()
		{
			Rect r = EditorGUILayout.GetControlRect(false, 48f);

			GUI.Box(r, GUIContent.none, (GUIStyle)"HelpBox");

			float padding = 4;
			float line = r.yMin + padding;
			float width = r.width - padding * 2;

			GUI.Label(new Rect(r.xMin + padding, line, width, 16), "Add/Remove Hit Box Groups here:", (GUIStyle)"MiniLabel");
			line += 18;

			if (GUI.Button(new Rect(r.xMin + padding, line, width, 23), new GUIContent("Find Contact Group Settings")))
			{
				EditorGUIUtility.PingObject(ContactGroupSettings.Single);
			}
		}
#endif
	}


#if UNITY_EDITOR

	[CustomEditor(typeof(ContactGroupSettings))]
	[CanEditMultipleObjects]
	public class ContactGroupSettingsEditor : Editor
	{

		public override void OnInspectorGUI()
		{
			ContactGroupSettings.Single.DrawGui(target, false, false, true);
		}
	}
#endif
}

