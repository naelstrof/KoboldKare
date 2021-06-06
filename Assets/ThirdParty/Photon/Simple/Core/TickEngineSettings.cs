// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Photon.Pun.Simple
{
#if UNITY_EDITOR
	[HelpURL(Internal.SimpleDocsURLS.TICK_SETTINGS)]
#endif
	public class TickEngineSettings : SettingsScriptableObject<TickEngineSettings>
	{

#if UNITY_EDITOR

		public override string HelpURL { get { return Internal.SimpleDocsURLS.TICK_SETTINGS; } }

		public static string instructions = "";
#endif

		public enum FrameCountEnum { FrameCount12 = 12, FrameCount30 = 30, FrameCount60 = 60, FrameCount120 = 120 }
		public enum BufferCorrection { Manual, Auto }
		public enum LogInfoLevel { All, WarningsAndErrors, ErrorsOnly, None }

		[Space]
		[Tooltip("Enable/Disable for the NetMaster timing callbacks. This needs to be enabled for networking to work.")]
		public bool enableTickEngine = true;


		[Tooltip("Disable to pause sending Updates to Relay if no other Players are in the current Room. Default is false to save on data usage. Enable if your code requires updates to echo back to controller.")]
        public bool sendWhenSolo = false;

        [Header("Debugging")]

		[SerializeField]
		private LogInfoLevel logLevel = LogInfoLevel.WarningsAndErrors;
		public static LogInfoLevel LogLevel { get { return Single.logLevel; } }

		[Tooltip("The size of the circular buffer.")]
		[SerializeField]
		[HideInInspector]
		private FrameCountEnum _frameCount = FrameCountEnum.FrameCount30;
		public static int frameCount;

		[SerializeField]
		[HideInInspector]
		private BufferCorrection _bufferCorrection = BufferCorrection.Manual;

		[Tooltip("Target size of the frame buffer. This is the number of frames in the buffer that is considered ideal. " +
			"The lower the number the less latency, but with a greater risk of buffer underruns that lead to extrapolation/hitching.")]
		[SerializeField] [HideInInspector] private int _targetBufferSize = 2;
		//public static int TargetBufferSize { get { return Single._targetBufferSize; } }
		public static int targetBufferSize;

		[Tooltip("Buffer sizes above this value wll be considered to be excessive, and will trigger multiple frames being processed to shrink the buffer.")]
		[SerializeField] [HideInInspector] private int _maxBufferSize = 3;
		public static int maxBufferSize;

		[Tooltip("Buffer sizes below this value will trigger the frames to hold for extra ticks in order to grow the buffer.")]
		[SerializeField] [HideInInspector] private int _minBufferSize = 1;
		public static int minBufferSize;

		[Tooltip("The number of ticks a buffer will be allowed to be below the the Min Buffer Size before starting to correct. " +
			"This value prevents overreaction to network hiccups and allows for a few ticks before applying harsh corrections. Ideally this value will be larger than Ticks Before Shrink.")]
		[SerializeField] [HideInInspector] private int _ticksBeforeGrow = 8;
		public static int ticksBeforeGrow;

		[Tooltip("The number of ticks a buffer will be allowed to exceed Max Buffer Size before starting to correct. " +
			"This value prevents overreaction to network hiccups and allows for a few ticks before applying harsh corrections. Ideally this value will be smaller than Ticks Before Grow.")]
		[SerializeField] [HideInInspector] private int _ticksBeforeShrink = 5;
		public static int ticksBeforeShrink;


		[Tooltip("States are sent post PhysX/FixedUpdate. Setting this to a value greater than one reduces these sends by only sending every X fixed tick.\n1 = Every Tick\n2 = Every Other\n3 = Every Third, etc.")]
		[SerializeField]
		[HideInInspector]
		private int _sendEveryXTick = 3;
		public static int sendEveryXTick = 3;
		//public static int SendEveryXTick { get { return Single.sendEveryXTick; } }

		[Space(4)]
#if UNITY_EDITOR
		public bool showGUIHeaders = true;
#endif
		//[Header("Code Generation")]
		//[Tooltip("Enables the codegen for PackObjects / PackAttributes. Disable this if you would like to suspend codegen. Existing codegen will remain, unless it produces errors.")]
		//public bool enableCodegen = true;

		//[Tooltip("Automatically deletes codegen if it produces any compile errors. Typically you will want to leave this enabled.")]
		//public bool deleteBadCode = true;

		/// cached runtime frame count values
		public static int halfFrameCount;
		public static int thirdFrameCount;
		public static int quaterFrameCount;
		public static int frameCountBits;
		public static float netTickInterval, netTickIntervalInv, targetBufferInterval;

#pragma warning disable 0414
        static float secondsOfBuffer;
        static float secondsOfHalfBuffer;
        static float bufferTargSecs;
#pragma warning restore 0414


        /// <summary>
        /// Get 1/3rd the value of the current frameCount setting. Do not hotpath this method please.
        /// </summary>
        public static int MaxKeyframes
		{
			get { return (int)Single._frameCount / 3; }
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Bootstrap()
		{

			var single = Single;
            single.CalculateBufferValues();
        }

        private void CalculateBufferValues()
        {
            /// TODO: lots of work needed to make Auto a thing.
			if (_bufferCorrection == BufferCorrection.Manual)
            {
                //targetBufferSize = single._targetBufferSize;
                minBufferSize = _minBufferSize;
                maxBufferSize = _maxBufferSize;
            }
            else
            {
                minBufferSize = _minBufferSize;
                maxBufferSize = _maxBufferSize;
            }

            frameCount = (int)_frameCount;
            halfFrameCount = frameCount / 2;
            thirdFrameCount = frameCount / 3;
            quaterFrameCount = frameCount / 4;

            frameCountBits = frameCount.GetBitsForMaxValue();

            netTickInterval = Time.fixedDeltaTime * _sendEveryXTick;
            netTickIntervalInv = 1f / (Time.fixedDeltaTime * _sendEveryXTick);
            targetBufferInterval = Time.fixedDeltaTime * _sendEveryXTick * _targetBufferSize;

            sendEveryXTick = _sendEveryXTick;

            targetBufferSize = _targetBufferSize;
            ticksBeforeGrow = _ticksBeforeGrow;
            ticksBeforeShrink = _ticksBeforeShrink;

            secondsOfBuffer = Time.fixedDeltaTime * _sendEveryXTick * frameCount;
            secondsOfHalfBuffer = secondsOfBuffer * .5f;
            bufferTargSecs = netTickInterval * _targetBufferSize;
        }

       
#if UNITY_EDITOR

        [MenuItem("Window/Photon Unity Networking/Tick Engine Settings", false, 105)]
        private static void SelectInstance()
        {
            Single.SelectThisInstance();
        }

		public override bool DrawGui(Object target, bool asFoldout, bool includeScriptField, bool initializeAsOpen = true, bool asWindow = false)
		{
			EditorGUI.BeginChangeCheck();

			bool isexpanded = base.DrawGui(target, asFoldout, includeScriptField, initializeAsOpen, asWindow);

			SerializedObject soTarget = new SerializedObject(Single);

			if (isexpanded)
			{
			
				EditorGUILayout.Space();

				SerializedProperty frameCount = soTarget.FindProperty("_frameCount");
				SerializedProperty _sendEveryXTick = soTarget.FindProperty("_sendEveryXTick");
				SerializedProperty bufferCorrection = soTarget.FindProperty("_bufferCorrection");

                EditorGUILayout.LabelField("Time Manager", (GUIStyle)"BoldLabel");

                Time.fixedDeltaTime = EditorGUILayout.DelayedFloatField("Fixed Timestep", Time.fixedDeltaTime);
                EditorGUILayout.HelpBox("Note: Fixed Timestep is the PhysX/FixedUpdate rate. Changing this value changes the Project Settings / Time settings, and will affect physics behavior in Unity.", MessageType.None);
                EditorGUILayout.LabelField("Ring Buffer", (GUIStyle)"BoldLabel");

				/// Limit sendEveryX inspector value to 4 if settings frameCount is 12. 5+ will not factor.
				if (bufferCorrection.enumValueIndex == (int)BufferCorrection.Manual && frameCount.intValue == 12 && _sendEveryXTick.intValue > 4)
				{
					_sendEveryXTick.intValue = 4;
				}

				EditorGUILayout.IntSlider(_sendEveryXTick, 1, 12);

				EditorGUILayout.PropertyField(bufferCorrection);

				if (bufferCorrection.enumValueIndex == (int)BufferCorrection.Manual)
				{

					EditorGUILayout.PropertyField(frameCount);
					DrawBufferSizes(soTarget);
				}
				else
				{
					AutoSetBuffer(frameCount, _sendEveryXTick);
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.PropertyField(frameCount);
					DrawBufferSizes(soTarget);
					EditorGUI.EndDisabledGroup();
				}

                CalculateBufferValues();
                //float secondsOfBuffer = Time.fixedDeltaTime * _sendEveryXTick.intValue * frameCount.intValue;
                //float secondsOfHalfBuffer = secondsOfBuffer * .5f;
                float fixrate = Time.fixedDeltaTime;
                //float netrate = Time.fixedDeltaTime * _sendEveryXTick.intValue;
                //float bufferTargSecs = netrate * _targetBufferSize;

                EditorGUILayout.LabelField(
					"Fixed/Sim Tick Rate:\n" +
					fixrate.ToString("G4") + " ms (" + (1 / fixrate).ToString("G4") + " ticks per sec)\n\n" +
					"Net Tick Rate:\n" +
					netTickInterval.ToString("G4") + " ms (" + (netTickIntervalInv).ToString("G4") + " ticks per sec)\n\n" +

					bufferTargSecs.ToString("0.000") + " ms target buffer size\n\n" +
					secondsOfBuffer.ToString("0.000") + " secs Buffer Total\n" +
					secondsOfHalfBuffer.ToString("0.000") + " secs Buffer Look Ahead\n" +
					secondsOfHalfBuffer.ToString("0.000") + " secs Buffer History"
					, new GUIStyle("HelpBox") { padding = new RectOffset(8, 8, 8, 8) });

				if (secondsOfBuffer < 1)
					EditorGUILayout.HelpBox("Warning: Less than 1 Second of buffer may break catastrophically for users with high pings. Increase FrameCount, increase SendEveryX value, or reduce the physics/fixed rate to make the buffer larger.", MessageType.Warning);

			}

			if (EditorGUI.EndChangeCheck())
			{
				soTarget.ApplyModifiedProperties();
				AssetDatabase.SaveAssets();
			}

			return isexpanded;

		}

		private void AutoSetBuffer(SerializedProperty frameCount, SerializedProperty sendEveryX)
		{
			float secondsPerTick = Time.fixedDeltaTime * sendEveryX.intValue;

			float framesNeeded = 1 / secondsPerTick;


			if (framesNeeded < (float)FrameCountEnum.FrameCount12)
				frameCount.intValue = (int)FrameCountEnum.FrameCount12;

			else if (framesNeeded < (float)FrameCountEnum.FrameCount30)
				frameCount.intValue = (int)FrameCountEnum.FrameCount30;

			else if (framesNeeded < (float)FrameCountEnum.FrameCount60)
				frameCount.intValue = (int)FrameCountEnum.FrameCount60;

			else
				frameCount.intValue = (int)FrameCountEnum.FrameCount120;

			//Debug.Log("Frames needed for 1 sec " + framesNeeded + " : " + secondsPerTick + " frameCount: " + FrameCount + " fixed: " + Time.fixedDeltaTime + " " + sendEveryX.intValue);

		}

		private void DrawBufferSizes(SerializedObject soTarget)
		{
			SerializedProperty _trgSize = soTarget.FindProperty("_targetBufferSize");
			SerializedProperty _min = soTarget.FindProperty("_minBufferSize");
			SerializedProperty _max = soTarget.FindProperty("_maxBufferSize");
			SerializedProperty _ticksBeforeGrow = soTarget.FindProperty("_ticksBeforeGrow");
			SerializedProperty _ticksBeforeShrink = soTarget.FindProperty("_ticksBeforeShrink");


			EditorGUILayout.BeginVertical(/*"HelpBox"*/);

			int bufferLimit = (int)_frameCount / 3;
			EditorGUILayout.IntSlider(_trgSize, 0, bufferLimit);

			if (_trgSize.intValue < 1) _trgSize.intValue = 1;
			if (_trgSize.intValue >= bufferLimit) _trgSize.intValue = bufferLimit;

			if (_min.intValue > _trgSize.intValue) _min.intValue = _trgSize.intValue;
			if (_max.intValue < _trgSize.intValue) _max.intValue = _trgSize.intValue;

			if (_max.intValue > bufferLimit) _max.intValue = bufferLimit;

			float min = _min.intValue;
			float max = _max.intValue;

			/// Min/Max slider row
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.MinMaxSlider("Buffer Min/Max", ref min, ref max, 0, bufferLimit);
			min = EditorGUILayout.DelayedIntField(GUIContent.none, (int)min, GUILayout.MaxWidth(23), GUILayout.MinWidth(23));
			max = EditorGUILayout.DelayedIntField(GUIContent.none, (int)max, GUILayout.MaxWidth(23), GUILayout.MinWidth(23));
			EditorGUILayout.EndHorizontal();

			_min.intValue = (int)min;
			_max.intValue = (int)max;

			if (_min.intValue > _trgSize.intValue) _min.intValue = _trgSize.intValue;
			if (_max.intValue < _trgSize.intValue) _max.intValue = _trgSize.intValue;

			EditorGUILayout.IntSlider(_ticksBeforeGrow, 1, 12);
			EditorGUILayout.IntSlider(_ticksBeforeShrink, 1, 12);

			EditorGUILayout.EndVertical();
		}

#endif
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(TickEngineSettings))]
	[CanEditMultipleObjects]
	public class TickEngineSettingsEditor : Editor
	{

		public override void OnInspectorGUI()
		{
			TickEngineSettings.Single.DrawGui(target, false, false, true);
		}
	}
#endif
}
