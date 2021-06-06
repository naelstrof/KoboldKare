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
	public class AutoOwnerComponentEnable : NetComponent
		, IOnAuthorityChanged
	{
		public enum EnableIf { Ignore, Owner, Other }

		[System.Serializable]
		public class ComponentToggle
		{
			public Behaviour component;
			public EnableIf enableIfOwned = EnableIf.Owner;
		}

		public bool includeChildren = true;
		public bool includeUnity = true;
		public bool includePhoton = false;
		public bool includeSimple = false;

#pragma warning disable 0414
        [HideInInspector] [SerializeField] private List<ComponentToggle> componentToggles = new List<ComponentToggle>();
		[HideInInspector] [SerializeField] private List<Behaviour> componentLookup = new List<Behaviour>();
#pragma warning restore 0414

#if UNITY_EDITOR
        protected override void Reset()
		{
			base.Reset();
			componentToggles.Clear();
			FindUnrecognizedComponents();
		}
#endif

		public override void OnStart()
		{
			base.OnStart();

			SwitchAuth(IsMine);
		}

		public override void OnJoinedRoom()
		{
			base.OnJoinedRoom();
			SwitchAuth(IsMine);
		}

		public override void OnAuthorityChanged(bool isMine, bool controllerChanged)
		{
			base.OnAuthorityChanged(isMine, controllerChanged);
			SwitchAuth(isMine);
		}

		private void SwitchAuth(bool isMine)
		{


			for (int i = 0; i < componentToggles.Count; ++i)
			{
				var item = componentToggles[i];

				if (item != null && item.enableIfOwned != EnableIf.Ignore && item.component != null)
				{
					item.component.enabled = (item.enableIfOwned == EnableIf.Owner) ? isMine : !isMine;
				}
			}

		}

#if UNITY_EDITOR

		private static List<Behaviour> components = new List<Behaviour>();
		public virtual void FindUnrecognizedComponents()
		{
			/// Cull any null components in the list
			for (int i = componentLookup.Count - 1; i >= 0; --i)
			{
				var comp = componentLookup[i];

				/// Remove null comps, or comps that are children when mode is not children
				if (comp == null || (!includeChildren && comp.gameObject != gameObject))
				{
					if (componentLookup.Contains(comp))
					{
						componentLookup.RemoveAt(i);
						componentToggles.RemoveAt(i);
					}
				}
			}

#if PUN_2_OR_NEWER

            if (includeChildren)
				transform.GetNestedComponentsInChildren<Behaviour, NetObject>(components);
			else
				GetComponents<Behaviour>(components);

#endif

            foreach (var comp in components)
			{
				if (comp == null)
					continue;

				var nspace = comp.GetType().Namespace;
				var fullname = comp.GetType().FullName;

				bool itemize;
				EnableIf defaultEnableIf;

				if (nspace == null || nspace == "")
				{
					itemize = true;
					defaultEnableIf = EnableIf.Owner;
				}
				else
				{
					defaultEnableIf = EnableIf.Owner;

					if (comp is AutoOwnerComponentEnable)
					{
						itemize = false;
					}
					else if (comp is NetObject)
					{
						itemize = false;
					}
#if PUN_2_OR_NEWER
					else if (comp is PhotonView)
					{
						itemize = false;
					}
#endif
					else if (nspace.StartsWith("UnityEngine"))
					{
						itemize = includeUnity;
						defaultEnableIf = EnableIf.Ignore;
					}
					else if (nspace.StartsWith("Photon."))
					{
						itemize = includePhoton;
						defaultEnableIf = EnableIf.Ignore;
					}
					else if (nspace.StartsWith("emotitron."))
					{
						itemize = includeSimple;
						defaultEnableIf = EnableIf.Ignore;
					}
					else
					{
						itemize = true;
					}

				}

				if (itemize)
				{
					if (!componentLookup.Contains(comp))
					{
						componentToggles.Add(new ComponentToggle() { component = comp, enableIfOwned = defaultEnableIf });
						componentLookup.Add(comp);
					}
				}
				else
				{
					if (componentLookup.Contains(comp))
					{
						componentToggles.RemoveAt(componentLookup.IndexOf(comp));
						componentLookup.Remove(comp);
					}
				}
			}
		}
#endif
                }


