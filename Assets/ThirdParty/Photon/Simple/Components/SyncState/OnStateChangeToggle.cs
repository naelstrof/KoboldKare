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

	public enum DisplayToggle { GameObject, Component, Renderer }

	public class OnStateChangeToggle : NetComponent
	, IOnStateChange
	{

		[HideInInspector]
		[Tooltip("How this object should be toggled. GameObject toggles gameObject.SetActive(), Renderer toggles renderer.enabled, and Component toggles component.enabled.")]
		public DisplayToggle toggle = DisplayToggle.GameObject;

		[Tooltip("User specified component to toggle enabled.")]
		[HideInInspector]
		public Component component;

		[HideInInspector]
		public GameObject _gameObject;

		[HideInInspector]
		public Renderer _renderer;

		[HideInInspector]
		public ObjStateLogic stateLogic = new ObjStateLogic();

		// Cached
		bool reactToAttached;
		MonoBehaviour monob;

		private bool show;

#if UNITY_EDITOR

		[HideInInspector]
        [Utilities.VersaMask(true, typeof(ObjStateEditor))]
        public ObjState currentState = (ObjState)(-1);


		protected override void Reset()
		{
			_gameObject = gameObject;
			_renderer = GetComponent<Renderer>();
		}

#endif
		public override void OnAwake()
		{
			base.OnAwake();
		
			if (toggle == DisplayToggle.Renderer)
			{
				if (_renderer == null)
					_renderer = GetComponent<Renderer>();
			}
			else if (toggle == DisplayToggle.Component)
			{
				monob = component as MonoBehaviour;
			}
			else
			{
				if (_gameObject == null)
					_gameObject = gameObject;
			}

			stateLogic.RecalculateMasks();

			reactToAttached = (((stateLogic.notMask & (int)ObjState.Mounted) == 0) && (stateLogic.stateMask & (int)ObjState.Mounted) != 0);

		}
		
		public void OnStateChange(ObjState newState, ObjState previousState, Transform pickup, Mount attachedTo = null, bool isReady = true)
		{

            //Debug.Log(transform.root.name + ":" + name + " " + photonView.ControllerActorNr + " " + state + " " + attachedTo);
#if UNITY_EDITOR
            currentState = newState;
#endif
			if (!isReady)
			{
                //Debug.Log(transform.root.name + ":" + name + " not ready!");
                show = false;
			}
			else
			{
				bool match = stateLogic.Evaluate((int)newState);

				if (match)
				{
					show = true;

                    /// If there is no object to attach to yet (due to keyframes) we need to keep this invisible.
                    if (reactToAttached)
                        if (attachedTo == null && (newState & ObjState.Mounted) != 0)
                            show = false;
                }
				else
					show = false;
			}

            //if (!photonView.IsMine)
            //    Debug.Log(photonView.ViewID + " " + photonView.name + " : " + GetType().Name + " " 
            //        + state +  " show: " + show + " match: " + stateLogic.Evaluate((int)state));
            DeferredEnable();
            //NetMasterCallbacks.postCallbackActions.Enqueue(DeferredEnable);
		}

		private void DeferredEnable()
		{
			switch (toggle)
			{

				case DisplayToggle.GameObject:
					{
						_gameObject.SetActive(show);
						break;
					}

				case DisplayToggle.Component:
					{
						if (monob)
							monob.enabled = show;
						break;
					}

				case DisplayToggle.Renderer:
					{
						if (_renderer)
							_renderer.enabled = show;
						break;
					}
			}
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(OnStateChangeToggle))]
	[CanEditMultipleObjects]
	public class OnStateChangeToggleEditor : StateReactorHeaderEditor
	{

		protected override string Instructions
		{
			get { return "Ties object toggles to OnStateChange callbacks."; }
		}

		protected int[] stateValues = (int[])System.Enum.GetValues(typeof(ObjState));
		protected string[] stateNames = System.Enum.GetNames(typeof(ObjState));

		protected SerializedProperty stateMask, notMask;
		protected SerializedProperty toggle, operation, currentState;

		public override void OnEnable()
		{
			base.OnEnable();
			stateMask = serializedObject.FindProperty("stateMask");
			notMask = serializedObject.FindProperty("notMask");
			toggle = serializedObject.FindProperty("toggle");
			operation = serializedObject.FindProperty("operation");
			currentState = serializedObject.FindProperty("currentState");
			currentState.isExpanded = true;
		}

		public override void OnInspectorGUI()
		{
			
			base.OnInspectorGUI();
			var _target = target as OnStateChangeToggle;

			

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(toggle);
			
			if (toggle.enumValueIndex == (int)DisplayToggle.Component)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("component"));
			else if (toggle.enumValueIndex == (int)DisplayToggle.GameObject)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_gameObject"));
			else if (toggle.enumValueIndex == (int)DisplayToggle.Renderer)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_renderer"));

			_target.stateLogic.DrawGUI(serializedObject.FindProperty("stateLogic"));


			if (_target.GetComponent<NetObject>())
				EditorGUILayout.HelpBox("<b>NetObject detected on this GameObject!</b>\n\nThis component OnPickup will disable the entire net object (including the respawn timer), which is likely unintentional." +
					" Make the NetObject root an empty object and put the mesh and this component on a child instead, so that networked object remains active.", MessageType.Warning);

			if (Application.isPlaying)
			{
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(currentState);
                EditorGUI.EndDisabledGroup();
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
		}
	}
#endif
}
