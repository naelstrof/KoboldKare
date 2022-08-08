using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class PrecisionGrabber : MonoBehaviourPun, IPunObservable, ISavable {
    [SerializeField] private GameObject handDisplayPrefab;
    [SerializeField] private Transform view;
    [SerializeField] private GameObject freezeVFXPrefab;
    [SerializeField] private AudioPack unfreezeSound;
    [SerializeField] private Kobold kobold;
    
    private static RaycastHit[] hits = new RaycastHit[10];
    private static readonly int GrabbingHash = Animator.StringToHash("Grabbing");
    private static readonly int BrightnessContrastSaturation = Shader.PropertyToID("_HueBrightnessContrastSaturation");
    private RaycastHitDistanceComparer raycastHitDistanceComparer;
    private Animator previewHandAnimator;
    private Transform previewHandTransform;
    private Grab currentGrab;
    private List<Grab> frozenGrabs;
    private const float springForce = 100f;
    private const float breakForce = 10000f;
    private const float maxGrabDistance = 2.5f;
    private bool previewGrab;
    private List<Grab> removeIds;
    
    [SerializeField]
    private Collider[] ignoreColliders;
    private class Grab {
        public PhotonView photonView { get; private set; }
        public Rigidbody body { get; private set; }
        public Vector3 localColliderPosition { get; private set; }
        public Vector3 localHitNormal { get; private set; }
        public bool affectingRotation { get; private set; }
        
        private ConfigurableJoint joint;
        private Collider collider;
        private Quaternion savedQuaternion;
        private Animator handDisplayAnimator;
        private Transform handTransform;
        private float distance;
        private Kobold owner;
        private Vector3 bodyAnchor;
        private Transform view;
        private bool frozen;
        private AudioPack unfreezePack;
        private Kobold targetKobold;
        private Quaternion startRotation;

        public Collider GetCollider() {
            return collider;
        }

        private ConfigurableJoint AddJoint(Vector3 worldPosition, Quaternion targetRotation, bool affRotation) {
            startRotation = body.rotation;
            ConfigurableJoint configurableJoint = body.gameObject.AddComponent<ConfigurableJoint>();
            configurableJoint.axis = Vector3.up;
            configurableJoint.secondaryAxis = Vector3.right;
            configurableJoint.connectedBody = null;
            configurableJoint.autoConfigureConnectedAnchor = false;
            if (owner.photonView.IsMine) {
                configurableJoint.breakForce = breakForce;
            } else {
                configurableJoint.breakForce = float.MaxValue;
            }
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
            slerpDrive.positionSpring = springForce;
            slerpDrive.positionDamper = 2f;
            configurableJoint.slerpDrive = slerpDrive;
            configurableJoint.massScale = 1f;
            configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
            configurableJoint.projectionAngle = 10f;
            configurableJoint.projectionDistance = 0.5f;
            configurableJoint.connectedMassScale = 1f;
            configurableJoint.enablePreprocessing = false;
            configurableJoint.configuredInWorldSpace = true;
            configurableJoint.xMotion = ConfigurableJointMotion.Limited;
            configurableJoint.yMotion = ConfigurableJointMotion.Limited;
            configurableJoint.zMotion = ConfigurableJointMotion.Limited;
            configurableJoint.anchor = body.transform.InverseTransformPoint(collider.transform.TransformPoint(localColliderPosition));
            configurableJoint.configuredInWorldSpace = true;
            configurableJoint.connectedBody = null;
            configurableJoint.connectedAnchor = worldPosition;
            if (affRotation) {
                configurableJoint.SetTargetRotation(targetRotation, startRotation);
                configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
                configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
                configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
            }
            return configurableJoint;
        }
        public Grab(Kobold owner, GameObject handDisplayPrefab, Transform view, Collider collider,
            Vector3 localColliderPosition, Vector3 localHitNormal, AudioPack unfreezePack) {
            this.collider = collider;
            this.localColliderPosition = localColliderPosition;
            this.localHitNormal = localHitNormal;
            body = collider.GetComponentInParent<Rigidbody>();
            savedQuaternion = body.rotation;
            Vector3 hitPosWorld = collider.transform.TransformPoint(localColliderPosition);
            bodyAnchor = body.transform.InverseTransformPoint(hitPosWorld);
            distance = Vector3.Distance(view.position, hitPosWorld);
            handDisplayAnimator = GameObject.Instantiate(handDisplayPrefab, owner.transform)
                .GetComponentInChildren<Animator>();
            handDisplayAnimator.gameObject.SetActive(true);
            handDisplayAnimator.SetBool(GrabbingHash, true);
            handTransform = handDisplayAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            photonView = collider.GetComponentInParent<PhotonView>();
            this.owner = owner;
            this.view = view;
            this.unfreezePack = unfreezePack;
            frozen = false;
            targetKobold = collider.GetComponentInParent<Kobold>();
            if (targetKobold != null) {
                targetKobold.ragdoller.PushRagdoll();
                body.maxAngularVelocity = 10f;
            } else {
                body.maxAngularVelocity = 20f;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            joint = AddJoint(hitPosWorld, Quaternion.identity, false);
            if (owner.photonView.IsMine) {
                photonView.RequestOwnership();
            }
        }
        
        public Grab(Kobold owner, GameObject handDisplayPrefab, Collider collider, Vector3 localColliderPosition,
            Vector3 localHitNormal, Vector3 worldAnchor, Quaternion rotation, bool affRotation, AudioPack unfreezePack) {
            this.collider = collider;
            this.localColliderPosition = localColliderPosition;
            this.localHitNormal = localHitNormal;
            savedQuaternion = rotation;
            body = collider.GetComponentInParent<Rigidbody>();
            photonView = collider.GetComponentInParent<PhotonView>();
            Vector3 hitPosWorld = collider.transform.TransformPoint(localColliderPosition);
            bodyAnchor = body.transform.InverseTransformPoint(hitPosWorld);
            handDisplayAnimator = GameObject.Instantiate(handDisplayPrefab, owner.transform)
                .GetComponentInChildren<Animator>();
            handDisplayAnimator.gameObject.SetActive(true);
            handDisplayAnimator.SetBool(GrabbingHash, true);
            handTransform = handDisplayAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            this.owner = owner;
            this.unfreezePack = unfreezePack;
            frozen = true;
            joint = AddJoint(worldAnchor, rotation, affRotation);
            targetKobold = collider.GetComponentInParent<Kobold>();
            if (targetKobold != null) {
                targetKobold.ragdoller.PushRagdoll();
                body.maxAngularVelocity = 10f;
            } else {
                body.maxAngularVelocity = 20f;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            if (owner.photonView.IsMine) {
                photonView.RequestOwnership();
            }
        }

        public float GetDistance() => distance;
        public Quaternion GetRotation() => savedQuaternion;

        public void Freeze() {
            if (frozen) {
                return;
            }
            frozen = true;
            if (joint != null) {
                Destroy(joint);
            }
            joint = AddJoint(collider.transform.TransformPoint(localColliderPosition), savedQuaternion, affectingRotation);
        }

        public Vector3 GetWorldPosition() {
            if (collider != null) {
                return collider.transform.TransformPoint(localColliderPosition);
            } else {
                return handTransform.transform.position;
            }
        }

        public bool Valid() {
            bool valid = body != null && owner != null && photonView != null && joint != null;
            return valid;
        }
        public void LateUpdate() {
            Vector3 worldNormal = collider.transform.TransformDirection(localHitNormal);
            Vector3 worldPoint = collider.transform.TransformPoint(localColliderPosition);
            handTransform.rotation = Quaternion.LookRotation(-worldNormal, Vector3.up) * Quaternion.AngleAxis(90f, Vector3.up);
            handTransform.position = worldPoint + handTransform.rotation * (Vector3.down*0.1f);
        }

        public void SetRotation(Quaternion rot) {
            savedQuaternion = rot;
            if (!affectingRotation) {
                affectingRotation = true;
            }
        }

        public void SetDistance(float dist) {
            distance = dist;
        }

        public void Rotate(Vector2 delta) {
            if (!affectingRotation) {
                affectingRotation = true;
                savedQuaternion = body.rotation;
            }
            savedQuaternion = Quaternion.AngleAxis(-delta.x, view.up)*savedQuaternion;
            savedQuaternion = Quaternion.AngleAxis(delta.y, view.right)*savedQuaternion;
        }

        public void AdjustDistance(float delta) {
            distance += delta;
            distance = Mathf.Max(distance, 0f);
        }

        public void FixedUpdate() {
            if (frozen) {
                return;
            }

            Vector3 holdPoint = view.position + view.forward * distance;

            if (joint != null) {
                joint.connectedAnchor = holdPoint;
                if (affectingRotation) {
                    joint.SetTargetRotation(savedQuaternion, startRotation);
                }
            }

            // Manual axis alignment, for pole jumps!
            if (!body.transform.IsChildOf(owner.body.transform) && !body.isKinematic) {
                body.velocity -= body.velocity * 0.5f;
                Vector3 axis = view.forward;
                Vector3 jointPos = body.transform.TransformPoint(bodyAnchor);
                Vector3 center = (view.position + jointPos) / 2f;
                Vector3 wantedPosition1 = center - axis * distance / 2f;
                //Vector3 wantedPosition2 = center + axis * distance / 2f;
                float ratio = Mathf.Clamp((body.mass / owner.body.mass), 0.75f, 1.25f);
                Vector3 force = (wantedPosition1 - view.position) * (springForce * 10f);
                owner.body.AddForce(force * ratio);
                //body.AddForce(-force * (1f / ratio));
            }
        }

        public void Release() {
            GameManager.instance.SpawnAudioClipInWorld(unfreezePack, GetWorldPosition());
            if (joint != null) {
                Destroy(joint);
            }
            if (targetKobold != null) {
                targetKobold.ragdoller.PopRagdoll();
            }
            Destroy(handDisplayAnimator.gameObject);
            if (body != null) {
                body.collisionDetectionMode = CollisionDetectionMode.Discrete;
                body.interpolation = RigidbodyInterpolation.None;
                body.maxAngularVelocity = Physics.defaultMaxAngularSpeed;
            }
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

    private void Start() {
        kobold.genesChanged += OnGenesChanged;
        OnGenesChanged(kobold.GetGenes());
        removeIds = new List<Grab>();
    }
    private void OnDestroy() {
        if (kobold != null) {
            kobold.genesChanged -= OnGenesChanged;
        }
        TryDrop();
        UnfreezeAll();
    }

    private void OnGenesChanged(KoboldGenes newGenes) {
        if (newGenes == null) {
            return;
        }

        Vector4 hbcs = new Vector4(newGenes.hue/255f, newGenes.brightness/255f, 0.5f, newGenes.saturation/255f);
        // Set color
        foreach (Renderer r in previewHandAnimator.GetComponentsInChildren<Renderer>()) {
            if (r == null) {
                continue;
            }
            foreach (Material m in r.materials) {
                m.SetVector(BrightnessContrastSaturation, hbcs);
            }
        }
    }

    private bool TryRaycastGrab(float maxDistance, out RaycastHit? previewHit) {
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
        if (previewGrab && currentGrab == null && TryRaycastGrab(maxGrabDistance, out RaycastHit? previewHit)) {
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

    public bool HasGrab() {
        return currentGrab != null;
    }

    public bool TryRotate(Vector2 delta) {
        if (currentGrab == null) {
            return false;
        }
        currentGrab.Rotate(delta);
        return true;
    }
    
    public bool TryAdjustDistance(float delta) {
        if (currentGrab == null) {
            return false;
        }
        currentGrab.AdjustDistance(delta);
        return true;
    }

    [PunRPC]
    private void GrabRPC(int viewID, int colliderNum, Vector3 localHit, Vector3 localHitNormal) {
        PhotonView otherPhotonView = PhotonNetwork.GetPhotonView(viewID);
        if (otherPhotonView == null) {
            return;
        }
        Collider[] colliders = otherPhotonView.GetComponentsInChildren<Collider>();
        currentGrab = new Grab(kobold, previewHandAnimator.gameObject, view, colliders[colliderNum], localHit, localHitNormal, unfreezeSound);
    }

    public void TryGrab() {
        if (currentGrab != null || !photonView.IsMine) {
            return;
        }
        if (!TryRaycastGrab(maxGrabDistance, out RaycastHit? hitTest)) {
            return;
        }

        RaycastHit hit = hitTest.Value;
        Vector3 localHit = hit.collider.transform.InverseTransformPoint(hit.point);
        Vector3 localHitNormal = hit.collider.transform.InverseTransformDirection(hit.normal);

        PhotonView otherView = hit.collider.GetComponentInParent<PhotonView>();
        Collider[] colliders = otherView.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            if (colliders[i] == hit.collider) {
                photonView.RPC(nameof(GrabRPC), RpcTarget.All,  otherView.ViewID, i, localHit, localHitNormal);
                break;
            }
        }
    }

    [PunRPC]
    private void FreezeRPC(int grabViewID, int colliderNum, Vector3 localColliderPosition, Vector3 localHitNormal, Vector3 worldAnchor, Quaternion rotation, bool affRotation) {
        PhotonView grabView = PhotonNetwork.GetPhotonView(grabViewID);
        Collider[] colliders = grabView.GetComponentsInChildren<Collider>();
        frozenGrabs.Add(new Grab(kobold, previewHandAnimator.gameObject, colliders[colliderNum], localColliderPosition, localHitNormal,
            worldAnchor, rotation, affRotation, unfreezeSound));
        Destroy(GameObject.Instantiate(freezeVFXPrefab, worldAnchor, Quaternion.identity), 5f);
    }

    public bool TryFreeze() {
        if (currentGrab == null || !photonView.IsMine) {
            return false;
        }
        // Tell everyone we've made a frozen joint here.
        Collider[] colliders = currentGrab.photonView.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            if (currentGrab.GetCollider() == colliders[i]) {
                photonView.RPC(nameof(FreezeRPC), RpcTarget.All, currentGrab.photonView.ViewID,
                    i, currentGrab.localColliderPosition, currentGrab.localHitNormal, currentGrab.GetWorldPosition(),
                    currentGrab.body.rotation, currentGrab.affectingRotation);
                TryDrop();
                return true;
            }
        }

        return false; // Should never happen
    }

    [PunRPC]
    private void UnfreezeRPC(int viewID, int rigidbodyID) {
        PhotonView checkView = PhotonNetwork.GetPhotonView(viewID);
        Rigidbody[] bodies = checkView.GetComponentsInChildren<Rigidbody>();
        for(int i=0;i<frozenGrabs.Count;i++) {
            Grab frozenGrab = frozenGrabs[i];
            if (frozenGrab.body == bodies[rigidbodyID]) {
                frozenGrab.Release();
                frozenGrabs.RemoveAt(i--);
            }
        }
    }

    [PunRPC]
    private void UnfreezeAllRPC() {
        foreach (var frozenGrab in frozenGrabs) {
            frozenGrab.Release();
        }
        frozenGrabs.Clear();
    }

    public bool TryUnfreeze() {
        if (!photonView.IsMine) {
            return false;
        }

        if (!TryRaycastGrab(100f, out RaycastHit? testHit)) {
            return false;
        }

        bool foundGrabs = false;
        RaycastHit hit = testHit.Value;
        hit.collider.GetComponentInParent<PhotonView>();
        for(int i=0;i<frozenGrabs.Count;i++) {
            Grab frozenGrab = frozenGrabs[i];
            if (frozenGrab.body == hit.rigidbody) {
                frozenGrab.Release();
                frozenGrabs.RemoveAt(i--);
                foundGrabs = true;
            }
        }

        return foundGrabs;
    }

    public void UnfreezeAll() {
        if (frozenGrabs.Count > 0 && photonView.IsMine) {
            UnfreezeAllRPC();
            photonView.RPC(nameof(UnfreezeAllRPC), RpcTarget.Others);
        }
    }

    [PunRPC]
    private void DropRPC() {
        currentGrab?.Release();
        currentGrab = null;
    }
    
    public void TryDrop() {
        if (currentGrab != null && photonView.IsMine) {
            photonView.RPC(nameof(DropRPC), RpcTarget.All);
            return;
        }
        
        currentGrab?.Release();
        currentGrab = null;
    }

    private void LateUpdate() {
        DoPreview();
        Validate();
        currentGrab?.LateUpdate();
        foreach (var f in frozenGrabs) {
            f.LateUpdate();
        }
    }

    private void Validate() {
        if (currentGrab != null && !currentGrab.Valid()) {
            TryDrop();
        }

        removeIds.Clear();
        foreach (var f in frozenGrabs) {
            if (!f.Valid()) {
                removeIds.Add(f);
            }
        }

        if (photonView.IsMine) {
            foreach (Grab fgrab in removeIds) {
                if (fgrab.photonView == null || !fgrab.photonView.IsMine) {
                    fgrab.Release();
                    frozenGrabs.Remove(fgrab);
                    continue;
                }
                Rigidbody[] bodies = fgrab.photonView.GetComponentsInChildren<Rigidbody>();
                for (int i = 0; i < bodies.Length; i++) {
                    if (bodies[i] == fgrab.body) {
                        photonView.RPC(nameof(UnfreezeRPC), RpcTarget.All, fgrab.photonView.ViewID, i);
                        break;
                    }
                }
            }
        }

        removeIds.Clear();
    }


    private void FixedUpdate() {
        Validate();
        currentGrab?.FixedUpdate();
        foreach (var grab in frozenGrabs) {
            grab.FixedUpdate();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            if (currentGrab != null) {
                stream.SendNext(currentGrab.GetRotation());
                stream.SendNext(currentGrab.GetDistance());
            } else {
                stream.SendNext(Quaternion.identity);
                stream.SendNext(2f);
            }
        } else {
            Quaternion rot = (Quaternion)stream.ReceiveNext();
            float dist  = (float)stream.ReceiveNext();
            if (currentGrab != null) {
                currentGrab.SetRotation(rot);
                currentGrab.SetDistance(dist);
            }
        }
    }

    public void Save(BinaryWriter writer, string version) {
    }

    public void Load(BinaryReader reader, string version) {
    }
}
