// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Compression;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
    /// <summary>
    /// Base class of synced projectile/hitscan sources.
    /// </summary>
    /// <typeparam name="TFrame"></typeparam>
    public abstract class SyncShootBase : SyncObject<SyncShootBase.Frame>
        , IOnNetSerialize
        , IOnPostSimulate
        , IOnPreUpdate
        , IOnIncrementFrame
        , IOnSnapshot
        , IOnInterpolate
    {
        #region Inspector

        [Tooltip("Specify the transform hitscans/projectiles will originate from. If null this gameObject will be used as the origin.")]
        [SerializeField] protected Transform origin;
        [SerializeField] public KeyCode triggerKey = KeyCode.None;

        #endregion

        // Cached
        protected IContactTrigger contactTrigger;
        public IContactTrigger ContactTrigger { get { return contactTrigger; } }

        protected bool hasSyncContact;

        // internal States
        protected bool triggerQueued;

        public override int ApplyOrder
        {
            get
            {
                return ApplyOrderConstants.HITSCAN;
            }
        }

        public class Frame : FrameBase
        {
            public uint triggerMask;

            public Frame() : base() { }
            public Frame(int frameId) : base(frameId) { }

            public override void CopyFrom(FrameBase sourceFrame)
            {
                /// We do not want to copy triggers (would produce new trigger events)
                triggerMask = 0;
            }

            public override void Clear()
            {
                base.Clear();
                triggerMask = 0;
            }
        }

        public override void OnAwakeInitialize(bool isNetObject)
        {
            base.OnAwakeInitialize(isNetObject);

            contactTrigger = transform.GetNestedComponentInParents<IContactTrigger, NetObject>();
            hasSyncContact = contactTrigger.SyncContact != null;

            if (origin == null)
                origin = transform;

        }

        public virtual void OnPreUpdate()
        {
            if (IsMine && Input.GetKeyDown(triggerKey))
                QueueTrigger();
        }

        /// <summary>
        /// Call this on the authority to initiate a hitscan. Actual firing may be deferred based on settings.
        /// </summary>
        public virtual void QueueTrigger()
        {
            if (enabled && gameObject.activeInHierarchy)
                triggerQueued = true;
        }


        #region Serialization


        public virtual SerializationFlags OnNetSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
        {

            Frame frame = frames[frameId];

            int sendEveryXTick = TickEngineSettings.sendEveryXTick;

            /// Serialize TriggerMask (each 
            if (frame.triggerMask != 0)
            {
                buffer.WriteBool(true, ref bitposition);
                buffer.Write(frame.triggerMask, ref bitposition, sendEveryXTick);
                return SerializationFlags.HasContent /*| SerializationFlags.ForceReliable*/;

            }
            else
            {
                buffer.WriteBool(false, ref bitposition);
                return SerializationFlags.None;
            }

        }

        public virtual SerializationFlags OnNetDeserialize(int originFrameId, byte[] buffer, ref int bitposition, FrameArrival arrival)
        {
            Frame frame = frames[originFrameId];

            int sendEveryXTick = TickEngineSettings.sendEveryXTick;

            if (buffer.ReadBool(ref bitposition))
            {
                frame.triggerMask = buffer.ReadUInt32(ref bitposition, sendEveryXTick);
                frame.content = FrameContents.Complete;
                return SerializationFlags.HasContent;
            }
            else
            {
                frame.triggerMask = 0;
                frame.content = FrameContents.Empty;
                return SerializationFlags.None;
            }
        }

        #endregion Serialization


        public virtual void OnPostSimulate(int frameId, int subFrameId, bool isNetTick)
        {
            if (!IsMine)
                return;

            Frame frame = frames[frameId];

            /// Clear the trigger mask for new frames
            /// TODO: this is probably not good now that Snapshot will rerun on late frame arrivals
            if (subFrameId == 0)
            {
                frame.Clear();
            }

            /// Process Fire 
            if (triggerQueued) //subFrameId < SimpleSyncSettings.sendEveryXTick)
            {
                frame.triggerMask |= (uint)1 << subFrameId;

                if (Trigger(frame, subFrameId))
                    TriggerCosmetic(frame, subFrameId);

                triggerQueued = false;
                ///TODO: is this needed?
                frame.content = FrameContents.Complete;
            }
        }

        /// <summary>
        /// Since shots can be spread over the duration of a frame, we apply them onIncrement.
        /// </summary>
        public virtual void OnIncrementFrame(int newFrameId, int newSubFrameId, int previousFrameId, int prevSubFrameId)
        {
            if (IsMine)
                return;

            if (ReferenceEquals(targFrame, null))
                return;

            if (targFrame.content == FrameContents.Complete)
            {
                int offset = (newSubFrameId == 0) ? (TickEngineSettings.sendEveryXTick - 1) : newSubFrameId - 1;
                ApplySubframe(newFrameId, newSubFrameId, offset);

            }
        }


        protected virtual void ApplySubframe(int newFrameId, int newSubFrameId, int offset)
        {
            if ((targFrame.triggerMask & (1 << offset)) != 0)
            {
                if (Trigger(targFrame, newSubFrameId, NetMaster.RTT))
                    TriggerCosmetic(targFrame, newSubFrameId, NetMaster.RTT);
            }
        }

        /// <summary>
        /// Instantiate the weapon graphic and hit tests code if applicable. Results should be stored to the frame.
        /// </summary>
        /// <param name="frame"></param>
        protected abstract bool Trigger(Frame frame, int subFrameId, float timeshift = 0);
        protected virtual void TriggerCosmetic(Frame frame, int subFrameId, float timeshift = 0)
        {

        }

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SyncShootBase), true)]
    [CanEditMultipleObjects]
    public class SyncShootBaseEditor : SyncObjectEditor
    {
        protected override string HelpURL
        {
            get { return "subsystems#projectiles_and_hitscans"; }
        }

        protected override string Instructions
        {
            get
            {
                return "Trigger by calling this" + typeof(SyncShootBase).Name + ".QueueTrigger()";
            }
        }
        protected override string TextTexturePath
        {
            get
            {
                return "Header/SyncNetHitText";
            }
        }
    }
#endif
}
