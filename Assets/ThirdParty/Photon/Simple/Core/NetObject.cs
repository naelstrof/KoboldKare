// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Simple.Internal;

#if GHOST_WORLD
using Photon.Pun.Simple.GhostWorlds;
#endif

using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Utilities;
using Photon.Compression.Internal;
using Photon.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple

{
    public enum RigidbodyType { None, RB, RB2D }

    [DisallowMultipleComponent]

    [HelpURL(SimpleDocsURLS.OVERVIEW_PATH)]
    [RequireComponent(typeof(PhotonView))]
    public class NetObject : MonoBehaviour
        , IMatchmakingCallbacks
        , IOnPhotonViewPreNetDestroy
        , IOnPhotonViewOwnerChange
        , IOnPhotonViewControllerChange
        , IOnPreUpdate
        , IOnPreSimulate
        , IOnPostSimulate
        , IOnQuantize
        , IOnIncrementFrame
        , IOnPreQuit

    {
        #region Inspector Fields


        [SerializeField]
        [HideInInspector]
        [Tooltip("Enabling this will tell the serializer to completely exclude this net object from serialization if none of its content has changed. " +
            "While this will remove heartbeat data, It may also produce undesirable extrapolation and buffer resizing behavior, as receiving clients will see this as a network failure.")]
        protected bool skipWhenEmpty = false;
        public bool SkipWhenEmpty { get { return skipWhenEmpty; } }

        [SerializeField]
        [HideInInspector]
        [Tooltip("Controls if incoming NetObject updates from a current non-owner/controller should be ignored. " +
            "The exception to this is if the controller is currently null/-1, which indicates that the initial ownership messages from the Master has not yet arrived or been applied, " +
            "in which case the first arriving updates originating Player will be treated as the current Controller, regardless of this setting.")]
        protected bool ignoreNonControllerUpdates = true;
        public bool IgnoreNonControllerUpdates { get { return ignoreNonControllerUpdates; } }

        [SerializeField]
        [HideInInspector]
        [Tooltip("When enabled, if a frame update for this Net Object arrives AFTER that frame number has already been applied (it will have been reconstructed/extrapolated with a best guess)," +
            " the incoming update will be immediately applied, and all frames between that frame and the current snapshot will be reapplied.")]
        protected bool resimulateLateArrivals = true;
        public bool ResimulateLateArrivals { get { return resimulateLateArrivals; } }

        protected Rigidbody _rigidbody;
        public Rigidbody Rb { get { return _rigidbody; } }

        protected Rigidbody2D _rigidbody2D;
        public Rigidbody2D Rb2D { get { return _rigidbody2D; } }

        #endregion

        #region Static NetObject Lookups and Pools

        public static NonAllocDictionary<int, NetObject> activeControlledNetObjs = new NonAllocDictionary<int, NetObject>();
        public static NonAllocDictionary<int, NetObject> activeUncontrolledNetObjs = new NonAllocDictionary<int, NetObject>();

        private static Queue<NetObject> pendingActiveNetObjDictChanges = new Queue<NetObject>();

        /// <summary>
        /// Prevent objects from being added or removed from the activeControlledNetObjs dictionary. Any changes are queued and applied after unlock.
        /// </summary>
        public static bool NetObjDictsLocked
        {
            set
            {
                netObjDictsLocked = value;

                if (!value)
                {
                    for (int i = 0, cnt = pendingActiveNetObjDictChanges.Count; i < cnt; ++i)
                    {
                        var no = pendingActiveNetObjDictChanges.Dequeue();
                        no.DetermineActiveAndControlled(no.photonView.IsMine);
                    }
                }
            }
        }
        private static bool netObjDictsLocked;

        #endregion

        #region Collider Lookup

        /// TODO: Pool these?
        [System.NonSerialized] public Dictionary<Component, int> colliderLookup = new Dictionary<Component, int>();
        [System.NonSerialized] public List<Component> indexedColliders = new List<Component>();
        [System.NonSerialized] public int bitsForColliderIndex;

        #endregion

        #region ObjReady

        [System.NonSerialized] public FastBitMask128 frameValidMask;
        /// <summary>
        /// Record of the connection that produced the each deserialized frame. Index is the frameId.
        /// </summary>
        [System.NonSerialized] public int[] originHistory;

        [System.NonSerialized] public FastBitMask128 syncObjReadyMask;
        [System.NonSerialized] public FastBitMask128 packObjReadyMask;
        [System.NonSerialized] private readonly Dictionary<Component, int> packObjIndexLookup = new Dictionary<Component, int>();

        public void OnSyncObjReadyChange(SyncObject sobj, ReadyStateEnum readyState)
        {
            int syncObjIndex = sobj.SyncObjIndex;

            if (readyState != ReadyStateEnum.Unready)
            {
                syncObjReadyMask[syncObjIndex] = true;
            }
            else
            {
                syncObjReadyMask[syncObjIndex] = false;
            }

            AllObjsAreReady = syncObjReadyMask.AllAreTrue && packObjReadyMask.AllAreTrue;
        }

        public void OnPackObjReadyChange(Component pobj, ReadyStateEnum readyState)
        {
            int packObjIndex = packObjIndexLookup[pobj];

            if (readyState != ReadyStateEnum.Unready)
            {
                packObjReadyMask[packObjIndex] = true;
            }
            else
            {
                packObjReadyMask[packObjIndex] = false;
            }

            AllObjsAreReady = syncObjReadyMask.AllAreTrue && packObjReadyMask.AllAreTrue;
        }

        private bool _allObjsAreReady;
        public bool AllObjsAreReady
        {
            get
            {
                return photonView.IsMine ? true : _allObjsAreReady;
            }
            private set
            {
                //if (GetComponent<SyncPickup>())
                //    Debug.Log(name + " <b>ALL READY Try</b> " + _allObjsAreReady + " : " + value);

                if (_allObjsAreReady == value)
                    return;

                _allObjsAreReady = value;

                for (int i = 0; i < onNetObjReadyCallbacks.Count; ++i)
                    onNetObjReadyCallbacks[i].OnNetObjReadyChange(value);

                packObjReadyMask.SetAllTrue();
                syncObjReadyMask.SetAllTrue();
            }
        }

        #endregion ObjReady

        #region Cached

        protected int viewID;
        public int ViewID { get { return viewID; } }

#if GHOST_WORLD
        [System.NonSerialized] private Haunted haunted;
#endif

        [System.NonSerialized] public PhotonView photonView;

        #endregion

        #region Startup / Shutdown

#if UNITY_EDITOR

        private void Reset()
        {
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(this, false);


            if (!_rigidbody)
                _rigidbody = transform.GetNestedComponentInChildren<Rigidbody, NetObject>(true);

            if (!_rigidbody)
                _rigidbody2D = transform.GetNestedComponentInChildren<Rigidbody2D, NetObject>(true);
        }
#endif

        protected void Awake()
        {
            //validFrames = new BitArray(SimpleSyncSettings.frameCount + 1);
            frameValidMask = new FastBitMask128(TickEngineSettings.frameCount);
            originHistory = new int[TickEngineSettings.frameCount];
            for (int i = 0, cnt = TickEngineSettings.frameCount; i < cnt; ++i)
                originHistory[i] = -1;


            if (!_rigidbody)
                _rigidbody = transform.GetNestedComponentInChildren<Rigidbody, NetObject>(true);

            if (!_rigidbody)
                _rigidbody2D = transform.GetNestedComponentInChildren<Rigidbody2D, NetObject>(true);

            photonView = GetComponent<PhotonView>();
            if (photonView == null)
            {
                Debug.LogWarning("PhotonView missing from NetObject on GameObject '" + name + "'. One will be added to suppress errors, but this object will likely not be networked correctly.");
                photonView = gameObject.AddComponent<PhotonView>();
            }

            CollectAndReorderInterfaces();

            this.IndexColliders();

            transform.GetNestedComponentsInChildren<IOnAwake, NetObject>(onAwakeCallbacks, true);

            /// OnAwake Callbacks
            for (int i = 0, cnt = onAwakeCallbacks.Count; i < cnt; ++i)
                onAwakeCallbacks[i].OnAwake();
        }

        private void Start()
        {
            transform.GetNestedComponentsInChildren<IOnStart, NetObject>(onStartCallbacks, true);

            /// OnStart Callbacks
            for (int i = 0, cnt = onStartCallbacks.Count; i < cnt; ++i)
                onStartCallbacks[i].OnStart();

            if (PhotonNetwork.IsConnectedAndReady)
                OnChangeAuthority(photonView.IsMine, true);

            viewID = photonView.ViewID;
        }

        private void OnEnable()
        {
            NetMasterCallbacks.RegisterCallbackInterfaces(this, true);

            PhotonNetwork.AddCallbackTarget(this);
            photonView.AddCallbackTarget(this);

            DetermineActiveAndControlled(photonView.IsMine);
            /// OnPostEnable Callbacks
            for (int i = 0, cnt = onEnableCallbacks.Count; i < cnt; ++i)
                onEnableCallbacks[i].OnPostEnable();
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            photonView.RemoveCallbackTarget(this);
            NetMasterCallbacks.RegisterCallbackInterfaces(this, false);

            DetermineActiveAndControlled(photonView.IsMine);

            /// OnPostDisable Callback
            for (int i = 0, cnt = onDisableCallbacks.Count; i < cnt; ++i)
                onDisableCallbacks[i].OnPostDisable();
        }

        public void OnPreQuit()
        {
            /// OnQuit Callbacks
            for (int i = 0, cnt = onPreQuitCallbacks.Count; i < onPreQuitCallbacks.Count; ++i)
                onPreQuitCallbacks[i].OnPreQuit();
        }


        public void OnPreNetDestroy(PhotonView rootView)
        {
            var rootNetObj = rootView.GetComponent<NetObject>();

            if (rootNetObj == null)
                return;

            /// OnDestroy Callbacks
            for (int i = 0, cnt = onPreNetDestroyCallbacks.Count; i < cnt; ++i)
                onPreNetDestroyCallbacks[i].OnPreNetDestroy(rootNetObj);
        }

        private void OnDestroy()
        {
            if (activeControlledNetObjs.ContainsKey(photonView.ViewID))
                activeControlledNetObjs.Remove(photonView.ViewID);

            if (activeUncontrolledNetObjs.ContainsKey(photonView.ViewID))
                activeUncontrolledNetObjs.Remove(photonView.ViewID);
        }

        /// TODO: likely not needed if moving UnmountAll to OnDisable
        /// <summary>
        /// Destroy an object in a way that respects Simple.
        /// </summary>
        public virtual void PrepareForDestroy()
        {
            var mounts = GetComponent<MountsManager>();
            if (mounts)
                mounts.UnmountAll();
        }

        #region PUN Callbacks


        public void OnOwnerChange(Realtime.Player newOwner, Realtime.Player previousOwner)
        {

#if UNITY_EDITOR
            Debug.Log(Time.time + " '" + name + "' ViewId: " + photonView.ViewID + " <b>Owner changed</b> from " +
                (previousOwner == null ? "null" : previousOwner.ActorNumber.ToString()) + " to " +
                (newOwner == null ? "null" : newOwner.ActorNumber.ToString()) +
                " IsMine: " + photonView.IsMine);
#endif

            OnChangeAuthority(photonView.IsMine, true);
        }

        public void OnControllerChange(Realtime.Player newController, Realtime.Player previousController)
        {

#if UNITY_EDITOR
            Debug.Log(Time.time + " '" + name + "' ViewId: " + photonView.ViewID + " <b>Controller changed</b> from " +
                (previousController == null ? "null" : previousController.ActorNumber.ToString()) + " to " + 
                (newController == null ? "null" : newController.ActorNumber.ToString()) + 
                " IsMine: " + photonView.IsMine);
#endif

            OnChangeAuthority(photonView.IsMine, true);
        }

//        public void OnOwnershipRequest(PhotonView targetView, Photon.Realtime.Player requestingPlayer) { }

//        public void OnOwnershipTransferred(PhotonView targetView, Photon.Realtime.Player previousOwner)
//        {

//            //if (GetComponent<SyncPickup>())
//            //    Debug.LogError(Time.time + " " + name + " Ownership Transfer");

//            /// Only respond if this pv changed owners
//            if (targetView != photonView)
//                return;

//#if UNITY_EDITOR
//            Debug.Log(Time.time + " " + name + " " + photonView.ViewID + " <b>Ownership Changed</b> " + photonView.OwnerActorNr);
//#endif

//            OnChangeAuthority(photonView.IsMine, true);

//        }

        public void OnFriendListUpdate(List<FriendInfo> friendList) { }
        public void OnCreatedRoom() { }
        public void OnCreateRoomFailed(short returnCode, string message) { }
        public void OnJoinedRoom()
        {

            transform.GetNestedComponentsInChildren<IOnJoinedRoom, NetObject>(onJoinedRoomCallbacks, true);

            /// OnAwake Callbacks
            for (int i = 0, cnt = onJoinedRoomCallbacks.Count; i < cnt; ++i)
                onJoinedRoomCallbacks[i].OnJoinedRoom();

            OnChangeAuthority(photonView.IsMine, true);
        }

        public void OnJoinRoomFailed(short returnCode, string message) { }
        public void OnJoinRandomFailed(short returnCode, string message) { }
        public void OnLeftRoom() { }

        /// <summary>
        /// Adds and Removes this NetObject from the active/owned list based on IsMine and ActiveAndEnabled.
        /// Effectively caches which netObjects are part of serialization and such. Call this any time a change to enabled or owners change.
        /// </summary>
        private void DetermineActiveAndControlled(bool amController)
        {
            int key = photonView.ViewID;

            if (netObjDictsLocked)
            {
                pendingActiveNetObjDictChanges.Enqueue(this);
                return;
            }

            bool InControllerOfList = activeControlledNetObjs.ContainsKey(key);
            bool InOthersList = activeUncontrolledNetObjs.ContainsKey(key);

            if (isActiveAndEnabled)
            {
                if (amController)
                {
                    if (!InControllerOfList)
                        activeControlledNetObjs.Add(key, this);

                    if (InOthersList)
                        activeUncontrolledNetObjs.Remove(key);

                }
                else
                {

                    if (InControllerOfList)
                        activeControlledNetObjs.Remove(key);

                    if (!InOthersList)
                        activeUncontrolledNetObjs.Add(key, this);
                }
            }
            else
            {
               
                if (InControllerOfList)
                    activeControlledNetObjs.Remove(key);

                if (InOthersList)
                    activeUncontrolledNetObjs.Remove(key);
            }

            //if (GetComponent<SyncPickup>())
            //    Debug.Log(Time.time + " " + name + " <b>Change Controller</b>" + amController);
        }


        #endregion

        public void OnChangeAuthority(bool isMine, bool controllerHasChanged)
        {
            DetermineActiveAndControlled(isMine);

            /// OnAuthorityChanged Callbacks
            for (int i = 0, cnt = onAuthorityChangedCallbacks.Count; i < cnt; ++i)
                onAuthorityChangedCallbacks[i].OnAuthorityChanged(isMine, controllerHasChanged);

            /// Owner assumes all objects are ready, since it is the state authority
            if (isMine)
            {
                AllObjsAreReady = true;
            }
        }

        #endregion

        #region Outgoing Callbacks

        private static List<Component> reusableComponents = new List<Component>();

        private static readonly List<IOnJoinedRoom> onJoinedRoomCallbacks = new List<IOnJoinedRoom>();
        private static readonly List<IOnAwake> onAwakeCallbacks = new List<IOnAwake>();
        private static readonly List<IOnStart> onStartCallbacks = new List<IOnStart>();

        private readonly List<IOnEnable> onEnableCallbacks = new List<IOnEnable>();
        private readonly List<IOnDisable> onDisableCallbacks = new List<IOnDisable>();
        public readonly List<IOnPreUpdate> onPreUpdateCallbacks = new List<IOnPreUpdate>();

        public readonly List<IOnAuthorityChanged> onAuthorityChangedCallbacks = new List<IOnAuthorityChanged>();

        public readonly List<IOnNetSerialize> onNetSerializeCallbacks = new List<IOnNetSerialize>();
        public readonly List<IOnCriticallyLateFrame> onCriticallyLateFrameCallbacks = new List<IOnCriticallyLateFrame>();
        public readonly List<IOnIncrementFrame> onIncrementFramesCallbacks = new List<IOnIncrementFrame>();
        public readonly List<IOnSnapshot> onSnapshotCallbacks = new List<IOnSnapshot>();
        public readonly List<IOnQuantize> onQuantizeCallbacks = new List<IOnQuantize>();
        public readonly List<IOnInterpolate> onInterpolateCallbacks = new List<IOnInterpolate>();
        public readonly List<IOnCaptureState> onCaptureCurrentStateCallbacks = new List<IOnCaptureState>();
        public readonly List<IOnPreSimulate> onPreSimulateCallbacks = new List<IOnPreSimulate>();
        public readonly List<IOnPostSimulate> onPostSimulateCallbacks = new List<IOnPostSimulate>();
        public readonly List<IOnPreQuit> onPreQuitCallbacks = new List<IOnPreQuit>();
        public readonly List<IOnPreNetDestroy> onPreNetDestroyCallbacks = new List<IOnPreNetDestroy>();

        private readonly List<IOnNetObjReady> onNetObjReadyCallbacks = new List<IOnNetObjReady>();

        private readonly List<SyncObject> syncObjects = new List<SyncObject>();
        private readonly List<PackObjRecord> packObjects = new List<PackObjRecord>();

        private class PackObjRecord
        {
            public Component component;
            public PackObjectDatabase.PackObjectInfo info;
            public PackFrame[] packFrames;
            public FastBitMask128 prevReadyMask;
            public FastBitMask128 readyMask;
            public IPackObjOnReadyChange onReadyCallback;
        }

        /// <summary>
        /// Find all of the callback interfaces on children, and add them to the callback list, respecting the ApplyOrder value.
        /// </summary>
        private void CollectAndReorderInterfaces()
        {

            /// Collect all components to avoid doing this over and over
            //GetComponentsInChildren(true, reusableFindSyncObjs);
            transform.GetNestedComponentsInChildren<Component, NetObject>(reusableComponents);

            for (int order = 0, cnt = reusableComponents.Count; order <= ApplyOrderConstants.MAX_ORDER_VAL; ++order)
            {
                for (int index = 0; index < cnt; ++index)
                {
                    var comp = reusableComponents[index];
                    /// Don't include self, or you will stack overflow hard.
                    if (comp == this)
                        continue;

                    var iApplyOrder = comp as IApplyOrder;

                    /// Apply any objects without IApplyOrder to the middle timing of 5
                    if (ReferenceEquals(iApplyOrder, null))
                    {
                        if (order == ApplyOrderConstants.DEFAULT)
                        {
                            
                            AddInterfaces(comp);
                            AddPackObjects(comp);
                        }
                    }
                    else
                    if (iApplyOrder.ApplyOrder == order)
                    {
                        AddInterfaces(comp);
                    }
                }
            }

            syncObjReadyMask = new FastBitMask128(syncObjects.Count);
            packObjReadyMask = new FastBitMask128(packObjects.Count);

            for (int i = 0; i < syncObjects.Count; ++i)
            {
                var so = syncObjects[i];
                so.SyncObjIndex = i;

                ///// Add NetObj to ReadyStateChange callback
                ///// and Simulate firing of ReadyState changes on syncObjs that we may have missed due to order of exec.
                //so.onReadyCallbacks += OnSyncObjReadyChange;
                OnSyncObjReadyChange(so, so.ReadyState);
            }

        }



        /// <summary>
        /// Remove a component from all NetObj callback lists.
        /// </summary>
        /// <param name="comp"></param>
        public void RemoveInterfaces(Component comp) { AddInterfaces(comp, true); }

        private void AddInterfaces(Component comp, bool remove = false)
        {
            AddInterfaceToList(comp, onEnableCallbacks, remove);
            AddInterfaceToList(comp, onDisableCallbacks, remove);
            AddInterfaceToList(comp, onPreUpdateCallbacks, remove);

            AddInterfaceToList(comp, onAuthorityChangedCallbacks, remove);

            AddInterfaceToList(comp, onCaptureCurrentStateCallbacks, remove);

            AddInterfaceToList(comp, onNetSerializeCallbacks, remove, true);
            AddInterfaceToList(comp, onQuantizeCallbacks, remove, true);
            AddInterfaceToList(comp, onIncrementFramesCallbacks, remove, true);
            AddInterfaceToList(comp, onSnapshotCallbacks, remove, true);
            AddInterfaceToList(comp, onCriticallyLateFrameCallbacks, remove, true);
            AddInterfaceToList(comp, onInterpolateCallbacks, remove, true);

            AddInterfaceToList(comp, onPreSimulateCallbacks, remove);
            AddInterfaceToList(comp, onPostSimulateCallbacks, remove);
            AddInterfaceToList(comp, onPreQuitCallbacks, remove);
            AddInterfaceToList(comp, onPreNetDestroyCallbacks, remove);

            AddInterfaceToList(comp, onNetObjReadyCallbacks, remove);
            AddInterfaceToList(comp, syncObjects, remove);


        }

        private void AddInterfaceToList<T>(object comp, List<T> list, bool remove, bool checkSerializationOptional = false) where T : class
        {
            T cb = comp as T;
            if (!ReferenceEquals(cb, null))
            {
                /// Check if this syncObj is flagged to be excluded from serialization
                if (checkSerializationOptional)
                {
                    var optionalCB = cb as ISerializationOptional;
                    if (!ReferenceEquals(optionalCB, null))
                    {
                        if (optionalCB.IncludeInSerialization == false)
                            return;
                    }
                }

                T tcomp = comp as T;
                if (remove && list.Contains(tcomp))
                    list.Remove(tcomp);
                else
                    list.Add(tcomp);
            }

        }

        #region PackObjects




        /// <summary>
        /// Check if passed Component has a PackObject attribute, if so add it to the callback list for this NetObj
        /// </summary>
        private void AddPackObjects(Component comp)
        {
            if (comp == null)
                return;

            /// Add PackObjRecord
            System.Type compType = comp.GetType();
            if (comp.GetType().GetCustomAttributes(typeof(PackObjectAttribute), false).Length != 0)
            {
                var packObjInfo = PackObjectDatabase.GetPackObjectInfo(compType);
                if (packObjInfo == null)
                    return;

                var newrecord = new PackObjRecord()
                {
                    component = comp,
                    onReadyCallback = comp as IPackObjOnReadyChange,
                    info = packObjInfo,
                    packFrames = packObjInfo.FactoryFramesObj(comp, TickEngineSettings.frameCount),
                    prevReadyMask = new FastBitMask128(packObjInfo.fieldCount),
                    readyMask = new FastBitMask128(packObjInfo.fieldCount)
                };

                // set any readyMask bits that are triggers to true - they are always ready.
                packObjIndexLookup.Add(comp, packObjects.Count);
                packObjects.Add(newrecord);
            }
        }

        #endregion PackObjs

        #endregion Interfaces


        public void OnPreUpdate()
        {
            /// OnPreUpdate Callbacks
            for (int i = 0, cnt = onPreUpdateCallbacks.Count; i < cnt; ++i)
                onPreUpdateCallbacks[i].OnPreUpdate();
        }

        #region Logging



#if TICKS_TO_UICONSOLE && (DEBUG || UNITY_EDITOR || DEVELOPMENT_BUILD)

        /// Virtual console logging

        int lastConsoleTick;
        static bool consoleRefreshing;

        private void Update()
        {
            consoleRefreshing = false;
        }

        private void LateUpdate()
        {

            if (!photonView.IsMine)
            {
                if (lastConsoleTick == NetMaster.CurrentFrameId)
                    return;

                if (!consoleRefreshing)
                    Debugging.UIConsole.Clear();

                consoleRefreshing = true;
                lastConsoleTick = NetMaster.CurrentFrameId;

                for (int i = 0; i < TickManager.connections.Count; ++i)
                {
                    var offsetinfo = TickManager.perConnOffsets[TickManager.connections[i]];
                }
                Debugging.UIConsole.Refresh();
            }
        }

#endif

        #endregion

        #region Serialization

        /// <summary>
        /// Generate a state tick.
        /// </summary>
        /// <param name="frameId"></param>
        public SerializationFlags GenerateMessage(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
        {
            //if (GetComponent<SyncPickup>())
            //    Debug.Log(frameId  + " <b>CaptureStates... </b>" );

            OnCaptureCurrentState(frameId);

            OnQuantize(frameId);

            //if (GetComponent<SyncPickup>())
            //    Debug.Log(frameId + " <b>OnSerialize... </b>");

            /// Begin SyncObject Serialization content
            var flags = OnSerialize(frameId, buffer, ref bitposition, writeFlags);
            return flags;
        }

#if SNS_WARNINGS
        /// <summary>
        /// Storage for frame arrival times for comparison against consumption time. Ridiculous long, but its just getting the max Enum value as the size of the buffer.
        /// </summary>
        private readonly float?[] bufferAddTime =
            new float?[(int)((TickEngineSettings.FrameCountEnum[])System.Enum.GetValues(typeof(TickEngineSettings.FrameCountEnum)))[((TickEngineSettings.FrameCountEnum[])System.Enum.GetValues(typeof(TickEngineSettings.FrameCountEnum))).Length - 1] + 1];
#endif

        /// <summary>
        /// Serialize all SyncObjs on this NetObj
        /// </summary>
        public SerializationFlags OnSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
        {
            //if ((writeFlags & SerializationFlags.NewConnection) != 0)
            //	Debug.LogError("NEW CONN " + writeFlags);

            SerializationFlags flags = writeFlags;

#if SNS_WARNINGS
            /// Integrity check
            buffer.Write(111, ref bitposition, 8);
#endif

            /// Serialize Pack Objects
            int prevFrameId = ((frameId == 0) ? TickEngineSettings.frameCount : frameId) - 1;
            int pcnt = packObjects.Count;
            for (int i = 0; i < pcnt; ++i)
            {
                var p = packObjects[i];
                var pframe = p.packFrames[frameId];

                /// make placeholder for mask bits (we don't know them yet)
                int mcnt = p.info.fieldCount;
                int maskpos = bitposition;
                bitposition += mcnt;

                int maskOffset = 0;
                flags |= p.info.PackFrameToBuffer(pframe, p.packFrames[prevFrameId], ref pframe.mask, ref maskOffset, buffer, ref bitposition, frameId, writeFlags);

                /// go back and write the mask bits
                for (int m = 0; m < mcnt; ++m)
                    buffer.WriteBool(pframe.mask[m], ref maskpos);

            }

#if SNS_WARNINGS
            /// Integrity check
            buffer.Write(123, ref bitposition, 8);
#endif

            /// Serialize SyncComponents
            for (int i = 0, cnt = onNetSerializeCallbacks.Count; i < cnt; ++i)
            {

#if SNS_REPORTS && (UNITY_EDITOR || DEVELOPMENT_BUILD)
				int holdpos = bitposition;
				flags |= onNetSerialize[i].OnNetSerialize(frameId, buffer, ref bitposition);
				SimpleDataMonitor.AddData(onNetSerialize[i] as ISyncObject, bitposition - holdpos);
#else
                flags |= onNetSerializeCallbacks[i].OnNetSerialize(frameId, buffer, ref bitposition, writeFlags);
#endif

#if SNS_WARNINGS
                /// Integrity check
                buffer.Write(234, ref bitposition, 8);
#endif
            }
            return flags;
        }

        bool processedInitialBacklog;
        float firstDeserializeTime;


        public void OnDeserialize(int connId, int originFrameId, byte[] buffer, ref int bitposition, bool hasData, FrameArrival arrival)
        {

#if SNS_WARNINGS

            bufferAddTime[originFrameId] = Time.time;
#endif

            if (hasData)
            {


#if SNS_WARNINGS
                /// Integrity check
                if (buffer.Read(ref bitposition, 8) != 111)
                    Debug.LogError("Failed Integrity check pre PackObjs.");
#endif

                frameValidMask[originFrameId] = true;
                originHistory[originFrameId] = connId;

                /// Deserialize Pack Objects
                //int prevFrameId = ((localframeId == 0) ? SimpleSyncSettings.FrameCount : localframeId) - 1;
                int pcnt = packObjects.Count;
                for (int i = 0; i < pcnt; ++i)
                {
                    var p = packObjects[i];
                    var pframe = p.packFrames[originFrameId];

                    int mcnt = p.info.fieldCount;
                    for (int m = 0; m < mcnt; ++m)
                        pframe.mask[m] = buffer.ReadBool(ref bitposition);

                    int maskOffset = 0;

                    //Debug.Log(Time.time + " PRE ------------------- ");
                    var flag = p.info.UnpackFrameFromBuffer(pframe, ref pframe.mask, ref pframe.isCompleteMask, ref maskOffset, buffer, ref bitposition, originFrameId, SerializationFlags.None);

                    //Debug.Log(localframeId + " Des READY? flg: " + flag + "  readymaskAllTrue: " + p.readyMask.AllAreTrue + " complete frame: " 
                    //	+ pframe.isCompleteMask.PrintMask(null) + " ready: " 
                    //	+ p.readyMask.PrintMask(null));

                    /// Experimental - Apply valid values as they arrive if pack object isn't fully ready. Ensures even a late arriving reliable full update counts toward Ready

                    if (arrival >= FrameArrival.IsSnap && !p.readyMask.AllAreTrue && (flag & SerializationFlags.IsComplete) != 0)
                    {
                        /// Add any always ready bits (Triggers)
                        p.readyMask.OR(p.info.defaultReadyMask);
                        p.readyMask.OR(pframe.isCompleteMask);
                        /// Only write to syncvars that are not already marked as valid
                        FastBitMask128 newchangesmask = !p.readyMask & pframe.mask;
                        maskOffset = 0;
                        p.info.CopyFrameToObj(pframe, p.component, ref newchangesmask, ref maskOffset);

                        BroadcastReadyMaskChange(p);

                        //Debug.Log(localframeId + "<b> Des PRE COMPLETE? </b>" + p.readyMask.AllAreTrue + " changes: " + newchangesmask.PrintMask(null));
                    }
                }


#if SNS_WARNINGS
                /// Integrity check
                if (buffer.Read(ref bitposition, 8) != 123)
                    Debug.LogError(name + " Failed Integrity check post PackObjs. OrigFid: " + originFrameId);
#endif
                /// Deserialize SyncObjs
                for (int i = 0, cnt = onNetSerializeCallbacks.Count; i < cnt; ++i)
                {
                    onNetSerializeCallbacks[i].OnNetDeserialize(originFrameId, buffer, ref bitposition, arrival);

                    /// Experimental - immediately apply complete frames to unready sync objects.
                    //if (arrival == FrameArrival.IsLate && (flag & SerializationFlags.IsComplete) != 0 && !syncObjReadyMask[i])
                    //	Debug.Log("Call an early Apply here when a method exists for that.");

#if SNS_WARNINGS
                    /// Integrity check
                    if (buffer.Read(ref bitposition, 8) != 234)
                        Debug.LogError(name + " Failed Integrity check post SyncObjs. " + onNetSerializeCallbacks[i].GetType().Name + " origFid: " + originFrameId);
#endif
                }


                /// Late update handling.
                if (resimulateLateArrivals)
                    if (arrival >= FrameArrival.IsSnap)
                    {
                        int framecount = TickEngineSettings.frameCount;

                        int targFid = originFrameId + 1;
                        if (targFid >= framecount)
                            targFid -= framecount;

                        int snapFid = originFrameId;

                        int prevFid = originFrameId - 1;
                        if (prevFid < 0)
                            prevFid += framecount;

                        for (int r = 0, rcnt = (int)arrival; r <= rcnt; r++)
                        {
                            //Debug.Log(name + " <b>Reapplying Snapshots due to late arrival </b> (" + rcnt + ") " + snapFid + " > " + targFid);
                            for (int i = 0, cnt = onSnapshotCallbacks.Count; i < cnt; ++i)
                            {
                                bool prevIsValid, snapIsValid, targIsValid;
                                //if (ignoreNonControllerUpdates)
                                //{
                                //    int controllerActorNr = photonView.ControllerActorNr;
                                //    prevIsValid = frameValidMask[prevFid] && originHistory[prevFid] == controllerActorNr;
                                //    snapIsValid = frameValidMask[snapFid] && originHistory[snapFid] == controllerActorNr;
                                //    targIsValid = frameValidMask[targFid] && originHistory[targFid] == controllerActorNr;
                                //}
                                //else
                                {
                                    prevIsValid = frameValidMask[prevFid];
                                    snapIsValid = frameValidMask[snapFid];
                                    targIsValid = frameValidMask[targFid];
                                }

                                onSnapshotCallbacks[i].OnSnapshot(prevFid, snapFid, targFid, prevIsValid, snapIsValid, targIsValid);
                            }

                            if (r == rcnt)
                                break;

                            prevFid = snapFid;
                            snapFid = targFid;
                            targFid++;
                            if (targFid >= framecount)
                                targFid -= framecount;
                        }


                        for (int i = 0, cnt = onCriticallyLateFrameCallbacks.Count; i < cnt; ++i)
                        {
                            onCriticallyLateFrameCallbacks[i].HandleCriticallyLateFrame(originFrameId);
                        }
                    }
            }
        }

        #endregion

        #region NetMaster Events

        public void OnPreSimulate(int frameId, int _currSubFrameId)
        {
            for (int i = 0, cnt = onPreSimulateCallbacks.Count; i < cnt; ++i)
            {
                var cb = onPreSimulateCallbacks[i];
                var b = cb as Behaviour;
                if (b.enabled && b.gameObject.activeInHierarchy)
                   cb.OnPreSimulate(frameId, _currSubFrameId);
            }
        }

        public void OnPostSimulate(int frameId, int subFrameId, bool isNetTick)
        {
            for (int i = 0, cnt = onPostSimulateCallbacks.Count; i < cnt; ++i)
            {
                var cb = onPostSimulateCallbacks[i];
                var b = cb as Behaviour;
                if (b.enabled && b.gameObject.activeInHierarchy)
                    cb.OnPostSimulate(frameId, subFrameId, isNetTick);
            }

            //if (GetComponent<SyncPickup>())
            //	Debug.LogError(pv.OwnerActorNr + " " + pv.IsMine);
        }

        public SerializationFlags OnNetSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
        {
            //Debug.Log(name + " OnNetSerialize NO " + pv.IsMine + " / " + pv.IsMine);
            //if (!photonView.IsMine)
            //{
            //    return SerializationFlags.None;
            //}

            //if (controllerHasChanged)
            //{
            //    writeFlags |= SerializationFlags.ForceReliable;
            //    controllerHasChanged = false;
            //}

            //if ((writeFlags & SerializationFlags.Force) != 0)
            //	Debug.LogError("FORCE " + writeFlags);

            if (photonView.Group != 0)

            {
                /// TODO: Ideally objects will not be individually serializing themselves into their own send.
                /// Serialize and Send this netobj state if this is a net tick.
                buffer = NetMsgSends.reusableNetObjBuffer;
                int localbitposition = 0;

                /// Write FrameId
                buffer.Write((uint)frameId, ref localbitposition, TickEngineSettings.frameCountBits);

                /// Write not end of netObjs bool
                //buffer.WriteBool(true, ref localbitposition);

                /// Write viewID
                buffer.WritePackedBytes((uint)viewID, ref localbitposition, 32);

                /// Placeholder for data size. False means this is a contentless heartbea
                int holdHasDataPos = localbitposition;
                buffer.WriteBool(true, ref localbitposition);

                /// Placeholder for data size. False means this is a contentless heartbeat
                int holdDataSizePos = localbitposition;
                localbitposition += NetMaster.BITS_FOR_NETOBJ_SIZE;

                SerializationFlags lclflags = GenerateMessage(frameId, buffer, ref localbitposition, writeFlags);

                if (lclflags == SerializationFlags.None)
                {
                    if (skipWhenEmpty)
                        return SerializationFlags.None;
                    else
                    {
                        /// revise the hasData bool to be false and rewind the bitwriter.
                        localbitposition = holdHasDataPos;
                        buffer.WriteBool(false, ref bitposition);
                    }
                }

                if (lclflags != SerializationFlags.None || !SkipWhenEmpty)
                {
                    /// Revise the data size now that we know it
                    buffer.Write((uint)(localbitposition - holdDataSizePos), ref holdDataSizePos, NetMaster.BITS_FOR_NETOBJ_SIZE);

                    /// Write end of netObjs marker
                    buffer.WritePackedBytes(0, ref localbitposition, 32);

                    NetMsgSends.Send(buffer, localbitposition, gameObject, lclflags, false);
                }

                /// We sent this object at the netObj level, so we report back to the NetMaster that nothing has been added to the master byte[] send.
                return SerializationFlags.None;
            }
            var flags = GenerateMessage(frameId, buffer, ref bitposition, writeFlags);

            return flags;
        }

        //public void OnNetDeserialize(int sourceFrameId, int originFrameId, int localFrameId, byte[] buffer, ref int bitposition)
        //{
        //    throw new System.NotImplementedException();
        //}

        public void OnCaptureCurrentState(int frameId)
        {
            /// Capture PackObjs
            int pcnt = packObjects.Count;
            for (int i = 0; i < pcnt; ++i)
            {
                var p = packObjects[i];
                p.info.CaptureObj(p.component, p.packFrames[frameId]);
            }

            /// Capture SyncObjs
            for (int i = 0, cnt = onCaptureCurrentStateCallbacks.Count; i < cnt; ++i)
                onCaptureCurrentStateCallbacks[i].OnCaptureCurrentState(frameId);
        }

        public void OnQuantize(int frameId)
        {
            for (int i = 0, cnt = onQuantizeCallbacks.Count; i < cnt; ++i)
            {
                var cb = onQuantizeCallbacks[i];
                var b = cb as Behaviour;
                if (b.enabled && b.gameObject.activeInHierarchy)
                    cb.OnQuantize(frameId);
            }
        }

        #endregion

        #region Snapshot / Interpolate Events

        public void OnIncrementFrame(int newFrameId, int newSubFrameId, int previousFrameId, int prevSubFrameId)
        {
            for (int i = 0, cnt = onIncrementFramesCallbacks.Count; i < cnt; ++i)
                onIncrementFramesCallbacks[i].OnIncrementFrame(newFrameId, newSubFrameId, previousFrameId, prevSubFrameId);
        }

        public bool OnSnapshot(int localTargFrameId)
        {


            if (!photonView)
                return false;

            //if (pv.IsMine)
            //    return false;

            if (!photonView.enabled)
                return false;

            /// TODO: Cache this properly
            ConnectionTickOffsets connectionOffsets;
            if (!TickManager.perConnOffsets.TryGetValue(photonView.ControllerActorNr, out connectionOffsets))
                return false;

            if (ReferenceEquals(connectionOffsets, null))
                return false;

            if (!connectionOffsets.hadInitialSnapshot)
                return false;

            int advanceCount = connectionOffsets.advanceCount;
            if (advanceCount == 0)
                return false;

            int framecount = TickEngineSettings.frameCount;

            int originTargFrameId = connectionOffsets.ConvertFrameLocalToOrigin(localTargFrameId);

            int snapFid = originTargFrameId - 1;
            if (snapFid < 0)
                snapFid += framecount;

            int prevFid = originTargFrameId - 2;
            if (prevFid < 0)
                prevFid += framecount;
            //int snapFid = NetMaster.PreviousFrameId;

            //Debug.Log(GetType().Name + " OnSnap - passed  - advance count: " + offsets.advanceCount);

            //if (advanceCount != 1)
            //    Debug.Log("<b>ADVANCE </b>" + snapFid + " > " + originTargFrameId + " : " + advanceCount);
            //else
            //    Debug.Log("ADVANCE " + snapFid + " > " + originTargFrameId + " : " + advanceCount);

            int frameCount = TickEngineSettings.frameCount;


            for (int a = 0, targFid = originTargFrameId; a < advanceCount; ++a)
            {
                targFid = originTargFrameId + a;
                if (targFid >= frameCount)
                    targFid -= frameCount;

                int invalidateFId = targFid - TickEngineSettings.halfFrameCount;
                if (invalidateFId < 0)
                    invalidateFId += frameCount;


                /// Snap Pack Objects
                bool isFrameValid = frameValidMask[snapFid];

                //if (packSnapIsValid/* || packTargIsValid*/)
                {
                    bool packTargIsValid = frameValidMask[targFid];

                    int pcnt = packObjects.Count;
                    for (int i = 0; i < pcnt; ++i)
                    {
                        PackObjRecord p = packObjects[i];
                        var snapf = p.packFrames[snapFid];
                        var targpf = p.packFrames[targFid];

                        //if (advanceCount != 1)
                        //    Debug.Log(snapFid + " > " + targFid);

                        /// update readymask with any new valid fields
                        p.readyMask.OR(p.info.defaultReadyMask);
                        p.readyMask.OR(snapf.isCompleteMask);

                        //Debug.Log(Time.time + " snap " + snapFid + " - " + p.readyMask.PrintMask(null) + " : " + p.info.defaultReadyMask.PrintMask(null) + " : " + snapf.isCompleteMask.PrintMask(null));

                        /// TODO: when extrapolation is implemented, it will replace this basic copy
                        if (!packTargIsValid)
                            p.info.CopyFrameToFrame(snapf, targpf);

                        /// Snapshot callbacks - fire every net tick changed or not
                        int maskOffset = 0;
                        p.info.SnapObject(snapf, targpf, p.component, ref p.readyMask, ref maskOffset);

                        /// Apply new Snap value / Callback only fires on changes
                        if (isFrameValid)
                        {
                            maskOffset = 0;
                            p.info.CopyFrameToObj(snapf, p.component, ref snapf.mask, ref maskOffset);
                        }

                        /// Ready Mask has Changed - Issue callback
                        if (p.readyMask.Compare(p.prevReadyMask) == false)
                        {
                            BroadcastReadyMaskChange(p);
                        }
                    }
                }

                //Debug.Log(GetType().Name + " OnSnap - callback count " + onSnapshot.Count);


                bool prevIsValid, snapIsValid, targIsValid;
                //if (ignoreNonControllerUpdates)
                //{
                //    int controllerActorNr = photonView.ControllerActorNr;
                //    prevIsValid = frameValidMask[prevFid] && originHistory[prevFid] == controllerActorNr;
                //    snapIsValid = frameValidMask[snapFid] && originHistory[snapFid] == controllerActorNr;
                //    targIsValid = frameValidMask[targFid] && originHistory[targFid] == controllerActorNr;
                //}
                //else
                {
                    prevIsValid = frameValidMask[prevFid];
                    snapIsValid = frameValidMask[snapFid];
                    targIsValid = frameValidMask[targFid];
                }

                /// Snap SyncObjs
                for (int i = 0, cnt = onSnapshotCallbacks.Count; i < cnt; ++i)
                {
                    onSnapshotCallbacks[i].OnSnapshot(prevFid, snapFid, targFid, prevIsValid, snapIsValid, targIsValid);
                }

                prevFid = snapFid;
                snapFid = targFid;

                /// TODO: Needs a better home

                frameValidMask[invalidateFId] = false;

#if SNS_WARNINGS
                //Debug.Log(currTargFrameId + " New Target Time on Buffer " + (bufferAddTime[currTargFrameId].HasValue ? (Time.time - bufferAddTime[currTargFrameId]).ToString() : "NULL"));
                bufferAddTime[targFid] = null;
#endif
            }

            //if (advanceCount != 1)
            //    Debug.Log("<b>END ADVANCE </b>" + snapFid + " : " + advanceCount);

            return true;
        }

        private void BroadcastReadyMaskChange(PackObjRecord p)
        {
            //Debug.Log("Ready change " + p.readyMask.AllAreTrue);

            OnPackObjReadyChange(p.component, p.readyMask.AllAreTrue ? ReadyStateEnum.Ready : ReadyStateEnum.Unready);

            IPackObjOnReadyChange onReadyCallback = p.onReadyCallback;
            if (!ReferenceEquals(onReadyCallback, null))
                onReadyCallback.OnPackObjReadyChange(p.readyMask, p.readyMask.AllAreTrue);

            p.prevReadyMask.Copy(p.readyMask);
        }

        public bool OnInterpolate(int localSnapFrameId, int localTargFrameId, float t)
        {
            //return false;



            /// TODO: Cache this properly
            ConnectionTickOffsets offsets;
            if (!TickManager.perConnOffsets.TryGetValue(photonView.ControllerActorNr, out offsets))
                return false;

            // Not sure why this is needed, but it is.
            if (ReferenceEquals(offsets, null))
                return false;

            if (!offsets.hadInitialSnapshot)
                return false;

            int originSnapFrameId = offsets.ConvertFrameLocalToOrigin(localSnapFrameId);
            int originTargFrameId = offsets.ConvertFrameLocalToOrigin(localTargFrameId);


            /// Interpolate Pack Objects - only interpolate currently if both snap and targ are valid.
            /// TODO: This will change if/when extrapolate is added to Pack Object system
            if (offsets.validFrameMask[originTargFrameId])
            {
                //Debug.Log("NOBJ Interp " + snapFId + " : " + targFId + packObjValidMask[snapFId] + " : " + packObjValidMask[targFId]);

                int pcnt = packObjects.Count;
                for (int i = 0; i < pcnt; ++i)
                {
                    var p = packObjects[i];
                    var snappf = p.packFrames[originSnapFrameId];
                    var targpf = p.packFrames[originTargFrameId];
                    int maskOffset = 0;
                    //Debug.Log(name + "Interp initialized " + frameValidMask[originSnapFrameId] + " > " + frameValidMask[originTargFrameId]);

                    p.info.InterpFrameToObj(snappf, targpf, p.component, t, ref p.readyMask, ref maskOffset);
                }
            }


            ///  Interpolation Sync Obj
            for (int i = 0, cnt = onInterpolateCallbacks.Count; i < cnt; ++i)
            {
                var cb = onInterpolateCallbacks[i];
                var b = cb as Behaviour;
                if (b.enabled && b.gameObject.activeInHierarchy)
                    cb.OnInterpolate(originSnapFrameId, originTargFrameId, t);
            }

            return true;
        }

        #endregion
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(NetObject))]
    public class NetObjectEditor : NetCoreHeaderEditor
    {
        protected override string HelpURL
        {
            get { return SimpleDocsURLS.CORE_OBJS_PATH + @"#netobject"; }
        }

        protected override string TextTexturePath
        {
            get { return "Header/NetObjectText"; }
        }

        protected override string Instructions
        {
            get
            {
                return "Extends functionality of PhotonView component. Collects all networking interfaces from child components," +
                " and relays network callbacks, serialization, and events between the NetMaster and synced components on this object.";
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            // Iterate all Bool members, we want them as ToggleLeft since the names are so long.
            var iterator = serializedObject.GetIterator();
            iterator.Next(true);

            EditorGUI.BeginChangeCheck();

            while (iterator.Next(false))
                if (iterator.propertyType == SerializedPropertyType.Boolean && iterator.name != "m_Enabled")
                {
                    iterator.boolValue = EditorGUILayout.ToggleLeft(GetGUIContent(iterator), iterator.boolValue);
                }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();

            TickEngineSettings.Single.DrawGui(target, true, false, false);
        }
    }

#endif
}