#if UNITY_EDITOR
        [CustomEditor(typeof(AutoOwnerComponentEnable))]
	internal class AutoOwnerComponentEnableEditor : AccessoryHeaderEditor
	{
		SerializedProperty componentToggles;

		protected override string Instructions
		{
			get
			{
				return "Automatically enables and disables components on a Net Object based on ownership. " +
					"Use this to disable controller code on non-authority instances " +
					"(which is a requirement for networking).";
			}
		}

		protected bool isExpanded = true;

		public override void OnEnable()
		{
			base.OnEnable();
			componentToggles = serializedObject.FindProperty("componentToggles");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			(target as AutoOwnerComponentEnable).FindUnrecognizedComponents();
			serializedObject.Update();

			RenderList(componentToggles);
		}

		private void RenderList(SerializedProperty list)
		{
			int cnt = list.arraySize;
			if (cnt == 0)
			{
				EditorGUILayout.LabelField("[0] Components Found");
				return;
			}

			GUIContent foldoutlabel = new GUIContent("[" + cnt + "] Components Found");

			var headerrect = EditorGUILayout.GetControlRect(true, 18);
			//headerrect.xMin += 12;

			isExpanded = EditorGUI.Foldout(headerrect, isExpanded, foldoutlabel, (GUIStyle)"Foldout");
			//EditorGUI.LabelField(headerrect, "[" + cnt + "] Component" + ((cnt == 1) ? ":" : "s:"), new GUIStyle() { padding = new RectOffset(6, 6, 2, 6) });
			EditorGUI.LabelField(headerrect, "Enable If:", new GUIStyle() { padding = new RectOffset(6, 6, 2, 6), alignment = TextAnchor.UpperRight });

			if (isExpanded)
			{
				EditorGUILayout.BeginVertical(new GUIStyle("HelpBox"));

				int deleteIndex = -1;


				for (int i = 0; i < cnt; ++i)
				{
					var listitem = list.GetArrayElementAtIndex(i);
					var enableIfOwned = listitem.FindPropertyRelative("enableIfOwned");
					var comp = listitem.FindPropertyRelative("component");

					//EditorGUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(), margin = new RectOffset() });

					/// Row One
					{
						bool notused = enableIfOwned.intValue == (int)AutoOwnerComponentEnable.EnableIf.Ignore;

						EditorGUILayout.BeginHorizontal(new GUIStyle() { fixedHeight = 18  });
                        {

                            var obj = comp.objectReferenceValue;

                            EditorGUI.BeginDisabledGroup(notused);
                            {
                                if (obj)
                                    EditorGUILayout.LabelField(comp.objectReferenceValue.GetType().Name, GUILayout.MinWidth(48));
                                else
                                    EditorGUILayout.LabelField("none", GUILayout.MinWidth(48));
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUI.BeginChangeCheck();

                            EditorGUILayout.PropertyField(enableIfOwned, GUIContent.none, GUILayout.MaxWidth(64), GUILayout.MinWidth(54));

                            if (EditorGUI.EndChangeCheck())
                            {
                                serializedObject.ApplyModifiedProperties();
                            }

                        }
                        EditorGUILayout.EndHorizontal();
					}

					//EditorGUILayout.EndVertical();
				}

				if (deleteIndex != -1)
				{
					Undo.RecordObject(target, "Delete List Item " + deleteIndex);
					list.DeleteArrayElementAtIndex(deleteIndex);
					serializedObject.ApplyModifiedProperties();
				}

				EditorGUILayout.EndVertical();

			}
		}
	}
#endif
}
