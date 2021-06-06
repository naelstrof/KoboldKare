// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using emotitron.Utilities.GUIUtilities;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
	public enum ReadyStateEnum { Unready, Ready, Disabled }

    //public interface ISyncObject
    //{
    //	int SyncObjIndex { get; set; }
    //	ReadyStateEnum ReadyState { get; set; }
    //       void ResetBuffers();
    //}


    /// <summary>
    /// The base class of all Simple Sync networked components.
    /// </summary>
    [HelpURL(Internal.SimpleDocsURLS.SYNCCOMPS_PATH)]
    public abstract class SyncObject : NetComponent // NetObjComponent
		, IOnEnable
		, IOnDisable
		, IApplyOrder
	{

        #region IApplyOrder Implementations

#if UNITY_EDITOR
        [ShowIfInterface(typeof(IAdjustableApplyOrder), ApplyOrderConstants.TOOLTIP, 0, ApplyOrderConstants.MAX_ORDER_VAL)]
#endif
		public int _applyOrder = ApplyOrderConstants.DEFAULT;
		public virtual int ApplyOrder { get { return _applyOrder; } }

		#endregion

		#region Inspector

		[Tooltip("Every X Net Tick this object will serialize a full update, regardless of having changed or not.")]
		[Range(1, 12)]
		[HideInInspector]
		[SerializeField]
		protected int keyframeRate = 1; // NetObjAdapter.NET_LIB == NetLibrary.PUN2 ? 1 : 0;

		[Tooltip("When enabled, components will be instructed to check for changes and serialize them. When disabled, components will be instructed to ONLY send keyframes.")]
		[HideInInspector]
		[SerializeField]
		protected bool useDeltas = true;
		public bool UseDeltas { get { return useDeltas; } set { useDeltas = value; } }
		
		#endregion Inspector

		// Cached values
		//protected static int frameCount, frameCountBits, sendEveryXTick;

		/// <summary>
		/// Checks if the supplied frame is a keyframe.
		/// </summary>
		/// <param name="frameId"></param>
		public bool IsKeyframe(int frameId)
		{
			return (keyframeRate != 0) && ((frameId % keyframeRate) == 0 || frameId == TickEngineSettings.frameCount);
		}

        public virtual void ResetBuffers() { }

		#region IReadyable Implementations

		const string ALWAYS_READY_TOOLTIP =
			"When true, the NetObject will not factor this SyncObj's ready state into firing IOnNetObjReady callbacks.";
		[ShowIfInterface(typeof(IReadyable), ALWAYS_READY_TOOLTIP)]
		public bool _alwaysReady = true;
		public virtual bool AlwaysReady { get { return _alwaysReady; } }

		#endregion

		#region ISyncOptional Implementation

		const string INCLUDE_SERIALIZATION_TOOLTIP =
			"When false, the NetObject will not serialize this SyncObj's state. This cannot be changed at runtime. Disable this component on the owner to disable sync at runtime instead.";
		//[ShowIfInterface(typeof(ISerializationOptional), INCLUDE_SERIALIZATION_TOOLTIP)]
		[Tooltip(INCLUDE_SERIALIZATION_TOOLTIP)]
		[HideInInspector] public bool serializeThis = true;
		public virtual bool IncludeInSerialization { get { return serializeThis; } }
		
		#endregion

		public bool SkipWhenEmpty { get { return false; } }

		[System.NonSerialized]
		protected int syncObjIndex;
		public int SyncObjIndex { get { return syncObjIndex; } set { syncObjIndex = value; } }
		
		#region Readyable

		protected ReadyStateEnum _readyState;
		public ReadyStateEnum ReadyState
		{
			get { return _readyState; }
			set
			{
				///// The ready check is to eliminate trying to sort out race conditions for startup. All startup changes to ReadyState will trigger callbacks.
				if (_readyState == value /*&& ReadyState == ReadyStateEnum.Ready*/)
					return;

                //if (value == ReadyStateEnum.Ready)
                //    if (GetComponent<SyncPickup>())
                //        Debug.Log(Time.time + "<color=orange> Ready change </color>" + name + " " + this.GetType().Name + " " + _readyState + "->" + value
                //            + " parent: <b>" + (transform.parent ? transform.parent.name : "none") + " y: " + transform.localPosition.y + "</b>");

                _readyState = value;

				/// Notify netObj of change in state
				netObj.OnSyncObjReadyChange(this, _readyState);

				/// Notify listeners of change in ready state
				if (onReadyCallbacks != null)
					onReadyCallbacks.Invoke(this, value);
			}
		}

		#endregion

		public System.Action<SyncObject, ReadyStateEnum> onReadyCallbacks;
		
		public override void OnPostEnable()
		{
			base.OnPostEnable();
			///// IReadyable objects start life Unready by default. Non IReadyable objects will never be Unready.
			//var iReadyable = this as IReadyable;

			//if (ReferenceEquals(iReadyable, null) || iReadyable.AlwaysReady /*|| _readyState != ReadyStateEnum.Unready*/)
			//	ReadyState = ReadyStateEnum.Ready;
			//else
			//	ReadyState = ReadyStateEnum.Unready;

		}

		
		public override void OnAuthorityChanged(bool isMine, bool controllerChanged)
		{
			base.OnAuthorityChanged(isMine, controllerChanged);

            if (!controllerChanged)
                return;

			/// IReadyable objects start life Unready by default. Non IReadyable objects will never be Unready.
			var iReadyable = this as IReadyable;

			if (ReferenceEquals(iReadyable, null) || iReadyable.AlwaysReady || _readyState != ReadyStateEnum.Unready)
				ReadyState = ReadyStateEnum.Ready;
			else if (!isActiveAndEnabled)
				ReadyState = ReadyStateEnum.Disabled;
			else
				ReadyState = ReadyStateEnum.Unready;
		}

		public override void OnPostDisable()
		{
			base.OnPostDisable();
			/// Disabled SyncObjs should be considered ready, so they don't hold up the visibility endlessly
			ReadyState = ReadyStateEnum.Disabled;

		}


#if UNITY_EDITOR

		protected override void OnValidate()
		{
			base.OnValidate();

			/// Be sure that keyframe rate is 1 for objects not using keyframe. May not be needed, but still defining this spec.
			if ((this is IUseKeyframes) == false)
			{
				if (keyframeRate != 1)
				{
					keyframeRate = 1;
				}
			}
		}

#endif

		#region Initialization / Shutdown

		/// <summary>
		/// Be sure to use base.OnAwake() when overriding. 
		/// This is called when the NetObject runs Awake(). All code that depends on the NetObj being initialized should use this
		/// rather than Awake();
		/// </summary>
		public override void OnAwake()
		{
			base.OnAwake();
			////netObjIsAwake = true;
			//frameCount = SimpleSyncSettings.frameCount;
			//frameCountBits = SimpleSyncSettings.frameCountBits;
			//sendEveryXTick = SimpleSyncSettings.sendEveryXTick;
		}

		/// This is here to ensure the enable checkbox visible for the Component
		protected virtual void OnEnable()
		{
			
		}

		#endregion

		protected int ConvertSecsToTicks(float seconds)
		{
			return (int)(seconds / (Time.fixedDeltaTime * TickEngineSettings.sendEveryXTick))/* - 1*/;
		}
	}


