// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Utilities;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{

#if UNITY_EDITOR
	[HelpURL(Internal.SimpleDocsURLS.MOUNT_SYSTEM)]
#endif

	public class MountSettings : SettingsScriptableObject<MountSettings>
	{

		#region Inspector

		[HideInInspector]
		[SerializeField]
		private List<string> mountNames = new List<string>()
		{
			"Root",

			"1", "2", "3", "4"
		};

		#endregion

		public static int mountTypeCount;
		public static int bitsForMountId;

		public override void Initialize()
		{
			base.Initialize();
			mountTypeCount = Single.mountNames.Count;
			bitsForMountId = (mountTypeCount - 1).GetBitsForMaxValue();
		}

		///// <summary>
		///// Returns the min number of bits required to describe any value between 0 and uint maxvalue
		///// </summary>
		//public static int GetBitsForMaxValue(int maxvalue)
		//{
		//	for (int i = 0; i < 32; ++i)
		//		if (maxvalue >> i == 0)
		//			return i;
		//	return 32;
		//}

		/// <summary>
		/// Returns the index of the mount with the indicated name. If not found returns an index of -1.
		/// </summary>
		/// <param name="name"></param>
		public static int GetIndex(string name)
		{
			return single.mountNames.IndexOf(name);
		}

		public static string GetName(int index)
		{
			if (index >= mountTypeCount)
				return null;
			else
				return single.mountNames[index];
		}

        public static int AllTrueMask
        {
            get
            {
                int cnt = Single.mountNames.Count;

                if (cnt == 32)
                    return -1;

                return (int)(((long)1 << Single.mountNames.Count) - 1);
            }
        }



#if UNITY_EDITOR

       
		public override string HelpURL { get { return Internal.SimpleDocsURLS.MOUNT_SYSTEM; } }

		public override string SettingsName { get { return "Mount Settings"; } }

		public static string[] ToArray() { return Single.mountNames.ToArray(); }

		/// <summary>
		/// Returns the index of the mount with the indicated name. If not found returns an index of -1.
		/// </summary>
		/// <param name="name"></param>
		public static int GetOrCreate(string name)
		{
			var mountNames = single.mountNames;
			int index = mountNames.IndexOf(name);
			if (index == -1)
			{
				index = mountNames.Count;
				mountNames.Add(name);
				mountTypeCount = mountNames.Count;
			}

			return index;
		}

        [MenuItem("Window/Photon Unity Networking/Mount Settings", false, 204)]
        private static void SelectInstance()
        {
            Single.SelectThisInstance();
        }

        public override bool DrawGui(Object target, bool asFoldout, bool includeScriptField, bool initializeAsOpen = true, bool asWindow = false)
		{

			bool isExpanded = base.DrawGui(target, asFoldout, includeScriptField, initializeAsOpen, asWindow);


			if (isExpanded)
			{
				EditorGUI.BeginChangeCheck();

				SerializedObject soTarget = new SerializedObject(Single);

				var mountNames = soTarget.FindProperty("mountNames");
				int cnt = mountNames.arraySize;


				/// Make sure we have the required root / 0 entry.
				if (cnt == 0)
				{
					mountNames.InsertArrayElementAtIndex(0);

				}
				var rootElement = mountNames.GetArrayElementAtIndex(0);

				if (rootElement.stringValue != "Root")
					rootElement.stringValue = "Root";

				/// Draw the Mount Define box
				EditorGUILayout.LabelField("Defined Mounts:");
                EditorUtils.DrawEditableList(mountNames, true, "Mount");

				/// Save changes
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(Single, "Modify Mount Settings " + mountNames.arraySize);
					soTarget.ApplyModifiedProperties();

					/// Modify our cached values for the new settings
					MountSettings.mountTypeCount = mountNames.arraySize;
					MountSettings.bitsForMountId = (mountNames.arraySize - 1).GetBitsForMaxValue();

					EditorUtility.SetDirty(this);
					AssetDatabase.SaveAssets();
				}

				/// Count slider and bits report
				EditorGUILayout.HelpBox(bitsForMountId + " bits for MountIds", MessageType.None);

			}

			return isExpanded;
		}

#endif

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void Bootstrap()
		{
			var single = Single;
		}



	}

#if UNITY_EDITOR

	[CustomEditor(typeof(MountSettings))]
	[CanEditMultipleObjects]
	public class MountSettingsEditor : Editor
	{
		public override void OnInspectorGUI()
		{

			MountSettings.Single.DrawGui(target, false, false, true);
		}
	}
#endif
}
