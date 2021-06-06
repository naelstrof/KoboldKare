// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Photon.Pun.Simple.Internal
{
	
	public class TickManager : IInRoomCallbacks
    {
		public readonly static Dictionary<int, ConnectionTickOffsets> perConnOffsets = new Dictionary<int, ConnectionTickOffsets>();
		public readonly static List<int> connections = new List<int>();

        /// <summary>
        /// Use the 'Single' property instead of this single field if you are uncertain if the instance has been set yet. 
        /// This field is exposed to allow a slightly faster alternative to the Single property, when you can be certain the singleton has been set.
        /// </summary>
		public static TickManager single;

		/// <summary>
		/// Flag indicates that the next update should be flagged as needing to be reliable.
		/// </summary>
		public static bool needToSendInitialForNewConn;


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void Bootstrap()
		{
			single = new TickManager();
			PhotonNetwork.NetworkingClient.AddCallbackTarget(single);
		}


        #region PUN Room Callbacks

        public void OnPlayerEnteredRoom(Realtime.Player newPlayer)
        {
            AddConnection(newPlayer.ActorNumber);
        }

        public void OnPlayerLeftRoom(Realtime.Player otherPlayer)
        {
            RemoveConnection(otherPlayer.ActorNumber);
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }
        public void OnPlayerPropertiesUpdate(Realtime.Player targetPlayer, Hashtable changedProps) { }
        public void OnMasterClientSwitched(Realtime.Player newMasterClient) { }

        #endregion

        /// <summary>
        /// Run this prior to OnSnapshot, to establish if the number of snapshots for connection objects needs to be a value other than 1.
        /// </summary>
        public static void PreSnapshot(int currentFrameId)
		{
            
			for (int i = 0; i < connections.Count; ++i)
				if (!ReferenceEquals(perConnOffsets[connections[i]], null))
                {
                    var offsets = perConnOffsets[connections[i]];

                    var originFrameId = offsets.ConvertFrameLocalToOrigin(currentFrameId);
                    float timeBeforeConsumption = offsets.validFrameMask[originFrameId] ? Time.time - offsets.frameArriveTime[originFrameId] : -1;

                    offsets.frameTimeBeforeConsumption[originFrameId] = timeBeforeConsumption;
                    //Debug.Log(timeBeforeConsumption);

                    offsets.SnapshotAdvance();
                }
        }

		public static void PostSnapshot(int currentFrameId)
		{
			for (int i = 0; i < connections.Count; ++i)
				if (!ReferenceEquals(perConnOffsets[connections[i]], null))
					perConnOffsets[connections[i]].PostSnapshot();
		}


		/// <summary>
		/// Notify tick manager of an incoming frame, so it can register/modify offsets for that connection.
		/// Returns FrameId translated into localFrameId.
		/// </summary>
		public static ConnectionTickOffsets LogIncomingFrame(int connId, int originFrameId, out FrameArrival arrival)
		{
            ConnectionTickOffsets offsets;

			int frameCount = TickEngineSettings.frameCount;

            //Debug.Log("conn: " + connId + " fid: " + originFrameId);

			if (!perConnOffsets.TryGetValue(connId, out offsets) || offsets == null)
			{
				LogNewConnection(connId, originFrameId, frameCount, out offsets);
			}

			int localFrameId = originFrameId + offsets.originToLocalFrame;
			if (localFrameId >= frameCount)
				localFrameId -= frameCount;

            offsets.frameArriveTime[originFrameId] = Time.time;

			int currTargFrameId = NetMaster.CurrentFrameId; // offsets.currTargFrameId;

			//bool frameIsInFuture;
            int frameOffsetFromCurrentTarg;

            if (localFrameId == currTargFrameId)
			{
				//frameIsInFuture = false;
                frameOffsetFromCurrentTarg = 0;
			}
			else
			{
				/// Flag frame as valid if it is still in the future
				frameOffsetFromCurrentTarg = currTargFrameId - localFrameId;
                if (frameOffsetFromCurrentTarg < 0)
					frameOffsetFromCurrentTarg += frameCount;
                if (frameOffsetFromCurrentTarg >= TickEngineSettings.halfFrameCount)
                    frameOffsetFromCurrentTarg -= frameCount;

				//frameIsInFuture = frameOffsetFromCurrentTarg != 0 && frameOffsetFromCurrentTarg < TickEngineSettings.halfFrameCount;
			}

#if UNITY_EDITOR
            const string STR_TAG = "\nSeeing many of these messages indicates a buffer underrun, due to an unstable connection, too small a buffer setting, or too high a tick rate.";
#endif

            if (frameOffsetFromCurrentTarg >= 0)
            {


                if (frameOffsetFromCurrentTarg == 0)
                {
#if UNITY_EDITOR
                    if (TickEngineSettings.LogLevel == TickEngineSettings.LogInfoLevel.All)
                    {
                        string strframes = " Incoming Frame: <b>" + originFrameId + "</b> Current Interpolation: " + offsets.ConvertFrameLocalToOrigin(NetMaster.PreviousFrameId) + "->" + offsets.ConvertFrameLocalToOrigin(currTargFrameId);
                        Debug.Log("<b>Late Update </b>conn: " + connId + " " + strframes + ". Already interpolating to this frame. Not critically late, but getting close." + STR_TAG);
                    }
#endif
                }
                else if (frameOffsetFromCurrentTarg == 1)
                {
#if UNITY_EDITOR
                    if (TickEngineSettings.LogLevel >= TickEngineSettings.LogInfoLevel.WarningsAndErrors)
                    {
                        string strframes = " Incoming Frame: <b>" + originFrameId + "</b> Current Interpolation: " + offsets.ConvertFrameLocalToOrigin(NetMaster.PreviousFrameId) + "->" + offsets.ConvertFrameLocalToOrigin(currTargFrameId);
                        Debug.LogWarning("<b>Critically Late Update</b> conn: " + connId + " " + strframes + " Already applied and now interpolating from this frame. Snapshots will be rewound and reapplied." + STR_TAG);
                    }
#endif
                }
                else if (frameOffsetFromCurrentTarg >= TickEngineSettings.halfFrameCount)
                {
#if UNITY_EDITOR
                    string strframes = " Incoming Frame: <b>" + originFrameId + "</b> Current Interpolation: " + offsets.ConvertFrameLocalToOrigin(NetMaster.PreviousFrameId) + "->" + offsets.ConvertFrameLocalToOrigin(currTargFrameId);
                    Debug.LogError("<b>Critically Late Update</b> conn: " + connId + " " + strframes + " DATA LOSS HAS OCCURRED EVEN FOR RELIABLE PACKETS. Increase Buffer size in "
                        + typeof(TickEngineSettings).Name + "." + STR_TAG);
#endif
                }
                else
                {
#if UNITY_EDITOR
                    if (TickEngineSettings.LogLevel >= TickEngineSettings.LogInfoLevel.WarningsAndErrors)
                    {
                        string strframes = " Incoming Frame: <b>" + originFrameId + "</b> Current Interpolation: " + offsets.ConvertFrameLocalToOrigin(NetMaster.PreviousFrameId) + "->" + offsets.ConvertFrameLocalToOrigin(currTargFrameId);
                        Debug.LogWarning("<b>Critically Late Update</b> conn: " + connId + " " + strframes + " Already applied and now interpolating from this frame. Snapshots will be rewound and reapplied" + STR_TAG);
                    }
#endif
                }
            }
            else if (frameOffsetFromCurrentTarg <= (-(TickEngineSettings.halfFrameCount)))
            {
#if UNITY_EDITOR
                string strframes = " Incoming Frame: <b>" + originFrameId + "</b> Current Interpolation: " + offsets.ConvertFrameLocalToOrigin(NetMaster.PreviousFrameId) + "->" + offsets.ConvertFrameLocalToOrigin(currTargFrameId);
                Debug.LogError("<b>Critically Late Update</b> conn: " + connId + " " + strframes + " DATA LOSS HAS OCCURRED EVEN FOR RELIABLE PACKETS. Increase Buffer size in "
                        + typeof(TickEngineSettings).Name + "." + STR_TAG);
#endif
            }

            arrival = (FrameArrival)frameOffsetFromCurrentTarg;

            //Debug.Log(Time.time + " ARRIVAL " + arrival + " fid:" + originFrameId + " half: " + TickEngineSettings.halfFrameCount);

            bool frameIsInFuture = frameOffsetFromCurrentTarg <= 0;

            offsets.frameArrivedTooLate |= !frameIsInFuture;
			offsets.validFrameMask.Set(originFrameId, true /*frameIsInFuture*/);

            return offsets;
		}

		private static void LogNewConnection(int connId, int originFrameId, int frameCount, out ConnectionTickOffsets offsets)
		{
			int currentFrame = NetMaster.CurrentFrameId;

			/// Apply default offset from current local frame
			int startingFrameId = currentFrame + (TickEngineSettings.targetBufferSize /*+ 1*/);
			while (startingFrameId >= frameCount)
				startingFrameId -= frameCount;

			int originToLocal = startingFrameId - originFrameId;
			if (originToLocal < 0)
				originToLocal += frameCount;

			int localToOrigin = frameCount - originToLocal;
			if (localToOrigin < 0)
				localToOrigin += frameCount;

			/// Currently local and origin are the same.
			/// TODO: Pool these
			offsets = new ConnectionTickOffsets(connId, originToLocal, localToOrigin);

			AddConnection(connId, offsets);

		}

		private static void AddConnection(int connId, ConnectionTickOffsets offsets = null)
		{
#if PUN_2_OR_NEWER
            /// We don't treat own own connection as a thing
            if (PhotonNetwork.LocalPlayer.ActorNumber == connId)
				return;
#endif
			if (!connections.Contains(connId))
			{
				perConnOffsets.Add(connId, offsets);
				connections.Add(connId);
				/// Add this connection to the NetSends list of targets for a reliable update.
				NetMsgSends.newPlayers.Add(connId);
				needToSendInitialForNewConn = true;
			}
			else
			{
				perConnOffsets[connId] = offsets;
			}
		}

		public static void RemoveConnection(int connId)
		{
			if (perConnOffsets.ContainsKey(connId))
			{
                perConnOffsets.Remove(connId);
				connections.Remove(connId);
			}
		}

	}
}

