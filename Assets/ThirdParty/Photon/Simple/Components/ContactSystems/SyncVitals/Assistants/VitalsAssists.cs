// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

#if UNITY_EDITOR
#if PUN_2_OR_NEWER

using UnityEngine;
using UnityEditor;
using Photon.Utilities;
using System.Collections.Generic;
using Photon.Pun;

namespace Photon.Pun.Simple.Assists
{

	public static class VitalsAssists
	{



        [MenuItem(AssistHelpers.ADD_TO_OBJ_FOLDER + "System/Vitals", false, AssistHelpers.PRIORITY)]
        public static void AddVitalsSystem()
		{
			var go = NetObjectAssists.GetSelectedGameObject();
			if (!go)
				return;

			AddSystem(go, true);
		}


		[MenuItem(AssistHelpers.ADD_TO_OBJ_FOLDER + "Vitals UI", false, AssistHelpers.PRIORITY)]
        public static void GenerateVitalsUI()
		{
			var selection = Selection.activeGameObject;
			GenerateVitalsUI(selection);
		}


        public static SystemPresence GetSystemPresence(this GameObject go, params MonoBehaviour[] depends)
		{
			var sv = go.transform.GetNestedComponentInChildren<SyncVitals, NetObject>(true);
			if (!sv)
				sv =go.transform.GetNestedComponentInParents<SyncVitals, NetObject>();

			if (sv)
			{
				if (sv.gameObject == go)
					return SystemPresence.Complete;
				else
					return SystemPresence.Nested;

			}

			if (go.transform.GetNestedComponentInChildren<SyncVitals, NetObject>(true))
				return SystemPresence.Partial;
			else
				return SystemPresence.Absent;
		}

		public static List<SyncVitals> reuseableSyncVitals = new List<SyncVitals>();

		public static void AddSystem(this GameObject go, bool add, params MonoBehaviour[] depends)
		{
			if (add)
			{
				go.EnsureComponentExists<SyncVitals>();
				if (!go.transform.GetNestedComponentInChildren<VitalUI, NetObject>(true))
					VitalsAssists.GenerateVitalsUI(go);
			}

			else
			{
				go.transform.GetNestedComponentsInChildren<SyncVitals, NetObject>(reuseableSyncVitals);
				for (int i = reuseableSyncVitals.Count - 1; i >= 0; --i)
					Object.DestroyImmediate(reuseableSyncVitals[i]);

				DestroyVitalsUI(go);
			}
			
		}


        public static void DestroyVitalsUI(GameObject selection)
		{
			var uigo = selection.transform.Find("Vitals UI");
			if (uigo)
				Object.DestroyImmediate(uigo.gameObject);
		}

		public static GameObject GenerateVitalsUI(GameObject selection)
		{
			const float PAD_ABOVE_OBJ_BOUNDS = .1f;

			if (selection == null)
			{
				Debug.LogWarning("No selected GameObject. Cannot search for Vitals.");
				return null;
			}

			if (!selection.CheckReparentable())
				return null;

			var ivc = selection.GetComponentInChildren<IVitalsSystem>();

			if (ivc == null)
			{
				Debug.LogWarning("No " + typeof(IVitalsSystem).Name + " found on selected GameObject. Cannot find Vitals.");
				return null;
			}

			var ivcTransform = (ivc as Component).transform;

			var go = new GameObject("Vitals UI");
			go.transform.parent = ivcTransform;

			var defs = ivc.Vitals.vitalDefs;
			int count = defs.Count;

			for (int i = 0; i < count; ++i)
			{
				var vgo = new GameObject(defs[i].VitalName.name + " UI");
				vgo.transform.parent = go.transform;
				VitalUI vitalUI = vgo.AddComponent<VitalUI>();
				vitalUI.targetVital = defs[i].VitalName;
				vitalUI.ApplyVitalsSource((UnityEngine.Object)ivc);
				vitalUI.AddDefaultUIPrefab();
			}

			int boundsfound;
			var bounds = selection.CollectMyBounds(BoundsTools.BoundsType.Both, out boundsfound, true);
			var selectionpos = selection.transform.position;
			go.transform.position = new Vector3(selectionpos.x, bounds.max.y + bounds.size.y * PAD_ABOVE_OBJ_BOUNDS, selectionpos.z);

			return go;
		}
	}

}
#endif
#endif