using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using SimpleJSON;
using UnityEngine;

public class SmoothCharacterPhoton : MonoBehaviourPun, IPunObservable, ISavable {
    private Ragdoller ragdoller;
    private CharacterControllerAnimator controllerAnimator;
    private Vector3 currentVelocity;
    private struct Frame {
        public Vector3 position;
        public Quaternion rotation;
        public float time;

        public Frame(Vector3 pos, Quaternion rotation, float time) {
            position = pos;
            this.rotation = rotation;
            this.time = time;
        }
    }

    private bool init;
    private Frame lastFrame;
    private Frame newFrame;

    private void Awake() {
        body = GetComponent<Rigidbody>();
        ragdoller = GetComponent<Ragdoller>();
        controllerAnimator = GetComponent<CharacterControllerAnimator>();
        
        lastFrame = new Frame(body.transform.position, body.transform.rotation, Time.time);
        newFrame = new Frame(body.transform.position, body.transform.rotation, Time.time);
    }
    
    private void LateUpdate() {
        if (photonView.IsMine) {
            body.isKinematic = controllerAnimator.IsAnimating() || ragdoller.ragdolled;
            currentVelocity = body.velocity;
            return;
        }

        body.isKinematic = true;
        float time = Time.time - (1f/PhotonNetwork.SerializationRate);
        float diff = newFrame.time - lastFrame.time;
        if (diff == 0f) {
            body.transform.position = newFrame.position;
            body.transform.rotation = newFrame.rotation;
            return;
        }
        float t = (time - lastFrame.time) / diff;
        Vector3 desiredPosition = Vector3.LerpUnclamped(lastFrame.position, newFrame.position, Mathf.Clamp((float)t, -0.25f, 1.25f));
        body.transform.position = Vector3.SmoothDamp(body.transform.position, desiredPosition, ref currentVelocity, 0.1f);
        body.transform.rotation = Quaternion.LerpUnclamped(lastFrame.rotation, newFrame.rotation, Mathf.Clamp((float)t, -0.25f, 1.25f));
    }
    
    private Rigidbody body;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(body.transform.position);
            stream.SendNext(body.transform.rotation);
            lastFrame = newFrame;
            newFrame = new Frame(body.transform.position, body.transform.rotation, Time.time);
        } else {
            //float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            lastFrame = newFrame;
            newFrame = new Frame((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext(), Time.time);
            if (!init) {
                lastFrame = newFrame;
                init = true;
            }
        }
    }

    public void Save(JSONNode node) {
        node["position.x"] = body.position.x;
        node["position.y"] = body.position.y;
        node["position.z"] = body.position.z;
    }

    public void Load(JSONNode node) {
        float x = node["position.x"];
        float y = node["position.y"];
        float z = node["position.z"];
        body.position = new Vector3(x, y, z);
    }
}
