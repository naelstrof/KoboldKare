using System;
using System.Collections.Generic;
using Naelstrof.BodyProportion;
using NetStack.Quantization;
using NetStack.Serialization;
using Photon.Pun;
using Photon.Realtime;
using SimpleJSON;
using UnityEngine;

public class Ragdoller : MonoBehaviourPun, IPunObservable, ISavable, IOnPhotonViewOwnerChange {
    public delegate void RagdollEventHandler(bool ragdolled);
    public event RagdollEventHandler RagdollEvent;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private KoboldCharacterController controller;
    [SerializeField]
    private Rigidbody[] ragdollBodies;
    [SerializeField]
    private Rigidbody body;
    private CollisionDetectionMode oldCollisionMode;
    [SerializeField]
    private BodyProportionBase bodyProportion;
    public bool ragdolled {get; private set;}
    private int ragdollCount;
    [SerializeField]
    private Rigidbody hipBody;
    [SerializeField]
    private JigglePhysics.JiggleRigBuilder tailRig;

    private PositionPacket lastPacket;
    private PositionPacket nextPacket;

    private Kobold kobold;

    [SerializeField]
    private LODGroup group;
    
    private bool locked;
    public Rigidbody[] GetRagdollBodies() {
        return ragdollBodies;
    }

    private void FixedUpdate() {
        if (!ragdolled) {
            return;
        }

        Vector3 diff = transform.position - hipBody.position;
        transform.position -= diff * 0.5f;
        hipBody.transform.position += diff * 0.5f;
    }

    public void SetLocked(bool newLockState) {
        locked = newLockState;
        if (locked && ragdolled) {
            SetRagdolled(false, ragdollCount);
        } else {
            if (ragdollCount > 0) {
                SetRagdolled(true, ragdollCount);
            }
        }
    }

    private class SavedJointAnchor {
        public SavedJointAnchor(ConfigurableJoint joint) {
            this.joint = joint;
            this.jointAnchor = joint.connectedAnchor;
        }

        public void Set() {
            joint.connectedAnchor = jointAnchor;
        }

        private ConfigurableJoint joint;
        private Vector3 jointAnchor;
    }
    
    private List<SavedJointAnchor> jointAnchors;
    
    private struct PositionPacket {
        public PositionPacket(float t, Vector3 p) {
            time = t;
            networkedPosition = p;
        }
        public float time;
        public Vector3 networkedPosition;
    }

    private class RigidbodyNetworkInfo {
        private struct Packet {
            public Packet(float t, Quaternion rot) {
                time = t;
                networkedRotation = rot;
            }
            public float time;
            public Quaternion networkedRotation;
        }

        public Rigidbody body { get; private set; }
        private Packet lastPacket;
        private Packet nextPacket;
        private Vector3 savedLocalPosition;
        public RigidbodyNetworkInfo(Rigidbody body) {
            this.body = body;
            savedLocalPosition = body.transform.localPosition;
            lastPacket = nextPacket = new Packet(Time.time, body.transform.rotation);
        }
        public void SetNetworkPosition(Quaternion rotation, float time) {
            lastPacket = nextPacket;
            nextPacket = new Packet(time, rotation);
        }
        public void UpdateState(bool ours, bool ragdolled) {
            if (ours) {
                body.isKinematic = !ragdolled;
                body.interpolation = ragdolled ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
                return;
            }
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.None;
            if (ragdolled) {
                float time = Time.time - (1f / PhotonNetwork.SerializationRate);
                float diff = nextPacket.time - lastPacket.time;
                if (diff == 0f) {
                    return;
                }
                float t = (time - lastPacket.time) / diff;
                //body.velocity = (nextPacket.networkedPosition - lastPacket.networkedPosition) / (float)diff;
                body.transform.localPosition = savedLocalPosition;
                body.transform.rotation = Quaternion.LerpUnclamped(lastPacket.networkedRotation,
                    nextPacket.networkedRotation, Mathf.Clamp((float)t, -0.25f, 1.25f));
            }
        }

    }

    private List<RigidbodyNetworkInfo> rigidbodyNetworkInfos;

