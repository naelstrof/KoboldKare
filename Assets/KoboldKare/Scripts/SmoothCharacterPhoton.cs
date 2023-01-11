using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NetStack.Quantization;
using NetStack.Serialization;
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
        var worldBounds = PlayAreaEnforcer.GetWorldBounds();
        
        if (stream.IsWriting) {
            QuantizedVector3 quantizedPosition = BoundedRange.Quantize(body.transform.position, worldBounds);
            float angle = Vector3.SignedAngle(Vector3.forward, body.transform.forward, Vector3.up);
            BitBuffer bitBuffer = new BitBuffer(8);
            bitBuffer.Add(worldBounds[0].GetRequiredBits(), quantizedPosition.x)
                     .Add(worldBounds[1].GetRequiredBits(), quantizedPosition.y)
                     .Add(worldBounds[2].GetRequiredBits(), quantizedPosition.z)
                     .AddUShort(HalfPrecision.Quantize(angle));
            
            stream.SendNext(bitBuffer);
            lastFrame = newFrame;
            newFrame = new Frame(body.transform.position, body.transform.rotation, Time.time);
        } else {
            BitBuffer data = (BitBuffer)stream.ReceiveNext();
            QuantizedVector3 quantizedPosition = new QuantizedVector3(data.Read(worldBounds[0].GetRequiredBits()), data.Read(worldBounds[1].GetRequiredBits()), data.Read(worldBounds[2].GetRequiredBits()));
            Vector3 realPosition = BoundedRange.Dequantize(quantizedPosition, worldBounds);
            float angle = HalfPrecision.Dequantize(data.ReadUShort());
            Quaternion realRotation = Quaternion.AngleAxis(angle, Vector3.up);
            
            lastFrame = newFrame;
            newFrame = new Frame(realPosition, realRotation, Time.time);
            if (!init) {
                lastFrame = newFrame;
                init = true;
            }
            PhotonProfiler.LogReceive(data.Length);
        }
    }

    public void Save(JSONNode node) {
        node["position.x"] = body.transform.position.x;
        node["position.y"] = body.transform.position.y;
        node["position.z"] = body.transform.position.z;
        float angle = Vector3.SignedAngle(Vector3.forward, body.transform.forward, Vector3.up);
        node["angle"] = angle;
    }

    public void Load(JSONNode node) {
        float x = node["position.x"];
        float y = node["position.y"];
        float z = node["position.z"];
        body.transform.position = new Vector3(x, y, z);
        if (node.HasKey("angle")) {
            Quaternion realRotation = Quaternion.AngleAxis(node["angle"], Vector3.up);
            body.transform.rotation = realRotation;
        }
    }
}
