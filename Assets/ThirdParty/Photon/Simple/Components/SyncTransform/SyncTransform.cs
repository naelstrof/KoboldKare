// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using emotitron.Compression;
using Photon.Compression;

namespace Photon.Pun.Simple
{
    public interface ISyncTransform
    {

    }

    public interface ITransformController
    {
#if UNITY_EDITOR
        bool AutoSync { get; }
#endif
        bool HandlesInterpolation { get; }
        bool HandlesExtrapolation { get; }
    }

    [DisallowMultipleComponent]
    public class SyncTransform : SyncObject<SyncTransform.Frame>
        , ISyncTransform
        , IOnSnapshot
        , IOnNetSerialize
        , IOnAuthorityChanged
        , IReadyable
        , IUseKeyframes
        , IDeltaFrameChangeDetect
        //, IAdjustableApplyOrder
        , IOnInterpolate
        , IOnCaptureState
        , IFlagTeleport
    {
        #region Inspector Fields

        [Tooltip("How lerping between tick states is achieved. 'Standard' is Linear. 'None' holds the previous state until t = 1. " +
            "'Catmull Rom' is experimental.")]
        public Interpolation interpolation = Interpolation.Linear;

        [Tooltip("Percentage of extrapolation from previous values. [0 = No Extrapolation] [.5 = 50% extrapolation] [1 = Undampened]. " +
            "This allows for gradual slowing down of motion when the buffer runs dry.")]
        [Range(0f, 1f)]
        public float extrapolateRatio = .5f;
        protected int extrapolationCount;

        [Tooltip("If the distance delta between snapshots exceeds this amount, object will move to new location without lerping. Set this to zero or less to disable (for some tiny CPU savings). You can manually flag a teleport by setting the HasTeleported property to True.")]
        public float teleportThreshold = 5f;
        private float teleportThresholdSqrMag;

        [Tooltip("Entire tick update from this client (all objects being serialized) will be sent as Reliable when FlagTeleport() has been called.")]
        public bool teleportReliable = false;

        public Dictionary<int, TransformCrusher> masterSharedCrushers = new Dictionary<int, TransformCrusher>();
        public TransformCrusher transformCrusher = new TransformCrusher()
        {
            PosCrusher = new ElementCrusher(TRSType.Position, false)
            {
                hideFieldName = true,
                XCrusher = new FloatCrusher(Axis.X, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat, AccurateCenter = true },
                YCrusher = new FloatCrusher(Axis.Y, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat, AccurateCenter = true },
                ZCrusher = new FloatCrusher(Axis.Z, TRSType.Position, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat, AccurateCenter = true },
            },
            RotCrusher = new ElementCrusher(TRSType.Quaternion, false)
            {
                hideFieldName = true,
                XCrusher = new FloatCrusher(Axis.X, TRSType.Euler, true) { Bits = 12, AccurateCenter = true },
                YCrusher = new FloatCrusher(Axis.Y, TRSType.Euler, true) { Bits = 12, AccurateCenter = true },
                ZCrusher = new FloatCrusher(Axis.Z, TRSType.Euler, true) { Bits = 12, AccurateCenter = true },
                QCrusher = new QuatCrusher(44, true, false),

                //QCrusher = new QuatCrusher(CompressLevel.uint64Hi, false, false)
            },
            SclCrusher = new ElementCrusher(TRSType.Scale, false)
            {
                hideFieldName = true,
                uniformAxes = ElementCrusher.UniformAxes.NonUniform,
                //UCrusher = new FloatCrusher(Axis.Uniform, TRSType.Scale, true) { BitsDeterminedBy = BitsDeterminedBy.HalfFloat, axis = Axis.Uniform, TRSType = TRSType.Scale }
                XCrusher = new FloatCrusher(BitPresets.Bits8, -1, 1, Axis.X, TRSType.Scale, true) { TRSType = TRSType.Scale, AccurateCenter = true, BitsDeterminedBy = BitsDeterminedBy.SetBits },
                YCrusher = new FloatCrusher(BitPresets.Bits8, -1, 1, Axis.Y, TRSType.Scale, true) { TRSType = TRSType.Scale, AccurateCenter = true, BitsDeterminedBy = BitsDeterminedBy.SetBits, Enabled = false },
                ZCrusher = new FloatCrusher(BitPresets.Bits8, -1, 1, Axis.Z, TRSType.Scale, true) { TRSType = TRSType.Scale, AccurateCenter = true, BitsDeterminedBy = BitsDeterminedBy.SetBits, Enabled = false },
            }
        };

        #endregion


        #region Teleport

        /// <summary>
        ///When IsMine: OnTeleport sets this true to indicate the next outgoing serialization should be flagged as a teleport.
        ///When !IsMine: Is set during Snapshot to indicate that interpolation should not occur.
        /// </summary>
        protected bool hasTeleported;
        //protected bool parentChanged;
        protected int teleNewParentId;
        protected Matrix preTeleportM = new Matrix();
        protected CompressedMatrix preTeleportCM = new CompressedMatrix();

        /// <summary>
        /// Be sure to call this in the Capture segment BEFORE you reparent the object. Captures and holds the current transform prior to
        /// changes you are about to make.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="scl"></param>
        public void FlagTeleport()
        {
            if (IsMine)
            {
                if (!hasTeleported)
                    CaptureCurrent(preTeleportM, preTeleportCM/*, Realm.Primary, true*/);

                //Debug.LogError(Time.time + " " + name + " Flag TELE ");
                this.hasTeleported = true;
            }
        }

        /// <summary>
        /// Internal. StateSync uses this method to tell the SyncTransform what the parent object is. 
        /// SyncTransform needs to know about parent changes to avoid interpolating/extrapolating across parent changes.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="newParent"></param>
        public void UpdateParent(ObjState state, Transform newParent)
        {
            teleNewParentId = newParent ? newParent.GetInstanceID() :
                (state & ObjState.Mounted) != 0 ? -2 : -1;

            //if (GetComponent<SyncState>())
            //	Debug.Log(Time.time + " <b><color=green>UpdateParent </color></b>" + teleNewParentId);
        }

        #endregion

        // Cached
        private Rigidbody rb;
        private Rigidbody2D rb2d;
        private List<ITransformController> iTransformControllers = new List<ITransformController>(1);

        protected bool allowInterpolation;
        public override bool AllowInterpolation { get { return allowInterpolation; } }

        protected bool allowReconstructionOfEmpty;
        public override bool AllowReconstructionOfEmpty { get { return allowReconstructionOfEmpty; } }

        public override int ApplyOrder
        {
            get
            {
                return ApplyOrderConstants.TRANSFORM;
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            /// Set the crushers to use local, so that re/deparenting is doable by default.
            transformCrusher.PosCrusher.local = true;
            transformCrusher.RotCrusher.local = true;
            transformCrusher.RotCrusher.TRSType = TRSType.Euler;
            transformCrusher.SclCrusher.local = true;

            /// Default alwaysReady based on root or not.
            _alwaysReady = transform.parent != null;
        }
#endif

        public override void OnAwake()
        {
            base.OnAwake();
            rb = GetComponent<Rigidbody>();
            rb2d = GetComponent<Rigidbody2D>();
            GetComponents(iTransformControllers);

            teleportThresholdSqrMag = teleportThreshold <= 0 ? 0 : teleportThreshold * teleportThreshold;

            ConnectSharedCaches();

            allowInterpolation = true;
            allowReconstructionOfEmpty = true;

            // cache whether this syncTransform is allowed to reconstruct missing frames
            for (int i = 0; i < iTransformControllers.Count; ++i)
            {
                var controller = iTransformControllers[i];
                allowInterpolation &= !controller.HandlesInterpolation;
                allowReconstructionOfEmpty &= !controller.HandlesExtrapolation;
            }
        }

        private void ConnectSharedCaches()
        {
            if (masterSharedCrushers.ContainsKey(prefabInstanceId))
                transformCrusher = masterSharedCrushers[prefabInstanceId];
            else
                masterSharedCrushers.Add(prefabInstanceId, transformCrusher);
        }

        private void OnDestroy()
        {
            framePool.Push(frames);
        }

        #region Frames

        public class Frame : FrameBase
        {
            public bool hasTeleported;
            public Matrix m;
            public CompressedMatrix cm;
            public SyncTransform owner;
            public Matrix telem;
            public CompressedMatrix telecm;
            public int parentHash;
            public int telePparentHash;

            public Frame() : base()
            {
                m = new Matrix();
                cm = new CompressedMatrix();
                telem = new Matrix();
                telecm = new CompressedMatrix();
                parentHash = -2;
            }

            public Frame(SyncTransform sst, int frameId) : base(frameId)
            {
                m = new Matrix();
                cm = new CompressedMatrix();
                telem = new Matrix();
                telecm = new CompressedMatrix();
                sst.transformCrusher.Capture(sst.transform, cm, m);
                var par = sst.transform.parent;
                parentHash = par ? par.GetInstanceID() : -1;
            }

            public Frame(Frame srcFrame, int frameId) : base(frameId)
            {
                m = new Matrix();
                cm = new CompressedMatrix();
                telem = new Matrix();
                telecm = new CompressedMatrix();
                CopyFrom(srcFrame);
            }

            public void Set(SyncTransform sst, int frameId)
            {
                sst.transformCrusher.Capture(sst.transform, cm, m);
            }

            public override void CopyFrom(FrameBase sourceFrame)
            {
                base.CopyFrom(sourceFrame);
                Frame src = sourceFrame as Frame;

                /// When copying a teleport frame, we use the tele values.
                if (src.hasTeleported)
                {
                    m.CopyFrom(src.telem);
                    cm.CopyFrom(src.telecm);
                }
                else
                {
                    m.CopyFrom(src.m);
                    cm.CopyFrom(src.cm);
                }

                hasTeleported = false; // src.hasTeleported;
                parentHash = src.parentHash;
            }

            //static readonly StringBuilder strb = new StringBuilder();
            /// <summary>
            /// Compares only the compressed values for equality
            /// </summary>
            public bool FastCompareCompressed(Frame other)
            {
                bool match = cm.Equals(other.cm);

                if (match)
                    return true;

                return false;
            }
            /// <summary>
            /// Compares only the compressed values for equality
            /// </summary>
            public bool FastCompareUncompressed(Frame other)
            {
                return
                    m.position == other.m.position &&
                    m.rotation == other.m.rotation &&
                    m.scale == other.m.scale;
            }

            public override void Clear()
            {
                base.Clear();
                hasTeleported = false;
                parentHash = -2;
            }
            public override string ToString()
            {
                return "[" + frameId + " " + m.position + " / " + m.rotation + "]";
            }
        }

        #endregion


        #region Frame Pooling

        public static Stack<Frame[]> framePool = new Stack<Frame[]>();

        /// <summary>
        /// We reuse frame buffers for Transforms.
        /// </summary>
        protected override void PopulateFrames()
        {
            int frameCount = TickEngineSettings.frameCount;

            /// Get frames from pool or create a new array.
            if (framePool.Count == 0)
            {
                frames = new Frame[frameCount + 1];
                /// Get the offtick frame the slow way, then just copy that for all the other frames.
                frames[frameCount] = new Frame(this, frameCount);
                for (int i = 0; i <= frameCount; ++i)
                    frames[i] = new Frame(frames[frameCount], i);
            }
            else
            {
                /// Get pooled frame, and populate with starting values from this
                frames = framePool.Pop();
                /// Get the offtick frame the slow way, then just copy that for all the other frames.
                frames[frameCount].Set(this, frameCount);
                for (int i = 0; i < frameCount; ++i)
                    frames[i].CopyFrom(frames[frameCount]);
            }
        }
        #endregion


        protected void CaptureCurrent(Matrix m, CompressedMatrix cm, bool forceUseTransform = false)
        {
            if (forceUseTransform)
            {
                transformCrusher.Capture(transform, cm, m);
            }
            else if (rb)
            {
                transformCrusher.Capture(rb, cm, m);

            }
            /// TODO: Not currently working
            else if (rb2d)
            {
                transformCrusher.Capture(rb2d, cm, m);
            }
            else
            {
                transformCrusher.Capture(transform, cm, m);
            }
        }

        public virtual void OnCaptureCurrentState(int frameId)
        {
            Frame frame = frames[frameId];
            frame.hasTeleported = hasTeleported;

            if (hasTeleported)
            {
                //Debug.LogError(frameId + " <color=blue>SST HasTeleported</color> m: " + frame.m.position + " -> tm: " + frame.telem.position + " "
                //	+ (transform.parent ? transform.parent.name : "null"));

                /// We want to use the captured values for the m and cm, as they were captured before possible parent change post teleport.
                frame.cm.CopyFrom(preTeleportCM);
                frame.m.CopyFrom(preTeleportM);
                CaptureCurrent(frame.telem, frame.telecm, true);
                transformCrusher.Apply(transform, frame.telem);

                //if (GetComponent<SyncPickup>())
                //	Debug.Log(Time.time + " " + name + " " + frameId + " <b>TELE</b> " + frame.telem.position + " : " + frame.m.position + " " + (transform.parent ? transform.parent.name : "null"));

                hasTeleported = false;
            }
            else
            {
                CaptureCurrent(frame.m, frame.cm);

                //if (GetComponent<SyncPickup>())
                //	Debug.Log(Time.time + " " + name + " " + frameId + " <b>CAP</b> " + frame.telem.position + " : " + frame.m.position + " " + (transform.parent ? transform.parent.name : "null"));
            }

        }

        #region Serialization

        public SerializationFlags OnNetSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
        {
            Frame frame = frames[frameId];

            bool hasTeleported = frame.hasTeleported;
            bool mustSend = hasTeleported || (writeFlags & SerializationFlags.NewConnection) != 0;

            /// Don't transmit data non-critical updates if this component is disabled. Allows for muting components
            /// Simply by disabling them at the authority side.
            /// Currently teleports and new connections still send even if disabled, but normal keyframes and changes are not sent.
            if (!mustSend && !isActiveAndEnabled)
            {
                buffer.WriteBool(false, ref bitposition);
                return SerializationFlags.None;
            }

            bool isKeyframe = IsKeyframe(frameId);

            /// Only check for changes if we aren't forced to send by a keyframe.
            if (!mustSend && !isKeyframe)
            {
                bool hascontent = useDeltas && (prevSentFrame == null || !frame.cm.Equals(prevSentFrame.cm));

                if (!hascontent)
                {
                    buffer.WriteBool(false, ref bitposition);
                    prevSentFrame = frame;
                    //Debug.LogError("Skipping " + frameId);
                    return SerializationFlags.None;
                }
            }

            SerializationFlags flags = SerializationFlags.HasContent;

            //Debug.LogError("OUT " + frameId + " " + frame.m.position);

            /// has content bool
            buffer.WriteBool(true, ref bitposition);

            ///Teleport handling
            buffer.WriteBool(hasTeleported, ref bitposition);
            if (hasTeleported)
            {
                //Debug.LogError(Time.time + " " + name + " " + frameId + " <b>SER TELE</b>");

                transformCrusher.Write(frame.telecm, buffer, ref bitposition);
                if (teleportReliable)
                    flags |= SerializationFlags.ForceReliable;
            }


            /// TRS handling
            transformCrusher.Write(frame.cm, buffer, ref bitposition);
            transformCrusher.Decompress(frame.m, frame.cm);
            prevSentFrame = frame;

            //if (frame.hasTeleported)
            //if (GetComponent<SyncPickup>())
            //    Debug.LogError(Time.time + " " + frame.frameId + ":" + frameId + " <b> TELE </b>" + frame.m.position + " -> " + frame.telem.position);

            //if (_hasTeleported)
            //	return SerializationFlags.HasChanged | SerializationFlags.ForceReliable;
            //else

            return flags;
        }

        public Frame prevSentFrame;

        public SerializationFlags OnNetDeserialize(int originFrameId, byte[] buffer, ref int bitposition, FrameArrival arrival)
        {
            //if (arrival != FrameArrival.IsFuture && snapFrame != null)
            //	Debug.Log("arrival lcl: " + localFrameId + " sn:" + snapFrame.frameId + " " + arrival);

#if PUN_2_OR_NEWER
            /// Needs to ignore any incoming updates that are the server/relay mirroring back what we sent
            var frame = (photonView.IsMine) ? offtickFrame : frames[originFrameId];
#else
			Frame frame = null;
#endif
            /// If enabled flag is false, we are done here.
            if (!buffer.ReadBool(ref bitposition))
            {
                frame.content = FrameContents.Empty;

                return SerializationFlags.None;
            }

            frame.content = FrameContents.Complete;

            bool _hasTeleported = buffer.ReadBool(ref bitposition);
            frame.hasTeleported = _hasTeleported;

            if (_hasTeleported)
            {
                //Debug.Log(localFrameId + " RCV TELE : trgf: " + (targFrame != null ? targFrame.frameId.ToString() : "null"));
                transformCrusher.Read(frame.telecm, buffer, ref bitposition);
                transformCrusher.Decompress(frame.telem, frame.telecm);
            }

            transformCrusher.Read(frame.cm, buffer, ref bitposition);
            transformCrusher.Decompress(frame.m, frame.cm);

            //if (GetComponent<SyncPickup>())
            //{
            //    if (_hasTeleported)
            //        Debug.Log(Time.time + " " + name + " <b>fr: " + originFrameId + " <color=blue>DES TRANS TELE</color></b> " + frame.m.position + " <b><color=red>tele: " + frame.telem.position + "</color></b> "+ frame.content);
            //    else
            //        Debug.Log(Time.time + " " + name + " <b>fr: " + originFrameId + " <color=blue>DES TRANS</color></b> " + frame.m.position + " " + frame.content);
            //}

            return /*frame.hasTeleported ? SerializationFlags.ForceReliable :*/ SerializationFlags.HasContent;
        }

        #endregion

        protected bool skipInterpolation;

        public override bool OnSnapshot(int prevFrameId, int snapFrameId, int targFrameId, bool prevIsValid, bool snapIsValid, bool targIsValid)
        {

            bool ready = base.OnSnapshot(prevFrameId, snapFrameId, targFrameId, prevIsValid, snapIsValid, targIsValid);

            //if (snapFrame.content == FrameContents.Empty || targFrame.content == FrameContents.Empty)
            //    Debug.LogError(newTargetFrameId + " " + ready + " ST Cant SNAP " + snapFrame.frameId + ":" + snapFrame.content + " " + targFrame.frameId + ":" + targFrame.content);

            //if (GetComponent<SyncPickup>())
            //    Debug.Log(Time.time + " " + name + " <b>" + snapFrame.frameId + " <color=green>APPLY TRNS</color></b> " + snapFrame.content + " " + ready + " " + snapFrame);

            if (!ready)
            {
                return false;
            }

            if (snapFrame.content == FrameContents.Empty)
                return false;

            if (targFrame.content == FrameContents.Empty)
                return false;

            bool snapTeleported = snapFrame.hasTeleported;

            //if (GetComponent<SyncPickup>())
            //    Debug.Log(
            //        "snap: " + snapFrame.frameId + " snap par: " + targFrame.parentHash + " <- newPar: " + teleNewParentId +
            //        "targ: " + targFrame.frameId + " targ par: " + targFrame.parentHash + " <- newPar: " + teleNewParentId);

            targFrame.parentHash = teleNewParentId;

            /// Clear the teleport flag every tick
            skipInterpolation = false;

            //if (snapFrame.hasTeleported)
            //    Debug.LogWarning(Time.time + " " + name + " TELEFRAME fr: " + snapFrame.frameId + ">" + targFrame.frameId + " " + snapFrame.m.position + " -> " + snapFrame.telem.position);

            /// Test for need to auto-teleport (excessive distance change)
            if (!snapTeleported /*&& targFrame.content != FrameContents.Empty*/)
            {
                if (teleportThresholdSqrMag > 0)
                {
                    /// If the targF is not a valid frame, we will use the current interpolated scene position for this test.
                    var newpos = targFrame.m.position;
                    var oldpos = snapTeleported ? snapFrame.telem.position : snapFrame.m.position;

                    if (Vector3.SqrMagnitude(newpos - oldpos) > teleportThresholdSqrMag)
                    {
#if UNITY_EDITOR
                        Debug.LogWarning(Time.time + " " + name + " fr: " + snapFrame.frameId + ">" + targFrame.frameId + " teleportThreshold distance exceeded. Teleport Distance: " + Vector3.Distance(newpos, oldpos) + " / " + teleportThreshold
                            + "  sqrmag: " + Vector3.SqrMagnitude(newpos - oldpos) + " / " + teleportThresholdSqrMag + " " + newpos + " " + oldpos);
#endif
                        skipInterpolation = true;
                    }
                }
            }

            //if (GetComponent<SyncPickup>())
            //{
            //    var str = Time.time + " <b><color=green>Apply Trans</color></b> " + name + " <b>SNAP</b> " + snapFrame.content + " " + snapFrame.frameId + ":" + targFrame.frameId + (snapFrame.hasTeleported ? " <b>TELEP</b> " : " ");
            //    str += (transform.parent ? ("<b>" + transform.parent.name + "</b>") : "<b>null</b>") + " SNAP TRANS";

            //    if (snapFrame.hasTeleported)
            //        str += " snappos: " + snapFrame.m.position + " <b><color=red>tele: </color>" + snapFrame.telem.position + " </b>" + " -> ";
            //    else
            //        str += " snappos: <b>" + snapFrame.telem.position + "</b> -> ";

            //    str += " targpos: <b>" + targFrame.m.position + " </b>";

            //    str += "par: " + (transform.parent ? transform.parent.name : "noparent") + " lclpos: " + transform.localPosition;
            //    Debug.Log(str);
            //}

            ApplyFrame(snapFrame);

            return true;
        }

        protected void ApplyFrame(Frame frame)
        {
            transformCrusher.Apply(transform, frame.hasTeleported ? frame.telem : frame.m);

        }

        public override bool OnInterpolate(int snapFrameId, int targFrameId, float t)
        {
            if (skipInterpolation)
                return false;

            bool ready = base.OnInterpolate(snapFrameId, targFrameId, t);

            if (!ready)
                return false;

            if (interpolation == Interpolation.None)
                return false;

            if (ReferenceEquals(targFrame, null))
                return false;

            if (snapFrame.content == FrameContents.Empty)
                return false;

            if (targFrame.content == FrameContents.Empty)
                return false;

            if (snapFrame.parentHash != targFrame.parentHash)
                return false;

            var snapM = snapFrame.hasTeleported ? snapFrame.telem : snapFrame.m;

            if (interpolation == Interpolation.Linear/* || pre1Frame.content != FrameContents.Complete*/)
                Matrix.Lerp(Matrix.reusable, snapM, targFrame.m, t);
            ///TODO: teleport handling with Catmul non existant
            else
                Matrix.CatmullRomLerpUnclamped(Matrix.reusable, prevFrame.m, snapFrame.m, targFrame.m, t);

            //if (transform.GetComponent<SyncPickup>())
            //Debug.Log(name + " OnInterpolate Completed " + targFrame.m.position);

            transformCrusher.Apply(transform, Matrix.reusable);

            return true;
        }

        #region Reconstruction


        protected override void InterpolateFrame(Frame targframe, Frame startframe, Frame endframe, float t)
        {
            ////return FrameContents.Empty;
            //if (GetComponent<SyncPickup>())
            //    Debug.Log("<b> Interp Frame</b> " + prevFrame.content + " : " + snapFrame.content + " " + start.parentHash + " : " + end.parentHash);

            /// Don't interpolate if parent has changed - -2 indicates unknown. Checking for -2 so that both being -2 doesn't get treated as "same".
            if (startframe.parentHash == -2 || startframe.parentHash != endframe.parentHash)
            {
                targFrame.content = FrameContents.Empty;
                //targ.CopyFrom(end);
            }
            else
            {
                targframe.CopyFrom(endframe);
                Matrix.Lerp(targframe.m, startframe.hasTeleported ? startframe.telem : startframe.m, endframe.m, t);
                transformCrusher.Compress(targframe.cm, targframe.m);
            }
        }

        protected override void ExtrapolateFrame(Frame prevframe, Frame snapframe, Frame targframe)
        {
            //if (GetComponent<SyncPickup>())
            //    Debug.Log(snapFrame.frameId + " par: " + snapFrame.parentHash + "/" + teleNewParentId);

            //if (extrapolateRatio == 0)
            //	return FrameContents.Empty;

            /// TODO: Not tested these uses of .Partial yet.

            /// Don't extrapolate if we don't have a valid snapframe - this should never happen and may eventually be removable.
            if (snapframe.content == FrameContents.Empty)
            {
                Debug.LogError(targframe.frameId + " Failed to extrapolate due to empty snapshot. Failsafing to current transform value.");
                targframe.content = FrameContents.Empty;
                return;
                //CaptureCurrent(targFrame.m, targFrame.cm);
                //return FrameContents.Extrapolated;
            }

            /// Copy Snap to get any teleport info copied over? (I forget what this was for)
            targframe.CopyFrom(snapframe);
            targframe.parentHash = teleNewParentId;

            /// If the previous frame was a teleport, we have nothing to extrapolate. We just copy the teleport pos/rot values.
            if (snapframe.hasTeleported)
            {
                //if (GetComponent<SyncPickup>())
                //    Debug.Log(Time.time + " <b>" + targFrame.frameId 
                //        + " [" + prevFrame.parentHash + " : " + snapFrame.parentHash + "]   <color=red>Trans Extrap</color></b> by copy snapFrame "
                //        + snapFrame.m.position + " -> " + snapFrame.telem.position + (prevFrame.hasTeleported ? " <b><color=red>tele</color></b>" : ""));

                return;
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR

            /// Don't extrapolate if we had a parent change (this would come from a SyncState update that arrived, without a transform sync also arriving)
            /// Is this possible? Since a parent change should force an update and teleport? Seems to only be capturing -2 (unknown) currently so still useful?
            if (snapframe.parentHash != teleNewParentId)
            {
                //if (GetComponent<SyncPickup>())
                //    Debug.LogError("<b>SHOULDNT HAPPEN - THIS IF SEGMENT IS JUST HERE TO DETECT SOME CODE THAT NEEDS REVISITING</b> " + snapFrame.parentHash + " " + teleNewParentId);

                targframe.content = FrameContents.Empty;
                return;
            }
#endif
            /// Empty lerp prev, so just go with copying the snap
            var prevContent = prevframe.content;
            if (prevContent == FrameContents.Empty)
            {
                //if (GetComponent<SyncPickup>())
                //    Debug.Log(Time.time + " <b>targframe: " + targFrame.frameId + "   " + targFrame.m.position + " "
                //        + "  <color=red>Extraped</color></b> by copy snapFrame " 
                //        + snapFrame.m.position + " snap tele-> " + (snapFrame.hasTeleported ? " <b>tele</b> " + snapFrame.telem.position : ""));

                return;
            }


            /// Don't lerp between prev and snap if prev was a teleport/parent change, or else the teleport delta will be treated as movement
            /// hash of -2 indicates "Unknown". Check for -2 because if both are that, they would look like match (when they are not).
            /// THIS MAY NOT BE NEEDED NOW, THE LERP BELOW USES THE TELEPORT VALUE IF THERE WAS A TELEPORT ON PREV
            if (prevContent == FrameContents.Complete)
            {
                var prevpar = prevframe.parentHash;
                if (prevpar == -2 || prevframe.hasTeleported || prevpar != snapframe.parentHash)
                {
                    //if (GetComponent<SyncPickup>())
                    //    Debug.Log(Time.time + " <b>" + targFrame.frameId
                    //        + " [" + prevFrame.parentHash + " : " + snapFrame.parentHash + "]  <color=red>Can't Extrap due to par change - </color></b> snapFrame " + snapFrame.frameId + " "
                    //        + snapFrame.m.position
                    //        + (snapFrame.hasTeleported ? " <color=red>tele:</color> " + snapFrame.telem.position : "")
                    //        + (prevFrame.hasTeleported ? " <b>prevframe teleported</b>" : ""));

                    return;
                }
            }

            //if (GetComponent<SyncPickup>())
            //Debug.Log(Time.time + " " + targFrame.frameId + " <b>Extrap</b> by LERP Pre1 " + pre1Frame.frameId + ":" + snapFrame.frameId + " "
            //    + ((pre1Frame.m.position - snapFrame.m.position).magnitude > 1 ? "<color=red>" : "<color=blue>")
            //    + pre1Frame.parentHash + ":" + snapFrame.parentHash + " " + pre1Frame.m.position + " " + snapFrame.m.position + "</color>");

            Matrix.LerpUnclamped(targframe.m, prevframe.hasTeleported ? prevframe.telem : prevframe.m, snapframe.m, 1 + extrapolateRatio);
            transformCrusher.Compress(targframe.cm, targframe.m);

            //if (GetComponent<SyncPickup>())
            //    Debug.Log(name + " <color=red>Extrap </color>" + prevFrame.frameId + " > " + snapFrame.frameId + " = " + targFrame.m.position);

            //return FrameContents.Extrapolated;
        }

        #endregion

    }
}