    private void Awake() {
        kobold = GetComponent<Kobold>();
        jointAnchors = new List<SavedJointAnchor>();
        foreach (Rigidbody ragdollBody in ragdollBodies) {
            if (ragdollBody.TryGetComponent(out ConfigurableJoint joint)) {
                jointAnchors.Add(new SavedJointAnchor(joint));
                joint.autoConfigureConnectedAnchor = false;
            }
        }

        rigidbodyNetworkInfos = new List<RigidbodyNetworkInfo>();
        foreach (Rigidbody ragdollBody in ragdollBodies) {
            rigidbodyNetworkInfos.Add(new RigidbodyNetworkInfo(ragdollBody));
        }
        lastPacket = nextPacket = new PositionPacket(Time.time, hipBody.transform.position);
    }

    [PunRPC]
    public void PushRagdoll() {
        PhotonProfiler.LogReceive(1);
        ragdollCount++;
        ragdollCount = Mathf.Max(0,ragdollCount);
        if (locked) {
            return;
        }

        if (ragdollCount > 0 && !ragdolled) {
            Ragdoll();
        } else if (ragdollCount == 0 && ragdolled) {
            StandUp();
        }
    }
    [PunRPC]
    public void PopRagdoll() {
        PhotonProfiler.LogReceive(1);
        ragdollCount--;
        ragdollCount = Mathf.Max(0,ragdollCount);
        if (locked) {
            return;
        }
        
        if (ragdollCount > 0 && !ragdolled) {
            Ragdoll();
        } else if (ragdollCount == 0 && ragdolled) {
            StandUp();
        }
    }
    void LateUpdate() {
        foreach(var networkInfo in rigidbodyNetworkInfos) {
            networkInfo.UpdateState(photonView.IsMine, ragdolled);
        }
        if (photonView.IsMine) {
            return;
        }
        if (ragdolled) {
            float time = Time.time - (1f / PhotonNetwork.SerializationRate);
            float diff = nextPacket.time - lastPacket.time;
            if (diff == 0f) {
                return;
            }
            float t = (time - lastPacket.time) / diff;
            hipBody.transform.position = Vector3.LerpUnclamped(lastPacket.networkedPosition, nextPacket.networkedPosition, Mathf.Clamp((float)t, -0.25f, 1.25f));
        }
    }
    private void Ragdoll() {
        if (ragdolled) {
            return;
        }

        foreach (var dickSet in kobold.activeDicks) {
            foreach (var penn in kobold.penetratables) {
                if (!penn.penetratable.name.Contains("Mouth")) {
                    continue;
                }

                dickSet.dick.RemoveIgnorePenetrable(penn.penetratable);
            }
        }

        foreach (var lod in group.GetLODs()) {
            foreach (Renderer renderer in lod.renderers) {
                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer) {
                    skinnedMeshRenderer.updateWhenOffscreen = true;
                }
            }
        }
        group.ForceLOD(0);
        tailRig.enabled = false;
        animator.enabled = false;
        bodyProportion.enabled = false;
        controller.enabled = false;
        foreach (Rigidbody b in ragdollBodies) {
            b.velocity = body.velocity;
            b.isKinematic = false;
            b.collisionDetectionMode = CollisionDetectionMode.Continuous;
            b.interpolation = RigidbodyInterpolation.Interpolate;
        }
        oldCollisionMode = body.collisionDetectionMode;
        body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        body.isKinematic = true;
        body.detectCollisions = false;

