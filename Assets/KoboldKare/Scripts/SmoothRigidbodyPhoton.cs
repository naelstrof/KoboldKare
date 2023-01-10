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
        if (stream.IsWriting) {
            QuantizedVector3 quantizedPosition = BoundedRange.Quantize(body.transform.position, PlayAreaEnforcer.GetWorldBounds());
            QuantizedQuaternion quantizedRotation = SmallestThree.Quantize(body.transform.rotation);
            
            BitBuffer bitBuffer = BufferPool.GetBitBuffer();
            bitBuffer.AddUInt(quantizedPosition.x)
                     .AddUInt(quantizedPosition.y)
                     .AddUInt(quantizedPosition.z)
                     .AddUInt(quantizedRotation.m)
                     .AddUInt(quantizedRotation.a)
                     .AddUInt(quantizedRotation.b)
                     .AddUInt(quantizedRotation.c);
            byte[] byteArray = BufferPool.GetArrayBuffer(bitBuffer.Length);
            bitBuffer.ToArray(byteArray);
            stream.SendNext(byteArray);
            
            lastFrame = newFrame;
            newFrame = new Frame(body.transform.position, body.transform.rotation, Time.time);
        } else {
            byte[] byteArray = (byte[])stream.ReceiveNext();
            BitBuffer bitBuffer = BufferPool.GetBitBuffer();
            bitBuffer.FromArray(byteArray, byteArray.Length);
            QuantizedVector3 quantizedPosition = new QuantizedVector3(bitBuffer.ReadUInt(), bitBuffer.ReadUInt(), bitBuffer.ReadUInt());
            QuantizedQuaternion quantizedRotation = new QuantizedQuaternion(bitBuffer.ReadUInt(), bitBuffer.ReadUInt(), bitBuffer.ReadUInt(), bitBuffer.ReadUInt());

            Vector3 realPosition = BoundedRange.Dequantize(quantizedPosition, PlayAreaEnforcer.GetWorldBounds());
            Quaternion realRotation = SmallestThree.Dequantize(quantizedRotation);
            
            lastFrame = newFrame;
            newFrame = new Frame(realPosition, realRotation, Time.time);
            if (!init) {
                lastFrame = newFrame;
                init = true;
            }
            PhotonProfiler.LogReceive(byteArray.Length+5);
        }
    }

    public void Save(JSONNode node) {
        var position = body.transform.position;
        node["position.x"] = position.x;
        node["position.y"] = position.y;
        node["position.z"] = position.z;
    }

    public void Load(JSONNode node) {
        float x = node["position.x"];
        float y = node["position.y"];
        float z = node["position.z"];
        body.transform.position = new Vector3(x, y, z);
    }
}
