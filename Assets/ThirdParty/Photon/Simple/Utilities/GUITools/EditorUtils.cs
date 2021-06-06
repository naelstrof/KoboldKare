// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
namespace Photon.Pun.Simple
{
	public static class EditorUtils
	{
		private static Texture2D _docsIconTexture;
		public static readonly GUIContent reusableGC = new GUIContent();

		public static Texture2D DocsIconTexture
		{
			get
			{
				if (_docsIconTexture == null)
					_docsIconTexture = (Texture2D)Resources.Load<Texture2D>("Header/DocsIcon");

				return _docsIconTexture;
			}
		}

		public static bool PropertyFieldWithDocsLink(Rect rect, SerializedProperty property, GUIContent label, string url)
		{
			var iconX = rect.xMin + EditorGUIUtility.labelWidth - 16 - 4;
			DrawDocsIcon(iconX, rect.yMin, url);
			return EditorGUI.PropertyField(rect, property, label);
		}

		public static bool PropertyFieldWithDocsLink(SerializedProperty property, GUIContent label, string url)
		{
			var rect = EditorGUILayout.GetControlRect();
			return PropertyFieldWithDocsLink(rect, property, label, url);
		}

		public static void DrawDocsIcon(float xMin, float yMin, string url)
		{
			Rect r = new Rect(xMin, yMin, 16, 16);
			GUI.DrawTexture(r, DocsIconTexture);
			EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
			reusableGC.text = null;
			reusableGC.tooltip = "Click to open Documentation";
			if (GUI.Button(r, reusableGC, GUIStyle.none))
				Application.OpenURL(url);
		}

		public static void CreateErrorIconF(float xmin, float ymin, string _tooltip)
		{
			GUIContent errorIcon = EditorGUIUtility.IconContent("CollabError");
			errorIcon.tooltip = _tooltip;
			EditorGUI.LabelField(new Rect(xmin, ymin, 16, 16), errorIcon);

		}

		/// <summary>
		/// If this gameobject is a clone of a prefab, will return that prefab source. Otherwise just returns the go that was supplied.
		/// </summary>
		public static GameObject GetPrefabSourceOfGameObject(this GameObject go)
		{
#if UNITY_2018_2_OR_NEWER
			return (PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject);
#else
			return (PrefabUtility.GetPrefabParent(go) as GameObject);
#endif
		}

		/// <summary>
		/// A not so efficient find of all instances of a prefab in a scene.
		/// </summary>
		public static List<GameObject> FindAllPrefabInstances(GameObject prefabParent)
		{
			//Debug.Log("Finding all instances of prefab '" + prefabParent.name +"' in scene");
			List<GameObject> result = new List<GameObject>();
			GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
			foreach (GameObject GO in allObjects)
			{
#if UNITY_2018_2_OR_NEWER
				var parPrefab = PrefabUtility.GetCorrespondingObjectFromSource(GO) as GameObject;
#else
				var parPrefab = PrefabUtility.GetPrefabParent(GO);
#endif


				if (parPrefab == prefabParent)
				{
					//Debug.Log("Found prefab instance: " + GO.name);

					UnityEngine.Object GO_prefab = parPrefab;
					if (prefabParent == GO_prefab)
						result.Add(GO);
				}
			}
			return result;
		}

