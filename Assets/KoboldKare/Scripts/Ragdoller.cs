using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JigglePhysics;
using Naelstrof.BodyProportion;
using Photon.Pun;
using Photon.Realtime;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

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
    private Transform hip;
    [SerializeField]
    private JigglePhysics.JiggleRigBuilder tailRig;

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

        Vector3 diff = transform.position - hip.position;
        transform.position -= diff * 0.5f;
        hip.position += diff * 0.5f;
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

    private class RigidbodyNetworkInfo {
        private struct Packet {
            public Packet(float t, Vector3 p, Quaternion rot) {
                time = t;
                networkedPosition = p;
                networkedRotation = rot;
            }
            public float time;
            public Vector3 networkedPosition;
            public Quaternion networkedRotation;
        }

        public Rigidbody body { get; private set; }
        private Packet lastPacket;
        private Packet nextPacket;
        public RigidbodyNetworkInfo(Rigidbody body) {
            this.body = body;
            lastPacket = new Packet(Time.time, body.transform.position, body.transform.rotation);
            nextPacket = new Packet(Time.time, body.transform.position, body.transform.rotation);
        }
        public void SetNetworkPosition(Vector3 position, Quaternion rotation, float time) {
            lastPacket = nextPacket;
            nextPacket = new Packet(time, position, rotation);
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
                body.transform.position = Vector3.LerpUnclamped(lastPacket.networkedPosition,
                                                              nextPacket.networkedPosition, Mathf.Clamp((float)t, -0.25f, 1.25f));
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

        //jiggleRig.interpolate = false;
        //jiggleSkin.interpolate = false;
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
        //body.interpolation = RigidbodyInterpolation.None;
        body.isKinematic = true;
        body.detectCollisions = false;
        //body.GetComponent<Collider>().enabled = false;

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
        //jiggleRig.interpolate = true;
        //jiggleSkin.interpolate = true;
        tailRig.enabled = true;
        Vector3 diff = hip.position - body.transform.position;
        body.transform.position += diff;
        hip.position -= diff;
        body.transform.position += Vector3.up*0.5f;
        Vector3 facingDir = hip.forward.With(y: 0f).normalized;
        body.transform.forward = facingDir;
        body.isKinematic = false;
        body.detectCollisions = true;
        //body.GetComponent<Collider>().enabled = true;
        body.collisionDetectionMode = oldCollisionMode;
        //body.interpolation = RigidbodyInterpolation.Interpolate;
        Vector3 averageVel = Vector3.zero;
        foreach (Rigidbody b in ragdollBodies) {
            averageVel += b.velocity;
        }
        averageVel /= ragdollBodies.Length;
        body.velocity = averageVel;
        controller.enabled = true;
        //RecursiveSetLayer(transform, LayerMask.NameToLayer("PlayerHitbox"), LayerMask.NameToLayer("Hitbox"));
        foreach (Rigidbody b in ragdollBodies) {
            b.interpolation = RigidbodyInterpolation.None;
            b.collisionDetectionMode = CollisionDetectionMode.Discrete;
            b.isKinematic = true;
        }
        //foreach(var penSet in penetratables) {
            //penSet.penetratable.SwitchBody(body);
        //}
        animator.enabled = true;
        bodyProportion.enabled = true;
        controller.enabled = true;
        RagdollEvent?.Invoke(false);
        ragdolled = false;
        //if (photonView.IsMine) {
            //photonView.RPC(nameof(SetRagdolled), RpcTarget.Others, false);
        //}
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(ragdolled);
            if (ragdolled) {
                for(int i=0;i<rigidbodyNetworkInfos.Count;i++) {
                    Rigidbody ragbody = rigidbodyNetworkInfos[i].body;
                    stream.SendNext(ragbody.transform.position);
                    stream.SendNext(ragbody.transform.rotation);
                    rigidbodyNetworkInfos[i].SetNetworkPosition(ragbody.transform.position, ragbody.transform.rotation, Time.time);
                }
            }
        } else {
            int totalSentBytes = sizeof(bool);
            //float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            if ((bool)stream.ReceiveNext()) {
                for(int i=0;i<ragdollBodies.Length;i++) {
                    rigidbodyNetworkInfos[i].SetNetworkPosition((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext(), Time.time);
                    totalSentBytes += sizeof(float) * 7;
                }
            } else {
                for (int i = 0; i < ragdollBodies.Length; i++) {
                    rigidbodyNetworkInfos[i].SetNetworkPosition(ragdollBodies[i].transform.position,
                        ragdollBodies[i].transform.rotation, Time.time);
                }
            }
            PhotonProfiler.LogReceive(totalSentBytes);
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
