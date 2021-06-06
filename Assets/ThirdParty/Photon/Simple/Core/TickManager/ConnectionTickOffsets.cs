// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using Photon.Utilities;

namespace Photon.Pun.Simple
{
	public class ConnectionTickOffsets
	{
        public int connId;
        public int originToLocalFrame, localToOriginFrame;
        //public int originToLocalTick, localToOriginTick;

        public int numOfSequentialFramesWithTooSmallBuffer;
		public int numOfSequentialFramesWithTooLargeBuffer;
		public bool frameArrivedTooLate;
		public bool hadInitialSnapshot;
		public int advanceCount;
		public float[] frameArriveTime;
		public float[] frameTimeBeforeConsumption;

        public int ConvertFrameLocalToOrigin(int localFrameId)
        {
            int frameCount = TickEngineSettings.frameCount;
            int originFrameId = localFrameId + localToOriginFrame;
            if (originFrameId >= frameCount)
                originFrameId -= frameCount;
            return originFrameId;
        }

        public int ConvertFrameOriginToLocal(int originFrameId)
        {
            int frameCount = TickEngineSettings.frameCount;
            int localFrameId = originFrameId + originToLocalFrame;
            if (localFrameId >= frameCount)
                localFrameId -= frameCount;
            return localFrameId;
        }


        public FastBitMask128 validFrameMask;

        public ConnectionTickOffsets(int connId, int originToLocal, int localToOrigin/*, int originToLocalTick, int localToOriginTick*/)
		{
            this.connId = connId;
			this.originToLocalFrame = originToLocal;
			this.localToOriginFrame = localToOrigin;
            //this.originToLocalTick = originToLocalTick;
            //this.localToOriginTick = localToOriginTick;

            int frameCount = TickEngineSettings.frameCount;

            validFrameMask = new FastBitMask128(frameCount + 1);
            frameArriveTime = new float[frameCount];
            frameTimeBeforeConsumption = new float[frameCount];
            for (int i = 0; i < frameCount; ++i)
            {
                frameTimeBeforeConsumption[i] = float.PositiveInfinity;
            }
        }

		/// <summary>
		/// Checks the state of the buffer, and returns the number of snapshots to advance to keep the buffer happy.
		/// </summary>
		public void SnapshotAdvance()
		{
            int frameCount = TickEngineSettings.frameCount;

            int currFrameId = NetMaster.CurrentFrameId;

            int origFrameId = currFrameId + localToOriginFrame;
            if (origFrameId >= frameCount)
                origFrameId -= frameCount;

            /// TODO: May be able to reduce this in the future to a less aggressive look ahead
            int validCount = validFrameMask.CountValidRange(origFrameId, TickEngineSettings.quaterFrameCount);

			if (!hadInitialSnapshot)
			{

				if (validCount == 0)
				{
					advanceCount = 0;
					return;
				}
				else if (validCount > TickEngineSettings.targetBufferSize)
				{
					advanceCount = validCount - TickEngineSettings.targetBufferSize;
                    //Debug.Log("Setting advanceCount " + advanceCount);
					return;
                }
			}

			/// Buffer emptied - either means drop/sever connection hang, or way behind.
			if (validCount == 0)
			{
				/// No valid frames, but we just received one late - buffer needs IMMEDIATE HARD correction
				if (frameArrivedTooLate)
				{
					numOfSequentialFramesWithTooLargeBuffer = 0;
					numOfSequentialFramesWithTooSmallBuffer = 0;
					frameArrivedTooLate = false;

#if SNS_WARNINGS && UNITY_EDITOR
					Debug.LogWarning(Time.time + " currlcl: " + currFrameId + " currorig: " + origFrameId + " <b><color=red>Frame arrived late with empty buffer</color> - HOLD</b> " + numOfSequentialFramesWithTooSmallBuffer + "/" + TickEngineSettings.ticksBeforeGrow);
#endif
					advanceCount = 0;
					return;
				
				}
				/// No frames have arrived late, looks like bad packetloss. Don't adjust the buffer in case it corrects.
				else
				{
					numOfSequentialFramesWithTooLargeBuffer = 0;
					advanceCount = 1;
				}

			}
			/// Buffer is too small
			else if (validCount < TickEngineSettings.minBufferSize)
			{
				numOfSequentialFramesWithTooLargeBuffer = 0;
				numOfSequentialFramesWithTooSmallBuffer += (frameArrivedTooLate ? 2 : 1);
				frameArrivedTooLate = false;

				if (numOfSequentialFramesWithTooSmallBuffer >= TickEngineSettings.ticksBeforeGrow)
				{
#if SNS_WARNINGS && UNITY_EDITOR
					Debug.LogWarning(Time.time + " <b>Buffer Low</b> - <b>HOLD</b>  conn: " + connId + " fid: " + currFrameId + " buffsze: " + validCount);
#endif
					advanceCount = 0;
					return;
				}
				else
					advanceCount = 1;
			}
			/// Buffer is too large
			else if (validCount > TickEngineSettings.maxBufferSize)
			{
				numOfSequentialFramesWithTooSmallBuffer = 0;
				if (numOfSequentialFramesWithTooLargeBuffer > TickEngineSettings.ticksBeforeGrow)
				{

					/// Limit advance to only one extra snapshot to shrink the buffer, unless this is startup - then we need to burn all backlog.
					advanceCount = (validCount - TickEngineSettings.targetBufferSize) + 1;

#if SNS_WARNINGS && UNITY_EDITOR
                    Debug.LogWarning(Time.time + " <b>SKIP  </b>Trimming Oversized Buffer for Player: " + connId + " advance: " + advanceCount + " validCount: " + validCount
						+ " frameArrivedTooLate:" + frameArrivedTooLate);
#endif
					numOfSequentialFramesWithTooLargeBuffer = 0; // /= 2;
				}
				else
				{
					advanceCount = 1;
					numOfSequentialFramesWithTooLargeBuffer++;
				}
			}
			/// Buffer is happy.
			else
			{
				numOfSequentialFramesWithTooLargeBuffer = 0;
				numOfSequentialFramesWithTooSmallBuffer = (frameArrivedTooLate ? 1 : 0);
				advanceCount = 1;
			}

			frameArrivedTooLate = false;
			return;

        }

		public void PostSnapshot()
		{
			int frameCount = TickEngineSettings.frameCount;
			int currFrameId = NetMaster.CurrentFrameId;

            int origFrameId = currFrameId + localToOriginFrame;
            if (origFrameId >= frameCount)
                origFrameId -= frameCount;

			int invalidate = origFrameId - (TickEngineSettings.quaterFrameCount);
			if (invalidate < 0)
				invalidate += frameCount;

            /// This clear could be a bit more intentional
            /// Clears valid frames so that lookahead isn't find outdated frames for its buffer adjustments.
            validFrameMask.ClearBitsBefore(invalidate, TickEngineSettings.quaterFrameCount);

            if (advanceCount > 0)
				hadInitialSnapshot = true;

			if (advanceCount != 1)
			{
				localToOriginFrame += (advanceCount - 1);
				if (localToOriginFrame < 0)
					localToOriginFrame += frameCount;
				else if (localToOriginFrame >= frameCount)
					localToOriginFrame -= frameCount;

				originToLocalFrame = frameCount - localToOriginFrame;
				if (originToLocalFrame < 0)
					originToLocalFrame += frameCount;
			}
		}
	}
}

