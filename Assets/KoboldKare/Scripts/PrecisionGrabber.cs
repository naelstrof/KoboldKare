using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;
using System.IO;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V12;

public class PrecisionGrabber : MonoBehaviourPun {
    [SerializeField] private GameObject handDisplayPrefab;
    [SerializeField] private Transform view;
    private static RaycastHit[] hits = new RaycastHit[10];
    private static readonly int GrabbingHash = Animator.StringToHash("Grabbing");
    private RaycastHitDistanceComparer raycastHitDistanceComparer;
    private Animator previewHandAnimator;
    private Transform previewHandTransform;
    private Grab currentGrab;
    private List<Grab> frozenGrabs;
    private Kobold kobold;
    
    private bool previewGrab;

    [SerializeField]
    private Collider[] ignoreColliders;
    private class Grab {
        private ConfigurableJoint joint;
        private PhotonView photonView;
        private Rigidbody body;
        private Collider collider;
        private Vector3 localColliderPosition;
        private Vector3 localHitNormal;
        private Quaternion savedQuaternion;
        private Animator handDisplayAnimator;
        private Transform handTransform;
        private float distance;
        private bool affectingRotation;
        private Kobold owner;
        private Vector3 bodyAnchor;
        private Transform view;

        public Grab(Kobold owner, GameObject handDisplayPrefab, Transform view, Collider collider,
            Vector3 localColliderPosition, Vector3 localHitNormal) {
            this.collider = collider;
            this.localColliderPosition = localColliderPosition;
            this.localHitNormal = localHitNormal;
            body = collider.GetComponentInParent<Rigidbody>();
            savedQuaternion = body.rotation * Quaternion.Inverse(view.rotation);
            Vector3 hitPosWorld = collider.transform.TransformPoint(localColliderPosition);
            bodyAnchor = body.transform.InverseTransformPoint(hitPosWorld);
            distance = Vector3.Distance(view.position, hitPosWorld);
            handDisplayAnimator = GameObject.Instantiate(handDisplayPrefab, owner.transform)
                .GetComponentInChildren<Animator>();
            handDisplayAnimator.SetBool(GrabbingHash, true);
            handTransform = handDisplayAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            this.owner = owner;
            this.view = view;
        }

        public bool Valid() {
            return body != null && owner != null && photonView != null;
        }

        public void FixedUpdate() {
            if (affectingRotation) {
                Vector3 forward = savedQuaternion * Vector3.forward;
                Vector3 up = savedQuaternion * Vector3.up;
                Quaternion rotAdjustment = Quaternion.FromToRotation(body.transform.forward, forward);
                rotAdjustment *= Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(body.transform.up, up),
                    0.5f);

                body.angularVelocity -= body.angularVelocity * 0.5f;
                body.AddTorque(new Vector3(rotAdjustment.x, rotAdjustment.y, rotAdjustment.z) * 32f,
                    ForceMode.VelocityChange);
            }

            Vector3 holdPoint = view.position + view.forward * distance;
            Vector3 objHoldPoint = body.transform.TransformPoint(bodyAnchor);

            // Manual axis alignment, for pole jumps!
            if (!body.transform.IsChildOf(owner.body.transform) && !body.isKinematic) {
                body.velocity -= body.velocity * 0.5f;
                Vector3 axis = view.forward;
                Vector3 jointPos = body.transform.TransformPoint(bodyAnchor);
                Vector3 center = (view.position + jointPos) / 2f;
                Vector3 wantedPosition1 = center - axis * distance / 2f;
                //Vector3 wantedPosition2 = center + axis * distance / 2f;
                float ratio = Mathf.Clamp((body.mass / owner.body.mass), 0.75f, 1.25f);
                Vector3 force = (wantedPosition1 - view.position) * 200f;
                owner.body.AddForce(force * ratio);
                body.AddForce(-force * (1f / ratio));
            }

            // Manual velocity to keep the prop where the user wants
            Vector3 towardGoal = holdPoint - objHoldPoint;
            body.AddForce(towardGoal * 50f);
        }

        public void Release() {
        }
    }

    private class RaycastHitDistanceComparer : IComparer {
        public int Compare(object x, object y) {
            RaycastHit a = (RaycastHit)x;
            RaycastHit b = (RaycastHit)y;
            return a.distance.CompareTo(b.distance);
        }
    }

    private void Awake() {
        raycastHitDistanceComparer = new RaycastHitDistanceComparer();
        previewHandAnimator = GameObject.Instantiate(handDisplayPrefab, transform)
            .GetComponentInChildren<Animator>();
        previewHandAnimator.SetBool(GrabbingHash, true);
        previewHandTransform = previewHandAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        previewHandAnimator.gameObject.SetActive(false);
        kobold = GetComponent<Kobold>();
        frozenGrabs = new List<Grab>();
    }

    private bool TryRaycastGrab(out RaycastHit? previewHit) {
        float maxDistance = 2.5f;
        int numHits = Physics.RaycastNonAlloc(view.position, view.forward, hits, maxDistance, GameManager.instance.precisionGrabMask, QueryTriggerInteraction.Ignore);
        if (numHits == 0) {
            previewHit = null;
            return false;
        }
        Array.Sort(hits, 0, numHits, raycastHitDistanceComparer);
        for (int i = 0; i < numHits; i++) {
            RaycastHit hit = hits[i];
            if (ignoreColliders.Contains(hit.collider)) {
                continue;
            }
            if (hit.distance > maxDistance) {
                continue;
            }

            previewHit = hit;
            return true;
        }
        previewHit = null;
        return false;
    }

    public void SetPreviewState(bool previewEnabled) {
        previewGrab = previewEnabled;
    }

    private void DoPreview() {
        if (previewGrab && currentGrab == null && TryRaycastGrab(out RaycastHit? previewHit)) {
            RaycastHit hit = previewHit.Value;
            previewHandTransform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up) * Quaternion.AngleAxis(90f, new Vector3(0.0f, 1.0f, 0.0f));
            previewHandTransform.position = hit.point + previewHandTransform.rotation*Vector3.down*0.1f;
            if (!previewHandAnimator.gameObject.activeInHierarchy) {
                previewHandAnimator.gameObject.SetActive(true);
            }
        } else {
            if (previewHandAnimator.gameObject.activeInHierarchy) {
                previewHandAnimator.gameObject.SetActive(false);
            }
        }
    }

    public void TryGrab() {
        if (currentGrab != null) {
            return;
        }
        if (!TryRaycastGrab(out RaycastHit? hitTest)) {
            return;
        }

        RaycastHit hit = hitTest.Value;
        Vector3 localHit = hit.collider.transform.InverseTransformPoint(hit.point);
        Vector3 localHitNormal = hit.collider.transform.InverseTransformDirection(hit.normal);
        currentGrab = new Grab(kobold, previewHandAnimator.gameObject, view, hit.collider, localHit, localHitNormal);
    }

    public void TryDrop() {
        currentGrab = null;
    }

    private void LateUpdate() {
        DoPreview();
    }

    private void FixedUpdate() {
        currentGrab?.FixedUpdate();

        for (int i = 0; i < frozenGrabs.Count; i++) {
            if (!frozenGrabs[i].Valid()) { frozenGrabs.RemoveAt(i--); }
        }

        foreach (var grab in frozenGrabs) {
            grab.FixedUpdate();
        }
    }
}
