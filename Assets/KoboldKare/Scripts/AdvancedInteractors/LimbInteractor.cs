using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon;
using Photon.Realtime;
using Photon.Pun;
// FIX PANINI PROJECTION

public class LimbInteractor : MonoBehaviour, IAdvancedInteractable {
    public HandIK handHandler;
    public enum Hand {
        Left = 0,
        Right
    }
    public Hand handTarget;
    public Vector3 handForward = Vector3.up;
    public Vector3 handUp = Vector3.forward;
    private Vector3 handRight = Vector3.right;
    private RopeJointConstraint joint;
    private bool ragdolled;
    private Kobold internalKobold;
    public Kobold kobold {
        get {
            if (internalKobold == null) {
                internalKobold = GetComponentInParent<Kobold>();
            }
            return internalKobold;
        }
    }
    public Rigidbody koboldBody;
    public Rigidbody koboldLimb;
    public Transform rootedTransform;
    public float _maxDistance = 1;
    public float _springStrength = 400;
    [Range(0f,1f)]
    public float _dampStrength = 0.1f;
    public UnityEvent OnGrab;

    private ConfigurableJoint otherJoint;
    public UnityEvent OnRelease;
    private Quaternion originalRot;
    private bool grabbed = false;
    public ConfigurableJoint AddJoint(Rigidbody hitBody, Vector3 worldAnchor) {
        //hit.rigidbody.rotation = Quaternion.identity;
        ConfigurableJoint joint = hitBody.gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        JointDrive drive = joint.xDrive;
        drive.positionSpring = _springStrength;
        drive.positionDamper = 2f;
        joint.xDrive = drive;
        joint.yDrive = drive;
        joint.zDrive = drive;
        joint.enablePreprocessing = false;
        joint.configuredInWorldSpace = true;
        //joint.xMotion = ConfigurableJointMotion.Locked;
        //joint.yMotion = ConfigurableJointMotion.Locked;
        //joint.zMotion = ConfigurableJointMotion.Locked;
        joint.anchor = hitBody.transform.InverseTransformPoint(worldAnchor);
        joint.connectedAnchor = worldAnchor;
        return joint;
    }
    public void OnInteract(Kobold k) {
        if (joint != null || otherJoint != null) {
            return;
        }
        OnGrab.Invoke();
        grabbed = true;
        joint = gameObject.AddComponent<RopeJointConstraint>();
        joint.anchor = transform.position;
        joint.connectedBody = koboldBody;
        joint.connectedAnchor = koboldBody.transform.InverseTransformPoint(rootedTransform.position);
        joint.springStrength = _springStrength;
        joint.dampStrength = _dampStrength;
        joint.maxDistance = _maxDistance;
        Vector3.OrthoNormalize(ref handForward, ref handUp, ref handRight);
        // Original rotation in HandIK space... which takes into account the original rotation of the skeleton because unity is dumb.
        originalRot = Quaternion.AngleAxis(90f, handRight) * Quaternion.Inverse(Quaternion.LookRotation(handUp, handForward));
    }

    public void Start() {
        kobold.ragdoller.RagdollEvent += RagdollEvent;
    }
    public void OnDestroy() {
        if (kobold != null) {
            kobold.ragdoller.RagdollEvent -= RagdollEvent;
        }
    }

    public void OnEndInteract(Kobold k) {
        OnRelease.Invoke();
        grabbed = false;
        if (otherJoint != null) {
            Destroy(otherJoint);
            otherJoint = null;
        }
        if (joint) {
            Destroy(joint);
            joint = null;
        }
        handHandler.UnsetIKTarget((int)handTarget);
    }

    public void InteractTo(Vector3 worldPosition, Quaternion worldRotation) {
        if (joint != null) {
            joint.anchor = worldPosition;
            joint.connectedAnchor = koboldBody.transform.InverseTransformPoint(rootedTransform.position);
            handHandler.SetIKTarget((int)handTarget, worldPosition, worldRotation*originalRot);
        } else {
            handHandler.UnsetIKTarget((int)handTarget);
        }
    }

    public bool ShowHand() {
        return true;
    }
    public bool PhysicsGrabbable() {
        return ragdolled;
    }

    public void RagdollEvent(bool ragdolled) {
        this.ragdolled = ragdolled;
        if (ragdolled) {
            if (joint != null) {
                joint.enabled = false;
            }
            if (koboldLimb != null && grabbed) {
                if (otherJoint != null) {
                    Destroy(otherJoint);
                }
                otherJoint = AddJoint(koboldLimb, transform.position);
            }
            OnGrab.Invoke();
        } else {
            if (otherJoint != null) {
                Destroy(otherJoint);
            }
            if (joint != null) {
                joint.enabled = true;
            }
            if (!grabbed) {
                OnRelease.Invoke();
            }
        }
    }
}
