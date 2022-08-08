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
        public RigidbodyNetworkInfo(Rigidbody body) {
            this.body = body;
            networkedPosition = body.position;
            networkedRotation = body.rotation;
        }
        public void SetNetworkPosition(Vector3 position, Quaternion rotation) {
            networkedPosition = position;
            networkedRotation = rotation;
        }

        public void UpdateState(bool ours) {
            if (ours && joint != null) {
                Destroy(joint);
            }

            if (ours) {
                return;
            }

            if (joint == null) {
                joint = AddJoint(networkedPosition, networkedRotation);
            }

            joint.connectedAnchor = networkedPosition;
            joint.SetTargetRotation(networkedRotation, startRotation);
        }

        private Rigidbody body;
        private Vector3 networkedPosition;
        private Quaternion networkedRotation;
        private float distance;
        private float angle;
        private ConfigurableJoint joint;
        private Quaternion startRotation;
        private const float springForce = 100f;
        private ConfigurableJoint AddJoint(Vector3 worldPosition, Quaternion targetRotation) {
            startRotation = body.rotation;
            ConfigurableJoint configurableJoint = body.gameObject.AddComponent<ConfigurableJoint>();
            configurableJoint.axis = Vector3.up;
            configurableJoint.secondaryAxis = Vector3.right;
            configurableJoint.connectedBody = null;
            configurableJoint.autoConfigureConnectedAnchor = false;
            configurableJoint.breakForce = float.MaxValue;
            JointDrive drive = configurableJoint.xDrive;
            SoftJointLimit sjl = configurableJoint.linearLimit;
            sjl.limit = 0f;
            configurableJoint.linearLimit = sjl;
            SoftJointLimitSpring sjls = configurableJoint.linearLimitSpring;
            sjls.spring = springForce;
            configurableJoint.linearLimitSpring = sjls;
            configurableJoint.linearLimit = sjl;
            drive.positionSpring = springForce;
            drive.positionDamper = 2f;
            configurableJoint.xDrive = drive;
            configurableJoint.yDrive = drive;
            configurableJoint.zDrive = drive;
            configurableJoint.rotationDriveMode = RotationDriveMode.Slerp;
            var slerpDrive = configurableJoint.slerpDrive;
            slerpDrive.positionSpring = springForce*2f;
            slerpDrive.maximumForce = float.MaxValue;
            slerpDrive.positionDamper = 2f;
            configurableJoint.slerpDrive = slerpDrive;
            configurableJoint.massScale = 1f;
            configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
            configurableJoint.projectionAngle = 10f;
            configurableJoint.projectionDistance = 0.5f;
            configurableJoint.connectedMassScale = 1f;
            configurableJoint.enablePreprocessing = false;
            configurableJoint.configuredInWorldSpace = true;
            configurableJoint.anchor = Vector3.zero;
            configurableJoint.connectedBody = null;
            configurableJoint.connectedAnchor = worldPosition;
            configurableJoint.SetTargetRotation(targetRotation, startRotation);
            configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
            return configurableJoint;
        }
    }

    private List<RigidbodyNetworkInfo> rigidbodyNetworkInfos;

    private void Awake() {
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
        foreach(var networkInfo in rigidbodyNetworkInfos) {
            networkInfo.UpdateState(photonView.IsMine);
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
        foreach (var savedJointAnchor in jointAnchors) {
            savedJointAnchor.Set();
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
                    rigidbodyNetworkInfos[i].SetNetworkPosition((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
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
