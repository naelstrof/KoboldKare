// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------
using Photon.Compression;
using Photon.Realtime;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
    public class SyncOwner : SyncObject<SyncOwner.Frame>
        , IOnCaptureState
        , IOnNetSerialize
        , IOnSnapshot
        , IUseKeyframes
        , IOnIncrementFrame
    {

        public override int ApplyOrder { get { return ApplyOrderConstants.OWNERSHIP; } }

        public bool reliableOwnerChange = true;

        protected bool pendingOwnerChange;
        protected int pendingOwnerId = -1;

        public class Frame : FrameBase
        {
            public int ownerActorId;
            public bool ownerHasChanged;

            public override void Clear()
            {
                ownerActorId = -1;
                ownerHasChanged = false;
                base.Clear();
            }

            public override void CopyFrom(FrameBase sourceFrame)
            {
                base.CopyFrom(sourceFrame);
                Frame src = sourceFrame as Frame;

                ownerActorId = src.ownerActorId;
                ownerHasChanged = false;
            }
        }

        public override void OnAuthorityChanged(bool isMine, bool controllerChanged)
        {
            base.OnAuthorityChanged(isMine, controllerChanged);

            if (!isMine)
            {
                pendingOwnerChange = false;
                ticksUntilOwnershipRetry = -1;
            }
        }

        //public void TakeOwnership()
        //{

        //}

        public void TransferOwner(int newOwnerId)
        {
            if (photonView.IsMine)
            {
                pendingOwnerChange = true;
                pendingOwnerId = newOwnerId;
            }
        }


        public void OnCaptureCurrentState(int frameId)
        {

            Frame frame = frames[frameId];

            if (pendingOwnerChange)
            {

                //Debug.Log(Time.time + " fid: " + frameId + " CAP OWNER " + pendingOwnerId);
                var photonView = this.photonView;

                // If we are master and we are taking control of scene objects, immediately change owner. Otherwise we are just setting the new owner as controller and awaiting them to take over.
                if (photonView.OwnerActorNr != 0)
                {
                    ticksUntilOwnershipRetry = TickEngineSettings.frameCount;
                }

                NetMasterCallbacks.postCallbackActions.Enqueue(DeferredOwnerChange);

                frame.ownerActorId = pendingOwnerId;
                frame.ownerHasChanged = true;
                pendingOwnerChange = false;

            }
            else
            {
                frames[frameId].ownerActorId = photonView.OwnerActorNr;
            }
        }

        public SerializationFlags OnNetSerialize(int frameId, byte[] buffer, ref int bitposition, SerializationFlags writeFlags)
        {
            
            var frame = frames[frameId];

            bool sendReliable = frame.ownerHasChanged && (reliableOwnerChange || keyframeRate == 0 || (writeFlags & SerializationFlags.NewConnection) != 0);
            bool hascontent = sendReliable || IsKeyframe(frameId);

            //if (sendReliable)
            //    Debug.LogError(frameId + " OWNER " + frame.ownerActorId + " " + frame.ownerHasChanged);

            if (!hascontent)
            {
                buffer.Write(0, ref bitposition, 1);
                return SerializationFlags.None;
            }

            buffer.Write(1, ref bitposition, 1);

            var flags = SerializationFlags.HasContent;

            buffer.WritePackedBytes((uint)frame.ownerActorId, ref bitposition, 32);

            if (sendReliable)
                flags |= SerializationFlags.ForceReliable;

            return flags;
        }

        public SerializationFlags OnNetDeserialize(int originFrameId, byte[] buffer, ref int bitposition, FrameArrival frameArrival)
        {
            var frame = frames[originFrameId];

            // First read content, hold off on applying content to frame until we are sure we want to keep it.
            bool hascontent = buffer.Read(ref bitposition, 1) != 0;
            int ownerActorId = hascontent ? (int)buffer.ReadPackedBytes(ref bitposition, 32) : -1;
            
            // Ignore incoming updates if we are the current owner and the source is not the recognized controller. 
            // Leaves frame as it was - since this might overwrite valid data.
            if (photonView.IsMine) // && hascontent && ownerActorId !=  && photonView.ControllerActorNr != netObj.originHistory[originFrameId])
            {
                //Debug.LogError("fid:" + originFrameId + " DES IGNORE newown: " + frame.ownerActorId);
                return SerializationFlags.None;
            }

            if (hascontent)
            {
                frame.content = FrameContents.Complete;
                frame.ownerActorId = ownerActorId;
                //Debug.LogError("fid:" + originFrameId + " DES newown: " + frame.ownerActorId + " cur:" + NetMaster.CurrentFrameId + " ctr:" + photonView.OwnerActorNr + ":" + photonView.ControllerActorNr) ;

                return SerializationFlags.HasContent;
            }
            else
            {
                frame.content = FrameContents.Empty;
                return SerializationFlags.None;
            }
        }

        //public override bool OnSnapshot(int prevFrameId, int snapFrameId, int targFrameId, bool prevIsValid, bool snapIsValid, bool targIsValid)
        //{
        //    var frame = frames[snapFrameId];
        //    var ready = base.OnSnapshot(prevFrameId, snapFrameId, targFrameId, prevIsValid, snapIsValid, targIsValid);
        //    Debug.LogError(frame.ownerActorId + " -> " + photonView.OwnerActorNr);

        //    return ready;

        //}

        protected override void ApplySnapshot(Frame snapframe, Frame targframe, bool snapIsValid, bool targIsValid)
        {
            if (snapIsValid && snapframe.content == FrameContents.Complete)
            {
                int newOwnerId = snapframe.ownerActorId;

                //if (photonView.AmOwner && newOwnerId != photonView.OwnerActorNr)
                //    Debug.LogError("fid:" + snap.frameId + " SNAP ILLEGAL new: " + snap.ownerActorId + " old: " + photonView.OwnerActorNr + ":" + photonView.ControllerActorNr);
                //else
                {
                    //if (GetComponent<SyncPickup>())
                    //    Debug.LogError(Time.time + " ENQU OWN fid:" + snap.frameId + " SNAP new: " + snap.ownerActorId + " old: " + photonView.OwnerActorNr);

                    pendingOwnerId = newOwnerId;

                    NetMasterCallbacks.postCallbackActions.Enqueue(DeferredOwnerChange);

                    ticksUntilOwnershipRetry = -1;

                    //if (false)
                    //    DeferredOwnerChange();
                }

            }
        }

        protected void DeferredOwnerChange()
        {
            //if (newOwnerId != photonView.OwnerActorNr)
            {
                Realtime.Player pendingOwner;
                PhotonNetwork.CurrentRoom.Players.TryGetValue(pendingOwnerId, out pendingOwner);

                //Debug.LogError(Time.time + " SYNC_OWN Apply newOwner? " + (pendingOwner == null ? "null" : pendingOwnerId.ToString()));

                photonView.SetOwnerInternal(pendingOwner, pendingOwnerId);
               
            }

        }


        protected int ticksUntilOwnershipRetry = -1;

        public void OnIncrementFrame(int newFrameId, int newSubFrameId, int previousFrameId, int prevSubFrameId)
        {
            if (photonView.IsMine)
                return;

            if (newSubFrameId != 0)
                return;

            //Debug.LogError(ticksUntilOwnershipRetry);

            if (ticksUntilOwnershipRetry >= 0)
            {
                if (ticksUntilOwnershipRetry == 0)
                {
                    // Fallback ownership transfer in case our tick based method suffered horrendous loss.
                    Debug.LogError(name + " FALLBACK OWNER CHANGE " + photonView.ControllerActorNr);
                    photonView.TransferOwnership(photonView.Controller);
                    ticksUntilOwnershipRetry = TickEngineSettings.frameCount;
                }
                else
                {
                    ticksUntilOwnershipRetry--;
                }
            }

        }


        //public void TEST()
        //{
        //    if (!test)
        //        return;


        //    if (photonView.IsMine)
        //    {
        //        var players = PhotonNetwork.PlayerList;
        //        Debug.Log("TEST " + players.Length);

        //        int escape = 0;
        //        Player player;

        //        int rand;
        //        do
        //        {
        //            rand = Random.Range(0, players.Length);
        //            player = players[rand];
        //            escape++;

        //            if (escape > 100)
        //                break;

        //        } while (players.Length > 1 && players[rand] == PhotonNetwork.LocalPlayer);

        //        if (escape > 100)
        //            Debug.Log("Overflow finding other player.");

        //        Debug.Log("Changing player to " + player.ActorNumber + " / " + players.Length);
        //        TransferOwner(player.ActorNumber);

        //    }
        //}
#if UNITY_EDITOR

        [CustomEditor(typeof(SyncObject<>), true)]
        [CanEditMultipleObjects]
        public class SyncObjectTFrameEditor : Simple.SyncObjectEditor
        {
            protected override string TextTexturePath
            {
                get
                {
                    return "Header/SyncOwnerText";
                }
            }
        }

#endif
    }
}
