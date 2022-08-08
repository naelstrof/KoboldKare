using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SmoothCharacterPhoton : MonoBehaviourPun, IPunObservable, ISavable, IOnPhotonViewOwnerChange {
    private Ragdoller ragdoller;
    private CharacterControllerAnimator controllerAnimator;
    private struct Frame {
        public Vector3 position;
        public Quaternion rotation;
        public double time;

        public Frame(Vector3 pos, Quaternion rotation, double time) {
            position = pos;
            this.rotation = rotation;
            this.time = time;
        }
    }
    private Frame lastFrame;
    private Frame newFrame;

    private void Start() {
        body = GetComponent<Rigidbody>();
        ragdoller = GetComponent<Ragdoller>();
        controllerAnimator = GetComponent<CharacterControllerAnimator>();
        
        lastFrame = new Frame(body.transform.position, body.transform.rotation, PhotonNetwork.Time);
        newFrame = new Frame(body.transform.position, body.transform.rotation, PhotonNetwork.Time);
    }
    
    private void LateUpdate() {
        if (photonView.IsMine) {
            body.isKinematic = controllerAnimator.IsAnimating() || ragdoller.ragdolled;
            return;
        }

        body.isKinematic = true;
        double time = PhotonNetwork.Time - (1d/PhotonNetwork.SerializationRate);
        double diff = newFrame.time - lastFrame.time;
        if (diff == 0f) {
            return;
        }
        double t = (time - lastFrame.time) / diff;
        body.transform.position = Vector3.LerpUnclamped(lastFrame.position, newFrame.position, Mathf.Clamp((float)t, -0.25f, 1.25f));
        body.transform.rotation = Quaternion.LerpUnclamped(lastFrame.rotation, newFrame.rotation, Mathf.Clamp((float)t, -0.25f, 1.25f));
    }
    
    private Rigidbody body;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(body.transform.position);
            stream.SendNext(body.transform.rotation);
        } else {
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            lastFrame = newFrame;
            newFrame = new Frame((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext(), info.SentServerTime+lag);
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(body.position.x);
        writer.Write(body.position.y);
        writer.Write(body.position.z);
    }

    public void Load(BinaryReader reader, string version) {
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        body.position = new Vector3(x, y, z);
    }

    public void OnOwnerChange(Player newOwner, Player previousOwner) {
        body.useGravity = newOwner == PhotonNetwork.LocalPlayer;
    }
}
