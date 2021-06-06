// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Pun.Simple.Internal;

using Photon.Compression;
using ExitGames.Client.Photon;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
    [HelpURL(SimpleDocsURLS.OVERVIEW_PATH)]
    public class NetMaster : MonoBehaviour

    {
       
        /// <summary>
        /// Singleton instance of the NetMaster. "There can be only one."
        /// </summary>
        public static NetMaster single;

        public static bool isShuttingDown;

        /// <summary>
        /// Value used in Update() timing to generate the t value for OnInterpolate calls.
        /// </summary>
        protected static float lastSentTickTime;

        #region Properties

        private static int _currFrameId, _currSubFrameId, _prevFrameId, _prevSubFrameId;
        public static int CurrentFrameId { get { return _currFrameId; } }
        /// <summary>
        /// When Every X Tick is being sent, ticks are numbered by the sent interval. 
        /// Simulation ticks between these maintain the same FrameId, and increment the SubFrameId.
        /// Frames are sent when the SubFrameId equals flips back to zero.
        /// </summary>
        public static int CurrentSubFrameId { get { return _currSubFrameId; } }
        public static int PreviousFrameId { get { return _prevFrameId; } }
        public static int PreviousSubFrameId { get { return _prevSubFrameId; } }

        public static float NormTimeSinceFixed { get; private set; }

        protected static float rtt;
        public static float RTT { get { return rtt; } }

        #endregion

        /// <summary>
        /// Startup Bootstrap for finding/Creating NetMaster singleton.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EnsureExistsInScene()
        {
            if (!TickEngineSettings.Single.enableTickEngine)
                return;

            /// Some basic singleton enforcement
            GameObject go = null;

            if (single)
            {
                go = single.gameObject;
            }
            else
            {
                /// Use NetMasterLate go if that singleton exists (this will be rare or never, but just here to cover all cases)
                if (NetMasterLate.single)
                    go = NetMasterLate.single.gameObject;

                single = FindObjectOfType<NetMaster>();
                if (single)
                {
                    go = single.gameObject;
                }
                else
                {
                    // No singletons exist... make a new GO
                    if (!go)
                        go = new GameObject("Net Master");

                    single = go.AddComponent<NetMaster>();
                }
            }

            // Enforce singleton for NetMasterLate
            if (!NetMasterLate.single)
            {
                NetMasterLate.single = FindObjectOfType<NetMasterLate>();

                if (!NetMasterLate.single)
                    NetMasterLate.single = go.AddComponent<NetMasterLate>();
            }

            NetMsgCallbacks.RegisterCallback(ReceiveMessage);
        }

        private void Awake()
        {

            if (single && single != this)
            {
                /// If a singleton already exists, destroy the old one - TODO: Not sure about this behavior yet. Allows for settings changes with scene changes.
                Destroy(single);
            }

            single = this;

            DontDestroyOnLoad(this);

            _prevFrameId = TickEngineSettings.frameCount - 1;
            _prevSubFrameId = TickEngineSettings.sendEveryXTick - 1;
        }


        private void OnApplicationQuit()
        {
            isShuttingDown = true;

            NetMasterCallbacks.OnPreQuitCallbacks();
        }

        private bool simulationHasRun = false;

        private void FixedUpdate()
        {
            /// Disable Simple if no NetObjects exist.
            if (NetObject.activeControlledNetObjs.Count == 0 && NetObject.activeUncontrolledNetObjs.Count == 0)
                return;

            if (!TickEngineSettings.single.enableTickEngine)
                return;

            /// Halt everything if networking isn't ready.
            bool readyToSend = NetMsgSends.ReadyToSend;
            if (!readyToSend)
            {
                DoubleTime.SnapFixed();
                return;
            }

            if (simulationHasRun)
                PostSimulate();

            DoubleTime.SnapFixed();

#if PUN_2_OR_NEWER

            /// Make sure we don't have any incoming messages. PUN checks this pre-Update but not so explicitly at the top of the fixed.
            /// We want to ensure that we are running our simulation with the most current network input/states so best to make sure we have all that is available.
            /// Make sure Photon isn't holding out on us just because a FixedUpdate didn't happen this Update()
            bool doDispatch = true;

            while (PhotonNetwork.InRoom && PhotonNetwork.IsMessageQueueRunning && doDispatch)
            {
                // DispatchIncomingCommands() returns true of it found any command to dispatch (event, result or state change)
                doDispatch = PhotonNetwork.NetworkingClient.LoadBalancingPeer.DispatchIncomingCommands();
            }

            rtt = PhotonNetwork.GetPing() * .001f;
#endif

            simulationHasRun = true;
        }

        void Update()
        {
            /// Disable Simple if no NetObjects exist.
            if (NetObject.activeControlledNetObjs.Count == 0 && NetObject.activeUncontrolledNetObjs.Count == 0)
                return;

            if (!TickEngineSettings.single.enableTickEngine)
                return;

            if (simulationHasRun)
                PostSimulate();

            DoubleTime.SnapUpdate();

            NormTimeSinceFixed = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

            NetMasterCallbacks.OnPreUpdateCallbacks();

            float t = (Time.time - lastSentTickTime) / (TickEngineSettings.netTickInterval);

            // Interpolate NetObjects
            NetObject.NetObjDictsLocked = true;
            foreach (var no in NetObject.activeUncontrolledNetObjs.Values)
                no.OnInterpolate(_prevFrameId, _currFrameId, t);
            NetObject.NetObjDictsLocked = false;

            // Interpolate Others
            NetMasterCallbacks.OnInterpolateCallbacks(_prevFrameId, _currFrameId, t);
        }

        private void LateUpdate()
        {
            NetMasterCallbacks.OnPreLateUpdateCallbacks();
        }

        /// <summary>
        /// Unity lacks a PostPhysX/PostSimulation callback, so this is the closest we can get to creating one.
        /// If this happens during Update, it is important to sample values from the simulaion (such as rb.position, or your own tick based sim results)
        /// RATHER than from the scene objects, which may be interpolated.
        /// </summary>
        void PostSimulate()
        {
            bool isNetTick = _currSubFrameId == TickEngineSettings.sendEveryXTick - 1;

            NetMasterCallbacks.OnPostSimulateCallbacks(_currFrameId, _currSubFrameId, isNetTick);

            if (isNetTick)
                SerializeAllAndSend();

            IncrementFrameId();

            simulationHasRun = false;
        }

        /// <summary>
        /// Increment the current frameId. If we are sending every X simulation tics, the Subframe only gets incremented.
        /// Frames are serialized and sent just before CurrentFrameId is incremented (after all subframes have been simulated).
        /// </summary>
        private static void IncrementFrameId()
        {
            _prevSubFrameId = _currSubFrameId;
            _currSubFrameId++;

            if (_currSubFrameId >= TickEngineSettings.sendEveryXTick)
            {
                _currSubFrameId = 0;
                _prevFrameId = _currFrameId;

                _currFrameId++;
                if (_currFrameId >= TickEngineSettings.frameCount)
                    _currFrameId = 0;
            }

            NetMasterCallbacks.OnIncrementFrameCallbacks(_currFrameId, _currSubFrameId, _prevFrameId, _prevSubFrameId);

            if (_currSubFrameId == 0)
            {
                ///  Insert pre snapshot tick manager test for number of snaps needed per connection.
                TickManager.PreSnapshot(_currFrameId);

                // Snapshot NetObjects
                NetObject.NetObjDictsLocked = true;
                foreach (var no in NetObject.activeUncontrolledNetObjs.Values)
                    no.OnSnapshot(_currFrameId);
                NetObject.NetObjDictsLocked = false;

                // Snapshot Others
                NetMasterCallbacks.OnSnapshotCallbacks(_currFrameId);

                TickManager.PostSnapshot(_currFrameId);

                lastSentTickTime = Time.fixedTime;
            }
        }


        public const int BITS_FOR_NETOBJ_SIZE = 16;
        private static void SerializeAllAndSend()
        {
            byte[] buffer = NetMsgSends.reusableBuffer;
            int bitposition = 0;

            SerializationFlags writeFlags;
            SerializationFlags flags;
            if (TickManager.needToSendInitialForNewConn)
            {
                writeFlags = SerializationFlags.NewConnection | SerializationFlags.ForceReliable | SerializationFlags.Force;
                flags = SerializationFlags.HasContent;
                TickManager.needToSendInitialForNewConn = false;
            }
            else
            {
                writeFlags = SerializationFlags.None;
                flags = SerializationFlags.None;
            }

            /// Write frameId
            buffer.Write((uint)_currFrameId, ref bitposition, TickEngineSettings.frameCountBits);

            NetMasterCallbacks.OnPreSerializeTickCallbacks(_currFrameId, buffer, ref bitposition);

            #region NetObject Serialization

            /// Loop through owned NetObjects
            NetObject.NetObjDictsLocked = true;

            NonAllocDictionary<int, NetObject> controlledObjs = NetObject.activeControlledNetObjs;

            SerializeNetObjDict(controlledObjs, buffer, ref bitposition, ref flags, writeFlags);

            //NonAllocDictionary<int, NetObject> ownedButNotControlledObjs = NetObject.activeOwnedNetObjs;

            //if (ownedButNotControlledObjs.Count > 0)
            //    Debug.Log("LIMBO COUNT " + ownedButNotControlledObjs.Count);

            //SerializeNetObjDict(ownedButNotControlledObjs, buffer, ref bitposition, ref flags, writeFlags);

            //foreach (var no in ownedObjs)
            //{

            //    /// Not end of netobjs write bool
            //    int holdStartPos = bitposition;

            //    /// Write viewID
            //    buffer.WritePackedBytes((uint)no.ViewID, ref bitposition, 32);

            //    /// Write hadData bool
            //    int holdHasDataPos = bitposition;
            //    buffer.WriteBool(true, ref bitposition);

            //    /// Log the data size write position and write a placeholder.
            //    int holdDataSizePos = bitposition;
            //    bitposition += BITS_FOR_NETOBJ_SIZE;

            //    var objflags = no.OnNetSerialize(_currFrameId, buffer, ref bitposition, writeFlags);

            //    /// Skip netobjs if they had nothing to say
            //    if (objflags == SerializationFlags.None)
            //    {
            //        /// Rewind if this is a no-data write.
            //        if (no.SkipWhenEmpty)
            //        {
            //            bitposition = holdStartPos;
            //        }
            //        else
            //        {
            //            bitposition = holdHasDataPos;
            //            buffer.WriteBool(false, ref bitposition);
            //        }
            //    }
            //    else
            //    {
            //        /// Revise the data size now that we know it.
            //        flags |= objflags;
            //        int bitcount = bitposition - holdDataSizePos;
            //        buffer.Write((uint)bitcount, ref holdDataSizePos, BITS_FOR_NETOBJ_SIZE);
            //    }

            //    //Debug.Log(objflags + " / flg: " + (onNetSerialize[i]  as Component).name + " " + flags);
            //}

            NetObject.NetObjDictsLocked = false;

            #endregion

            // Any deferrered Ownership changes from SyncState happen here
            while (NetMasterCallbacks.postSerializationActions.Count > 0)
                NetMasterCallbacks.postSerializationActions.Dequeue().Invoke();

            if (flags == SerializationFlags.None)
                return;

            /// End of NetObject write bool
            buffer.WritePackedBytes(0, ref bitposition, 32);

            NetMsgSends.Send(buffer, bitposition, null, flags, true);
        }

        private static void SerializeNetObjDict(NonAllocDictionary<int, NetObject> dict, byte[] buffer, ref int bitposition, ref SerializationFlags flags, SerializationFlags writeFlags)
        {
            foreach (var no in dict.Values)
            {

                /// Not end of netobjs write bool
                int holdStartPos = bitposition;

                /// Write viewID
                buffer.WritePackedBytes((uint)no.ViewID, ref bitposition, 32);

                /// Write hadData bool
                int holdHasDataPos = bitposition;
                buffer.WriteBool(true, ref bitposition);

                /// Log the data size write position and write a placeholder.
                int holdDataSizePos = bitposition;
                bitposition += BITS_FOR_NETOBJ_SIZE;

                var objflags = no.OnNetSerialize(_currFrameId, buffer, ref bitposition, writeFlags);

                /// Skip netobjs if they had nothing to say
                if (objflags == SerializationFlags.None)
                {
                    /// Rewind if this is a no-data write.
                    if (no.SkipWhenEmpty)
                    {
                        bitposition = holdStartPos;
                    }
                    else
                    {
                        bitposition = holdHasDataPos;
                        buffer.WriteBool(false, ref bitposition);
                    }
                }
                else
                {
                    /// Revise the data size now that we know it.
                    flags |= objflags;
                    int bitcount = bitposition - holdDataSizePos;
                    buffer.Write((uint)bitcount, ref holdDataSizePos, BITS_FOR_NETOBJ_SIZE);
                }

                //Debug.Log(objflags + " / flg: " + (onNetSerialize[i]  as Component).name + " " + flags);
            }
        }


        /// <summary>
        /// Incoming message receiver.
        /// </summary>
        public static void ReceiveMessage(object conn, int connId, byte[] buffer)
        {
            int frameCount = TickEngineSettings.frameCount;
            int bitposition = 0;


            /// Read frameId
            int originFrameId = (int)buffer.Read(ref bitposition, TickEngineSettings.frameCountBits);

            //Debug.Log("in " + originFrameId);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (originFrameId < 0 || originFrameId >= frameCount)
                Debug.LogError("INVALID FRAME ID " + originFrameId + " CORRUPT STREAM LIKELY DUE TO READ/WRITE DISAGREEMENT.");
#endif
            FrameArrival arrival;
            /*var offsets = */
            TickManager.LogIncomingFrame(connId, originFrameId, out arrival);

            ///// Read and Tick Pre Serialization
            //NetMasterCallbacks.OnPreDeserializeTickCallbacks(localFrameId, buffer, ref bitposition);

            /// Read all netobjs. Exits when we get a false leading bit.
            while (true)
            {
                /// Read next viewID
                int viewid = (int)buffer.ReadPackedBytes(ref bitposition, 32);

                if (viewid == 0)
                    break;

                /// Read hasData bool
                bool hasData = buffer.ReadBool(ref bitposition);

                /// No data... this is just a heartbeat
                if (!hasData)
                    continue;

                /// Read data size
                int holdDataSizePos = bitposition;
                int bitcount = (int)buffer.Read(ref bitposition, BITS_FOR_NETOBJ_SIZE);
                int expectedBitPosition = holdDataSizePos + bitcount;

                var pv = PhotonNetwork.GetPhotonView(viewid);
                var netobj = pv ? pv.GetComponent<NetObject>() : null;

                /// If netobj can't be found, jump to the next object in the stream
                if (ReferenceEquals(netobj, null))
                {
                    bitposition = expectedBitPosition;
                    continue;
                }

                /// Determine if this object should be deserialized or ignored based on controller authority.
                if (netobj.IgnoreNonControllerUpdates)
                {
                    int controllerActorNr = pv.ControllerActorNr;
                    int ownerActorNr = pv.OwnerActorNr;

                    if (controllerActorNr == -1)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.Log(pv.name + ":" + pv.ViewID + " OwnershipUpdate has not yet arrived from master, so owner is still unknown. Accepting update from Actor " + connId + " as temporary controller until OwnershipUpdate arrives and sets the Owner/Controller.");
#endif
                        pv.SetControllerInternal(connId);
                    }
                    else if (controllerActorNr != connId && ownerActorNr != connId)
                    {
                        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                            Debug.LogError("fid: " + originFrameId + "Update from ActorId: " + connId + " for object " + netobj.name + ":" + pv.ViewID + " , but that conn does not have controller authority. Current cowner:controller is "
                                + pv.OwnerActorNr + ":" + controllerActorNr);
#endif
                            bitposition = expectedBitPosition;
                            continue;
                        }
                    }
                }



                netobj.OnDeserialize(connId, originFrameId, buffer, ref bitposition, hasData, arrival);


#if UNITY_EDITOR
                if (bitposition != expectedBitPosition)
                    Debug.LogWarning("Deserialization mismatch for object " + netobj.name + " viewId: " + netobj.ViewID + " owned by: " + netobj.photonView.OwnerActorNr +
                        ". Bitposition off by: " + (expectedBitPosition - bitposition) + " bits.");
#endif
                /// Realign position for next NetObject to ensure any read/write mismatches from the previous object don't cascade.
                bitposition = expectedBitPosition;

            }
        }

        public static FrameArrival CheckFrameArrival(int incomingFrame)
        {
            int delta = incomingFrame - _prevFrameId;

            if (delta == 0)
                return FrameArrival.IsSnap;

            // change negative values into positive to check for wrap around.
            if (delta < 0)
                delta += TickEngineSettings.frameCount;

            if (delta == 1)
                return FrameArrival.IsTarget;

            if (delta >= TickEngineSettings.halfFrameCount)
                return FrameArrival.IsLate;

            return FrameArrival.IsFuture;

        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(NetMaster))]
    public class NetMasterEditor : NetCoreHeaderEditor
    {
        protected override string TextTexturePath
        {
            get
            {
                return "Header/NetMasterText";
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            const string desc = "Early timing singleton used by all Simple components. " +
            "Effectively a lightweight networking specific Update Manager with timing segments specifically useful for networking. " +
            "This component will be added automatically at runtime if one does not exist in your scene. " +
            "NetMaster is set to execute on the earliest Script Execution timing, " +
            "ensuring its Fixed/Late/Update callbacks occur before all other scene components.";

            EditorGUILayout.LabelField(desc, new GUIStyle("HelpBox") { wordWrap = true, alignment = TextAnchor.UpperLeft });
        }
    }

#endif
}

