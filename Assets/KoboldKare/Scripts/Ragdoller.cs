using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Naelstrof.BodyProportion;
using Photon.Pun;
using Photon.Realtime;
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
    private List<Vector3> savedJointAnchors;
    [SerializeField]
    private BodyProportionBase bodyProportion;
    public bool ragdolled {get; private set;}
    private int ragdollCount;
    [SerializeField]
    private Transform hip;
    [SerializeField]
    private JigglePhysics.JiggleRigBuilder tailRig;
    [SerializeField]
    private LODGroup group;
    public Rigidbody[] GetRagdollBodies() {
        return ragdollBodies;
    }

    private class RigidbodyNetworkInfo {
        public RigidbodyNetworkInfo(Rigidbody body) {
            networkedPosition = body.position;
            networkedRotation = body.rotation;
            distance = 0f;
            angle = 0f;
        }
        public Vector3 networkedPosition;
        public Quaternion networkedRotation;
        public float distance;
        public float angle;
    }

    private List<RigidbodyNetworkInfo> rigidbodyNetworkInfos;

    private void Awake() {
        rigidbodyNetworkInfos = new List<RigidbodyNetworkInfo>();
        foreach (Rigidbody ragdollBody in ragdollBodies) {
            rigidbodyNetworkInfos.Add(new RigidbodyNetworkInfo(ragdollBody));
        }
    }

    void Start() {
        savedJointAnchors = new List<Vector3>();
        foreach (Rigidbody ragdollBody in ragdollBodies) {
            if (ragdollBody.GetComponent<CharacterJoint>() == null) {
                continue;
            }
            savedJointAnchors.Add(ragdollBody.GetComponent<CharacterJoint>().connectedAnchor);
            ragdollBody.GetComponent<CharacterJoint>().autoConfigureConnectedAnchor = false;
        }

    }
    [PunRPC]
    public void PushRagdoll() {
        ragdollCount++;
        ragdollCount = Mathf.Max(0,ragdollCount);
        if (ragdollCount > 0 && !ragdolled) {
            Ragdoll();
        } else if (ragdollCount == 0 && ragdolled) {
            StandUp();
        }
    }
    [PunRPC]
    public void PopRagdoll() {
        ragdollCount--;
        ragdollCount = Mathf.Max(0,ragdollCount);
        if (ragdollCount > 0 && !ragdolled) {
            Ragdoll();
        } else if (ragdollCount == 0 && ragdolled) {
            StandUp();
        }
    }
    void FixedUpdate() {
        if (photonView.IsMine || !ragdolled) {
            return;
        }

        for(int i=0;i<ragdollBodies.Length;i++) {
            Rigidbody ragbody = ragdollBodies[i];
            ragbody.position = Vector3.MoveTowards(ragbody.position, rigidbodyNetworkInfos[i].networkedPosition, rigidbodyNetworkInfos[i].distance * (1.0f / PhotonNetwork.SerializationRate));
            ragbody.rotation = Quaternion.RotateTowards(ragbody.rotation, rigidbodyNetworkInfos[i].networkedRotation, rigidbodyNetworkInfos[i].angle * (1.0f / PhotonNetwork.SerializationRate));
        }
    }
    private void Ragdoll() {
        if (ragdolled) {
            return;
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
        int i = 0;
        foreach (Rigidbody ragdollBody in ragdollBodies) {
            CharacterJoint j = ragdollBody.GetComponent<CharacterJoint>();
            if (j == null) {
                continue;
            }
            //j.anchor = Vector3.zero;
            j.connectedAnchor = savedJointAnchors[i++];
        }
        // FIXME: For somereason, after kobolds get grabbed and tossed off of a live physics animation-- the body doesn't actually stay kinematic. I'm assuming due to one of the ragdoll events.
        // Adding this extra set fixes it for somereason, though this is not a proper fix.
        body.isKinematic = true;
        RagdollEvent?.Invoke(true);
        ragdolled = true;
    }
    
    private void SetRagdolled(bool ragdolled) {
        if (ragdolled) {
            Ragdoll();
        } else {
            StandUp();
        }
        ragdollCount = 0;
    }
    // This was a huuuUUGE pain, but for somereason joints forget their initial orientation if you switch bodies.
    // I tried a billion different things to try to reset the initial orientation, this was the only thing that worked for me!
    private void StandUp() {
        if (!ragdolled) {
            return;
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
                foreach (Rigidbody ragbody in ragdollBodies) {
                    stream.SendNext(ragbody.position);
                    stream.SendNext(ragbody.rotation);
                }
            }
        } else {
            if ((bool)stream.ReceiveNext()) {
                for(int i=0;i<ragdollBodies.Length;i++) {
                    rigidbodyNetworkInfos[i].networkedPosition = (Vector3)stream.ReceiveNext();
                    rigidbodyNetworkInfos[i].networkedRotation = (Quaternion)stream.ReceiveNext();
                    rigidbodyNetworkInfos[i].distance = Vector3.Distance(ragdollBodies[i].position, rigidbodyNetworkInfos[i].networkedPosition);
                    rigidbodyNetworkInfos[i].angle = Quaternion.Angle(ragdollBodies[i].rotation, rigidbodyNetworkInfos[i].networkedRotation);
                }
            }
        }
    }
    public void Save(BinaryWriter writer, string version) {
        writer.Write(ragdolled);
    }
    public void Load(BinaryReader reader, string version) {
        SetRagdolled(reader.ReadBoolean());
    }

    public void OnOwnerChange(Player newOwner, Player previousOwner) {
        if (ReferenceEquals(newOwner, PhotonNetwork.LocalPlayer)) {
            ragdollCount = 0;
        }
    }
}
