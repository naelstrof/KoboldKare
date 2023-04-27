using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviourPun {
    private Kobold player;
    private int maxGrabCount = 1;
    private Rigidbody body;
    [SerializeField]
    private float springStrength = 200f;
    [SerializeField][Range(0f,100f)]
    private float dampingStrength = 30f;
    [SerializeField]
    private Vector3 defaultOffset = Vector3.forward;

    public delegate void UIAction(bool active);

    public event UIAction activateUIChanged;
    public event UIAction throwUIChanged;

    //[SerializeField] private GameObject activateUI;
    //[SerializeField] private GameObject throwUI;
    private ColliderSorter sorter;

    private bool activating;
    private class GrabInfo {
        public IGrabbable grabbable { get; private set; }
        public Rigidbody body { get; private set; }
        public CollisionDetectionMode collisionDetectionMode { get; private set; }
        public RigidbodyInterpolation interpolation { get; private set; }
        public Kobold kobold { get; private set; }
        public Kobold owner { get; private set; }
        private DriverConstraint driverConstraint;
        private ConfigurableJoint joint;
        private float springStrength;
        private float dampingStrength;
        private bool valid = true;
        private float grabTime;
        public GenericWeapon weapon { get; private set; }
        private void RecursiveSetLayer(Transform t, int fromLayer, int toLayer) {
            for(int i=0;i<t.childCount;i++ ) {
                RecursiveSetLayer(t.GetChild(i), fromLayer, toLayer);
            }
            if (t.gameObject.layer == fromLayer) {
                t.gameObject.layer = toLayer;
            }
        }
        public GrabInfo(Kobold owner, IGrabbable grabbable, float springStrength, float dampingStrength, Vector3 viewPos, Quaternion viewRot, Vector3 offset) {
            grabTime = Time.time;
            this.owner = owner;
            this.grabbable = grabbable;
            this.springStrength = springStrength;
            this.dampingStrength = dampingStrength;
            body = grabbable.transform.GetComponentInParent<Rigidbody>();
            if (body == null) {
                valid = false;
                return;
            }

            collisionDetectionMode = body.collisionDetectionMode;
            interpolation = body.interpolation;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            driverConstraint = body.gameObject.AddComponent<DriverConstraint>();
            driverConstraint.springStrength = this.springStrength;
            driverConstraint.body = body;
            driverConstraint.dampingStrength = this.dampingStrength;
            driverConstraint.SetWorldAnchor(viewPos+viewRot*offset);
            driverConstraint.SetWorldAnchor(viewPos+viewRot*offset);
            grabbable.photonView.RequestOwnership();
            weapon = grabbable.transform.GetComponentInParent<GenericWeapon>();
            if (weapon != null) {
                driverConstraint.angleSpringStrength = 32f;
                driverConstraint.SetWorldAnchor(viewPos+viewRot*(weapon.GetWeaponHoldPosition()+offset));
            }

            kobold = grabbable.transform.GetComponentInParent<Kobold>();
            if (kobold != null) {
                Rigidbody ragdollBody = kobold.GetComponent<Ragdoller>().GetHip();
                joint = AddJoint(ragdollBody, ragdollBody.position);
            }
            RecursiveSetLayer(body.transform, LayerMask.NameToLayer("UsablePickups"), LayerMask.NameToLayer("PlayerNocollide"));
            valid = true;
        }

        public bool Valid() {
            bool v = ((Component)grabbable)!=null && driverConstraint != null && valid;
            if (v && Time.time - grabTime > 2f) {
                v &= grabbable.photonView.IsMine;
            }

            return v;
        }

        public void Activate() {
            if (weapon != null) {
                weapon.OnFire(owner);
            } else {
                body.velocity += OrbitCamera.GetPlayerIntendedRotation()* Vector3.forward * 10f;
                Release();
            }
        }
        public void StopActivate() {
            if (weapon != null) {
                weapon.OnEndFire(owner);
            }
        }

        public void Release() {
            if (!valid) {
                return;
            }
            
            if (driverConstraint != null) {
                Destroy(driverConstraint);
            }

            if (joint != null) {
                Destroy(joint);
            }

            if (((Component)grabbable) != null && body != null) {
                grabbable.photonView.RPC(nameof(IGrabbable.OnReleaseRPC), RpcTarget.All, owner.photonView.ViewID, body.velocity);
                RecursiveSetLayer(body.transform, LayerMask.NameToLayer("PlayerNocollide"),
                    LayerMask.NameToLayer("UsablePickups"));
                if (kobold != null && owner != null) {
                    owner.GetComponent<Grabber>().AddGivebackKobold(kobold);
                }
            }
            
            valid = false;
        }

        public void Set(Vector3 position, Quaternion viewRot, Vector3 offset) {
            driverConstraint.anchor = body.transform.InverseTransformPoint(grabbable.GrabTransform().position);
            if (weapon != null) {
                Quaternion fq = Quaternion.FromToRotation(weapon.GetWeaponBarrelTransform().forward, viewRot*Vector3.forward)*Quaternion.FromToRotation(weapon.GetWeaponBarrelTransform().up, viewRot*Vector3.up);
                driverConstraint.forwardVector = fq * body.transform.forward;
                driverConstraint.upVector = fq * body.transform.up;
                driverConstraint.SetWorldAnchor(position+viewRot*(weapon.GetWeaponHoldPosition()+offset));
            } else {
                driverConstraint.SetWorldAnchor(position+viewRot*offset);
            }

            if (joint != null) {
                joint.connectedAnchor = position+viewRot*offset;
            }
        }
        
        private ConfigurableJoint AddJoint(Rigidbody targetBody, Vector3 worldPosition) {
            ConfigurableJoint configurableJoint = targetBody.gameObject.AddComponent<ConfigurableJoint>();
            configurableJoint.axis = Vector3.up;
            configurableJoint.secondaryAxis = Vector3.right;
            configurableJoint.connectedBody = null;
            configurableJoint.autoConfigureConnectedAnchor = false;
            configurableJoint.breakForce = float.MaxValue;
            JointDrive drive = configurableJoint.xDrive;
            drive.positionSpring = springStrength*30f;
            drive.positionDamper = 1f;
            configurableJoint.xDrive = drive;
            configurableJoint.yDrive = drive;
            configurableJoint.zDrive = drive;
            var linearLimit = configurableJoint.linearLimit;
            linearLimit.limit = 0.1f;
            linearLimit.bounciness = 0f;
            var spring = configurableJoint.linearLimitSpring;
            spring.spring = springStrength*30f;
            spring.damper = 1f;
            configurableJoint.linearLimitSpring = spring;
            configurableJoint.linearLimit = linearLimit;
            configurableJoint.rotationDriveMode = RotationDriveMode.Slerp;
            configurableJoint.massScale = 1f;
            configurableJoint.projectionMode = JointProjectionMode.None;
            configurableJoint.projectionAngle = 10f;
            configurableJoint.projectionDistance = 0.5f;
            configurableJoint.connectedMassScale = 1f;
            configurableJoint.enablePreprocessing = false;
            configurableJoint.configuredInWorldSpace = true;
            configurableJoint.anchor = Vector3.zero;
            configurableJoint.connectedBody = null;
            configurableJoint.connectedAnchor = worldPosition;
            configurableJoint.xMotion = ConfigurableJointMotion.Limited;
            configurableJoint.yMotion = ConfigurableJointMotion.Limited;
            configurableJoint.zMotion = ConfigurableJointMotion.Limited;
            return configurableJoint;
        }
    }
    private List<GrabInfo> grabbedObjects;

    private class GiveBackKobold {
        public Coroutine routine;
        public Kobold kobold;
    }

    private List<GiveBackKobold> giveBackKobolds;
    //private float thrownUntouchableTime = 0.4f;
    private static Collider[] colliders = new Collider[32];
    
    [SerializeField]
    private Transform view;

    public void SetView(Transform newView) {
        view = newView;
    }

    public void OnDestroy() {
        TryDrop();
    }

    public void SetMaxGrabCount(int count) {
        maxGrabCount = count;
    }
    public void Validate() {
        bool canActivate = false;
        bool canThrow = false;
        for (int i = 0; i < grabbedObjects.Count; i++) {
            if (grabbedObjects[i] == null || !grabbedObjects[i].Valid()) {
                grabbedObjects[i]?.Release();
                grabbedObjects.RemoveAt(i--);
                continue;
            }
            
            if (grabbedObjects[i].weapon != null) {
                canActivate = true;
            }

            if (grabbedObjects[i].body != null) {
                canThrow = true;
            }

        }

        activateUIChanged?.Invoke(canActivate);
        if (!canActivate) {
            throwUIChanged?.Invoke(canThrow);
        } else {
            throwUIChanged?.Invoke(false);
        }
    }

    private IEnumerator GiveBackKoboldAfterDelay(GiveBackKobold giveBackKobold) {
        while (giveBackKobold.kobold.photonView.IsMine) {
            if (giveBackKobold.kobold.GetComponent<Ragdoller>().ragdolled) {
                yield return new WaitForSeconds(5f);
            } else {
                yield return new WaitForSeconds(0.25f);
            }

            if (giveBackKobold.kobold.photonView.IsMine) {
                foreach (Player p in PhotonNetwork.PlayerList) {
                    if (giveBackKobold.kobold.photonView.IsMine && (Kobold)p.TagObject == giveBackKobold.kobold) {
                        giveBackKobold.kobold.photonView.TransferOwnership(p);
                        break;
                    }
                }
            }
        }
        giveBackKobolds.Remove(giveBackKobold);
    }

    public void AddGivebackKobold(Kobold other) {
        foreach (Player p in PhotonNetwork.PlayerList) {
            if ((Kobold)p.TagObject == other) {
                GiveBackKobold giveBackKobold = new GiveBackKobold(){kobold = other};
                giveBackKobold.routine = StartCoroutine(GiveBackKoboldAfterDelay(giveBackKobold));
                giveBackKobolds.Add(giveBackKobold);
                break;
            }
        }
    }

    private void RemoveGivebackKobold(Kobold other) {
        if (other!= null) {
            for (int j = 0; j < giveBackKobolds.Count; j++) {
                if (giveBackKobolds[j].kobold == other) {
                    if (giveBackKobolds[j].routine != null) {
                        StopCoroutine(giveBackKobolds[j].routine);
                    }
                    giveBackKobolds.RemoveAt(j--);
                }
            }
        }
    }

    public void TryDrop() {
        foreach (var grab in grabbedObjects) {
            grab.Release();
        }

        grabbedObjects.Clear();
    }

    private void Awake() {
        grabbedObjects = new List<GrabInfo>();
        giveBackKobolds = new List<GiveBackKobold>();
        sorter = new ColliderSorter();
        player = GetComponent<Kobold>();
    }

    private void GetForwardAndUpVectors(GenericWeapon[] weapons, out Vector3 averageForward, out Vector3 averageUp, out Vector3 averageOffset) {
        averageForward = Vector3.zero;
        averageUp = Vector3.zero;
        averageOffset = Vector3.zero;
        foreach (GenericWeapon w in weapons) {
            averageForward += w.GetWeaponBarrelTransform().forward;
            averageOffset += w.GetWeaponHoldPosition();
            averageUp += w.GetWeaponBarrelTransform().up;
        }
        averageForward /= weapons.Length;
        averageOffset /= weapons.Length;
        averageUp /= weapons.Length;
        averageForward = Vector3.Normalize(averageForward);
        averageUp = Vector3.Normalize(averageUp);
    }

    private class ColliderSorter : IComparer<Collider> {
        private Ray internalRay;
        private const int checkCount = 4;
        public void SetRay(Ray ray) {
            internalRay = ray;
        }

        public int Compare(Collider x, Collider y) {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            float closestX = float.MaxValue;
            float closestY = float.MaxValue;
            if (x is MeshCollider { convex: false }) {
                return -1;
            }
            if (y is MeshCollider { convex: false }) {
                return 1;
            }

            for (int i = 0; i < checkCount; i++) {
                float t = (float)i / (float)checkCount;
                Vector3 checkPoint = internalRay.GetPoint(t);
                closestX = Mathf.Min(closestX, Vector3.Distance(checkPoint, x.ClosestPoint(checkPoint)));
                closestY = Mathf.Min(closestY, Vector3.Distance(checkPoint,y.ClosestPoint(checkPoint)));
            }
            return closestX.CompareTo(closestY);
        }
    }

    private Vector3 GetViewPos() {
        if (photonView.IsMine) {
            return view.position;
        }
        float distance = Vector3.Distance(view.position, OrbitCamera.GetPlayerIntendedPosition());
        Vector3 viewPos = Vector3.MoveTowards(OrbitCamera.GetPlayerIntendedPosition(), view.position, Mathf.Max(distance - 1f, 0f));
        return viewPos;
    }

    public void TryGrab(bool multiGrabMode) {
        int maxGrabCountLocal = maxGrabCount;
        if (!multiGrabMode) {
            maxGrabCountLocal = 1;
        }
        if (grabbedObjects.Count >= maxGrabCountLocal) {
            return;
        }

        var position = GetViewPos()+OrbitCamera.GetPlayerIntendedRotation()*defaultOffset;
        int hits = Physics.OverlapSphereNonAlloc(position, 1f, colliders);
        sorter.SetRay(new Ray(position, OrbitCamera.GetPlayerIntendedRotation()*Vector3.forward));
        System.Array.Sort(colliders, 0, hits, sorter);
        for (int i = 0; i < hits; i++) {
            IGrabbable grabbable = colliders[i].GetComponentInParent<IGrabbable>();
            if (grabbable == null || grabbable == (IGrabbable)player) {
                continue;
            }
            bool contains = false;
            foreach (GrabInfo grabInfo in grabbedObjects) {
                if (grabInfo.grabbable == grabbable) {
                    contains = true;
                    break;
                }
            }

            if (contains) {
                continue;
            }

            if (grabbable.CanGrab(player)) {
                grabbable.photonView.RPC(nameof(IGrabbable.OnGrabRPC), RpcTarget.All, photonView.ViewID);
                GrabInfo info = new GrabInfo(player, grabbable, springStrength, dampingStrength, GetViewPos(),OrbitCamera.GetPlayerIntendedRotation(), defaultOffset);
                // Destroyed on grab, creatures gib on grab.
                if (!info.Valid()) {
                    return;
                }

                grabbedObjects.Add(info);
                RemoveGivebackKobold(info.kobold);
            }
            if (grabbedObjects.Count >= maxGrabCountLocal) {
                return;
            }
        }
    }

    public void LateUpdate() {
        Validate();
        foreach (var grab in grabbedObjects) {
            grab.Set(GetViewPos(), OrbitCamera.GetPlayerIntendedRotation(), defaultOffset);
        }
    }
    public void TryStopActivate() {
        Validate();
        if (!activating) {
            return;
        }
        activating = false;
        foreach (var t in grabbedObjects) {
            t.StopActivate();
        }
    }
    public void TryActivate() {
        Validate();
        if (activating) {
            return;
        }
        activating = true;
        foreach (var t in grabbedObjects) {
            t.Activate();
        }
    }
}