        // We need to know the final result of our ragdoll before we update the anchors.
        Physics.SyncTransforms();
        bodyProportion.ScaleSkeleton();
        Physics.SyncTransforms();
        foreach (var savedJointAnchor in jointAnchors) {
            savedJointAnchor.Set();
        }
        // FIXME: For somereason, after kobolds get grabbed and tossed off of a live physics animation-- the body doesn't actually stay kinematic. I'm assuming due to one of the ragdoll events.
        // Adding this extra set fixes it for somereason, though this is not a proper fix.
        body.isKinematic = true;
        RagdollEvent?.Invoke(true);
        ragdolled = true;
    }
    
    private void SetRagdolled(bool ragdolled, int newRagdollCount = 0) {
        if (ragdolled) {
            Ragdoll();
        } else {
            StandUp();
        }
        ragdollCount = newRagdollCount;
    }
    // This was a huuuUUGE pain, but for somereason joints forget their initial orientation if you switch bodies.
    // I tried a billion different things to try to reset the initial orientation, this was the only thing that worked for me!
    private void StandUp() {
        if (!ragdolled) {
            return;
        }
        foreach (var dickSet in kobold.activeDicks) {
            foreach (var penn in kobold.penetratables) {
                if (!penn.penetratable.name.Contains("Mouth")) {
                    continue;
                }

                dickSet.dick.AddIgnorePenetrable(penn.penetratable);
            }
        }
        foreach (var lod in group.GetLODs()) {
            foreach (Renderer renderer in lod.renderers) {
                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer) {
                    skinnedMeshRenderer.updateWhenOffscreen = false;
                }
            }
        }
        group.ForceLOD(-1);
        tailRig.enabled = true;
        Vector3 diff = hipBody.position - body.transform.position;
        body.transform.position += diff;
        hipBody.position -= diff;
        body.transform.position += Vector3.up*0.5f;
        Vector3 facingDir = hipBody.transform.forward.With(y: 0f).normalized;
        body.transform.forward = facingDir;
        body.isKinematic = false;
        body.detectCollisions = true;
        body.collisionDetectionMode = oldCollisionMode;
        Vector3 averageVel = Vector3.zero;
        foreach (Rigidbody b in ragdollBodies) {
            averageVel += b.velocity;
        }
        averageVel /= ragdollBodies.Length;
        body.velocity = averageVel;
        controller.enabled = true;
        foreach (Rigidbody b in ragdollBodies) {
            b.interpolation = RigidbodyInterpolation.None;
            b.collisionDetectionMode = CollisionDetectionMode.Discrete;
            b.isKinematic = true;
        }
        animator.enabled = true;
        bodyProportion.enabled = true;
        controller.enabled = true;
        RagdollEvent?.Invoke(false);
        ragdolled = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        int bitsPerElement = 12;
        if (stream.IsWriting) {
            BitBuffer sendBuffer = new BitBuffer(12);
            sendBuffer.AddBool(ragdolled);
            if (ragdolled) {
                Vector3 hipPosition = hipBody.transform.position;
                
                sendBuffer.AddUInt(BitConverter.ToUInt32(BitConverter.GetBytes(hipPosition.x), 0));
                sendBuffer.AddUInt(BitConverter.ToUInt32(BitConverter.GetBytes(hipPosition.y), 0));
                sendBuffer.AddUInt(BitConverter.ToUInt32(BitConverter.GetBytes(hipPosition.z), 0));
                lastPacket = nextPacket;
                nextPacket = new PositionPacket(Time.time, hipBody.transform.position);
                foreach (var t in rigidbodyNetworkInfos) {
                    Rigidbody ragbody = t.body;
                    QuantizedQuaternion rot = SmallestThree.Quantize(ragbody.transform.rotation, bitsPerElement);
                    sendBuffer.Add(2,rot.m);
                    sendBuffer.Add(bitsPerElement,rot.a);
                    sendBuffer.Add(bitsPerElement,rot.b);
                    sendBuffer.Add(bitsPerElement,rot.c);
                    t.SetNetworkPosition(ragbody.transform.rotation, Time.time);
                }
            }
            stream.SendNext(sendBuffer);
        } else {
            BitBuffer data = (BitBuffer)stream.ReceiveNext();
            if (data.ReadBool()) {
                lastPacket = nextPacket;
                float xPosition = BitConverter.ToSingle(BitConverter.GetBytes(data.ReadUInt()));
                float yPosition = BitConverter.ToSingle(BitConverter.GetBytes(data.ReadUInt()));
                float zPosition = BitConverter.ToSingle(BitConverter.GetBytes(data.ReadUInt()));
                nextPacket = new PositionPacket(Time.time, new Vector3(xPosition, yPosition, zPosition));

                foreach (var t in rigidbodyNetworkInfos) {
                    QuantizedQuaternion rot = new QuantizedQuaternion(data.Read(2), data.Read(bitsPerElement),
                        data.Read(bitsPerElement), data.Read(bitsPerElement));
                    t.SetNetworkPosition(SmallestThree.Dequantize(rot), Time.time);
                }
            } else {
                foreach (var t in rigidbodyNetworkInfos) {
                    t.SetNetworkPosition(t.body.transform.rotation, Time.time);
                }
            }
            PhotonProfiler.LogReceive(data.Length);
        }
    }
    public void Save(JSONNode node) {
        node["ragdolled"] = ragdolled;
    }
    public void Load(JSONNode node) {
        SetRagdolled(node["ragdolled"]);
    }

    public void OnOwnerChange(Player newOwner, Player previousOwner) {
        if (ReferenceEquals(newOwner, PhotonNetwork.LocalPlayer)) {
            ragdollCount = 0;
        }
    }
}
