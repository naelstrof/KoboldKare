// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



namespace Photon.Compression
{
	public class WorldBoundsSelectAttributeAttribute : PropertyAttribute
	{

	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(WorldBoundsSelectAttributeAttribute))]
	public class StringListPopupAttributeDrawer : PropertyDrawer
	{
		private static GUIContent[] worldBoundsNames;
		private readonly static GUIContent GC_boundsGroup =  new GUIContent("Bounds Group");

		public override void OnGUI(Rect r, SerializedProperty p, GUIContent label)
		{
			//WorldBoundsSelectAttributeAttribute target = (attribute as WorldBoundsSelectAttributeAttribute);

			/// Rebuild a list of the Group names for WorldMapBounds
			var worldBoundsSettings = WorldBoundsSettings.Single.worldBoundsGroups;
			int cnt = worldBoundsSettings.Count;

			// If the names array doesn't exist or is the wrong size, scrap it and make one that is the correct size.
			if (worldBoundsNames == null || worldBoundsNames.Length != cnt)
			{
				worldBoundsNames = new GUIContent[cnt];
			}
			for (int i = 0; i < cnt; ++i)
			{
				worldBoundsNames[i] = new GUIContent(worldBoundsSettings[i].name);
			}

			r.height = 16;
			//EditorGUI.LabelField(r, label, (GUIStyle)"MiniLabel");
			//int idx = EditorGUI.Popup(r, new GUIContent(" "), p.intValue, worldBoundsNames);
			int idx;
			if (label.text == "")
				idx = EditorGUI.Popup(r, GC_boundsGroup, p.intValue, worldBoundsNames, (GUIStyle)"GV Gizmo DropDown");
			else
				idx = EditorGUI.Popup(r, GC_boundsGroup, p.intValue, worldBoundsNames);

			if (idx != p.intValue)
			{
				p.intValue = idx;
				p.serializedObject.ApplyModifiedProperties();
			}
			var wbgs = WorldBoundsSettings.Single.worldBoundsGroups;
			var wbg = wbgs[p.intValue];

			if (label.text != "")
			{
				r.yMin += 16;
				r.height = WorldBoundsGroup.BoundsReportHeight;

				// if the worldbounds layer is no longer valid, reset to default
				if (p.intValue >= wbgs.Count)
					p.intValue = 0;

				EditorGUI.LabelField(r, wbg.BoundsReport(), (GUIStyle)"HelpBox");
			}

			/// IF this boundsGroupID isn not longer valid, reset it default (0)
			if (p.intValue >= wbgs.Count)
			{
				p.intValue = 0;
				p.serializedObject.ApplyModifiedProperties();
			}

			var so = new SerializedObject(WorldBoundsSettings.Single);
			var wbgc = so.FindProperty("worldBoundsGroups").GetArrayElementAtIndex(p.intValue).FindPropertyRelative("crusher");
			var wbgc_hght = EditorGUI.GetPropertyHeight(wbgc);
			r.yMin += r.height + 2;
			r.height = wbgc_hght;
			EditorGUI.PropertyField(r,wbgc, (wbg.activeWorldBounds.Count > 0) ? WorldBoundsSettings.GC_RANGES_DISABLED : WorldBoundsSettings.GC_RANGES_ENABLED);
		}

		public override float GetPropertyHeight(SerializedProperty p, GUIContent label)
		{

			var so = new SerializedObject(WorldBoundsSettings.Single);
			var wbgc = so.FindProperty("worldBoundsGroups").GetArrayElementAtIndex(p.intValue).FindPropertyRelative("crusher");
			var wbgc_hght = EditorGUI.GetPropertyHeight(wbgc);

			return 10f + WorldBoundsGroup.BoundsReportHeight + wbgc_hght;// base.GetPropertyHeight(property, label) * 3;
		}
	}
	#endif
}




