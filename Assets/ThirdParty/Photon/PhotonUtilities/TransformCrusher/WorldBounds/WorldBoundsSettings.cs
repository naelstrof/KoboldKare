// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Photon.Utilities;
using UnityEngine;
using emotitron.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Compression
{

    [CreateAssetMenu()]
    public class WorldBoundsSettings : SettingsScriptableObject<WorldBoundsSettings>
    {

#if UNITY_EDITOR

        public const string HELP_URL = "";
        public override string HelpURL { get { return HELP_URL; } }
        public override string AssetPath { get { return @"Assets/emotitron/TransformCrusher/WorldBounds/Resources/"; } }
        public override string SettingsDescription
        {
            get
            {
                return "(BETA) World Bounds Settings established shared Transform Crusher bitpackers. These can be used by any SyncTransform component that is on the root of synced objects." +
                    "\n\nWorldBounds components can be added to scene objects, and their bounding boxes will automatically factored into the World Bounds position ranges.";
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            var single = Single;
            var wbgs = single.worldBoundsGroups;

            if (single != null && wbgs.Count > 0)
                defaultWorldBoundsCrusher = single.worldBoundsGroups[0].crusher;
        }

        [HideInInspector]
        public List<WorldBoundsGroup> worldBoundsGroups = new List<WorldBoundsGroup>();
        public static ElementCrusher defaultWorldBoundsCrusher;

        protected override void Awake()
        {
            base.Awake();

            if (worldBoundsGroups.Count == 0)
                worldBoundsGroups.Add(new WorldBoundsGroup());
        }

        public override void Initialize()
        {
            base.Initialize();

            if (worldBoundsGroups.Count == 0)
                worldBoundsGroups.Add(new WorldBoundsGroup());

            defaultWorldBoundsCrusher = worldBoundsGroups[0].crusher;

            foreach (var wbs in worldBoundsGroups)
            {
                wbs.RecalculateWorldCombinedBounds();
            }
        }

        public static void RemoveWorldBoundsFromAll(WorldBounds wb)
        {
            var wbgs = Single.worldBoundsGroups;
            for (int i = 0; i < wbgs.Count; ++i)
            {
                var awb = wbgs[i].activeWorldBounds;
                if (awb.Contains(wb))
                {
                    awb.Remove(wb);
                    wbgs[i].RecalculateWorldCombinedBounds();
                }
            }
        }

        public static int TallyBits(ref int index, BitCullingLevel bcl = BitCullingLevel.NoCulling)
        {
            var wbs = Single.worldBoundsGroups;

            // if WorldBounds layer is no longer valid, reset to default
            if (index >= wbs.Count)
                index = 0;

            var crusher = single.worldBoundsGroups[index].crusher;
            return
                crusher.XCrusher.GetBits(bcl) +
                crusher.YCrusher.GetBits(bcl) +
                crusher.ZCrusher.GetBits(bcl);
        }

#if UNITY_EDITOR


        [MenuItem("Window/Photon Unity Networking/World Bounds Settings", false, 208)]
        private static void SelectInstance()
        {
            Single.SelectThisInstance();
        }

        const float helpboxhght = WorldBoundsGroup.BoundsReportHeight; //42f  ;

		public static readonly GUIContent GC_RANGES_DISABLED = new GUIContent("World Bounds (Auto Ranges)", "DISABLE_RANGES");
		public static readonly GUIContent GC_RANGES_ENABLED = new GUIContent("World Bounds");

		public override bool DrawGui(Object target, bool asFoldout, bool includeScriptField, bool initializeAsOpen = true, bool asWindow = false)
		{
			bool isexpanded = base.DrawGui(target, asFoldout, includeScriptField, initializeAsOpen, asWindow);

			if (!isexpanded)
				return isexpanded;

			var so = new SerializedObject(this);
			so.Update();
			EditorGUI.BeginChangeCheck();

			SerializedProperty wbgs = so.FindProperty("worldBoundsGroups");

			Rect r;
			//r = EditorGUILayout.GetControlRect();
			//EditorGUI.LabelField(r, "Group Name", (GUIStyle)"MiniLabel");
			////r.xMax -= 16;
			//EditorGUI.LabelField(r, "Resolution (x/1 units)", CrusherDrawer.miniLabelRight);

			for (int i = 0; i < wbgs.arraySize; ++i)
			{
				var wbg_sp = wbgs.GetArrayElementAtIndex(i);
				var crshr_sp = wbg_sp.FindPropertyRelative("crusher");
				float wbg_hght = EditorGUI.GetPropertyHeight(wbg_sp);
				bool nobounds = worldBoundsGroups[i].activeWorldBounds.Count == 0;
				
				float crshr_hght = EditorGUI.GetPropertyHeight(crshr_sp);

				const float PAD = 6;
				r = EditorGUILayout.GetControlRect(false, wbg_hght + helpboxhght + crshr_hght + PAD * 4);

				EditorGUI.LabelField(r, GUIContent.none, (GUIStyle)"HelpBox");
				r.xMin += PAD; r.xMax -= PAD;
				r.yMin += PAD; r.yMax -= PAD;
				r.height = wbg_hght;

				r.xMax -= 18;
				/// World Bound Group Drawer
				EditorGUI.PropertyField(r,  wbg_sp);
				r.xMax += 18;

				/// Delete button
				EditorGUI.BeginDisabledGroup(i == 0);
				{
					Rect btnr = r;
					//btnr.yMin -= wbg_hght;
					btnr.xMin = btnr.xMax - 16;
					btnr.width = 16;
					btnr.height = 16;
					if (GUI.Button(btnr, "X"))
					{
						wbgs.DeleteArrayElementAtIndex(i);
						so.ApplyModifiedProperties();
						break;
					}
				}
				EditorGUI.EndDisabledGroup();


				r.yMin += wbg_hght + PAD;
				r.height = helpboxhght;

				/// Make sure we are working with current values.
				if (!nobounds)
					worldBoundsGroups[i].RecalculateWorldCombinedBounds();

				const string noWBsfound = "There are no WorldBounds components active in the current scene for this group.";
				string summary = (worldBoundsGroups[i].ActiveBoundsObjCount == 0) ? noWBsfound : worldBoundsGroups[i].BoundsReport();
				//r.xMax += 16;
				EditorGUI.LabelField(r, summary, (GUIStyle)"HelpBox");
				
				

				r.yMin += helpboxhght + PAD * 2;
				r.height = crshr_hght;

				/// Draw editable crusher if there are no worldbounds
				//EditorGUI.BeginDisabledGroup(!nobounds);
				var wbgc = (nobounds) ? GC_RANGES_ENABLED : GC_RANGES_DISABLED;
				EditorGUI.PropertyField(r, crshr_sp, wbgc);
				//EditorGUI.EndDisabledGroup();	

				//EditorGUILayout.Space();
			}


			if (GUI.Button(EditorGUILayout.GetControlRect(), "Add Bounds Group"))
			{
				int newidx = wbgs.arraySize;

				Single.worldBoundsGroups.Add(new WorldBoundsGroup());
				so.Update();

				//wbgs.InsertArrayElementAtIndex(newidx);
				wbgs.GetArrayElementAtIndex(newidx).FindPropertyRelative("name").stringValue = WorldBoundsGroup.newAddName;

				so.ApplyModifiedProperties();
				EnsureNamesAreUnique(newidx);
				so.Update();
			}

			EditorGUILayout.Space();

			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(so.targetObject);
			}

			return isexpanded;
		}

		private static HashSet<string> namecheck = new HashSet<string>();
		public static void EnsureNamesAreUnique(int newestIndex = -1)
		{
			namecheck.Clear();
			var wbs = Single.worldBoundsGroups;
			bool haschanged = false;

			for (int i = 0; i < wbs.Count; ++i)
			{
				var wbsi = wbs[i];
				// Try adding newest changed last, so it will get its named changed, rather than an existing duplicate
				if (i == newestIndex && i != 0)
					continue;

				if (i == 0)
				{
					if (wbsi.name != WorldBoundsGroup.defaultName)
					{
						haschanged = true;
						wbsi.name = WorldBoundsGroup.defaultName;
					}
				}
				else
					while (namecheck.Contains(wbsi.name))
					{
						haschanged = true;
						wbsi.name += "X";
					}

				namecheck.Add(wbsi.name);
			}

			// Change the newest changed name as needed (if supplied and valid) last
			if (newestIndex > 0 && newestIndex < wbs.Count)
				while (namecheck.Contains(wbs[newestIndex].name))
				{
					haschanged = true;
					wbs[newestIndex].name += "X";
				}

			if (haschanged)
			{
				EditorUtility.SetDirty(Single);
			}
		}
#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(WorldBoundsSettings))]
	public class WorldBoundsSOEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			WorldBoundsSettings.Single.DrawGui(target, false, false, true);

		}
	}
#endif
}