		/// <summary>
		/// Add missing components, Ideally adds to the prefab master, so it appears on any scene versions and doesn't require Apply.
		/// </summary>
		public static T EnsureRootComponentExists<T>(this GameObject go, bool isExpanded = true) where T : Component
		{
#if UNITY_2018_2_OR_NEWER
			GameObject parPrefab = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
#else
			GameObject parPrefab = PrefabUtility.GetPrefabParent(go) as GameObject;
#endif

			T component;

			if (parPrefab)
			{
				component = parPrefab.GetComponent<T>();

				// Remove the NI from a scene object before we add it to the prefab
				if (component == null)
				{
					List<GameObject> clones = FindAllPrefabInstances(parPrefab);

					// Delete all instances of this root component in all instances of the prefab, so when we add to the prefab source they all get it - without repeats
					foreach (GameObject clone in clones)
					{
						T[] comp = clone.GetComponents<T>();
						foreach (T t in comp)
						{
							Debug.Log("Destroy " + t);
							GameObject.DestroyImmediate(t);

						}
					}
					component = parPrefab.AddComponent<T>();
					UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(component, isExpanded);
				}
			}

			// this is has no prefab source
			else
			{
				component = go.GetComponent<T>();
				if (!component)
				{
					component = go.AddComponent<T>();
					UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(component, isExpanded);
				}
			}

			return component;
		}

		/// <summary>
		/// Add missing components, Ideally adds to the prefab master, so it appears on any scene versions and doesn't require Apply.
		/// </summary>
		public static Component EnsureRootComponentExists(this GameObject go, Type type, bool isExpanded = true)
		{
			if (type == null)
				return null;

#if UNITY_2018_2_OR_NEWER
			GameObject parPrefab = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
#else
			GameObject parPrefab = PrefabUtility.GetPrefabParent(go) as GameObject;
#endif
			Component component;

			if (parPrefab)
			{
				component = parPrefab.GetComponent(type);
				// Remove the NI from a scene object before we add it to the prefab
				if (component == null)
				{
					List<GameObject> clones = FindAllPrefabInstances(parPrefab);

					// Delete all instances of this root component in all instances of the prefab, so when we add to the prefab source they all get it - without repeats
					foreach (GameObject clone in clones)
					{
						Component[] comp = clone.GetComponents(type);
						foreach (Component t in comp)
							GameObject.DestroyImmediate(t);
					}
					component = parPrefab.AddComponent(type);
					UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(component, isExpanded);
				}
			}

			// this is has no prefab source
			else
			{

				component = go.GetComponent(type);

				if (!component)
				{
					component = go.AddComponent(type);
					UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(component, isExpanded);
				}
			}

			return component;
		}

		static List<Component> components = new List<Component>();

		/// <summary>
		/// DestroyImmediate a component on the parent prefab of this object if possible, otherwise on this object.
		/// </summary>
		public static void DeleteComponentAtSource(this GameObject go, Type type)
		{
			if (type == null)
				return;

#if UNITY_2018_2_OR_NEWER
			GameObject parPrefab = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
#else
			GameObject parPrefab = PrefabUtility.GetPrefabParent(go) as GameObject;
#endif

			if (parPrefab)
			{
				List<GameObject> clones = FindAllPrefabInstances(parPrefab);

				// Delete all instances of this root component in all instances of the prefab, so when we add to the prefab source they all get it - without repeats
				foreach (GameObject clone in clones)
				{
					clone.GetComponents(type, components);
					foreach (Component t in components)
						GameObject.DestroyImmediate(t);
				}
			}

			// this is has no prefab source
			else
			{
				go.GetComponents(type, components);
				foreach (Component t in components)
					GameObject.DestroyImmediate(t);
			}

		}

		public static GUIStyle listBoxStyle;

		public static void DrawEditableList(SerializedProperty listSP, bool lockZero, string labelPrefix = null, float labelwidth = -1, int maxCount = 32, bool forceUnique = true, GUIStyle guiStyle = null)
		{
			const int PAD = 6;
			const float BTTN_WIDTH = 20;

			if (guiStyle == null)
			{
				if (listBoxStyle == null)
					listBoxStyle = new GUIStyle("HelpBox") { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(PAD, PAD, PAD, PAD) };

				guiStyle = listBoxStyle;
			}

			if (labelwidth < 0)
				labelwidth = 64;

			EditorGUILayout.BeginVertical(guiStyle);

			EditorGUI.BeginChangeCheck();

			int cnt = listSP.arraySize;

			int delete = -1;
			int add = -1;

			for (int i = 0; i < cnt; i++)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Height(18));

