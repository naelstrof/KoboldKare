// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Photon.Utilities;
using UnityEngine;

using Photon.Compression;
using Photon.Realtime;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{

    [DisallowMultipleComponent]

    public class SyncState : SyncObject<SyncState.Frame>
        , IMountable
        , IOnCaptureState
        , IOnNetSerialize
        , IOnSnapshot
        //, IOnCriticallyLateFrame
        , IReadyable
        , IOnNetObjReady
        , IUseKeyframes
    {
        public override int ApplyOrder { get { return ApplyOrderConstants.STATES; } }

        #region Inspector Items

        [EnumMask(true, typeof(ObjStateEditor))] public ObjState initialState = ObjState.Despawned;
        [EnumMask(true, typeof(ObjStateEditor))] public ObjState respawnState = ObjState.Visible;
        [EnumMask(true, typeof(ObjStateEditor))] public ObjState readyState = ObjState.Visible;
        [EnumMask(true, typeof(ObjStateEditor))] public ObjState unreadyState = ObjState.Despawned;

        [Tooltip("Mount types this NetObject can be attached to.")]
        public MountMaskSelector mountableTo;

        [Tooltip("Automatically return this object to its starting position and attach to original parent when ObjState changes from Despawned to any other state.")]
        public bool autoReset = true;

        [Tooltip("Automatically will request ownership transfer to the owner of NetObjects this becomes attached to.")]
        public bool autoOwnerChange = true;

        [Tooltip("Parent Mount changes will force the entire update from this client to send Reliable. This ensures a keyframe of parenting and position reaches all clients (which isn't certain with packetloss), but can cause a visible hanging if packetloss is present on the network.")]
        public bool mountReliable = true;

        #endregion

        [System.NonSerialized] protected Frame currentState = new Frame();
        [System.NonSerialized] protected Mount currentMount = null;
        [System.NonSerialized] protected bool netObjIsReady;

        #region IMountable Requirements

        public Mount CurrentMount { get { return currentMount; } set { currentMount = value; } }
        public bool IsThrowable { get { return true; } }
        public bool IsDroppable { get { return true; } }
        public Rigidbody Rb { get { return netObj.Rb; } }
        public Rigidbody2D Rb2d { get { return netObj.Rb2D; } }

        #endregion

        // Cached Values
        //protected bool foundExternalISpawnControl;
        protected MountsManager mountsLookup;
        protected SyncTransform syncTransform;
        protected SyncOwner syncOwner;
        protected ISpawnController iSpawnController;

        /// <summary>
        /// [mountTypeId, index]
        /// </summary>
        protected Dictionary<int, int> mountTypeIdToIndex = new Dictionary<int, int>();
        protected int[] indexToMountTypeId;
        protected int bitsForMountType;

        public override bool AllowReconstructionOfEmpty { get { return false; } }

        /// <summary>
        /// The state to which this object will be set when Respawn is called.
        /// </summary>
        protected StateChangeInfo respawnStateInfo;

        [System.NonSerialized] public List<IOnStateChange> onStateChangeCallbacks = new List<IOnStateChange>();
        [System.NonSerialized] public List<IFlagTeleport> flagTeleportCallbacks = new List<IFlagTeleport>();

        public class Frame : FrameBase
        {

            public enum Changes { None, MountIdChange }
            public ObjState state;
            //public bool respawn;
            public int? mountToViewID;
            public int? mountTypeId;

            public Frame() : base() { }

            public Frame(int frameId) : base(frameId) { }

            public override void CopyFrom(FrameBase sourceFrame)
            {
                base.CopyFrom(sourceFrame);
                Frame src = sourceFrame as Frame;
                state = src.state;
                //respawn = false;
                mountToViewID = src.mountToViewID;
                mountTypeId = src.mountTypeId;
            }

            public override void Clear()
            {
                base.Clear();
                state = 0;
                mountToViewID = null;
                mountTypeId = null;
                //respawn = false;
            }

            public bool Compare(Frame otherFrame)
            {
                if (/*respawn != otherFrame.respawn ||*/
                    state != otherFrame.state ||
                    mountToViewID != otherFrame.mountToViewID ||
                    mountTypeId != otherFrame.mountTypeId)
                    return false;

                return true;
            }
        }

#if UNITY_EDITOR

        protected override void Reset()
        {
            base.Reset();
            _alwaysReady = false;
            mountableTo = new MountMaskSelector(true);
        }

#endif

        public override void OnAwake()
        {
            base.OnAwake();

            iSpawnController = GetComponent<ISpawnController>();

            syncTransform = GetComponent<SyncTransform>();
            syncOwner = GetComponent<SyncOwner>();

            transform.GetNestedComponentsInChildren<IOnStateChange, NetObject>(onStateChangeCallbacks);
            transform.GetComponents(flagTeleportCallbacks);

            mountsLookup = netObj.GetComponent<MountsManager>();
        }

        public override void OnStart()
        {

            /// TEST - this code fixed startup rendering, but not fully tested. Likely needs to stay here.
            ChangeState(new StateChangeInfo(initialState, transform.parent ? transform.parent.GetComponent<Mount>() : null, true));

            base.OnStart();

            respawnStateInfo = new StateChangeInfo(
               respawnState,
               transform.parent ? transform.parent.GetComponent<Mount>() : null,
               transform.localPosition,
               transform.localRotation,
               null, true);

            /// Cache values for mountType serialization. We get the total possible mount options from this objects SyncState
            var mountableToCount = (mountableTo.mask).CountTrueBits(out indexToMountTypeId, MountSettings.mountTypeCount);

            bitsForMountType = mountableToCount.GetBitsForMaxValue();

            for (int i = 0; i < mountableToCount; ++i)
            {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (mountTypeIdToIndex.ContainsKey(indexToMountTypeId[i]))
                    Debug.LogError(name + " " + photonView.OwnerActorNr + " Mount Key Exists: " + indexToMountTypeId[i] + " i:" + i + " count: " + mountableToCount);
#endif

                mountTypeIdToIndex.Add(indexToMountTypeId[i], i);
            }
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            if (iSpawnController != null)
                return;

            // Intentionally first. NetObj start may want to change this immediately.
            ChangeState(new StateChangeInfo(initialState, transform.parent ? transform.parent.GetComponent<Mount>() : null, true));

        }

        public override void OnAuthorityChanged(bool isMine, bool controllerChanged)
        {
            base.OnAuthorityChanged(isMine, controllerChanged);

            /// Clear the queue, because there may be some items that never got consumed due to an authority change at startup.
            stateChangeQueue.Clear();
            /// TEST: added because multiple ownership changes were not detecting mount change for force reliable.
            prevSerializedFrame = null;
        }

        /// <summary>
        /// Respond to NetObject changes in Ready here.
        /// </summary>
        public virtual void OnNetObjReadyChange(bool ready)
        {
            //Debug.Log(name + " " + photonView.OwnerActorNr + " <color=purple>OnNetObjReadyChange</color> " + ready);

            netObjIsReady = ready;

            if (IsMine && iSpawnController != null && !iSpawnController.AllowNetObjectReadyCallback(ready))
            {
                //Debug.Log(name + " Supressing NetObjReady, object still counting down to respawn");
                return;
            }

            if (ready)
            {
                //if (!photonView.IsMine)
                //    Debug.Log(Time.time + " " + name + " <color><b>Ready</b></color> " + readyState + " currState: " + currentState.state +
                //                " par: " + currentMount + " tr: " + transform.position + " rb: " + Rb.position);

                /// We only want to change the state if the state currently matches unready. Otherwise authority changes trigger the default states.
                //if (currentState.state == unreadyState)
                ChangeState(new StateChangeInfo(readyState, currentMount, true));
                //ChangeState(new StateChangeInfo(currentState.state | ObjState.Visible, currentMount, false, true));
            }
            else
            {
                //if (!photonView.IsMine)
                //    Debug.Log(name + " <b>UnReady</b> " + readyState + " currState: " + currentState.state);

                ChangeState(new StateChangeInfo(unreadyState, currentMount, true));
                //ChangeState(new StateChangeInfo(currentState.state & ~ObjState.Visible, currentMount, false, true));
            }
        }

        #region State Change Shortcuts

        //public void Attach(Mount mount)
        //{
        //	/// Keep the current visibile state, change all others to just Attached
        //	var currentVisibleFlag = (currentState.state & ObjState.Visible);
        //	//Debug.Log(name + "<b> QUE Attach </b>");
        //	stateChangeQueue.Enqueue(new StateChangeInfo(currentVisibleFlag | ObjState.Attached, mount, true));


        //}


        public void SoftMount(Mount attachTo)
        {
            const ObjState MOUNT_ADD_FLAGS = ObjState.Mounted;
            const ObjState MOUNT_REM_FLAGS = ObjState.Dropped | ObjState.Transit;

            ObjState newstate = attachTo ? (currentState.state & ~MOUNT_REM_FLAGS) | MOUNT_ADD_FLAGS : currentState.state & ~MOUNT_ADD_FLAGS;
            //QueueStateChange(newstate, attachTo, false);
            ChangeState(new StateChangeInfo(newstate, attachTo, false));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mountTo"></param>
        public void HardMount(Mount mountTo)
        {
            const ObjState MOUNT_ADD_FLAGS = ObjState.Mounted | ObjState.Anchored;
            const ObjState MOUNT_REM_FLAGS = ObjState.Dropped | ObjState.Transit;

            ObjState newstate = mountTo ? (currentState.state & ~MOUNT_REM_FLAGS) | MOUNT_ADD_FLAGS : currentState.state & ~MOUNT_ADD_FLAGS;
            //QueueStateChange(newstate, mountTo, false);
            ChangeState(new StateChangeInfo(newstate, mountTo, false));
        }


        public void Spawn()
        {
            // Not Implemented... put a generic just works Spawn call here for making an Object pop back to life where it originated
        }

        public void Respawn(bool immediate)
        {
            //Debug.LogError(Time.time + " Respawn " + immediate);

            //Debug.Log(Time.time + " " + name + "<b> QUE Respawn </b> curr state = " + currentState.state);
            if (immediate)
                ChangeState(respawnStateInfo);
            else
                stateChangeQueue.Enqueue(respawnStateInfo);
        }

        public void Despawn(bool immediate)
        {
            //Debug.LogError(Time.time + " Despawn " + immediate);
            if (immediate)
                ChangeState(new StateChangeInfo(ObjState.Despawned, null, true));
            else
                stateChangeQueue.Enqueue(new StateChangeInfo(ObjState.Despawned, null, true));
        }

        /// <summary>
        /// Handler that unparents this object from any mount immediately (rather than on the next tick Capture).
        /// Put in place for handling despawning of objects, so that mounts on NetObjects can unmount all objects before self-destructing.
        /// </summary>
        public void ImmediateUnmount()
        {
            //Debug.Log(name + "<b> IMMEDIATE UNMOUNT </b>");
            stateChangeQueue.Clear();
            ChangeState(new StateChangeInfo(ObjState.Visible | ObjState.Transit | ObjState.Dropped, null, true));
        }

        public void Drop(Mount newMount, bool force = false)
        {
            const ObjState state = ObjState.Visible | ObjState.Dropped;
            stateChangeQueue.Enqueue(new StateChangeInfo(state, newMount, force));
        }

        public void Throw(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            const ObjState state = ObjState.Visible | ObjState.Dropped | ObjState.Transit;
            stateChangeQueue.Enqueue(new StateChangeInfo(state, null, position, rotation, velocity, false));
        }

        public void ThrowLocal(Transform origin, Vector3 offset, Vector3 velocity)
        {
            const ObjState state = ObjState.Visible | ObjState.Dropped | ObjState.Transit;
            stateChangeQueue.Enqueue(new StateChangeInfo(state, null, origin.TransformPoint(offset), origin.TransformPoint(velocity), false));
        }

        #endregion


        protected Queue<StateChangeInfo> stateChangeQueue = new Queue<StateChangeInfo>();

        //bool teleportNeeded = false;

        public virtual void QueueStateChange(ObjState newState, Mount newMount, bool force)
        {
            //if (IsMine)
            //	Debug.Log(Time.time + " " + name + "<b> QUE STATE </b>" + newState);

            stateChangeQueue.Enqueue(new StateChangeInfo(newState, newMount, null, null, force));
        }

        public virtual void QueueStateChange(ObjState newState, Mount newMount, Vector3 offset, Vector3 velocity, bool force)
        {
            //if (IsMine)
            //	Debug.Log(Time.time + " " + name + "<b> QUE STATE </b>" + newState);

            stateChangeQueue.Enqueue(new StateChangeInfo(newState, newMount, offset, velocity, force));
        }

        protected virtual void DequeueStateChanges()
        {
            //int newOwnerId = -1;

            while (stateChangeQueue.Count > 0)
            {
                var stateChangeInfo = stateChangeQueue.Dequeue();

                /*newOwnerId = */
                ChangeState(stateChangeInfo);
            }

            /// TODO: TEST removed, only letting this change on new owners snapshot currently. May be able to readd this safely.
            //if (autoOwnerChange && newOwnerId != -1)
            //{
            //	Debug.LogError("DISABLED");
            //	pv.TransferOwnership(newOwnerId);
            //}
        }

        /// <summary>
        /// Call this method to change the state of this object. This state will be synced over the network,
        /// and callbacks will trigger locally and remotely. Typically it is preferred to call QueueStateChange(), 
        /// which will defer the ChangeState application until the appropriate timing.
        /// </summary>
        protected virtual void ChangeState(StateChangeInfo stateChangeInfo)
        {
            if (!gameObject)
            {
                Debug.LogWarning(name + " has been destroyed. Will not try to change state.");
                return;
            }

            //if (photonView.IsMine && iSpawnController != null && iSpawnController.SupressStateMask != ~ObjState.Despawned)
            //{
            //    Debug.Log("<color=red>Suppressing State change due to spawn timer</color>");
            //    stateChangeInfo.objState &= iSpawnController.SupressStateMask;
            //}


            //if (GetComponent<SyncPickup>())
            //    Debug.LogError(Time.time + " " + name + " <b>ChangeState " + stateChangeInfo + "</b> par: " + currentMount + " tr: " + transform.position + " rb: " + Rb.position);

            var oldState = currentState.state;
            var oldMount = currentMount;
            var newState = stateChangeInfo.objState;
            var newMount = stateChangeInfo.mount;

            bool respawn;
            /// Assuming first Visible after a despawn is a Respawn - this is here to handle lost teleport packets
            if (autoReset && oldState == ObjState.Despawned && newState != ObjState.Despawned && (newState & ObjState.Anchored) == 0)
            {
                stateChangeInfo = new StateChangeInfo(respawnStateInfo) { objState = stateChangeInfo.objState };
                respawn = true;
            }
            else
                respawn = false;

            var force = stateChangeInfo.force;

            bool stateChanged = newState != oldState;
            bool mountChanged = oldMount != newMount;

            var prevParent = transform.parent;


            /// Test nothing has changed
            if (!force && !stateChanged && !mountChanged)
            {
                //if (GetComponent<SyncPickup>())
                //    Debug.LogError("NO CHANGE");
                return;
            }


            currentState.state = newState;
            var prevMount = currentMount;
            currentMount = newMount;

            //if (GetComponent<SyncPickup>())
            //    Debug.LogError("<b>ChangeState </b>" + currentState + " par: " + currentMount);

            bool nowMounted = (newState & ObjState.Mounted) != 0;
            bool mountInfoWasCulled = nowMounted && ReferenceEquals(newMount, null);

            // TODO: Might not be needed.
            /// Handling for attached without a valid Mount - Test for if we have a reference to a Mount, but it is null (Unity destroyed). Sets to hard null and reruns this method.
            if (nowMounted && !mountInfoWasCulled && newMount == null)
            {
                Debug.LogError("Invalid Mount!");
                InvalidMountHandler(newState, newMount, force);
                return;
            }

            if (IsMine)
                if (mountChanged || respawn)
                {
                    //Debug.LogError("TELEPORT");
                    for (int i = 0; i < flagTeleportCallbacks.Count; ++i)
                        flagTeleportCallbacks[i].FlagTeleport();
                }

            /// The State is mounted
            /*else*/
            if (mountChanged)
            {
                /// Attaching to a mount
                // If Attached bit is true
                if (nowMounted)
                {
#if PUN_2_OR_NEWER
                    //if (GetComponent<SyncPickup>() /*&& newState == (ObjState)(15)*/)
                    //    Debug.LogError(Time.time + " " + name + " " + newState + " ATTACH " + newMount.name + " ");

                    currentState.mountToViewID = newMount.ViewID;
                    currentState.mountTypeId = newMount.mountType.id;
#endif
                    transform.parent = newMount.transform;

                    // If anchor bit is true
                    if ((newState & ObjState.AnchoredPosition) != 0)
                    {
                        transform.localPosition = new Vector3();
                        //Debug.LogError("<b>Anchor Pos</b> " + transform.parent.name + " " + transform.localPosition);
                    }

                    // If anchor bit is true
                    if ((newState & ObjState.AnchoredRotation) != 0)
                    {
                        transform.localRotation = new Quaternion();
                        //Debug.Log("<b>Anchor Rot</b>");
                    }
                }

                /// Detaching from a mount
                else
                {
                    //if (GetComponent<SyncPickup>() /*&& newState == (ObjState)(15)*/)
                    //    Debug.Log(Time.time + " " + name + " " + newState + " DETTACH " + (newMount ? newMount.name : " null"));

                    currentState.mountToViewID = null;
                    currentState.mountTypeId = null;
                    transform.parent = null;
                }

                // TODO: Add handling for dismounting returning ownership to scene?
                // Ownership changes are deferred, to let the current tick finish processing before changing Owner/Controller.
                if (autoOwnerChange && newMount && !ReferenceEquals(oldMount, newMount))
                {

                    ChangeOwnerToParentMountsOwner();

                }
                Mount.ChangeMounting(this, prevMount, newMount);
            }

            var pos = stateChangeInfo.offsetPos;
            var rot = stateChangeInfo.offsetRot;
            var vel = stateChangeInfo.velocity;

            if (rot.HasValue)
            {
                //if (GetComponent<SyncPickup>())
                //    Debug.Log(Time.time + " " + name + " STATE ROT APPLY " + rot.Value);

                transform.rotation = rot.Value;
            }

            // TODO: Move pos/rot handling to virtual method
            if (pos.HasValue)
            {
                //if (GetComponent<SyncPickup>())
                //    Debug.Log(Time.time + " " + name + " STATE POS APPLY " + pos.Value);

                transform.position = pos.Value;

            }

            if (vel.HasValue)
            {

                var rb = netObj.Rb;
                if (rb)
                {
                    rb.velocity = vel.Value;
                }
                else
                {
                    var rb2d = netObj.Rb2D;

                    if (rb2d)
                    {

                        rb2d.velocity = vel.Value;
                    }
                }
            }

            ///// Apply the vector values
            //this.ApplyVectors(stateChangeInfo, prevParent, onTeleportCallbacks);

            //Debug.Log(Time.time + " " + stateChangeInfo + " mountisactive? " + (currentMount ? currentMount.isActiveAndEnabled.ToString() : "null"));

            if (mountChanged || stateChanged || force)
            {
                //if (GetComponent<SyncPickup>())
                //    Debug.Log(Time.time + " " + name +  " " + photonView.OwnerActorNr + " <b>NEW STATE: " + newState + "</b> ready? " + netObj.AllObjsAreReady);

                /// Send out callbacks
                for (int i = 0; i < onStateChangeCallbacks.Count; ++i)
                    onStateChangeCallbacks[i].OnStateChange(newState, oldState, transform, currentMount, netObjIsReady);
            }

        }

        //        private void TransferToMountOwner(Mount mount)
        //        {
        //#if PUN_2_OR_NEWER


        //            //Debug.LogWarning(Time.time + name + " <b>TransferToMountOwner</b> " + mountPV.IsMine + " " + pv.OwnerActorNr + " " + mountOwnerId);

        //            //if (mountPV.IsMine && photonView.OwnerActorNr != mountOwnerId)
        //            {
        //                //var ownershipTransfer = photonView.OwnershipTransfer;
        //                //if (ownershipTransfer == Photon.Pun.OwnershipOption.Takeover || (ownershipTransfer == Photon.Pun.OwnershipOption.Request && photonView.IsMine))
        //                //{
        //                //    photonView.TransferOwnership(mountOwnerId);

        //                //}

        //                // TEST: Locally set ownership on apply rather than waiting for Pun2 to do it

        //                // Ownership changes are deferred, to let the current tick finish processing before changing Owner/Controller.
        //                NetMasterCallbacks.onPostSimulationActions.Enqueue(ChangeOwnerToParentMountsOwner);
        //            }
        //#endif
        //        }

        /// <summary>
        /// Deferred method that applies pending onwer changes. This is used internally.
        /// </summary>
        private void ChangeOwnerToParentMountsOwner()
        {
            if (!IsMine)
                return;

            if (!syncOwner)
            {
                Debug.LogWarning(name + " cannot automatically change owner without a " + typeof(SyncOwner).Name + " component.");
                return;
            }

            // We are only changing ownership if there is a mount. 
            // This null test is for the edge case of throwing at the same time as colliding with another players object creates a race condition and the owner ends up being null/0
            if (currentMount == null)
                return;

            PhotonView mountView = currentMount.PhotonView;

            //Debug.LogError(Time.time + " DEFERRED New Owner " + (mountView ? mountView.name + ":" + mountView.Owner.ToString() : "null"));

            if (mountView)
            {
                Realtime.Player pendingOwner = mountView.Owner;
                int pendingOwnerId = pendingOwner == null ? 0 : pendingOwner.ActorNumber;

                //photonView.SetOwnerInternal(pendingOwner, pendingOwnerId);
                if (this.autoOwnerChange)
                    this.syncOwner.TransferOwner(pendingOwnerId);
            }
            else
            {
                //photonView.SetOwnerInternal(null, 0);
                if (this.autoOwnerChange)
                    GetComponent<SyncOwner>().TransferOwner(0);

            }

            // TEST invalidate all netobj buffers on ownership change
            //netObj.frameValidMask.SetAllFalse();
        }

        /// <summary>
        /// Modify StateChange call if the mount was invalid (Mount likely destroyed).
        /// </summary>
        protected virtual void InvalidMountHandler(ObjState newState, Mount newMount, bool force)
        {
            Debug.LogWarning("Invalid Mount Handled!!");
            ChangeState(new StateChangeInfo(ObjState.Visible, null, true));
        }

        /// <summary>
        /// Attempt to change to a different Mount on the same object.
        /// </summary>
        /// <param name="newMountId"></param>
        public virtual bool ChangeMount(int newMountId)
        {
            if (ReferenceEquals(currentMount, null))
            {
                Debug.LogWarning("'" + name + "' is not currently mounted, so we cannot change to a different mount.");
                return false;
            }

            if ((mountableTo & (1 << newMountId)) == 0)
            {
                Debug.LogWarning("'" + name + "' is trying to switch to a mount '" + MountSettings.GetName(newMountId) + "' , but mount is not set as valid in SyncState.");
                return false;
            }

            var lookup = currentMount.mountsLookup.mountIdLookup;

            if (!lookup.ContainsKey(newMountId))
            {
                Debug.LogWarning("'" + name + "' doesn't contain a mount for '" + MountSettings.GetName(newMountId) + "'.");
                return false;
            }
            var attachTo = lookup[newMountId];

            //Debug.Log("New Mount:" + attachTo + ":" + newMountId);

            ChangeState(new StateChangeInfo(currentState.state, attachTo, false));

            return true;
        }


        public void OnCaptureCurrentState(int frameId)
        {
            DequeueStateChanges();

            Frame frame = frames[frameId];

            frame.CopyFrom(currentState);

            //if (GetComponent<SyncPickup>())
            //    Debug.LogError(frameId + " <b>Capture </b>" + frame.state + " / " + currentState.state);

            //if (currentState.respawn)
            //{
            //	frame.respawn = true;
            //	currentState.respawn = false;
            //}
        }

        #region Serialization

        // TODO: May not be needed due to has changed checks
        protected Frame prevSerializedFrame;

        public SerializationFlags OnNetSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
        {

            //if (false)
            /// Don't transmit data if this component is disabled. Allows for muting components
            /// Simply by disabling them at the authority side.
            if (!enabled) // This was ActiveAndEnabled... but that is a problem if mounting immediately to a spawned object, as it is disabled for some reason.
            {
                // leading bool
                buffer.WriteBool(false, ref bitposition);
                return SerializationFlags.None;
            }

            Frame frame = frames[frameId];

            bool iskeyframe = IsKeyframe(frameId);
            bool isNewConnection = (writeFlags & SerializationFlags.NewConnection) != 0;
            bool isFirstSend = ReferenceEquals(prevSerializedFrame, null);
            bool stateHasChanged = isFirstSend || frame.state != prevSerializedFrame.state;
            //bool parentHasChanged = isFirstSend || frame.attachedToViewID != prevSerializedFrame.attachedToViewID;
            bool mountHasChanged = isFirstSend || frame.mountTypeId != prevSerializedFrame.mountTypeId || frame.mountToViewID != prevSerializedFrame.mountToViewID;
            bool needsComplete = isNewConnection || iskeyframe || isFirstSend;
            bool hasChanged = stateHasChanged || mountHasChanged;
            bool hasContent = needsComplete || hasChanged;
            bool forceReliable = isFirstSend || isNewConnection || (keyframeRate == 0 && hasContent) || (mountReliable && mountHasChanged);

            // If we determined there is nothing to send, first bit 0 and exit
            if (!hasContent && !forceReliable)
            {
                // leading bool
                buffer.WriteBool(false, ref bitposition);
                return SerializationFlags.None;
            }

            // Leading bool
            buffer.WriteBool(true, ref bitposition);

            var flags = SerializationFlags.HasContent;

            // Determine if anything requires this be made a Reliable send
            if (forceReliable)
            {
                //if (GetComponent<SyncPickup>())
                //    Debug.LogError(Time.time + " <b>" +  frame.frameId + ":" + frameId + "</b> STATE " + isFirstSend + " " + isNewConnection + " " + (keyframeRate == 0 && hasContent) + " " + (mountReliable && mountHasChanged) + " " + frame.state);

                flags |= SerializationFlags.ForceReliable;
            }

            /// Write State - it is cheap enough to send it every tick
            buffer.Write((ulong)frame.state, ref bitposition, 6);

            if ((frame.state & ObjState.Mounted) != 0)
            {
                if (mountHasChanged || needsComplete)
                {
                    /// Write HasMount info bit
                    if (!iskeyframe)
                        buffer.Write(1, ref bitposition, 1);

                    //if (GetComponent<SyncPickup>())
                    //    Debug.LogError(frameId + " Write Parent " + frame.mountToViewID);

                    // Write Parent viewId
                    buffer.WritePackedBytes((uint)frame.mountToViewID, ref bitposition, 32);

                    // If there is more than one mount type this is allowed to mount to, indicate the mount type index (not the index on the parent, the index from allowed types for this SyncState)
                    if (bitsForMountType > 0)
                    {
                        var mountidx = mountTypeIdToIndex[frame.mountTypeId.Value];
                        buffer.Write((uint)mountidx, ref bitposition, bitsForMountType);
                    }
                }
                else
                {
                    /// Write HasMount info bit
                    if (!iskeyframe)
                        buffer.Write(0, ref bitposition, 1);
                }
            }

            //if (GetComponent<SyncPickup>())
            //    Debug.Log(frameId + " <b>SER</b> " + flags + " " + frame.state + " " + frame.attachedToViewID + " : " + frame.attachedToMountTypeId + " mounthaschagned: " + mountHasChanged + " needscomplete: " + needsComplete );

            prevSerializedFrame = frame;
            return flags;
        }

        public SerializationFlags OnNetDeserialize(int originFrameId, byte[] buffer, ref int bitposition, FrameArrival arrival)
        {

            Frame frame = frames[originFrameId];

            if (!buffer.ReadBool(ref bitposition))
            {
                //if (GetComponent<SyncPickup>())
                //    Debug.LogError(originFrameId + " No Content");

                return SerializationFlags.None;
            }

            SerializationFlags flags = SerializationFlags.HasContent;

            /// Read State
            frame.state = (ObjState)buffer.Read(ref bitposition, 6);

            bool isAttached = (frame.state & ObjState.Mounted) != 0;

            if (!isAttached)
            {
                frame.content = FrameContents.Complete;
            }
            else if (IsKeyframe(originFrameId) || buffer.Read(ref bitposition, 1) == 1)
            {
                /// Read attached
                if ((frame.state & ObjState.Mounted) != 0)
                {
                    frame.mountToViewID = (int?)buffer.ReadPackedBytes(ref bitposition, 32);
                    if (bitsForMountType > 0)
                    {
                        int mountidx = (int)buffer.Read(ref bitposition, bitsForMountType);
                        int mountTypeId = indexToMountTypeId[mountidx];
                        frame.mountTypeId = (int?)mountTypeId;
                    }
                    else
                        frame.mountTypeId = 0;
                }
                frame.content = FrameContents.Complete;
            }

            /// State is attached, but because this is a delta frame the parent info is missing
            else
            {
                frame.mountToViewID = null;
                frame.mountTypeId = null;
                frame.content = FrameContents.Partial;
            }

            //if (GetComponent<SyncPickup>())
            //    Debug.Log(Time.time + " " + flags + " " + name + " <b> fr: " + frame.frameId + " <color=purple>DES STATE</color></b> " + frame.content + " " + frame.state +
            //        ((frame.state & ObjState.Visible) != 0 ? " VISIBLE " : " ") +
            //        ((frame.state & ObjState.Mounted) != 0 ? " MOUNTED " + frame.mountToViewID : " ")
            //        );

            return flags;
        }

        #endregion Serialization

        //public void HandleCriticallyLateFrame(int frameId)
        //{
        //    return;

        //    if (GetComponent<SyncPickup>())
        //        Debug.LogWarning("<b>APPLY CRIT LATE ?</b>" + frameId);

        //    var frame = frames[frameId];
        //    var state = frame.state;
        //    var mount = GetMount(frame.mountToViewID, frame.mountTypeId);

        //    // Late mount change messages we are applying
        //    if (mount != currentMount)
        //    {
        //        Debug.LogWarning("<b>APPLY CRIT LATE </b>" + frameId);
        //        ApplyFrame(frame);

        //    }
        //}

        protected override void ApplySnapshot(Frame snapframe, Frame targframe, bool snapIsValid, bool targIsValid)
        {

            if (snapframe.content == FrameContents.Empty)
            {
                return;
            }

            /// Notifying the SyncTransform of any parent changes, since they are managed here. Less than ideal, but the alternative is to completely move parent handing to SyncTransform.
            /// If the frame being passed is the snapFrame (we aren't doing some kind of rewind), notify the transform sync of upcoming parent changes
            if (targframe.content == FrameContents.Complete) // & FrameContents.Partial) != 0)
            {
                Transform par;
                if (targframe.mountToViewID.HasValue)
                {
                    var targmount = GetMount(targframe.mountToViewID, targframe.mountTypeId);
                    par = targmount ? targmount.transform.parent : null;
                }
                else
                    par = null;

                if (syncTransform)
                    syncTransform.UpdateParent(targframe.state, par);
            }

            //if (GetComponent<SyncPickup>() /*&& snapFrame.state == (ObjState)15*/)
            //    Debug.Log(Time.time + " " + name + " <color=green><b>APPLY STATE</b></color> content :" + snapFrame.content +
            //        "\n<b>snap: fid: " + snapFrame.frameId + "</b> snapState: " + (snapFrame.state + "  attchTo: " + snapFrame.mountToViewID + ":" + snapFrame.mountTypeId) +
            //        " <b>Targ: fid: " + targFrame.frameId + "</b> targState: " +
            //        (targFrame.content != 0 ? (targFrame.state + "  attchTo: " + targFrame.mountToViewID + ":" + targFrame.mountTypeId) : " lost") + " " + transform.localPosition + " " +
            //        netObj.AllObjsAreReady
            //        );

            if (snapframe.content >= FrameContents.Extrapolated)
                ApplyFrame(snapframe);
        }


        private void ApplyFrame(Frame frame)
        {


            var state = frame.state;

            Mount applyMount;

            bool mounted = ((state & ObjState.Mounted) != 0);
            bool force = false;

            if (mounted)
            {
                int? snapAttachedToViewID = frame.mountToViewID;

                /// attached ID will only be sent on keyframes, so the id may be null even though pickup is attached. Use the old id value if so.
                if (snapAttachedToViewID.HasValue)
                {
                    int? snapAttachedToMountId = frame.mountTypeId;

                    var mount = GetMount(snapAttachedToViewID, snapAttachedToMountId);

                    // Exit if state has not changed.
                    if (ReferenceEquals(mount, currentMount) && state == currentState.state)
                        return;

                    if (mount)
                    {
                        //if (autoOwnerChange && !ReferenceEquals(currentMount, mount))
                        //    TransferToMountOwner(mount);

                        applyMount = mount;
                        ReadyState = ReadyStateEnum.Ready;
                        force = true;
                    }
                    else
                        applyMount = currentMount;

                    //if (GetComponent<SyncPickup>())
                    //    Debug.Log(Time.time + " " + name + " snapMount: " + snapMount);

                }
                /// Because of delta frames and packetloss, we know this is attached, but don't know what to!
                else
                {
                    /// Has become attached. Since we don't know
                    if (currentMount == null)
                    {
                        //if (GetComponent<SyncPickup>())
                        //	Debug.Log("<color=red>UNREADY</color>");

                        //ReadyState = ReadyStateEnum.Unready;
                        applyMount = null;
                    }
                    else
                    {
                        ReadyState = ReadyStateEnum.Ready;
                        applyMount = currentMount;
                        force = true;
                    }
                }
            }
            /// Detached
            else
            {
                ReadyState = ReadyStateEnum.Ready;
                force = true;
                applyMount = null;
            }

            //if (syncTransform && attach || snapMount != null)
            //	syncTransform.UpdateParent(targFrame.state, null);
            //if (GetComponent<SyncPickup>())
            //    Debug.Log("Snap " + snapState + " " + snapFrame.attachedToViewID + ":" + snapFrame.attachedToMountTypeId + " --- " + snapMount);

            ChangeState(new StateChangeInfo(state, applyMount, force/*snapFrame.content == FrameContents.Complete*/));
        }


        public static Mount GetMount(int? viewID, int? mountId)
        {
            if (!viewID.HasValue || !mountId.HasValue)
                return null;

            var pv = PhotonNetwork.GetPhotonView(viewID.Value);
            var mounts = pv ? pv.GetComponent<MountsManager>() : null;

            if (mounts)
                return mounts.mountIdLookup[mountId.Value];

            return null;
        }

        //protected override void ConstructMissingFrame(Frame prevFrame, Frame snapFrame, Frame targFrame)
        //{
        //    //base.ConstructMissingFrame(prevFrame, snapFrame, targFrame);
        //}



    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SyncState))]
    [CanEditMultipleObjects]
    public class SyncStateEditor : SyncObjectEditor
    {
        protected override string HelpURL
        {
            get { return Internal.SimpleDocsURLS.SYNCCOMPS_PATH + "#syncstate_component"; }
        }

        protected override string TextTexturePath
        {
            get
            {
                return "Header/SyncStateText";
            }
        }

        protected override string Instructions
        {
            get
            {
                return "Manages and syncs the State (Visibility, Attachment, Dropped, etc) of this NetObject. " +
                    "Calls to <b>syncState.ChangeState()</b> will replicate, and " +
                    "Components with <b>" +

                    typeof(IOnStateChange).Name + "</b> will receive callbacks.";
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            MountSettings.Single.DrawGui(target, true, false, false, false);
        }

    }
#endif
}

