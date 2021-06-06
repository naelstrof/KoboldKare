// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Photon.Compression;

using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Photon.Pun.Simple.Internal
{
    public enum ReceiveGroup { Others, All, Master }
    /// <summary>
    /// Unified code for sending network messages across different Network Libraries.
    /// </summary>
    public static class NetMsgSends
    {

        private static bool unreliableCapable;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CacheSettings()
        {
            // TODO: this check for UDP eventually can be removed. Workaround for known PUN2 bug in sending unreliable to websockets
            unreliableCapable = PhotonNetwork.NetworkingClient.LoadBalancingPeer.UsedProtocol == ConnectionProtocol.Udp;
        }

        public static byte[] reusableBuffer = new byte[16384];
        public static byte[] reusableNetObjBuffer = new byte[4096];

        public static HashSet<int> newPlayers = new HashSet<int>();
        //private static List<int> reliableTargets = new List<int>();
        //private static List<int> unreliableTargets = new List<int>();

        public static void Send(this byte[] buffer, int bitposition, UnityEngine.Object refObj, SerializationFlags flags, bool flush = false)
        {

            var currentRoom = PhotonNetwork.CurrentRoom;

            if (PhotonNetwork.OfflineMode || currentRoom == null || currentRoom.Players == null)
            {
                return;
            }

            bool sendToSelf = (flags & SerializationFlags.SendToSelf) != 0;

            // no need to send OnSerialize messages while being alone (these are not buffered anyway)
            if (!sendToSelf && !TickEngineSettings.single.sendWhenSolo && currentRoom.Players.Count <= 1)
            {
                return;
            }

            ReceiveGroup sendTo = sendToSelf ? ReceiveGroup.All : ReceiveGroup.Others;


            int bytecount = (bitposition + 7) >> 3;

            var nc = PhotonNetwork.NetworkingClient;

            DeliveryMode deliveryMode;

            if (newPlayers.Count > 0)
            {
                deliveryMode = DeliveryMode.Reliable;
                newPlayers.Clear();
            }
            else
            {
                bool forceReliable = (flags & SerializationFlags.ForceReliable) != 0;
                deliveryMode = unreliableCapable ? (forceReliable ? DeliveryMode.ReliableUnsequenced : DeliveryMode.Unreliable) : DeliveryMode.Reliable;
            }

            //if (deliveryMode != DeliveryMode.Unreliable)
            //    Debug.LogError("Forced Reliable Send");

            SendOptions sendOptions = new SendOptions() { DeliveryMode = deliveryMode };

#if PUN_2_19_OR_NEWER
            var slice = PhotonNetwork.NetworkingClient.LoadBalancingPeer.ByteArraySlicePool.Acquire(buffer, 0, bytecount);
#else
            System.ArraySegment<byte> slice = new System.ArraySegment<byte>(buffer, 0, bytecount);
#endif
            ///// Send reliable copies of the update to new connections
            //if (newPlayers.Count != 0)
            //{
            //    // Convert newPlayers hash into two target lists for sending.
            //    reliableTargets.Clear();
            //    unreliableTargets.Clear();

            //    foreach(var player in PhotonNetwork.PlayerList)
            //    {
            //        int playerId = player.ActorNumber;
            //        if (newPlayers.Contains(player.ActorNumber))
            //        {
            //            reliableTargets.Add(playerId);
            //        }
            //        else
            //        {
            //            unreliableTargets.Add(playerId);
            //        }
            //    }

            //    newPlayers.Clear();

            //    // TODO: this check for UDP eventually can be removed. Workaround for known PUN2 bug in sending unreliable to websockets
            //    SendOptions forceSendOpts = new SendOptions { DeliveryMode = unreliableCapable ? DeliveryMode.ReliableUnsequenced : DeliveryMode.Reliable };

            //    targetRaiseOpts.TargetActors = reliableTargets.ToArray();
            //    nc.OpRaiseEvent(NetMsgCallbacks.DEF_MSG_ID, slice, targetRaiseOpts, forceSendOpts);

            //    targetRaiseOpts.TargetActors = unreliableTargets.ToArray();
            //    nc.OpRaiseEvent(NetMsgCallbacks.DEF_MSG_ID, slice, targetRaiseOpts, sendOptions);

            //}
            //else
            {
                nc.OpRaiseEvent(NetMsgCallbacks.DEF_MSG_ID, slice, opts[(int)sendTo], sendOptions);
            }

            if (flush)
                nc.Service();

        }

        public static bool ReadyToSend { get { return PhotonNetwork.NetworkClientState == ClientState.Joined; } }

        private static RaiseEventOptions[] opts = new RaiseEventOptions[3]
        {
            new RaiseEventOptions() { Receivers = ReceiverGroup.Others },
            new RaiseEventOptions() { Receivers = ReceiverGroup.All },
            new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient }
        };

        public static bool AmActiveServer { get { return false; } }
    }

}
