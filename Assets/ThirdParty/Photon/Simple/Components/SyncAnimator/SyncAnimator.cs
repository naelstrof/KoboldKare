// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

using emotitron.Compression;
using Photon.Pun.Simple.Internal;
using Photon.Pun.UtilityScripts;
using Photon.Utilities;
using Photon.Compression;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace Photon.Pun.Simple
{

    public class SyncAnimator : SyncObject<SyncAnimator.Frame>
        , IOnSnapshot
        , IOnNetSerialize
        , IOnAuthorityChanged
        , ISyncAnimator
        , IReadyable
        , IUseKeyframes
        //, IAdjustableApplyOrder
        , IOnInterpolate
        , IOnCaptureState
    {

        #region Shared Cache (to avoid repeats of animator definitions with every prefab instance)

#if UNITY_EDITOR
        protected List<string> tempNamesList = new List<string>();
        [HideInInspector] public List<string> sharedTriggNames = new List<string>();
        [HideInInspector] public List<string> sharedStateNames = new List<string>();
        [HideInInspector] public List<string> sharedTransNames = new List<string>();
#endif
        private static Dictionary<int, Dictionary<int, int>> masterSharedTriggHashes = new Dictionary<int, Dictionary<int, int>>();
        private static Dictionary<int, List<int>> masterSharedTriggIndexes = new Dictionary<int, List<int>>();
        [HideInInspector] public List<int> sharedTriggIndexes = new List<int>();
        private Dictionary<int, int> sharedTriggHashes;

        private static Dictionary<int, Dictionary<int, int>> masterSharedStateHashes = new Dictionary<int, Dictionary<int, int>>();
        private static Dictionary<int, List<int>> masterSharedStateIndexes = new Dictionary<int, List<int>>();
        [HideInInspector] public List<int> sharedStateIndexes = new List<int>();
        private Dictionary<int, int> sharedStateHashes;

        //private static Dictionary<int, Dictionary<int, TransitionInfo>> masterSharedTransHashes = new Dictionary<int, Dictionary<int, TransitionInfo>>();
        //private static Dictionary<int, List<TransitionInfo>> masterSharedTransIndexes = new Dictionary<int, List<TransitionInfo>>();
        //[HideInInspector] public List<TransitionInfo> sharedTransIndexes = new List<TransitionInfo>();
        //private Dictionary<int, TransitionInfo> sharedTransHashes;

        #endregion

        #region Inspector Items

        [Tooltip("The Animator to sync. If null the first animator on this game object will be found and used.")]
        public Animator animator;

        [Tooltip("Disables applyRootMotion on any non-authority instances, to avoid a tug of war between the transform sync and root motion.")]
        public bool autoRootMotion = true;

        /// Passthrus
        [HideInInspector] public bool syncPassThrus = true;
        [HideInInspector] public NormalizedFloatCompression passthruNormTimeCompress = NormalizedFloatCompression.Bits10;

        /// States
        [HideInInspector] public bool syncStates = true;
        //[HideInInspector] public bool syncTransitions = false;
        [HideInInspector] public NormalizedFloatCompression normalizedTimeCompress = NormalizedFloatCompression.Bits10;
        [HideInInspector] public bool syncLayers = true;
        [HideInInspector] public bool syncLayerWeights = true;
        [HideInInspector] public NormalizedFloatCompression layerWeightCompress = NormalizedFloatCompression.Bits10;
        [System.NonSerialized] public int layerCount;

        /// Parameters
        [HideInInspector] public bool syncParams = true;
        [HideInInspector] public bool useGlobalParamSettings = true;

        private static Dictionary<int, ParameterDefaults> masterSharedParamDefaults = new Dictionary<int, ParameterDefaults>();
        [HideInInspector] public ParameterDefaults sharedParamDefaults = new ParameterDefaults();

        private static Dictionary<int, ParameterSettings[]> masterSharedParamSettings = new Dictionary<int, ParameterSettings[]>();
        [HideInInspector] public ParameterSettings[] sharedParamSettings = new ParameterSettings[0];
        [HideInInspector] public int paramCount;


        #endregion

        #region Local Cache

        /// cached stuff
        private int bitsForTriggerIndex;
        private int bitsForStateIndex;
        private int bitsForTransIndex;
        private int bitsForLayerIndex;
        private bool defaultRootMotion;

        /// History checks
        private int[] lastAnimationHash;
        private uint[] lastLayerWeight;
        private SmartVar[] lastSentParams;

        private Frame currentFrame;

        #endregion

        public override int ApplyOrder
        {
            get
            {
                return ApplyOrderConstants.ANIMATOR;
            }
        }

        public override bool AllowReconstructionOfEmpty { get { return false; } }

        #region Frame Struct/Queue/Pools

        /// <summary>
        /// Frame[] cannot be generically reused. We reuse Frame[] based on the prefabInstanceId, since different animators will have different
        /// numbers of parameters. Dict[prefabInstanceId, Stack[Frame[]]]
        /// </summary>
        public static Dictionary<int, Stack<Frame[]>> masterSharedFramePool = new Dictionary<int, Stack<Frame[]>>();

        public class Frame : FrameBase
        {
            public SyncAnimator syncAnimator;
            public SmartVar[] parameters;
            public int?[] stateHashes;
            public bool[] layerIsInTransition;
            public float[] normalizedTime;
            public float?[] layerWeights;
            public Queue<AnimPassThru> passThrus;

#if SNS_SYNCIK
			public IKState[] ikStates;
#endif

            public Frame() : base() { }

            public Frame(SyncAnimator syncAnimator, int frameId) : base(frameId)
            {
                this.syncAnimator = syncAnimator;

                int layerCount = syncAnimator.layerCount;

                stateHashes = new int?[layerCount];
                layerIsInTransition = new bool[layerCount];
                normalizedTime = new float[layerCount];
                layerWeights = new float?[layerCount];
                passThrus = new Queue<AnimPassThru>(2);

                parameters = new SmartVar[syncAnimator.paramCount];
                int paramcnt = syncAnimator.paramCount;
                for (int pid = 0; pid < paramcnt; ++pid)
                {
                    parameters[pid] = syncAnimator.sharedParamSettings[pid].defaultValue;
                }

#if SNS_SYNCIK

				/// Sync IK ref init (only what is needed)
				var syncfeet = syncAnimator.syncIKFeet;
				var synchands = syncAnimator.syncIKHands;

				if (syncfeet || synchands)
					ikStates = new IKState[4]
					{
						syncfeet ? new IKState() : null,
						syncfeet ? new IKState() : null,
						synchands ? new IKState() : null,
						synchands ? new IKState() : null,
					};
#endif
            }

            public override void CopyFrom(FrameBase sourceFrame)
            {
                base.CopyFrom(sourceFrame);

                Frame frame = sourceFrame as Frame;

                if (syncAnimator.syncParams)
                {
                    var ps = frame.parameters;
                    int paramcnt = ps.Length;
                    for (int i = 0; i < paramcnt; ++i)
                        parameters[i] = ps[i];
                }

                if (syncAnimator.syncStates)
                {
                    int lyrCnt = frame.stateHashes.Length;
                    for (int i = 0; i < lyrCnt; ++i)
                    {
                        stateHashes[i] = frame.stateHashes[i];
                        layerIsInTransition[i] = frame.layerIsInTransition[i];
                        normalizedTime[i] = frame.normalizedTime[i];
                        layerWeights[i] = frame.layerWeights[i];
                    }
                }

#if SNS_SYNCIK

				if (syncAnimator.syncIKHands || syncAnimator.syncIKFeet)
				{
					for (int i = 0; i < 4; ++i)
					{
						var ikTrg = ikStates[i];
						var ikSrc = frame.ikStates[i];
						ikTrg.pos = ikSrc.pos;
						ikTrg.rot = ikSrc.rot;
					}
				}
#endif

                /// Don't copy triggers - unless I decide otherwise. They are fire once and should not be repeated.
                //triggers = new Queue<TriggerItem>();
                //crossFades = new Queue<TriggerItem>();
            }

            public override void Clear()
            {
                base.Clear();

                for (int i = 0, cnt = stateHashes.Length; i < cnt; ++i)
                    stateHashes[i] = null;

                passThrus.Clear();

                for (int layer = 0, cnt = layerWeights.Length; layer < cnt; ++layer)
                {
                    layerWeights[layer] = null;
                    stateHashes[layer] = null;
                    normalizedTime[layer] = 0;
                    layerWeights[layer] = null;
                }

            }
        }

        //private void InvalidateFrame(Frame frame)
        //{
        //    int count = (syncLayers) ? layerCount : 1;
        //    for (int layer = 0; layer < count; ++layer)
        //    {
        //        frame.layerWeights[layer] = null;
        //        frame.stateHashes[layer] = null;
        //        frame.normalizedTime[layer] = 0;
        //        frame.layerWeights[layer] = null;
        //    }
        //}

        protected override void PopulateFrames()
        {
            Initialize();

            int frameCount = TickEngineSettings.frameCount;

            /// Animator frames vary from usage to usage, so there is a pool for every instanceID.
            Stack<Frame[]> pool;
            if (!masterSharedFramePool.TryGetValue(prefabInstanceId, out pool))
            {
                pool = new Stack<Frame[]>();
                masterSharedFramePool.Add(prefabInstanceId, pool);
            }
            if (pool.Count == 0)
            {
                frames = new Frame[frameCount + 1];

                for (int i = 0; i <= frameCount; ++i)
                    frames[i] = new Frame(this, i);
            }
            else
            {
                frames = pool.Pop();
                for (int i = 0; i <= frameCount; ++i)
                    frames[i].Clear();
            }
        }

        #endregion

        #region Unity Timings

#if UNITY_EDITOR

        const double AUTO_REBUILD_RATE = 10f;
        double lastReuildTime;

        /// <summary>
        /// Reindex all of the State and Trigger names in the current AnimatorController. Never hurts to run this (other than hanging the editor for a split second).
        /// </summary>
        public void RebuildIndexedNames()
        {
            /// always get new Animator in case it has changed.
            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator && EditorApplication.timeSinceStartup - lastReuildTime > AUTO_REBUILD_RATE)
            {
                lastReuildTime = EditorApplication.timeSinceStartup;

                AnimatorController ac = animator.GetController();
                if (ac != null)
                {
                    if (ac.animationClips == null || ac.animationClips.Length == 0)
                        Debug.LogWarning("'" + name + "' has an Animator with no animation clips. Some Animator Controllers require a restart of Unity, or for a Build to be made in order to initialize correctly.");

                    bool haschanged = false;

                    ac.GetTriggerNames(sharedTriggIndexes);
                    ac.GetStatesNames(sharedStateIndexes);

                    ac.GetTriggerNames(tempNamesList);
                    if (!CompareNameLists(tempNamesList, sharedTriggNames))
                    {
                        CopyNameList(tempNamesList, sharedTriggNames);
                        haschanged = true;
                    }

                    ac.GetStatesNames(tempNamesList);
                    if (!CompareNameLists(tempNamesList, sharedStateNames))
                    {
                        CopyNameList(tempNamesList, sharedStateNames);
                        haschanged = true;
                    }

                    if (haschanged)
                    {
                        Debug.Log(animator.name + " has changed. SyncAnimator indexes updated.");
                        EditorUtility.SetDirty(this);
                    }
                }
            }
        }

        private static bool CompareNameLists(List<string> one, List<string> two)
        {
            if (one.Count != two.Count)
                return false;

            for (int i = 0; i < one.Count; i++)
                if (one[i] != two[i])
                    return false;

            return true;
        }

        private static void CopyNameList(List<string> src, List<string> trg)
        {
            trg.Clear();
            for (int i = 0; i < src.Count; i++)
                trg.Add(src[i]);
        }


        protected override void Reset()
        {
            base.Reset();

            animator = null;
            FindUnsyncedAnimator();
            RebuildIndexedNames();

        }

#endif

        public override void OnAwake()
        {
            if (animator == null)
                FindUnsyncedAnimator();

            base.OnAwake();

            ConnectSharedCaches();

            if (animator)
                defaultRootMotion = animator.applyRootMotion;


            //Debug.LogError(prefabInstanceId);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            AutoRootMotion(IsMine);
        }

        public override void OnStart()
        {
            base.OnStart();
            AutoRootMotion(IsMine);
        }

        public override void OnAuthorityChanged(bool isMine, bool controllerChanged)
        {
            base.OnAuthorityChanged(isMine, controllerChanged);
            AutoRootMotion(isMine);
        }


        private void AutoRootMotion(bool isMine)
        {
            if (autoRootMotion && animator)
            {
                animator.applyRootMotion = (isMine) ? defaultRootMotion : false;
            }
        }

        private static List<Animator> foundAnimators = new List<Animator>();
        private static List<SyncAnimator> foundSyncs = new List<SyncAnimator>();
        /// <summary>
        /// Find an animator on this object that does not currently have a syncAnimator tied to it. This is just so
        /// adding new SyncAnimators does a pretty good job of guessing what to monitor without the dev having to
        /// manually wire them up.
        /// </summary>
        private void FindUnsyncedAnimator()
        {
            transform.GetNestedComponentsInChildren<Animator, NetObject>(foundAnimators);
            transform.GetNestedComponentsInChildren<SyncAnimator, NetObject>(foundSyncs);

            foreach (var a in foundAnimators)
            {
                bool used = false;
                foreach (var s in foundSyncs)
                {
                    used = false;
                    if (s.animator == a)
                    {
                        used = true;
                        break;
                    }
                }
                if (!used)
                {
                    animator = a;
                    break;
                }
            }
        }

        private void Initialize()
        {
            bitsForTriggerIndex = (sharedTriggIndexes.Count - 1).GetBitsForMaxValue();
            bitsForStateIndex = (sharedStateIndexes.Count - 1).GetBitsForMaxValue();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (animator == null)
                Debug.LogError("No Animator component found. Be sure " + name + " has an animator, or remove " + GetType().Name + ".");
#endif
            paramCount = animator.parameters.Length;

            layerCount = animator.layerCount;
            /// we don't use (layercount - 1), because our actual range is -1 to X ... so the extra value is needed for the + 1 shift during serialization.
            bitsForLayerIndex = layerCount.GetBitsForMaxValue();

            lastSentParams = new SmartVar[paramCount];
            /// TODO: these will now have values, check them rather than replace them
            ParameterSettings.RebuildParamSettings(animator, ref sharedParamSettings, ref paramCount, sharedParamDefaults);

            lastAnimationHash = new int[layerCount];
            lastLayerWeight = new uint[layerCount];

            // Cache some of the readonly parameter attributes
            for (int pid = 0; pid < paramCount; ++pid)
            {
                /// Start our lastSent values to the default so our param tests don't require any special checks.
                lastSentParams[pid] = sharedParamSettings[pid].defaultValue; // ; paramDefValue[pid];
            }

#if SNS_SYNCIK
			InitIK();
#endif

        }

        /// <summary>
        /// Cloned objects from prefabs share a common set of index/hash lookups as well as scraped parameter data from animators.
        /// Calling this at startup instructs each instance of Sync Animator to refenence those common instances and release their deserialized version for GC.
        /// This is a bit convoluted, but avoids having to resort to a ScirptableObject for exposing these things in the editor at runtime.
        /// </summary>
        private void ConnectSharedCaches()
        {
            /// Connect sharedTrigger states
            if (!masterSharedTriggHashes.ContainsKey(prefabInstanceId))
            {
                sharedTriggHashes = new Dictionary<int, int>();
                for (int i = 0; i < sharedTriggIndexes.Count; ++i)
                    if (sharedTriggHashes.ContainsKey(sharedTriggIndexes[i]))
                    {
                        Debug.LogError("There appear to be duplicate Trigger names in the animator controller on '" + name + "'. This will break " + GetType().Name + "'s ability to sync triggers.");
                    }
                    else
                        sharedTriggHashes.Add(sharedTriggIndexes[i], i);

                masterSharedTriggHashes.Add(prefabInstanceId, sharedTriggHashes);
                masterSharedTriggIndexes.Add(prefabInstanceId, sharedTriggIndexes);
            }
            else
            {
                sharedTriggHashes = masterSharedTriggHashes[prefabInstanceId];
                sharedTriggIndexes = masterSharedTriggIndexes[prefabInstanceId];
            }

            /// Connect sharedStates
            if (!masterSharedStateHashes.ContainsKey(prefabInstanceId))
            {
                sharedStateHashes = new Dictionary<int, int>();
                for (int i = 0; i < sharedStateIndexes.Count; ++i)
                    if (sharedStateHashes.ContainsKey(sharedStateIndexes[i]))
                    {
                        Debug.LogError("There appear to be duplicate State names in the animator controller on '" + name + "'. This will break " + GetType().Name + "'s ability to sync states.");
                    }
                    else
                        sharedStateHashes.Add(sharedStateIndexes[i], i);

                masterSharedStateHashes.Add(prefabInstanceId, sharedStateHashes);
                masterSharedStateIndexes.Add(prefabInstanceId, sharedStateIndexes);
            }
            else
            {
                sharedStateHashes = masterSharedStateHashes[prefabInstanceId];
                sharedStateIndexes = masterSharedStateIndexes[prefabInstanceId];
            }

            ParameterDefaults pd;
            if (masterSharedParamDefaults.TryGetValue(prefabInstanceId, out pd))
                sharedParamDefaults = pd;
            else
                masterSharedParamDefaults.Add(prefabInstanceId, sharedParamDefaults);

            ParameterSettings[] ps;
            if (masterSharedParamSettings.TryGetValue(prefabInstanceId, out ps))
                sharedParamSettings = ps;
            else
                masterSharedParamSettings.Add(prefabInstanceId, sharedParamSettings);
        }

        private void OnDestroy()
        {
            /// Add frames[] to pool
            masterSharedFramePool[prefabInstanceId].Push(frames);
        }
        #endregion

        #region Net Serialization/Deserialization

        /// <summary>
        /// NetObject serialize interface.
        /// </summary>
        public SerializationFlags OnNetSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
        {
            Frame frame = frames[frameId];

            /// Don't transmit data if this component is disabled. Allows for muting components
            /// Simply by disabling them at the authority side.
            if (frame.content == 0)
            {
                buffer.WriteBool(false, ref bitposition);
                return SerializationFlags.None;
            }

            /// hascontent bool
            buffer.WriteBool(true, ref bitposition);

            bool isKeyframe = IsKeyframe(frameId);

            return WriteAllToBuffer(frame, buffer, ref bitposition, isKeyframe);
        }

        /// <summary>
        /// NetObject deserialize interface.
        /// </summary>
        public SerializationFlags OnNetDeserialize(int originFrameId, byte[] buffer, ref int bitposition, FrameArrival arrival)
        {
            bool isKeyframe = IsKeyframe(originFrameId);

            /// Needs to ignore any incoming updates that are the server/relay mirroring back what we sent
            var frame = (IsMine) ? offtickFrame : frames[originFrameId];

            /// If hascontent flag is false, we are done here.
            if (!buffer.ReadBool(ref bitposition))
            {
                return SerializationFlags.None;
            }

            frame.content = FrameContents.Complete;

            ReadAllFromBuffer(frame, buffer, ref bitposition, isKeyframe);
            return SerializationFlags.HasContent;
        }

        #endregion

        #region Buffer Writer/Readers

        public virtual void OnCaptureCurrentState(int frameId)
        {
            Frame frame = frames[frameId];

            ///TODO: more may need to be done if not enabled, to ensure the frame is marked as not valid.
            if (!isActiveAndEnabled || !animator.isActiveAndEnabled)
            {
                frame.content = FrameContents.Empty;
                return;
            }

            if (syncParams)
                CaptureParameters(frame);

            if (syncPassThrus)
                CapturePassThrus(frame);

            if (syncStates)
                CaptureStates(frame);

            if (syncStates)
                CaptureLayerWeights(frame);

#if SNS_SYNCIK
			if (syncIKFeet)
				CaptureIK(frame);
#endif

            frame.content = FrameContents.Complete;
        }


        private SerializationFlags WriteAllToBuffer(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
        {
            SerializationFlags flags = SerializationFlags.None;

            /// Write Passthrough Trigger and Callback Events
            if (syncPassThrus)
                flags |= WritePassThrus(frame, buffer, ref bitposition, isKeyframe);

            if (syncParams)
                flags |= WriteParameters(frame, buffer, ref bitposition, isKeyframe);

            if (syncStates)
                flags |= WriteStates(frame, buffer, ref bitposition, isKeyframe);

            if (syncLayerWeights)
                flags |= WriteLayerWeights(frame, buffer, ref bitposition, isKeyframe);

#if SNS_SYNCIK
			if (syncIKFeet)
				flags |= WriteIK(frame, buffer, ref bitposition, isKeyframe);
#endif
            // Mark as always having content. Can revisit this later.
            return flags;
        }

        private void ReadAllFromBuffer(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
        {
            if (syncPassThrus)
                ReadPassThrus(frame, buffer, ref bitposition, isKeyframe);

            if (syncParams)
                ReadParameters(frame, buffer, ref bitposition, isKeyframe);

            if (syncStates)
                ReadStates(frame, buffer, ref bitposition, isKeyframe);

            if (syncLayerWeights)
                ReadLayerWeights(frame, buffer, ref bitposition, isKeyframe);

#if SNS_SYNCIK
			if (syncIKFeet)
				ReadIK(frame, buffer, ref bitposition, isKeyframe);
#endif

        }

        #region Parameter Handling

        /// <summary>
        /// Serialize frame parameters into buffer
        /// </summary>
        private SerializationFlags WriteParameters(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
        {
            var paramaters = frame.parameters;

            SerializationFlags flags = SerializationFlags.None;

            for (int pid = 0; pid < paramCount; ++pid)
            {
                var ps = sharedParamSettings[pid];

                if (!useGlobalParamSettings && !ps.include)
                    continue;

                var type = ps.paramType;

                if (type == AnimatorControllerParameterType.Int)
                {
                    int val = paramaters[pid];

                    if (isKeyframe || val != lastSentParams[pid])
                    {
                        if (!isKeyframe)
                            buffer.WriteBool(true, ref bitposition);

                        if (useGlobalParamSettings)
                            buffer.WriteSignedPackedBytes(val, ref bitposition, 32);
                        else
                            ps.icrusher.WriteValue(val, buffer, ref bitposition);

                        lastSentParams[pid] = val;
                        flags |= SerializationFlags.HasContent;
                    }
                    else
                    {
                        if (!isKeyframe)
                            buffer.WriteBool(false, ref bitposition);
                    }
                }

                else if (type == AnimatorControllerParameterType.Float)
                {
                    float val = paramaters[pid];
                    var fcrusher = ps.fcrusher;
                    uint cval = (useGlobalParamSettings) ? HalfUtilities.Pack(val) : (uint)fcrusher.Encode(val);

                    if (isKeyframe || cval != lastSentParams[pid].UInt)
                    {
                        if (!isKeyframe)
                            buffer.WriteBool(true, ref bitposition);

                        if (useGlobalParamSettings)
                            buffer.Write(cval, ref bitposition, 16);
                        else
                            fcrusher.WriteCValue(cval, buffer, ref bitposition);

                        lastSentParams[pid] = cval;

                        //Debug.Log("Float " + cval + ":" + val);
                        flags |= SerializationFlags.HasContent;
                    }
                    else
                    {
                        if (!isKeyframe)
                            buffer.WriteBool(false, ref bitposition);
                    }

                }

                else if (type == AnimatorControllerParameterType.Bool)
                {
                    bool val = paramaters[pid];
                    buffer.WriteBool(val, ref bitposition);

                    if (isKeyframe || val != lastSentParams[pid])
                        flags |= SerializationFlags.HasContent;
                }

                else if (type == AnimatorControllerParameterType.Trigger)
                {

                    bool val = paramaters[pid];
                    buffer.WriteBool(val, ref bitposition);

                    if (isKeyframe || val != lastSentParams[pid])
                        flags |= SerializationFlags.HasContent;
                }
            }
            return flags;
        }

        private void CaptureParameters(Frame frame)
        {
            var paramaters = frame.parameters;

            for (int pid = 0; pid < paramCount; ++pid)
            {
                var ps = sharedParamSettings[pid];

                if (!useGlobalParamSettings && !ps.include)
                    continue;

                var type = ps.paramType;
                int nameHash = ps.hash;

                switch (type)
                {
                    case AnimatorControllerParameterType.Float:
                        paramaters[pid] = animator.GetFloat(nameHash);
                        break;

                    case AnimatorControllerParameterType.Int:
                        paramaters[pid] = animator.GetInteger(nameHash);
                        break;

                    case AnimatorControllerParameterType.Bool:
                        paramaters[pid] = animator.GetBool(nameHash);
                        break;

                    case AnimatorControllerParameterType.Trigger:
                        paramaters[pid] = animator.GetBool(nameHash);
                        break;

                    default:
                        break;

                }
            }
        }

        private void ReadParameters(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
        {
            SmartVar[] parms = frame.parameters;

            ///  Less than ideal check to make sure we don't write None values over our extrapolated values
            ///  if this comes in late and is already in use.
            bool frameIsInUse = (ReferenceEquals(frame, targFrame)) || (ReferenceEquals(frame, snapFrame));

            for (int pid = 0; pid < paramCount; ++pid)
            {
                var ps = sharedParamSettings[pid];

                if (!useGlobalParamSettings && !ps.include)
                    continue;

                var type = ps.paramType;

                switch (type)
                {
                    case AnimatorControllerParameterType.Int:
                        {
                            bool used = isKeyframe ? true : buffer.ReadBool(ref bitposition);

                            if (used)
                            {
                                int val = (useGlobalParamSettings) ?
                                    buffer.ReadSignedPackedBytes(ref bitposition, 32) :
                                    ps.icrusher.ReadValue(buffer, ref bitposition);

                                parms[pid] = val;
                            }
                            else
                            {
                                if (!frameIsInUse)
                                    parms[pid] = SmartVar.None;
                            }
                            break;
                        }
                    case AnimatorControllerParameterType.Float:
                        {
                            bool used = isKeyframe ? true : buffer.ReadBool(ref bitposition);

                            if (used)
                            {
                                parms[pid] = (useGlobalParamSettings) ?
                                    buffer.ReadHalf(ref bitposition) :
                                    ps.fcrusher.ReadValue(buffer, ref bitposition);
                            }
                            else
                            {
                                if (!frameIsInUse)
                                    parms[pid] = SmartVar.None;
                            }
                            break;
                        }
                    case AnimatorControllerParameterType.Bool:
                        {
                            bool val = buffer.ReadBool(ref bitposition);

                            parms[pid] = val;
                            break;
                        }

                    case AnimatorControllerParameterType.Trigger:
                        {
                            bool val = buffer.ReadBool(ref bitposition);
                            parms[pid] = val;
                            break;
                        }

                }

                //if (type == AnimatorControllerParameterType.Int)
                //{
                //	bool used = isKeyframe ? true : buffer.ReadBool(ref bitposition);

                //	if (used)
                //	{
                //		int val = (useGlobalParamSettings) ?
                //			buffer.ReadSignedPackedBytes(ref bitposition, 32) :
                //			ps.icrusher.ReadValue(buffer, ref bitposition);

                //		parms[pid] = val;
                //	}
                //	else
                //	{
                //		if (!frameIsInUse)
                //			parms[pid] = SmartVar.None;
                //	}
                //}

                //else if (type == AnimatorControllerParameterType.Float)
                //{
                //	bool used = isKeyframe ? true : buffer.ReadBool(ref bitposition);

                //	if (used)
                //	{
                //		parms[pid] = (useGlobalParamSettings) ?
                //			buffer.ReadHalf(ref bitposition) :
                //			ps.fcrusher.ReadValue(buffer, ref bitposition);
                //	}
                //	else
                //	{
                //		if (!frameIsInUse)
                //			parms[pid] = SmartVar.None;
                //	}
                //}

                ///// Always include bools, since the mask of 1 needed to indicate included is the same size as just inccluding them.
                //else if (type == AnimatorControllerParameterType.Bool)
                //{
                //	bool val = buffer.ReadBool(ref bitposition);
                //	parms[pid] = val;
                //}

                ///// Always include triggers, since the mask of 1 needed to indicate included is the same size as just inccluding them.
                //else if (type == AnimatorControllerParameterType.Trigger)
                //{
                //	bool val = buffer.ReadBool(ref bitposition);
                //	parms[pid] = val;
                //}
            }
        }

        /// <summary>
        /// Many parameters will be SmartVar.None if keyframes are used - meaning they were unchanged.
        /// This method completes the current TargF using values from PrevF
        /// </summary>
        private void CompleteTargetParameters()
        {
            SmartVar[] prevParams = (snapFrame != null) ? snapFrame.parameters : targFrame.parameters;
            SmartVar[] targParams = targFrame.parameters;
            /// if this smartvar is none, then this value was left out of the update - meaning it was a repeat.
            /// Copy the previous value.
            /// TODO: This should use the extrapolate setting eventually?
            /// TODO: This should be its own loop so it doesn't happen every interpolate? Call in SNapshot?
            for (int pid = 0; pid < paramCount; ++pid)
            {
                SmartVar prevParam = prevParams[pid];
                SmartVar targParam = targParams[pid];
                var psettings = sharedParamSettings[pid];

                if (prevParam.TypeCode == SmartVarTypeCode.None)
                {
                    prevParam = psettings.defaultValue;
                    prevParams[pid] = prevParam;
                }

                if (targParam.TypeCode == SmartVarTypeCode.None)
                {
                    targParam = prevParam;
                    targParams[pid] = prevParam;
                }
            }
        }

        /// <summary>
        /// t value of zero will skip any tests and processing for lerps
        /// </summary>
        /// <param name="t"></param>
        private void InterpolateParams(float t)
        {

            SmartVar[] prevParams =/* (snapF != null) ?*/ snapFrame.parameters /*: targF.parameters*/;
            SmartVar[] targParams = targFrame.parameters;

            for (int pid = 0; pid < paramCount; ++pid)
            {

                var psettings = sharedParamSettings[pid];
                int hash = psettings.hash;
                if (!useGlobalParamSettings && !psettings.include)
                    continue;

                var type = psettings.paramType;

                SmartVar prevParam = prevParams[pid];
                SmartVar targParam = targParams[pid];

                if (prevParam.TypeCode == SmartVarTypeCode.None)
                    continue;

                if (targParam.TypeCode == SmartVarTypeCode.None)
                    continue;

                switch (type)
                {
                    case AnimatorControllerParameterType.Int:
                        {
                            if (sharedParamDefaults.includeInts == false)
                                continue;

                            /// zero t has no interpolation, so skip all the fancy checks and just apply prev
                            if (t == 0)
                            {
                                animator.SetInteger(hash, prevParam);
                                continue;
                            }

                            ParameterInterpolation interpmethod;

                            if (useGlobalParamSettings)
                                interpmethod = sharedParamDefaults.interpolateInts;
                            else
                                interpmethod = psettings.interpolate;

                            if (interpmethod == ParameterInterpolation.Hold)
                                continue;

                            int value =
                                (interpmethod == ParameterInterpolation.Advance) ? (int)targParam :
                                (interpmethod == ParameterInterpolation.Lerp) ? (int)Mathf.Lerp(prevParam, targParam, t) :
                                (int)psettings.defaultValue;

                            animator.SetInteger(hash, value);

                            break;
                        }


                    case AnimatorControllerParameterType.Float:
                        {
                            if (sharedParamDefaults.includeFloats == false)
                                continue;


                            if (t == 0)
                            {
                                animator.SetFloat(hash, prevParam);
                                continue;
                            }

                            ParameterInterpolation interpmethod;
                            if (useGlobalParamSettings)

                                interpmethod = sharedParamDefaults.interpolateFloats;
                            else
                                interpmethod = psettings.interpolate;

                            if (interpmethod == ParameterInterpolation.Hold)
                                continue;

                            SmartVar value =
                                (interpmethod == ParameterInterpolation.Lerp) ? (SmartVar)(Mathf.Lerp((float)prevParam, (float)targParam, t)) :
                                (interpmethod == ParameterInterpolation.Advance) ? targParam :
                                psettings.defaultValue;

                            animator.SetFloat(hash, value);

                            break;
                        }


                    case AnimatorControllerParameterType.Bool:
                        {
                            if (t != 0)
                                continue;

                            if (!sharedParamDefaults.includeBools)
                                continue;

                            animator.SetBool(hash, prevParam);
                            break;
                        }

                    case AnimatorControllerParameterType.Trigger:
                        {
                            if (t != 0)
                                continue;

                            if (!sharedParamDefaults.includeTriggers)
                                continue;

                            if (prevParam)
                                animator.SetTrigger(hash);
                            break;
                        }

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Extrapolate values for tne missing target frame using the previuos. Todo: Use the last two to get a curve.
        /// </summary>
        /// <param name="snap_params"></param>
        /// <param name="targ_params"></param>
        private void ExtrapolateParams(Frame prev, Frame targ, Frame newtarg)
        {
            if (ReferenceEquals(prev, null))
                return;

            var prev_params = prev.parameters;
            var targ_params = targ.parameters;

            /// if next frame from the buffer isn't flagged as valid, it hasn't arrived - Extrapolate
            for (int pid = 0; pid < paramCount; ++pid)
            {
                var ps = sharedParamSettings[pid];
                var type = ps.paramType;

                if (!useGlobalParamSettings && !ps.include)
                    continue;

                /// TODO: Actually wire up the Lerps?
                /// TODO: Make this Switch

                // Float lerps back toward default value on lost frames as a loss handling compromise currently.
                if (type == AnimatorControllerParameterType.Float)
                {
                    var extrapmethod = (useGlobalParamSettings) ? sharedParamDefaults.extrapolateFloats : ps.extrapolate;

                    newtarg.parameters[pid] =
                        (extrapmethod == ParameterExtrapolation.Hold) ? targ_params[pid] :
                        (extrapmethod == ParameterExtrapolation.Lerp) ? (SmartVar)(targ_params[pid] + ((float)targ_params[pid] - prev_params[pid])) :
                        ps.defaultValue;
                }

                else if (type == AnimatorControllerParameterType.Int)
                {
                    var extrapmethod = (useGlobalParamSettings) ? sharedParamDefaults.extrapolateInts : ps.extrapolate;

                    newtarg.parameters[pid] =
                        (extrapmethod == ParameterExtrapolation.Hold) ? targ_params[pid] :
                        (extrapmethod == ParameterExtrapolation.Lerp) ? (SmartVar)(targ_params[pid] + (targ_params[pid] - prev_params[pid])) :
                        ps.defaultValue;
                }

                else if (type == AnimatorControllerParameterType.Bool)
                {
                    var extrapmethod = (useGlobalParamSettings) ? sharedParamDefaults.extrapolateBools : ps.extrapolate;

                    newtarg.parameters[pid] =
                        (extrapmethod == ParameterExtrapolation.Hold) ? targ_params[pid] : ps.defaultValue;
                }

                /// TODO: this is unfinished
                else /*if (includeTriggers)*/
                {
                    var extrapmethod = (useGlobalParamSettings) ? sharedParamDefaults.extrapolateTriggers : ps.extrapolate;

                    newtarg.parameters[pid] =
                        (extrapmethod == ParameterExtrapolation.Hold) ? targ_params[pid] : ps.defaultValue;
                }
            }
        }

        #endregion

        #region Passhthru Calls

        private readonly Queue<AnimPassThru> passThruQueue = new Queue<AnimPassThru>(2);

        private void EnqueuePassthrough(PassThruType type, int hash, int layer, float ntime, float otime, float duration, LocalApplyTiming localApplyTiming)
        {
            var apt = new AnimPassThru(type, hash, layer, ntime, otime, duration, localApplyTiming);
            passThruQueue.Enqueue(apt);

            /// Pass through to actual Animator immediate if we are set to locally apply immediately, 
            /// or if pass throughs are disabled (we want to pass through when disabled for convenience)
            if (localApplyTiming == LocalApplyTiming.Immediately || !syncPassThrus)
                ExecutePassThru(apt);
        }

        public void SetTrigger(string triggerName, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            int hash = Animator.StringToHash(triggerName);
            EnqueuePassthrough(PassThruType.SetTrigger, hash, -1, -1, -1, -1, localApplyTiming);
        }
        public void SetTrigger(int hash, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            EnqueuePassthrough(PassThruType.SetTrigger, hash, -1, -1, -1, -1, localApplyTiming);
        }

        public void ResetTrigger(string triggerName, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            int hash = Animator.StringToHash(triggerName);
            EnqueuePassthrough(PassThruType.ResetTrigger, hash, -1, -1, -1, -1, localApplyTiming);
        }
        public void ResetTrigger(int hash, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            EnqueuePassthrough(PassThruType.ResetTrigger, hash, -1, -1, -1, -1, localApplyTiming);
        }

        public void Play(string stateName, int layer = -1, float normalizedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            int hash = Animator.StringToHash(stateName);
            EnqueuePassthrough(PassThruType.Play, hash, layer, normalizedTime, -1, -1, localApplyTiming);
        }
        public void Play(int hash, int layer = -1, float normalizedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            EnqueuePassthrough(PassThruType.Play, hash, layer, normalizedTime, -1, -1, localApplyTiming);
        }

        public void PlayInFixedTime(string stateName, int layer = -1, float fixedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            int hash = Animator.StringToHash(stateName);
            EnqueuePassthrough(PassThruType.PlayFixed, hash, layer, -1, fixedTime, -1, localApplyTiming);
        }
        public void PlayInFixedTime(int hash, int layer = -1, float fixedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            EnqueuePassthrough(PassThruType.PlayFixed, hash, layer, -1, fixedTime, -1, localApplyTiming);
        }

        public void CrossFade(string stateName, float duration, int layer = -1, float normalizedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            int hash = Animator.StringToHash(stateName);
            EnqueuePassthrough(PassThruType.CrossFade, hash, layer, normalizedTime, -1, duration, localApplyTiming);
        }
        public void CrossFade(int hash, float duration, int layer = -1, float normalizedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            EnqueuePassthrough(PassThruType.CrossFade, hash, layer, normalizedTime, -1, duration, localApplyTiming);
        }

        public void CrossFadeInFixedTime(string stateName, float duration, int layer = -1, float fixedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            int hash = Animator.StringToHash(stateName);
            EnqueuePassthrough(PassThruType.CrossFadeFixed, hash, layer, -1, fixedTime, duration, localApplyTiming);
        }
        public void CrossFadeInFixedTime(int hash, float duration, int layer = -1, float fixedTime = 0, LocalApplyTiming localApplyTiming = LocalApplyTiming.OnSend)
        {
            EnqueuePassthrough(PassThruType.CrossFadeFixed, hash, layer, -1, fixedTime, duration, localApplyTiming);
        }

        #endregion

        #region Passthru Handling

        /// <summary>
        /// Deques and executes frame passthrus
        /// </summary>
        private void ExecutePassThruQueue(Frame frame)
        {
            var passThrus = frame.passThrus;
            while (passThrus.Count > 0)
            {
                var pt = passThrus.Dequeue();
                ExecutePassThru(pt);
            }
        }

        private void ExecutePassThru(AnimPassThru pt)
        {
            int hash = pt.hash;
            switch (pt.passThruType)
            {
                case PassThruType.SetTrigger:
                    animator.SetTrigger(pt.hash);
                    break;
                case PassThruType.ResetTrigger:
                    animator.ResetTrigger(pt.hash);
                    break;
                case PassThruType.Play:
                    animator.Play(hash, pt.layer, pt.normlTime);
                    break;
                case PassThruType.PlayFixed:
                    animator.PlayInFixedTime(hash, pt.layer, pt.fixedTime);
                    break;
                case PassThruType.CrossFade:
                    animator.CrossFade(hash, pt.duration, pt.layer, pt.normlTime);
                    break;
                case PassThruType.CrossFadeFixed:
                    animator.CrossFadeInFixedTime(hash, pt.duration, pt.layer, pt.fixedTime);
                    break;
            }
        }

        private SerializationFlags WritePassThrus(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
        {
            var passThrus = frame.passThrus;
            SerializationFlags flags = (passThrus.Count == 0) ? SerializationFlags.None : SerializationFlags.HasContent;

            while (passThrus.Count > 0)
            {
                var pt = passThrus.Dequeue();
                var triggerType = pt.passThruType;
                int hash = pt.hash;

                bool isTrigger = triggerType == PassThruType.SetTrigger || triggerType == PassThruType.ResetTrigger;

                if (pt.localApplyTiming == LocalApplyTiming.OnSend)
                    ExecutePassThru(pt);

                /// Write first bool for has PassThru
                buffer.WriteBool(true, ref bitposition);

                /// Write TriggerType
                buffer.Write((uint)triggerType, ref bitposition, 3);

                int index;
                bool isIndexed;

                isIndexed =
                (isTrigger) ? sharedTriggHashes.TryGetValue(hash, out index) :
                sharedStateHashes.TryGetValue(pt.hash, out index);


#if UNITY_EDITOR
                if (!isIndexed)
                    Debug.LogWarning(GetType().Name +
                        " is networking a state/trigger that has not been indexed, resulting in greatly increased size. Be sure to click 'Rebuild Indexes' whenever states/triggers are added or removed.");
#endif
                /// Write IsIndexed bool
                buffer.WriteBool(isIndexed, ref bitposition);

                /// Write Hash
                if (isIndexed)
                    buffer.Write((uint)index, ref bitposition, isTrigger ? bitsForTriggerIndex : bitsForStateIndex);
                else
                    buffer.WriteSigned(pt.hash, ref bitposition, 32);

                /// Triggers do not use the following - we are done.
                if (isTrigger)
                    continue;

                /// Write layer
                bool useLayer = layerCount > 1;
                if (useLayer)
                    buffer.Write((uint)pt.layer + 1, ref bitposition, bitsForLayerIndex);

                bool useNormalizedTime = (triggerType == PassThruType.Play || triggerType == PassThruType.CrossFade);

                /// Write NonZero bool and normalizedTime
                if (useNormalizedTime)
                {
                    float ntime = pt.normlTime;

                    if (ntime == 0)
                        buffer.WriteBool(false, ref bitposition);
                    else
                    {
                        buffer.WriteBool(true, ref bitposition);
                        buffer.WriteNorm(ntime, ref bitposition, (int)passthruNormTimeCompress);
                    }
                }

                /// OR Write NonZero bool and FixedTime
                else
                {
                    float ftime = pt.fixedTime;

                    if (ftime == 0)
                        buffer.WriteBool(false, ref bitposition);
                    else
                    {
                        buffer.WriteBool(true, ref bitposition);
                        buffer.WriteHalf(ftime, ref bitposition);
                    }
                }

                bool isCrossFade = triggerType == PassThruType.CrossFade || triggerType == PassThruType.CrossFadeFixed;

                /// Write Crossfade duration
                if (isCrossFade)
                    buffer.WriteHalf(pt.duration, ref bitposition);
            }

            /// Write End of PassThru marker
            buffer.WriteBool(false, ref bitposition);

            return flags;
        }

        private void CapturePassThrus(Frame frame)
        {
            if (syncPassThrus)
                while (passThruQueue.Count > 0)
                    frame.passThrus.Enqueue(passThruQueue.Dequeue());
        }

        private void ReadPassThrus(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
        {

            while (buffer.ReadBool(ref bitposition))
            {
                /// Read type
                PassThruType triggerType = (PassThruType)buffer.Read(ref bitposition, 3);
                bool isTrigger = triggerType == PassThruType.SetTrigger || triggerType == PassThruType.ResetTrigger;

                /// Read isIndexed
                bool isIndexed = buffer.ReadBool(ref bitposition);

                /// Read Hash
                int hash;
                if (isIndexed)
                {
                    if (isTrigger)
                    {
                        hash = (int)buffer.Read(ref bitposition, bitsForTriggerIndex);
                        hash = sharedTriggIndexes[hash];
                    }
                    else
                    {
                        hash = (int)buffer.Read(ref bitposition, bitsForStateIndex);
                        hash = sharedStateIndexes[hash];
                    }
                }
                else
                {
                    hash = buffer.ReadSigned(ref bitposition, 32);
#if UNITY_EDITOR
                    Debug.LogWarning(GetType().Name +
                        " is networking a state/trigger that has not been indexed, resulting in greatly increased size. Be sure to click 'Rebuild Indexes' whenever states/triggers are added or removed. " + hash);
#endif
                }
                /// Trigger types don't use any of the floats... we are done if this is a trigger.
                if (isTrigger)
                {
                    frame.passThrus.Enqueue(new AnimPassThru(triggerType, hash, -1, -1, -1, -1));
                    continue;
                }

                /// Read layer
                int layer;
                bool useLayer = layerCount > 1;
                layer = useLayer ? ((int)buffer.Read(ref bitposition, bitsForLayerIndex) - 1) : -1;


                float normTime;
                float fixedTime;

                bool useNormalizedTime = triggerType == PassThruType.Play || triggerType == PassThruType.CrossFade;

                /// Read nonzero and normTime
                if (useNormalizedTime)
                {
                    bool nonZeroTime = buffer.ReadBool(ref bitposition);
                    normTime = (nonZeroTime) ? buffer.ReadNorm(ref bitposition, (int)passthruNormTimeCompress) : 0;
                    fixedTime = -1;
                }
                /// OR Read nonzero and fixedTime
                else
                {
                    bool nonZeroTime = buffer.ReadBool(ref bitposition);
                    fixedTime = (nonZeroTime) ? buffer.ReadHalf(ref bitposition) : 0;
                    normTime = -1;
                }

                /// Read Duration
                bool isCrossFade = triggerType == PassThruType.CrossFade || triggerType == PassThruType.CrossFadeFixed;
                float duration = isCrossFade ? buffer.ReadHalf(ref bitposition) : -1;

                frame.passThrus.Enqueue(new AnimPassThru(triggerType, hash, layer, normTime, fixedTime, duration));
            }
        }

        #endregion

#if SNS_SYNCIK

        #region IK Handling

        #region IK Inspector
		/// IK
		[HideInInspector] public bool syncIKHands = false;
		[HideInInspector] public bool syncIKFeet = false;

		[HideInInspector]
		public ElementCrusher ikFeetPosCrusher = new ElementCrusher(TRSType.Position, false)
		{
			local = true,
			XCrusher = new FloatCrusher(Axis.X, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat },
			YCrusher = new FloatCrusher(Axis.Y, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat },
			ZCrusher = new FloatCrusher(Axis.Z, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat }
		};

		[HideInInspector]
		public ElementCrusher ikHandPosCrusher = new ElementCrusher(TRSType.Position, false)
		{
			local = true,
			XCrusher = new FloatCrusher(Axis.X, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat },
			YCrusher = new FloatCrusher(Axis.Y, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat },
			ZCrusher = new FloatCrusher(Axis.Z, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat }
		};

		[HideInInspector]
		public ElementCrusher ikFeetRotCrusher = new ElementCrusher(TRSType.Quaternion, false)
		{
			local = true,
			QCrusher = new QuatCrusher(CompressLevel.uint32Med, true, false)
		};
		[HideInInspector]
		public ElementCrusher ikHandRotCrusher = new ElementCrusher(TRSType.Quaternion, false)
		{
			local = true,
			QCrusher = new QuatCrusher(CompressLevel.uint32Med, true, false)
		};

        #endregion IK Inspector

		//Vector3 heldLHandIKPos, heldLFeetIKPos, heldRHandIKPos, heldRFeetIKPos;
		//Quaternion heldLHandIKRot, heldLFeetIKRot, heldRHandIKRot, heldRFeetIKRot;
		private IKState[] heldIK = new IKState[4];
		private int startIKEnum, endIKEnum;

		private void InitIK()
		{
			startIKEnum = syncIKFeet ? 0 : syncIKHands ? 2 : 0;
			endIKEnum = syncIKHands ? 4 : syncIKFeet ? 2 : 0;

			if (syncIKFeet || syncIKHands)
				heldIK = new IKState[4]
				{
					syncIKFeet ? new IKState() : null,
					syncIKFeet ? new IKState() : null,
					syncIKHands ? new IKState() : null,
					syncIKHands ? new IKState() : null,
				};
		}

		public class IKState
		{
			public Vector3 pos;
			public Quaternion rot;
		}

		private SerializationFlags WriteIK(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
		{
			SerializationFlags flags = SerializationFlags.None;

			int end = endIKEnum;
			for (int i = startIKEnum; i < end; ++i)
			{
				var ikstate = frame.ikStates[i];
				ikFeetPosCrusher.CompressAndWrite(ikstate.pos, buffer, ref bitposition);
				ikFeetRotCrusher.CompressAndWrite(ikstate.rot, buffer, ref bitposition);

				flags = SerializationFlags.HasChanged;
			}

			return flags;
		}

		private void ReadIK(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
		{
			int end = endIKEnum;
			for (int i = startIKEnum; i < end; ++i)
			{
				var ikstate = frame.ikStates[i];
				ikstate.pos  = (Vector3)ikFeetPosCrusher.ReadAndDecompress(buffer, ref bitposition);
				ikstate.rot  = (Quaternion)ikFeetRotCrusher.ReadAndDecompress(buffer, ref bitposition);
			}
		}

		private void CaptureIK(Frame frame)
		{
			int end = endIKEnum;
			for (int i = startIKEnum; i < end; ++i)
			{
				var ikstate = frame.ikStates[i];
				var ikheld = heldIK[i];
				ikstate.pos = ikheld.pos;
				ikstate.rot = ikheld.rot;
			}
		}

		private void ApplyIK(Frame frame)
		{
			currentFrame = frame;
		}

		private void OnAnimatorIK(int layerIndex)
		{
			if (IsMine)
			{
				int end = endIKEnum;

				for (int i = startIKEnum; i < end; ++i)
				{
					var ikstate = heldIK[i];
					ikstate.pos = animator.GetIKPosition((AvatarIKGoal)i);
					ikstate.rot = animator.GetIKRotation((AvatarIKGoal)i);
				}
			}

			// Unowned objects apply the current frame values
			else
			{
				if (endIKEnum > 0 && !ReferenceEquals(currentFrame, null))
				{
					Frame currframe = this.currentFrame;
					int end = endIKEnum;
					for (int i = startIKEnum; i < end; ++i)
					{
						var ikstate = currframe.ikStates[i];
						animator.SetIKPosition((AvatarIKGoal)i, ikstate.pos);
						animator.SetIKRotation((AvatarIKGoal)i, ikstate.rot);
					}
				}
			}
		}

        #endregion
#endif

        #region State Handling

        private void CaptureStates(Frame frame)
        {
            int count = (syncLayers) ? layerCount : 1;
            for (int layer = 0; layer < count; ++layer)
            {
                frame.layerWeights[layer] = animator.GetLayerWeight(layer);

                if (animator.IsInTransition(layer))
                {
                    frame.layerIsInTransition[layer] = true;

                    //if (syncTransitions)
                    //{
                    //	AnimatorTransitionInfo transInfo = animator.GetAnimatorTransitionInfo(layer);
                    //	frame.stateHashes[layer] = transInfo.fullPathHash;
                    //	frame.normalizedTime[layer] = transInfo.normalizedTime;
                    //}
                }
                else
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                    frame.layerIsInTransition[layer] = false;
                    frame.stateHashes[layer] = stateInfo.fullPathHash;

                    float ntime = stateInfo.normalizedTime;

                    /// Even though Unity calls this NormalizedTime... it it happily wanders > 1 regularly.
                    /// Clamps looping with mod, and clamps non-looping with regular clamps.
                    if (normalizedTimeCompress == NormalizedFloatCompression.Full32 || normalizedTimeCompress == NormalizedFloatCompression.Half16)
                    {
                        /// Don't clamp if we aren't range compressing
                        frame.normalizedTime[layer] = ntime;
                    }
                    else
                    {
                        /// We need to clamp normalized values for the range based compression
                        if (stateInfo.loop)
                        {
                            /// Use modulus for looping
                            frame.normalizedTime[layer] = (ntime > 1) ? ntime % 1 : ntime;
                        }
                        else
                        {
                            /// Clamp to 1 for non-looping
                            frame.normalizedTime[layer] = ntime > 1 ? 1 : ntime < 0 ? 0 : ntime;
                        }
                    }
                }
            }
        }

        private SerializationFlags WriteStates(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
        {
            var statehashes = frame.stateHashes;
            var normaltimes = frame.normalizedTime;
            var lyerweights = frame.layerWeights;
            var lyerInTrans = frame.layerIsInTransition;

            SerializationFlags flags = SerializationFlags.None;

            int count = (syncLayers) ? layerCount : 1;
            for (int layer = 0; layer < count; ++layer)
            {

                int? layerStateHash = statehashes[layer];

                /// TODO: I forgot why I am treating a null state as a has change here. May not be the case.
                bool stateHasChange = !layerStateHash.HasValue || lastAnimationHash[layer] != layerStateHash.Value;

                /// Write State/NormTime
                if (isKeyframe || stateHasChange)
                {
                    /// Write include StateHash bool
                    buffer.WriteBool(true, ref bitposition);
                    bool isInTransition = lyerInTrans[layer];
                    buffer.WriteBool(isInTransition, ref bitposition);

                    if (isInTransition)
                    {
                        //						if (syncTransitions)
                        //						{
                        //							TransitionInfo ti;

                        //							bool isIndexed = sharedTransHashes.TryGetValue(layerStateHash.Value, out ti);
                        //							int index = (isIndexed) ? ti.index : -1;

                        //							buffer.WriteBool(isIndexed, ref bitposition);

                        //							if (isIndexed)
                        //							{
                        //								buffer.Write((uint)index, ref bitposition, bitsForTransIndex);
                        //							}
                        //							else
                        //							{
                        //#if UNITY_EDITOR
                        //								Debug.LogWarning(GetType().Name +
                        //									" is networking a transition that has not been indexed, resulting in greatly increased size. Be sure to click 'Rebuild Indexes' whenever transitions are added or removed.");
                        //#endif
                        //								Debug.LogError("Unknwn Trans " + layerStateHash.Value);
                        //								buffer.WriteSigned(layerStateHash.Value, ref bitposition, 32);
                        //							}

                        //							/// Write ntime for transition
                        //							/// We don't bother with time for index 0 - indicates an unusable transition
                        //							if (index != 0)
                        //							{
                        //								WriteNorm(normaltimes[layer], buffer, ref bitposition, normalizedTimeCompress);
                        //							}
                        //						}
                    }
                    else
                    {

                        int index = sharedStateIndexes.IndexOf(layerStateHash.Value);
                        bool useIndex = index != -1;

                        buffer.WriteBool(useIndex, ref bitposition);

                        if (useIndex)
                            buffer.Write((uint)sharedStateIndexes.IndexOf(layerStateHash.Value), ref bitposition, bitsForStateIndex);
                        else
                        {
#if UNITY_EDITOR
                            Debug.LogWarning(GetType().Name + " on GameObject '" + name +
                                "' is networking a state that has not been indexed, resulting in greatly increased size. Be sure to click 'Rebuild Indexes' whenever states are added or removed.");
#endif
                            buffer.WriteSigned(layerStateHash.Value, ref bitposition, 32);
                        }

                        /// Write ntime for state
                        float posnorm = (normaltimes[layer] + 1) * .5f;
                        buffer.WriteNorm(posnorm, ref bitposition, (int)normalizedTimeCompress);
                    }

                    lastAnimationHash[layer] = layerStateHash.HasValue ? layerStateHash.Value : 0;

                    flags |= SerializationFlags.HasContent;
                }
                else
                    buffer.WriteBool(false, ref bitposition);

                /// Write LayerWeights
                /// Write LayerWeights
                if (syncLayerWeights && layer != 0)
                {
                    float? weight = lyerweights[layer];

                    /// If value is 1, we only set this bool and are done.
                    if (weight == 1)
                    {
                        /// Write weight == 1 bool
                        buffer.WriteBool(true, ref bitposition);
                        flags |= SerializationFlags.HasContent;
                    }
                    else
                    {
                        /// Write weight != 1 bool
                        buffer.WriteBool(false, ref bitposition);

                        /// If value is 0, our second bool as true will indicate that, and we will be done.
                        if (weight == 0)
                        {
                            /// Write weight == 0 bool
                            buffer.WriteBool(true, ref bitposition);
                            flags |= SerializationFlags.HasContent;
                        }
                        else
                        {
                            /// Write weight != 0 bool
                            buffer.WriteBool(false, ref bitposition);

                            /// Get the compressed value to see if it has changed
                            int layerWeightBits = (int)layerWeightCompress;
                            var cLayerWeight = weight.Value.CompressNorm(layerWeightBits);

                            if (isKeyframe || (lastLayerWeight[layer] != cLayerWeight))
                            {
                                /// Write bool for included weight
                                buffer.WriteBool(true, ref bitposition);
                                buffer.Write(cLayerWeight, ref bitposition, layerWeightBits);
                                lastLayerWeight[layer] = cLayerWeight;
                                flags |= SerializationFlags.HasContent;
                            }
                            else
                            {
                                /// Write bool for not included weight
                                buffer.WriteBool(false, ref bitposition);
                            }
                        }
                    }
                }
            }
            return flags;
        }

        private void ReadStates(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
        {

            var statehashes = frame.stateHashes;
            var normaltimes = frame.normalizedTime;
            var lyerweights = frame.layerWeights;
            var lyerInTrans = frame.layerIsInTransition;

            int count = (syncLayers) ? layerCount : 1;
            for (int layer = 0; layer < count; ++layer)
            {
                bool stateHasChange = buffer.ReadBool(ref bitposition);

                if (stateHasChange)
                {
                    bool layerIsInTransition = buffer.ReadBool(ref bitposition);
                    lyerInTrans[layer] = layerIsInTransition;

                    if (layerIsInTransition)
                    {
                        //if (syncTransitions)
                        //{
                        //	/// Complex mess that determines if the hash was sent as an index or full hash
                        //	bool isIndexed = buffer.ReadBool(ref bitposition);

                        //	int hash = (isIndexed) ? (int)buffer.Read(ref bitposition, bitsForTransIndex) : buffer.ReadSigned(ref bitposition, 32);
                        //	if (isIndexed)
                        //	{
                        //		hash = sharedTransIndexes[hash].hash;
                        //	}

                        //	statehashes[layer] = hash;
                        //	normaltimes[layer] = (hash != 0) ? ReadNorm(buffer, ref bitposition, normalizedTimeCompress) : 0;
                        //}
                    }
                    else
                    {
                        bool isIndexed = buffer.ReadBool(ref bitposition);

                        int hash = (isIndexed) ? (int)buffer.Read(ref bitposition, bitsForStateIndex) : buffer.ReadSigned(ref bitposition, 32);
                        if (isIndexed)
                        {
                            hash = sharedStateIndexes[hash];
                        }

                        statehashes[layer] = hash;
                        var norm = buffer.ReadNorm(ref bitposition, (int)normalizedTimeCompress) * 2 - 1;
                        normaltimes[layer] = norm;
                    }
                }

                if (syncLayerWeights && layer != 0)
                {
                    bool isOne = buffer.ReadBool(ref bitposition);
                    if (isOne)
                        lyerweights[layer] = 1;
                    else
                    {
                        bool isZero = buffer.ReadBool(ref bitposition);
                        if (isZero)
                            lyerweights[layer] = 0;
                        else
                        {
                            bool weightHasChange = buffer.ReadBool(ref bitposition);
                            if (weightHasChange)
                            {
                                lyerweights[layer] = buffer.ReadNorm(ref bitposition, (int)layerWeightCompress);
                            }
                            else
                            {
                                lyerweights[layer] = null;
                            }
                        }
                    }
                }
            }
        }

        private void ApplyState(Frame applyFrame/*, Frame invalidateFrame*/)
        {

            int count = (syncLayers) ? layerCount : 1;
            for (int layer = 0; layer < count; ++layer)
            {
                ///// Set frame/layer as no longer valid (prevents from missed incoming frames finding this value again) 
                ///// Really need to add some masks or make these nullable
                //invalidateFrame.stateHashes[layer] = null;
                //invalidateFrame.normalizedTime[layer] = 0;
                //invalidateFrame.layerWeights[layer] = null;

                int? statehash = applyFrame.stateHashes[layer];
                bool isTransition = applyFrame.layerIsInTransition[layer];

                if (statehash.HasValue)
                {
                    if (isTransition)
                    {
                        //if (syncTransitions)
                        //{
                        //	TransitionInfo ti;
                        //	bool foundhash = sharedTransHashes.TryGetValue(statehash.Value, out ti);
                        //	if (!foundhash)
                        //	{
                        //		Debug.LogWarning("Unknown Transition " + statehash.Value + ", please report this to Davin Carten (emotitron)");
                        //		continue;
                        //	}

                        //	if (ti.durationIsFixed)
                        //		animator.CrossFadeInFixedTime(ti.destination, ti.duration, layer, applyFrame.normalizedTime[layer] * ti.duration);
                        //	//animator.Play(ti.destination, layer, applyFrame.normalizedTime[layer] * ti.duration);
                        //	else
                        //		animator.CrossFade(ti.destination, ti.duration, layer, applyFrame.normalizedTime[layer]);
                        //}
                    }

                    /// TODO: 0 check may not be a thing any more
                    else if (statehash.Value != 0)
                    {
                        animator.Play(statehash.Value, layer, applyFrame.normalizedTime[layer]);
                    }

                    if (syncLayerWeights)
                    {
                        float? layerWeight = applyFrame.layerWeights[layer];

                        if (layerWeight.HasValue)
                            animator.SetLayerWeight(layer, layerWeight.Value);
                    }
                }
            }
        }

        #endregion  // end state handling

        #region LayerWeight Handling

        private void CaptureLayerWeights(Frame frame)
        {
            int count = (syncLayers) ? layerCount : 1;
            for (int layer = 0; layer < count; ++layer)
            {
                frame.layerWeights[layer] = animator.GetLayerWeight(layer);
            }
        }

        private SerializationFlags WriteLayerWeights(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
        {
            var lyerweights = frame.layerWeights;

            SerializationFlags flags = SerializationFlags.None;

            int count = (syncLayers) ? layerCount : 1;
            for (int layer = 0; layer < count; ++layer)
            {

                /// Write LayerWeights
                if (syncLayerWeights && layer != 0)
                {
                    float? weight = lyerweights[layer];

                    /// If value is 1, we only set this bool and are done.
                    if (weight == 1)
                    {
                        /// Write weight == 1 bool
                        buffer.WriteBool(true, ref bitposition);
                        flags |= SerializationFlags.HasContent;
                    }
                    else
                    {
                        /// Write weight != 1 bool
                        buffer.WriteBool(false, ref bitposition);

                        /// If value is 0, our second bool as true will indicate that, and we will be done.
                        if (weight == 0)
                        {
                            /// Write weight == 0 bool
                            buffer.WriteBool(true, ref bitposition);
                            flags |= SerializationFlags.HasContent;
                        }
                        else
                        {
                            /// Write weight != 0 bool
                            buffer.WriteBool(false, ref bitposition);

                            /// Get the compressed value to see if it has changed
                            int layerWeightBits = (int)layerWeightCompress;
                            var cLayerWeight = weight.Value.CompressNorm(layerWeightBits);

                            if (isKeyframe || (lastLayerWeight[layer] != cLayerWeight))
                            {
                                /// Write bool for included weight
                                buffer.WriteBool(true, ref bitposition);
                                buffer.Write(cLayerWeight, ref bitposition, layerWeightBits);
                                lastLayerWeight[layer] = cLayerWeight;
                                flags |= SerializationFlags.HasContent;
                            }
                            else
                            {
                                /// Write bool for not included weight
                                buffer.WriteBool(false, ref bitposition);
                            }
                        }
                    }
                }
            }
            return flags;
        }


        private SerializationFlags ReadLayerWeights(Frame frame, byte[] buffer, ref int bitposition, bool isKeyframe)
        {
            var lyerweights = frame.layerWeights;

            SerializationFlags flags = SerializationFlags.None;

            int count = (syncLayers) ? layerCount : 1;
            for (int layer = 0; layer < count; ++layer)
            {
                if (syncLayerWeights && layer != 0)
                {
                    bool isOne = buffer.ReadBool(ref bitposition);
                    if (isOne)
                    {
                        lyerweights[layer] = 1;
                        flags |= SerializationFlags.HasContent;
                    }
                    else
                    {
                        bool isZero = buffer.ReadBool(ref bitposition);
                        if (isZero)
                        {
                            lyerweights[layer] = 0;
                            flags |= SerializationFlags.HasContent;
                        }
                        else
                        {
                            bool weightHasChange = buffer.ReadBool(ref bitposition);
                            if (weightHasChange)
                            {
                                lyerweights[layer] = buffer.ReadNorm(ref bitposition, (int)layerWeightCompress);
                                flags |= SerializationFlags.HasContent;
                            }
                            else
                            {
                                lyerweights[layer] = null;
                            }
                        }
                    }
                }
            }

            return SerializationFlags.HasContent;
        }


        private void ApplyLayerWeights(Frame applyFrame/*, Frame invalidateFrame*/)
        {
            int count = (syncLayers) ? layerCount : 1;
            for (int layer = 0; layer < count; ++layer)
            {
                ///// Set frame/layer as no longer valid (prevents from missed incoming frames finding this value again) 
                ///// Really need to add some masks or make these nullable
                ///// TODO: May not be needed if frames are properly being flagged as hasContent = false
                //invalidateFrame.layerWeights[layer] = null;

                if (syncLayerWeights)
                {
                    float? layerWeight = applyFrame.layerWeights[layer];

                    if (layerWeight.HasValue)
                        animator.SetLayerWeight(layer, layerWeight.Value);
                }
            }
        }



        #endregion

        #endregion

        #region Snapshot / Interpolate / Extrapolate

        //private bool hasInitialSnapshot;

        /// <summary>
        /// Advance the buffered state, getting a new target.
        /// </summary>
        public override bool OnSnapshot(int prevFrameId, int snapFrameId, int targFrameId, bool prevIsValid, bool snapIsValid, bool targIsValid)
        {

            /// TODO: change this to snapFrame and put after base call
            /// First Apply the previous targ, for end of interpolation application.
            bool wasValid = !ReferenceEquals(snapFrame, null) && snapFrame.content != 0; // netObj.validFrames.Get(targF.frameId);

            if (wasValid)
                ApplyFrame(snapFrame);

            bool ready = base.OnSnapshot(prevFrameId, snapFrameId, targFrameId, prevIsValid, snapIsValid, targIsValid);

            if (!ready)
                return false;

            /// End of Interpolation triggers and Events
            /// Since we are interpolating, to line up timing of all networked objects
            /// The actual occurance of a frame is when it ARRIVES at target.

            /// TODO: don't know if states act like triggers or params yet.

            CompleteTargetParameters();

            //if (snapFrame.content == FrameContents.Empty)
            //    Debug.LogError("Empty Animation Frame trying to Apply");

            ///// TODO: this is questionable
            //if (snapFrame.content != 0)
            //    InterpolateParams(0);

            return true;
        }

        private void ApplyFrame(Frame frame)
        {
            if (syncStates)
                ApplyState(frame/*, pre2Frame*/);

            if (syncLayerWeights)
                ApplyLayerWeights(frame/*, pre2Frame*/);

            /// triggers and crossfades don't extrapolate. Apply at end of interpolation.
            ExecutePassThruQueue(frame);

            InterpolateParams(0);

#if SNS_SYNCIK
			/// Store the frame for the deferred application of OnAnimatorIK
			if (endIKEnum > 0)
				ApplyIK(frame);
#endif
        }


        public override bool OnInterpolate(int snapFrameId, int targFrameId, float t)
        {
            bool ready = base.OnInterpolate(snapFrameId, targFrameId, t);

            if (!ready)
                return false;

            //if (amActingAuthority)
            //	return false;

            if (ReferenceEquals(targFrame, null))
                return false;

            if (targFrame.content == 0)
                return false;

            if (syncParams)
            {
                InterpolateParams(t);
            }
            return true;
        }

        ///  UNTESTED
        protected override void InterpolateFrame(Frame targframe, Frame startframe, Frame endframe, float t)
        {
            /// TODO: This currently just copies the last value
            targframe.CopyFrom(endframe);

            InterpolateState(targframe, startframe, endframe, t);
            //return FrameContents.Partial;
        }

        protected override void ExtrapolateFrame(Frame prevframe, Frame snapframe, Frame targframe)
        {
            /// TODO: try changing this from partial to extrapolated
            targframe.content = FrameContents.Partial;
            ExtrapolateParams(prevframe, snapframe, targframe);
            // TODO: Make this rewindable by passing the frames rather than assuming current
            ExtrapolateState();
        }

        private void ExtrapolateState()
        {
            /// Currently states are not extrapolated, they are just set to null to indicate do nothing.
            int count = (syncLayers) ? layerCount : 1;
            for (int layer = 0; layer < count; ++layer)
            {
                /// TEST
                targFrame.stateHashes[layer] = null;

                /// Old State Extrap code
                //var pre1hash = pre1Frame.stateHashes[layer];
                //var snaphash = snapFrame.stateHashes[layer];

                //targFrame.stateHashes[layer] = snaphash;

                //float snapTime = snapFrame.normalizedTime[layer];

                //if (pre1hash != snaphash && snapTime != 0)
                //{
                //	float delta = snapTime - pre1Frame.normalizedTime[layer];
                //	targFrame.normalizedTime[layer] = snapFrame.normalizedTime[layer] + delta;
                //	//Debug.LogError("<color=green>Good State Extrap</color> " + snapF.normalizedTime[layer]+ " " + targF.normalizedTime[layer]);
                //}
                //else
                //{
                //	targFrame.normalizedTime[layer] = snapTime;
                //	//Debug.LogError("<color=red>Bad State Extrap</color> " + snapTime);
                //}
            }
        }

        /// <summary>
        /// Recreate a missing frame using the current frame and a future frame that has arrived.
        /// </summary>
        private void InterpolateState(Frame targFrame, Frame strFrame, Frame endFrame, float t)
        {
            int count = (syncLayers) ? layerCount : 1;
            for (int layer = 0; layer < count; ++layer)
            {
                var strhash = strFrame.stateHashes[layer];
                var endhash = endFrame.stateHashes[layer];

                targFrame.stateHashes[layer] = endhash;

                float strTime = strFrame.normalizedTime[layer];
                float endTime = endFrame.normalizedTime[layer];

                if (strhash != endhash && strTime != 0)
                {
                    targFrame.normalizedTime[layer] = Mathf.LerpUnclamped(strTime, endTime, t);
                }
                else
                {
                    targFrame.normalizedTime[layer] = strTime;
                }
            }
        }
        #endregion

    }
}