				// Add [+] button
				var addbtnrect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(BTTN_WIDTH));
				if (lockZero && i != 0)
				{
					EditorGUI.BeginDisabledGroup(cnt >= maxCount);
					if (GUI.Button(addbtnrect, "+", (GUIStyle)"minibutton"))
						add = i;
					EditorGUI.EndDisabledGroup();
				}

				// Label
				EditorGUI.BeginDisabledGroup(lockZero && i == 0);
				string label = labelPrefix == null ? "[" + i + "]" : labelPrefix + " " + i;
				EditorGUILayout.LabelField(label, GUILayout.MaxWidth(labelwidth));
				EditorGUI.EndDisabledGroup();

				// Field
				SerializedProperty item = listSP.GetArrayElementAtIndex(i);

				var fieldrect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(64));
				if (lockZero && i == 0)
				{
					EditorGUI.BeginDisabledGroup(true);
					EditorGUI.LabelField(fieldrect, item.stringValue);
					EditorGUI.EndDisabledGroup();
				}
				else
				{
					if (item.propertyType == SerializedPropertyType.String)
						item.stringValue = EditorGUI.DelayedTextField(fieldrect, GUIContent.none, item.stringValue);
					else
						EditorGUILayout.PropertyField(item);
					
					// Delete [x] Button
					var delbuttonrect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(BTTN_WIDTH));
					if (GUI.Button(delbuttonrect, "X", (GUIStyle)"minibutton"))
						delete = i;
				}

				EditorGUILayout.EndHorizontal();
			}
			// space
			EditorGUILayout.GetControlRect(false, 4);

			// Bottom [add] Button
			EditorGUI.BeginDisabledGroup(cnt >= maxCount);
			if (GUI.Button(EditorGUILayout.GetControlRect(false, 20), labelPrefix != null ? "Add " + labelPrefix : "Add"))
				add = cnt;
			EditorGUI.EndDisabledGroup();

			if (add != -1)
			{
				listSP.InsertArrayElementAtIndex(add);

				if (forceUnique && !(listSP.propertyType == SerializedPropertyType.String))
					EnforceUnique(listSP, add);
				
			}

			if (delete != -1)
				listSP.DeleteArrayElementAtIndex(delete);
			
			EditorGUILayout.EndVertical();

			if (EditorGUI.EndChangeCheck())
			{
				if (forceUnique && !(listSP.propertyType == SerializedPropertyType.String))
					EnforceUnique(listSP);

				listSP.serializedObject.ApplyModifiedProperties();
			}
		}

		

		private static void EnforceUnique(SerializedProperty listSP, int indexToCheck)
		{
			int cnt = listSP.arraySize;
			var itemSP = listSP.GetArrayElementAtIndex(indexToCheck);

			bool failed;
			do
			{
				var itemString = itemSP.stringValue;
				failed = false;

				for (int i = 0; i < cnt; i++)
				{
					/// skip self
					if (i == indexToCheck)
						continue;

					/// If we have a collision, Add and X and start the test over.
					if (listSP.GetArrayElementAtIndex(i).stringValue == itemString)
					{
						itemSP.stringValue += "X";
						failed = true;
						break;
					}
				}
			} while (failed);
		}


		private static void EnforceUnique(SerializedProperty listSP)
		{
			for (int i = 0, cnt = listSP.arraySize; i < cnt; i++)
			{
				var item = listSP.GetArrayElementAtIndex(i);
				while (IsTagAlreadyUsed(listSP, item.stringValue, i))
					item.stringValue += "X";
			}
		}


		private static bool IsTagAlreadyUsed(SerializedProperty listSP, string tag, int countUpTo)
		{
			for (int i = 0; i < countUpTo; i++)
				if (listSP.GetArrayElementAtIndex(i).stringValue == tag)
					return true;

			return false;
		}
	}
}
#endif


