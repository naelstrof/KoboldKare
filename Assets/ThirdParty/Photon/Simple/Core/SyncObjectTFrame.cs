// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Pun.Simple.Internal;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

namespace Photon.Pun.Simple
{

	/// <summary>
	/// SyncObject base class with handling for frames and frame history.
	/// </summary>
	/// <typeparam name="TFrame">The derived FrameBase class to be used as the frame.</typeparam>
	public abstract class SyncObject<TFrame> : SyncObject
		where TFrame : FrameBase/*, new()*/
	{

		[System.NonSerialized] public TFrame[] frames;

		/// Runtime vars
		protected TFrame /*pre2Frame,*/ prevFrame, snapFrame, targFrame, /*nextFrame,*/ offtickFrame;
		protected bool hadInitialSnapshot;
		//protected bool hadInitialCompleteSnapshot;

		/// <summary>
		/// When overriding, but sure to keep base.Awake(). Also, frames are created and given indexes, but any other Initialization will still need to be
		/// explicitly called in the derived Awake().
		/// </summary>
		public override void OnAwake()
		{
			if (keyframeRate > TickEngineSettings.MaxKeyframes)
			{
				keyframeRate = TickEngineSettings.MaxKeyframes;
				Debug.LogWarning(name + "/" + GetType().Name + " keyframe setting exceeds max allowed for the current " + TickEngineSettings.single.name + ".frameCount setting. Reducing to " + keyframeRate);
			}
			base.OnAwake();

			PopulateFrames();

			offtickFrame = frames[TickEngineSettings.frameCount];
		}

		public override void OnPostDisable()
		{
			base.OnPostDisable();
			///TEST - Reset when disabled so new initialization isn't ignored (for scene objects after disconnect and reconnect)
			hadInitialSnapshot = false;
			//hadInitialCompleteSnapshot = false;
		}

		public override void OnAuthorityChanged(bool isMine, bool controllerChanged)
		{
			base.OnAuthorityChanged(isMine, controllerChanged);
            
            /// Clear all frames from a different controller to avoid ghosts of old non-empty frames.
            if (controllerChanged)
            {
                var controllerId = photonView.ControllerActorNr;
                var originHistory = netObj.originHistory;
                for (int i = 0, cnt = TickEngineSettings.frameCount; i < cnt; ++i)
                {
                    if (controllerId != originHistory[i])
                        frames[i].Clear();
                }
            }


            ///TODO: some logic about when this happens may be useful. Originally was only when changing to IsMine
            /// Not sure if this helps or hurts - not fully checked into the need for if this kind of reset is needed.
            hadInitialSnapshot = false;

        }

        /// <summary>
        /// Override this with frame initialization code. The default base just creates the frame instances and assigns them index values.
        /// </summary>
        protected virtual void PopulateFrames()
		{
			/// Basic factory, just gives each frame an index.
			FrameBase.PopulateFrames(ref frames);
		}

        protected virtual void InitialCompleteSnapshot(TFrame frame)
		{
			
		}

		/// <summary>
		/// Called every simulation tick, right after the frameId has been incremented by NetMaster. Base class advances/increments all of the frame references.
		/// </summary>
		/// <param name="targFrameId"></param>
		/// <param name="initialize"></param>
		/// <returns>Base will return false if snapshot is not ready.</returns>
		public virtual bool OnSnapshot(int prevFrameId, int snapFrameId, int targFrameId, bool prevIsValid, bool snapIsValid, bool targIsValid)
		{
            
			///TODO:should this be active in hierarchy?
			if (!enabled)
				return false;

            int frameCount = TickEngineSettings.frameCount;
            int halfFrameCount = TickEngineSettings.halfFrameCount;

			prevFrame = frames[prevFrameId];
			snapFrame = frames[snapFrameId];

            if (targFrameId < 0 || targFrameId >= frameCount)
                Debug.Log("BAD FRAME ID " + targFrameId);

			targFrame = frames[targFrameId];

            /// Invalidate old frames
            int invalidateframeId = snapFrameId + halfFrameCount;
            if (invalidateframeId >= frameCount)
                invalidateframeId -= frameCount;

            frames[invalidateframeId].Clear();

            /// Tick arrived for targFrame, but Frame may be partial, nochange or otherwise empty
            if (targIsValid)
            {
                switch (targFrame.content)
                {
                    case FrameContents.Empty:
                        if (AllowReconstructionOfEmpty)
                            ReconstructEmptyFrame();
                        break;

                    case FrameContents.Partial:
                        if (AllowReconstructionOfPartial)
                            ReconstructIncompleteFrame();
                        break;

                    case FrameContents.NoChange:
                        targFrame.CopyFrom(snapFrame);
                        break;

                    case FrameContents.Complete:
                        break;

                }
            }
            /// No tick arrived for targFrame - attempt reconstruction using snapFrame and prevFrame
            else
            {
                if (AllowReconstructionOfEmpty)
                {
                    /// snapFrame is valid, so some kind of reconstruction of targFrame is likely possible
                    if (snapIsValid || snapFrame.content >= FrameContents.Extrapolated)
                    {
                        //if (snapFrame.content == FrameContents.Empty)
                        //{
                        //    targFrame.content = FrameContents.Empty;

                        //    if (GetComponent<SyncPickup>() && this is SyncState)
                        //        Debug.Log(snapFrameId + ":" + targFrame +  " snap VALID but EMPTY");
                        //    return false;
                        //}

                        ConstructMissingFrame(prevFrame, snapFrame, targFrame);
                    }
                    /// Unable to reconstruct invalid targFrame from invalid snapFrame
                    else
                    {
                        //if (GetComponent<SyncPickup>() && this is SyncState)
                        //    Debug.Log(snapFrameId + ":" + targFrame.frameId + " snap INVALID");

                        targFrame.content = FrameContents.Empty;
                        //return false;
                    }
                }
                /// Reconstruction disabled, mark the invalid targFrame as empty
                else
                {
                    //if (GetComponent<SyncPickup>() && this is SyncState)
                    //    Debug.Log(snapFrameId + ">" + targFrameId + " targ EMPTY - targIsValid? " + netObj.frameValidMask[targFrameId]);

                    targFrame.content = FrameContents.Empty;
                    //return false;
                }
            }
            //}

            if (_readyState != ReadyStateEnum.Ready)
			{
				var initialFrame = snapFrame;
				var initialContent = initialFrame.content;

				if (initialContent == FrameContents.Complete /*|| (IsKeyframe(initialFrame.frameId)*/ /*&& initialContent != 0*/)
				{
					//Debug.Log(name + " " + GetType().Name + " <color=green>Initial - ReadyState = ready</color> " + (initialContent == FrameContents.Complete) + " " + (IsKeyframe(initialFrame.frameId)));
					InitialCompleteSnapshot(initialFrame);
					//hadInitialCompleteSnapshot = true;
					ReadyState = ReadyStateEnum.Ready;
					//ApplySnapshot();
				}
				//else
				//	Debug.Log(GetType().Name + " " + initialFrame.frameId + " still no first complete - initcontent: " + initialContent);
			}
            
            ApplySnapshot(snapFrame, targFrame, snapIsValid, targIsValid);

            return true;
		}

		///TODO: Make this abstract and use everywhere rather than OnSnapshot?
		protected virtual void ApplySnapshot(TFrame snapframe, TFrame targframe, bool snapIsValid, bool targIsValid)
		{

		}

        public virtual bool AllowInterpolation { get { return true; } }
		public virtual bool AllowReconstructionOfEmpty { get { return true; } }
		public virtual bool AllowReconstructionOfPartial { get { return true; } }

        /// <summary>
        /// Handling if a frame arrived, but the frame was flagged as FrameContents.Empty
        /// </summary>
        protected virtual void ReconstructEmptyFrame()
		{
            if (snapFrame.content == FrameContents.Extrapolated || snapFrame.content == FrameContents.Complete)
            {
                targFrame.content = FrameContents.Extrapolated;
                targFrame.CopyFrom(snapFrame);
            }
            else
            {
                targFrame.content = FrameContents.Empty;
            }
        }

        /// <summary>
        /// Handling if a frame arrived, but the frame was flagged as hasConent = true and isComplete = false
        /// </summary>
        protected virtual void ReconstructIncompleteFrame()
        {
            
            targFrame.CopyFrom(snapFrame);
            targFrame.content = FrameContents.Partial;
        }

        protected virtual void ConstructMissingFrame(TFrame prevFrame, TFrame snapframe, TFrame targframe)
		{
            //targFrame.content = FrameContents.Empty;
            //return;

            //if (GetComponent<SyncPickup>() && this is SyncState)
            //    //Debug.Log(GetType().Name + " " + targFrame.frameId + " Reconstruct Missing Frame:  targContent= " + targFrame.content);
            //    Debug.Log(GetType().Name + " " + snapFrame.frameId + ":" + targFrame.frameId + " Reconstruct Missing Frame:  " + snapFrame.content + " " + targFrame.content
            //        //+ " " + netObj.frameValidMask[snapFrame.frameId] + ":" + netObj.frameValidMask[targFrame.frameId]
            //        );


            /// if we are currently on a valid frame, we can attempt to look forward for another valid frame to reconstruct with a tween.
			bool snapFrameIsEmpty = snapframe.content == FrameContents.Empty; // offsets.validFrames[snapFrame.frameId];
            if (snapFrameIsEmpty)
            {
                //if (GetComponent<SyncPickup>() && this is SyncState)
                //    Debug.Log("SET " + targFrame + " targ EMPTY");

                targframe.content = FrameContents.Empty;
                return;
            }
			
            ConnectionTickOffsets offsets;
            bool found = TickManager.perConnOffsets.TryGetValue(ControllerActorNr, out offsets);
            if (!found)
            {
                Debug.LogError("CONN " + ControllerActorNr + " NOT ESTABLISHED IN TICK MANAGER YET.");
            }

			
            //if (snapFrameNotEmpty)
            // First see if we have a future valid frame we can interpolate to, as that will produce better results than a pure extrapolation.
			{
                int targFrameId = targframe.frameId;
				const int MAX_LOOKAHEAD = 3;
				for (int i = 2; i <= MAX_LOOKAHEAD; ++i)
				{
					int futureFid = targFrameId + i;
					if (futureFid >= TickEngineSettings.frameCount)
						futureFid -= TickEngineSettings.frameCount;

                    var futureFrame = frames[futureFid];
                    // Find a future frame that is complete to use as the end of our lerp.
                    if (netObj.frameValidMask[futureFid] /*(offsets.validFrames[futureFid] */ && futureFrame.content == FrameContents.Complete) //  ((validMask & (ulong)1 << futureFid) != 0)
					{
						float t = 1f / i;

                        InterpolateFrame(targframe, snapframe, futureFrame, t);
                       
                        var content = targframe.content;

                        if (content != FrameContents.Empty)
                        {
                            //if (GetComponent<SyncPickup>() && this is SyncState)
                            //    Debug.Log(targFrame.frameId + ":" + snapFrame.content + "->" + content + " " + GetType().Name + " <color=red><b>Extrap by Interpolation</b></color> " + snapFrame.frameId + "-->" + futureFrame.frameId);

                            return;
                        }
                    }
				}

                // For loop failed to reconstruct using a future frame for whatever reason - return empty.
                targframe.content = FrameContents.Empty;
			}
			
			/// No future valid frame found, just do a regular extrapolation
			ExtrapolateFrame(prevFrame, snapframe, targframe);

            //if (targFrame.content != 0 && this is SyncState)
            //    Debug.LogError(targFrame.frameId + ":" + targFrame.content + " " +
            //        GetType().Name + " <color=red><b> Extrapolated </b></color>" + prevFrame.frameId + ":" + prevFrame.content + " " +
            //        snapFrame.frameId + ":" + snapFrame.content);

        }

		/// <summary>
		/// Interpolate between SnapFrame and TargFrame.
		/// </summary>
		/// <param name="t"></param>
		/// <returns>Base will return false if snapshot is not ready. Set to true if interpolation can be done.</returns>
		public virtual bool OnInterpolate(int snapFrameId, int targFrameId, float t)
		{
            //Debug.Log("AllowInterpolation " + AllowInterpolation + "  isActiveAndEnabled " + isActiveAndEnabled + " hadInitialSnapshot " + hadInitialSnapshot + " IsMine " + IsMine);
			if (!AllowInterpolation)
				return false;

			if (!isActiveAndEnabled)
				return false;

			//if (!hadInitialSnapshot)
			//	return false;

			if (IsMine)
				return false;
			
			return true;
		}

		/// <summary>
		/// Interpolate used to construct a new from from two existing frames. Return true if this should flag that new frame now as having content.
		/// </summary>
		protected virtual void InterpolateFrame(TFrame targframe, TFrame startframe, TFrame endframe, float t)
		{
			targframe.Clear();
		}
		/// <summary>
		/// Interpolate a new TargFrame from previous frames. Return true if this should flag that new frame now as having content.
		/// </summary>
		protected virtual void ExtrapolateFrame(TFrame prevframe, TFrame snapframe, TFrame targframe)
		{
			targframe.Clear();
		}
		
	}

#if UNITY_EDITOR

	//[CustomEditor(typeof(SyncObject<>), isFallback = false)]
	//[CanEditMultipleObjects]
	//public class SyncObjectTFrameEditor : SyncObjectEditor
	//{
	//	protected override string HelpURL
	//	{
	//		get
	//		{
	//			return "";
	//		}
	//	}
	//	public override void OnInspectorGUI()
	//	{
	//		base.OnInspectorGUI();

	//	}
	//}

#endif
}