#if UNITY_EDITOR
	[CustomEditor(typeof(SyncObject), isFallback = false)]
	[CanEditMultipleObjects]
	public class SyncObjectEditor : HeaderEditorBase
	{
        protected override string HelpURL
        {
            get
            {
                return Internal.SimpleDocsURLS.SYNCCOMPS_PATH;
            }
        }

        protected readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();

		protected override string TextTexturePath { get { return "Header/SyncObjectText"; } }
        //protected override string TextFallbackPath { get { return "Header/SyncObjectText"; } }

        protected override string BackTexturePath  { get { return "Header/BlueBack"; } }
		//protected override string BackFallbackPath { get { return "Header/BlueBack"; } }

        protected override string IconTexturePath { get { return "Header/PUN_IconBlue"; } }



        protected override string GridTexturePath { get { return null; } }

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}

		protected override void OnInspectorGUIInjectMiddle()
		{
			base.OnInspectorGUIInjectMiddle();


			var serializeThis = serializedObject.FindProperty("serializeThis");
			bool serializationIsOptional = target is ISerializationOptional;

			if (serializationIsOptional)
			{
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(serializeThis);

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }


            IUseKeyframes useKeyframes = target as IUseKeyframes;
			if (!ReferenceEquals(useKeyframes, null) && (!serializationIsOptional || serializeThis.boolValue))
			{
                EditorGUI.BeginChangeCheck();

                var keyframeRate = serializedObject.FindProperty("keyframeRate");

				int maxkeyval = TickEngineSettings.MaxKeyframes;

				if (keyframeRate.intValue > maxkeyval)
				{
					keyframeRate.intValue = maxkeyval;
					serializedObject.ApplyModifiedProperties();
				}

				EditorGUILayout.IntSlider(keyframeRate, 0, TickEngineSettings.MaxKeyframes);

				/// If also includes control for handling delta frame change detection
				IDeltaFrameChangeDetect useDeltaframes = target as IDeltaFrameChangeDetect;
				if (!ReferenceEquals(useDeltaframes, null))
				{
					var useDeltas = serializedObject.FindProperty("useDeltas");
					EditorGUILayout.PropertyField(useDeltas);
				}

				if (EditorGUI.EndChangeCheck())
				{
					serializedObject.ApplyModifiedProperties();
				}
			}

			
		}

		private static Dictionary<object, bool> foldoutStates = new Dictionary<object, bool>();
		protected void ListFoundInterfaces<T>(GameObject go, List<T> list) where T : class
		{
			go.GetComponentsInChildren(list);

			reusableGC.text = "[" + list.Count + "] " + typeof(T).Name + " found:";
			reusableGC.tooltip = "Callbacks found on this NetObject";

			if (!foldoutStates.ContainsKey(list))
				foldoutStates.Add(list, false);

			foldoutStates[list] = EditorGUILayout.Foldout(foldoutStates[list], reusableGC);

			if (foldoutStates[list])
			{
				EditorGUILayout.BeginVertical(new GUIStyle("HelpBox"));

				if (list.Count == 0)
					EditorGUILayout.LabelField("<i>none</i>", richBox);
				else
				{
					EditorGUI.BeginDisabledGroup(true);
					for (int i = 0; i < list.Count; ++i)
					{
						EditorGUILayout.ObjectField(list[i] as Component, typeof(Component), false);
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUILayout.EndVertical();
			}
		}
	}
#endif

}

