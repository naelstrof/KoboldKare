using NetStack.Quantization;
using NetStack.Serialization;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;

public class SmoothRigidbodyPhoton : MonoBehaviourPun, IPunObservable, ISavable {
    //private JiggleRigBuilder[] jiggleRigs;
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
    private Frame lastFrame;
    private Frame newFrame;
    private bool init = false;
    
    private void Awake() {
        body = GetComponent<Rigidbody>();
        //jiggleRigs = GetComponentsInChildren<JiggleRigBuilder>();
        lastFrame = new Frame(body.transform.position, body.transform.rotation, Time.time);
        newFrame = new Frame(body.transform.position, body.transform.rotation, Time.time);
    }
    
    private void LateUpdate() {
        if (photonView.IsMine) {
            body.isKinematic = false;
            //foreach (JiggleRigBuilder jiggleRig in jiggleRigs) {
                //jiggleRig.interpolate = true;
            //}
            return;
        }
        //foreach (JiggleRigBuilder jiggleRig in jiggleRigs) {
            //jiggleRig.interpolate = false;
        //}
        body.isKinematic = true;
        float time = Time.time - (1f/PhotonNetwork.SerializationRate);
        float diff = newFrame.time - lastFrame.time;
        if (diff == 0f) {
            body.transform.position = newFrame.position;
            body.transform.rotation = newFrame.rotation;
            return;
        }
        float t = (time - lastFrame.time) / diff;
        //body.velocity = (newFrame.position - lastFrame.position) / (float)diff;
        body.transform.position = Vector3.LerpUnclamped(lastFrame.position, newFrame.position, Mathf.Clamp((float)t, -0.25f, 1.25f));
        body.transform.rotation = Quaternion.LerpUnclamped(lastFrame.rotation, newFrame.rotation, Mathf.Clamp((float)t, -0.25f, 1.25f));
    }
    
    private Rigidbody body;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        BoundedRange[] worldBounds = PlayAreaEnforcer.GetWorldBounds();
        int bitsPerElement = 12;
        
        if (stream.IsWriting) {
            QuantizedVector3 quantizedPosition = BoundedRange.Quantize(body.transform.position, worldBounds);
            QuantizedQuaternion quantizedRotation = SmallestThree.Quantize(body.transform.rotation, bitsPerElement);

            BitBuffer bitBuffer = new BitBuffer(8);
            bitBuffer.Add(worldBounds[0].GetRequiredBits(), quantizedPosition.x)
                     .Add(worldBounds[1].GetRequiredBits(), quantizedPosition.y)
                     .Add(worldBounds[2].GetRequiredBits(), quantizedPosition.z)
                     .Add(2, quantizedRotation.m)
                     .Add(bitsPerElement, quantizedRotation.a)
                     .Add(bitsPerElement, quantizedRotation.b)
                     .Add(bitsPerElement, quantizedRotation.c);
            stream.SendNext(bitBuffer);
            
            lastFrame = newFrame;
            newFrame = new Frame(body.transform.position, body.transform.rotation, Time.time);
        } else {
            BitBuffer data = (BitBuffer)stream.ReceiveNext();
            QuantizedVector3 quantizedPosition = new QuantizedVector3(data.Read(worldBounds[0].GetRequiredBits()), data.Read(worldBounds[1].GetRequiredBits()), data.Read(worldBounds[2].GetRequiredBits()));
            QuantizedQuaternion quantizedRotation = new QuantizedQuaternion(data.Read(2), data.Read(bitsPerElement), data.Read(bitsPerElement), data.Read(bitsPerElement));

            Vector3 realPosition = BoundedRange.Dequantize(quantizedPosition, PlayAreaEnforcer.GetWorldBounds());
            Quaternion realRotation = SmallestThree.Dequantize(quantizedRotation);
            
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
        var position = body.transform.position;
        var rotation = body.transform.rotation;
        node["position.x"] = position.x;
        node["position.y"] = position.y;
        node["position.z"] = position.z;
        node["rotation.x"] = rotation.x;
        node["rotation.y"] = rotation.y;
        node["rotation.z"] = rotation.z;
        node["rotation.w"] = rotation.w;
    }

    public void Load(JSONNode node) {
        float x = node["position.x"];
        float y = node["position.y"];
        float z = node["position.z"];
        body.transform.position = new Vector3(x, y, z);
        if (node.HasKey("rotation.x")) {
            float rx = node["rotation.x"];
            float ry = node["rotation.y"];
            float rz = node["rotation.z"];
            float rw = node["rotation.w"];
            body.transform.rotation = new Quaternion(rx, ry, rz, rw);
        }
    }
}
