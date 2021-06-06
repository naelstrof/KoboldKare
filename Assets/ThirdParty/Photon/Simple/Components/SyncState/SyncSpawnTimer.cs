// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Utilities;
using Photon.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{

    /// <summary>
    /// Indicates component controls the spawning and despawning of an object. SyncState uses this to check respawn/despawn conditions before applying state changes.
    /// </summary>
    public interface ISpawnController
    {
        //ObjState SupressStateMask { get; }
        bool AllowNetObjectReadyCallback(bool ready);
    }

	/// <summary>
	/// Automatically generates ChangeState events on the SyncState based on timers and triggers.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(SyncState))]
	public class SyncSpawnTimer : SyncObject<SyncSpawnTimer.Frame>
		, ISpawnController
        , ISerializationOptional
        , IOnNetSerialize
        , IUseKeyframes
		, IOnSnapshot
		, IOnCaptureState
		, IOnStateChange
	{

		public override int ApplyOrder { get { return ApplyOrderConstants.STATE_TIMER; } }

        #region Inspector

        [HideInInspector] [SerializeField]
        public float initialDelay = 0;

		[HideInInspector] [SerializeField] public bool respawnEnable = true;
		[EnumMask(true, typeof(ObjStateEditor))]
		[HideInInspector] [SerializeField] public ObjState despawnOn = ObjState.Mounted;
		[Tooltip("Number of seconds after respawn trigger before respawn occurs.")]
		[HideInInspector] [SerializeField] public float despawnDelay = 5f;


		[HideInInspector] [SerializeField] public bool despawnEnable = false;
		[EnumMask(true, typeof(ObjStateEditor))]
		[HideInInspector] [SerializeField] public ObjState respawnOn = ObjState.Despawned;
		[Tooltip("Number of seconds after respawn trigger before respawn occurs.")]
		[HideInInspector] [SerializeField] public float respawnDelay = 5f;

        //[EnumMask(true, typeof(ObjStateEditor))]
        //public ObjState suppressionMask = ObjState.Visible | ObjState.Dropped | ObjState.Transit;

		#endregion Inspector

		protected SyncState syncState;

#if UNITY_EDITOR
		protected override void Reset()
		{
			base.Reset();
			serializeThis = false;
		}
#endif

		// Current state
		[System.NonSerialized] protected int ticksUntilRespawn = -1;
		[System.NonSerialized] protected int ticksUntilDespawn = -1;

        // Cached values
        [System.NonSerialized] protected int spawnWaitAsTicks;
        [System.NonSerialized] protected int respawnWaitAsTicks;
		[System.NonSerialized] protected int despawnWaitAsTicks;
		[System.NonSerialized] protected bool hadInitialSpawn;

        protected int bitsForTicksUntilRespawn;
		protected int bitsForTicksUntilDespawn;


        #region Frame

        public class Frame : FrameBase
		{
			public int ticksUntilRespawn;
			public int ticksUntilDespawn;

			public Frame() : base() { }

			public Frame(int frameId) : base(frameId) { }

			public override void CopyFrom(FrameBase sourceFrame)
			{
				base.CopyFrom(sourceFrame);
				Frame src = sourceFrame as Frame;
				ticksUntilRespawn = src.ticksUntilRespawn;
				ticksUntilDespawn = src.ticksUntilDespawn;
			}

			public bool Compare(Frame otherFrame)
			{
				return 
					ticksUntilRespawn == otherFrame.ticksUntilRespawn &&
					ticksUntilDespawn == otherFrame.ticksUntilDespawn;
			}
		}

		#endregion Frame

		#region Startup

		public override void OnAwake()
		{
			base.OnAwake();

			if (netObj)
				syncState = netObj.GetComponent<SyncState>();

            //if (respawnEnable)
            //    syncState.respawnState = ObjState.Despawned;

            spawnWaitAsTicks = ConvertSecsToTicks(initialDelay);
			respawnWaitAsTicks = ConvertSecsToTicks(respawnDelay);
			despawnWaitAsTicks = ConvertSecsToTicks(despawnDelay);

			bitsForTicksUntilRespawn = System.Math.Max(respawnWaitAsTicks, spawnWaitAsTicks).GetBitsForMaxValue();
			bitsForTicksUntilDespawn = despawnWaitAsTicks.GetBitsForMaxValue();

		}

        /// <summary>
        /// Test used by SyncState to see if the OnNetObjReady callback should trigger the default states. Will return false if the spawn timer is still counting down, indicating that
        /// SyncState should not fire the default state change for Ready.
        /// </summary>
        /// <param name="ready"></param>
        /// <returns></returns>
        public bool AllowNetObjectReadyCallback(bool ready)
        {
            // Returning false indicates that the object normal response to a ready should not happen.
            if (ready && spawnWaitAsTicks >= 0)
                return false;

            return true;
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            if (photonView.IsMine)
            {
                //supressStateMask = spawnWaitAsTicks < 0 ? ~ObjState.Despawned : ~suppressionMask; ;
                ticksUntilRespawn = spawnWaitAsTicks;
            }
            else
            {
                //supressStateMask = ~ObjState.Despawned;
                ticksUntilRespawn = -1;
            }

        }
        public override void OnStart()
		{
			base.OnStart();

            ticksUntilRespawn = spawnWaitAsTicks;

            //Debug.Log(name + " " + photonView.ControllerActorNr + " countdown: <b>" + ticksUntilRespawn + "</b> " + hadInitialSpawn);
        }

		#endregion Startup

		//protected ObjState prevState = ObjState.Despawned;
		/// <summary>
		/// Responds to State change from SyncState
		/// </summary>
		public void OnStateChange(ObjState newState, ObjState previousState, Transform attachmentTransform, Mount attachTo = null, bool isReady = true)
		{
			
			if (newState == previousState)
				return;

			//if (IsMine)
			{
				if (/*hadInitialSpawn && */respawnEnable)
				{
					if (newState == ObjState.Despawned)
					{
                        //Debug.Log(Time.time + " " + name + " " + photonView.OwnerActorNr + " <b>Despawned State Chage</b>");

                        ticksUntilRespawn = respawnWaitAsTicks;
					}
					/// Check if the flag we are looking for just changed to true
					else if ((previousState & respawnOn) == 0 && (newState & respawnOn) != 0)
					{
                        Debug.Log(Time.time + " " + name + " " + photonView.OwnerActorNr + " <b>Reset </b> " + previousState + " <> " + newState);
                        ticksUntilRespawn = respawnWaitAsTicks;
					}
				}

				if (despawnEnable)
				{

					/// Check if the flag we are looking for just changed to true
					if ((previousState & despawnOn) == 0 && (newState & despawnOn) != 0)
					{
						ticksUntilDespawn = despawnWaitAsTicks;
					}
				}
			}
			

			//prevState = state;

		}

        //protected ObjState supressStateMask;
        //public ObjState SupressStateMask { get { return supressStateMask; } }

		public virtual void OnCaptureCurrentState(int frameId)
		{
            //if (GetComponent<SyncPickup>())
            //    Debug.LogError(name + " " + ticksUntilRespawn);

            Frame frame = frames[frameId];
            //Debug.Log(name + " " + photonView.ControllerActorNr + " countdown: <b>" + ticksUntilRespawn + "</b> " + hadInitialSpawn);
			/// First check for a respawn - this may belong in post or pre sim, but here for now
			if (!hadInitialSpawn || respawnEnable)
			{
				if (ticksUntilRespawn == 0)
				{
					syncState.Respawn(false);
                    hadInitialSpawn = true;
				}
				ticksUntilRespawn--;

				frame.ticksUntilRespawn = ticksUntilRespawn;
			}

			if (despawnEnable)
			{
				if (ticksUntilDespawn == 0)
				{
                    syncState.Despawn(false);
                }
                ticksUntilDespawn--;
				frame.ticksUntilDespawn = ticksUntilDespawn;
			}

		}

        #region Serialization

        public virtual SerializationFlags OnNetSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
        {
            
            /// TODO: This is ignoring keyframe setting

            Frame frame = frames[frameId];
            SerializationFlags flags = SerializationFlags.None;

            bool iskeyframe = IsKeyframe(frameId);

            if (!iskeyframe)
                return flags;

            if (respawnEnable)
            {
                /// Respawn
                int ticks = frame.ticksUntilRespawn;

                if (ticks >= 0)
                {
                    /// non -1 counter bool
                    buffer.WriteBool(true, ref bitposition);
                    buffer.Write((ulong)ticks, ref bitposition, bitsForTicksUntilRespawn);
                    flags |= SerializationFlags.HasContent;
                }
                else
                {
                    /// non -1 counter bool
                    buffer.WriteBool(false, ref bitposition);
                }
            }

            if (despawnEnable)
            {
                /// Despawn
                int ticks = frame.ticksUntilDespawn;
                if (ticks >= 0)
                {
                    /// non -1 counter bool
                    buffer.WriteBool(true, ref bitposition);
                    buffer.Write((ulong)ticks, ref bitposition, bitsForTicksUntilDespawn);

                    flags |= SerializationFlags.HasContent;
                }
                else
                {
                    /// non -1 counter bool
                    buffer.WriteBool(false, ref bitposition);
                }
            }

            return flags;
        }

        public SerializationFlags OnNetDeserialize(int originFrameId, byte[] buffer, ref int bitposition, FrameArrival frameArrival)
        {
           
            Frame frame = frames[originFrameId];
            SerializationFlags flags = SerializationFlags.None;

            bool iskeyframe = IsKeyframe(originFrameId);
            if (!iskeyframe)
            {
                frame.content = FrameContents.Empty;
                return flags;
            }

            if (respawnEnable)
            {
                /// Read ticksToRespawn
                if (buffer.ReadBool(ref bitposition))
                {
                    frame.ticksUntilRespawn = (int)buffer.Read(ref bitposition, bitsForTicksUntilRespawn);
                    flags |= SerializationFlags.HasContent;
                }
                else
                    frame.ticksUntilRespawn = -1;
            }

            if (despawnEnable)
            {
                if (buffer.ReadBool(ref bitposition))
                {
                    frame.ticksUntilDespawn = (int)buffer.Read(ref bitposition, bitsForTicksUntilDespawn);
                    flags |= SerializationFlags.HasContent;
                }
                else
                    frame.ticksUntilDespawn = -1;
            }

            frame.content = FrameContents.Complete;
            return flags;
        }

        #endregion Serialization

        protected override void ApplySnapshot(Frame snapframe, Frame targframe, bool snapIsValid, bool targIsValid)
		{
			/// Apply frame, otherwise predict respawn if we didn't get the frame where it would have happened.
			if (snapIsValid && snapframe.content > FrameContents.Empty)
            {
                //Debug.Log("snap change  tr: " + snapFrame.ticksUntilRespawn + " td:" + snapFrame.ticksUntilDespawn);
                if (respawnEnable)
                    ticksUntilRespawn = snapframe.ticksUntilRespawn;
                if (despawnEnable)
                    ticksUntilDespawn = snapframe.ticksUntilDespawn;
            }
			else
            {
                //Debug.Log("snap no change");
                if (respawnEnable)
                {
                    ticksUntilRespawn--;
                    targframe.ticksUntilRespawn = ticksUntilRespawn;
                }
                if (despawnEnable)
                {
                    ticksUntilDespawn--;
                    targframe.ticksUntilDespawn = ticksUntilDespawn;
                }
            }

            if (photonView.IsMine)
            {
                if (respawnEnable && ticksUntilRespawn == 0)
                {
                    syncState.Respawn(false);
                    //Debug.Log("snap change  tr: " + snapFrame.ticksUntilRespawn + " td:" + snapFrame.ticksUntilDespawn);
                }

                if (despawnEnable && ticksUntilDespawn == 0)
                {
                    syncState.Despawn(false);
                    //Debug.Log("snap change  tr: " + ticksUntilRespawn + ":" + snapFrame.ticksUntilRespawn + " td: " + ticksUntilRespawn + " " + snapFrame.ticksUntilDespawn);
                }

            }
        }
    }

#if UNITY_EDITOR

	[CustomEditor(typeof(SyncSpawnTimer))]
	[CanEditMultipleObjects]
	public class SyncSpawnTimerEditor : SyncObjectEditor
	{

		protected override string HelpURL
		{
			get { return Internal.SimpleDocsURLS.SYNCCOMPS_PATH + "#syncspawntimer_component"; }
		}
		
		protected override bool UseThinHeader { get { return true; } }

		protected override string Instructions
		{
			get
			{
				return "Responds to " + typeof(IOnStateChange).Name + " callbacks, and produces Spawn/Despawn calls to " + typeof(SyncState).Name + ".";
			}
		}

		protected const int BOX_PAD = 4;
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (spawnBox == null)
				spawnBox = new GUIStyle("HelpBox") { padding = new RectOffset(BOX_PAD, BOX_PAD, BOX_PAD, BOX_PAD) };

			EditorGUI.BeginChangeCheck();


            EditorGUILayout.BeginHorizontal();
            SerializedProperty initialDelay = serializedObject.FindProperty("initialDelay");
            EditorGUILayout.PropertyField(initialDelay, GUILayout.MinWidth(42));
            EditorGUILayout.LabelField(new GUIContent("Secs"), GUILayout.Width(42));
            EditorGUILayout.EndHorizontal();

            DrawBox(new GUIContent("Respawn Trigger"), serializedObject.FindProperty("respawnEnable"), serializedObject.FindProperty("respawnOn"), serializedObject.FindProperty("respawnDelay"));
			DrawBox(new GUIContent("Despawn Trigger"), serializedObject.FindProperty("despawnEnable"), serializedObject.FindProperty("despawnOn"), serializedObject.FindProperty("despawnDelay"));

			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}

		protected static GUIStyle spawnBox;

		protected virtual void DrawBox(GUIContent label, SerializedProperty enabled, SerializedProperty p, SerializedProperty delay)
		{
			var lwidth = GUILayout.MaxWidth((EditorGUIUtility.labelWidth - BOX_PAD) - 4);

			EditorGUILayout.BeginVertical(spawnBox);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label, lwidth);
			EditorGUILayout.PropertyField(enabled, GUIContent.none, GUILayout.MinWidth(42));
			EditorGUILayout.EndHorizontal();

			if (enabled.boolValue)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(new GUIContent("Trigger On", p.tooltip), lwidth);
				EditorGUILayout.PropertyField(p, GUIContent.none);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(new GUIContent("Delay", delay.tooltip), lwidth);
				EditorGUILayout.PropertyField(delay, GUIContent.none, GUILayout.MinWidth(42));
				EditorGUILayout.LabelField(new GUIContent("Secs"), GUILayout.Width(42));
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
		}

	}
#endif
}
