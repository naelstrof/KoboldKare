using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class Grabber : MonoBehaviourPun, IPunObservable, ISavable {
    public GameObject player;
    public int maxGrabCount = 1;
    public Rigidbody body;
    public float springStrength = 1000f;
    [Range(0f,0.5f)]
    public float dampingStrength = 0.1f;
    public float throwStrength = 800f;
    private Kobold internalKobold;
    public Kobold kobold {
        get {
            if (internalKobold == null) {
                internalKobold = GetComponentInParent<Kobold>();
            }
            return internalKobold;
        }
    }
    private HashSet<IGrabbable> intersectingGameObjects = new HashSet<IGrabbable>();
    private HashSet<IGrabbable> removeLater = new HashSet<IGrabbable>();
    private HashSet<IGrabbable> grabbedObjects = new HashSet<IGrabbable>();
    private HashSet<IGrabbable> thrownObjects = new HashSet<IGrabbable>();
    private HashSet<IGrabbable> droppedObjects = new HashSet<IGrabbable>();
    private class JointInfo  {
        public IGrabbable grabbable;
        public Rigidbody body;
        public DriverConstraint constraint;
    }
    private List<JointInfo> joints = new List<JointInfo>();
    public float droppedHighQualityTime = 10.0f;
    public float thrownUntouchableTime = 0.4f;
    private List<Vector3> weaponPoints = new List<Vector3>();
    public float weaponSeparation = 1f;
    public Transform cam;
    public UnityEvent OnEnterGrabbable;
    public UnityEvent OnExitGrabbable;
    public UnityEvent OnGrab;
    public UnityEvent OnDrop;
    public UnityEvent OnGrabThrowable;
    public UnityEvent OnGrabActivatable;
    public UnityEvent OnActivate;
    private bool hasDropped = false;
    [HideInInspector]
    public bool grabbing = false;
    [HideInInspector]
    public bool activating = false;

    private class RigidbodyMemory {
        public CollisionDetectionMode collision;
        public RigidbodyInterpolation interpolation;
    }

    private Dictionary<Rigidbody, RigidbodyMemory> highQualityRigidbodies;

    public void OnDestroy() {
        TryDrop();
    }
    
    public void SetMaxGrabCount( float grabCount ) {
        maxGrabCount = Mathf.CeilToInt(grabCount);
    }
    public IEnumerator WaitAndClearThrown(float time) {
        yield return new WaitForSeconds(time);
        thrownObjects.Clear();
    }
    public void Validate() {
        grabbedObjects.RemoveWhere(o => ((Component)o) == null);
        intersectingGameObjects.RemoveWhere(o => ((Component)o) == null);

        // Validate joints
        for( int i=0;i<joints.Count;i++) {
            JointInfo info = joints[i];
            if (info.body == null || ((Component)info.grabbable) == null || info.constraint == null) {
                TryDrop(info.grabbable);
            }
        }

        // Validate GameObjects
        thrownObjects.RemoveWhere(o => ((Component)o)== null);
        //intersectingGameObjects.RemoveWhere(o => ((Component)o)== null);
        droppedObjects.RemoveWhere(o => ((Component)o)== null);
    }

    private IEnumerator WaitAndClearHighQualityRigidbodies() {
        yield return new WaitForSeconds(10f);
        foreach (var pair in highQualityRigidbodies) {
            if (pair.Key != null) {
                pair.Key.interpolation = pair.Value.interpolation;
                pair.Key.collisionDetectionMode = pair.Value.collision;
            }
        }

        highQualityRigidbodies.Clear();
    }

    private void TryDrop(IGrabbable g) {
        if( ((Component)g) == null ) {
            return;
        }
        intersectingGameObjects.Remove(g);
        grabbedObjects.Remove(g);
        try {
            if (g.transform == null) {
                return;
            }
        } catch {
            return;
        }
        TryStopActivate(g);
        JointInfo info = joints.Find(i=> i.grabbable == g);
        while (info != null) {
            if (info.constraint != null) {
                Destroy(info.constraint);
            }
            joints.Remove(info);
            info = joints.Find(i=>i.grabbable == g);
        }
        //RecursiveSetLayer(g.transform, LayerMask.NameToLayer("Effects"), LayerMask.NameToLayer("Pickups"));
        g.OnRelease(kobold);
        foreach (Renderer ren in g.GetRenderers()) {
            if (ren == null) {
                continue;
            }
        }
        if (g.GetRigidBodies()[0] != null) {
            RecursiveSetLayer(g.GetRigidBodies()[0].transform.root, LayerMask.NameToLayer("PlayerNocollide"), LayerMask.NameToLayer("UsablePickups"));
        }
        droppedObjects.Add(g);
        //if (player.GetComponent<KoboldCharacterController>().groundRigidbody != null) {
            //StartCoroutine(WaitAndClearThrown(thrownUntouchableTime));
        //}
        StopCoroutine(nameof(WaitAndClearHighQualityRigidbodies));
        StartCoroutine(nameof(WaitAndClearHighQualityRigidbodies));
        if (grabbedObjects.Count <= 0) {
            grabbing = false;
        }
    }
    public void TryDrop() {
        if (!hasDropped) {
            OnDrop.Invoke();
            hasDropped = true;
        }
        if (grabbedObjects.Count <= 0 ) {
            return;
        }
        Validate();
        weaponPoints.Clear();
        HashSet<IGrabbable> copy = new HashSet<IGrabbable>(grabbedObjects);
        foreach( IGrabbable g in copy ) {
            TryDrop(g);
        }
        grabbedObjects.Clear();
        intersectingGameObjects.Clear();
        grabbing = false;
    }
    private void RecursiveSetLayer(Transform t, int fromLayer, int toLayer) {
        for(int i=0;i<t.childCount;i++ ) {
            RecursiveSetLayer(t.GetChild(i), fromLayer, toLayer);
        }
        if (t.gameObject.layer == fromLayer) {
            t.gameObject.layer = toLayer;
        }
    }

    private void Start() {
        highQualityRigidbodies = new Dictionary<Rigidbody, RigidbodyMemory>();
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
    public bool TryGrab(IGrabbable g) {
        if (grabbedObjects.Count >= maxGrabCount) {
            return false;
        }
        grabbedObjects.Add(g);
        Rigidbody firstBody = g.GetRigidBodies()[0];
        RecursiveSetLayer(firstBody.transform.root, LayerMask.NameToLayer("UsablePickups"), LayerMask.NameToLayer("PlayerNocollide"));
        if (!highQualityRigidbodies.ContainsKey(firstBody)) {
            highQualityRigidbodies.Add(firstBody,
                new RigidbodyMemory() { collision = firstBody.collisionDetectionMode, interpolation = firstBody.interpolation });
            firstBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            firstBody.interpolation = RigidbodyInterpolation.Interpolate;
        }
        foreach(Rigidbody r in g.GetRigidBodies()) {
            if (r == null ) {
                continue;
            }


            DriverConstraint j = r.gameObject.AddComponent<DriverConstraint>();
            j.springStrength = springStrength;
            j.connectedBody = body;
            j.dampingStrength = dampingStrength;
            j.softness = 1f;
            bool grabbed = g.OnGrab(kobold);
            if (!grabbed) {
                grabbedObjects.Remove(g);
                Destroy(j);
                return false;
            }
            j.anchor = r.transform.InverseTransformPoint(g.GrabTransform(r).position);
            j.connectedAnchor = Vector3.zero;
            GenericWeapon[] weapons = r.GetComponentsInChildren<GenericWeapon>();
            if (weapons.Length != 0) {
                OnGrabActivatable.Invoke();
                Vector3 averageForward;
                Vector3 averageUp;
                Vector3 averageOffset;
                GetForwardAndUpVectors(weapons, out averageForward, out averageUp, out averageOffset);
                //j.connectedAnchor = Vector3.down * 0.55f + Vector3.right * 0.80f + Vector3.back * 0.55f;
                j.connectedAnchor = averageOffset;
                Quaternion fq = Quaternion.FromToRotation(averageForward, r.transform.forward);
                Quaternion uq = Quaternion.FromToRotation(averageUp, r.transform.up);
                j.forwardVector = fq*body.transform.forward;
                j.upVector = uq*body.transform.up;
            } else {
                OnGrabThrowable.Invoke();
            }
            joints.Add(new JointInfo { body = r, grabbable = g, constraint = j});
        }
        return true;
    }
    public void TryGrab() {
        Validate();
        if (intersectingGameObjects.Count > 0 || grabbedObjects.Count > 0) {
            OnGrab.Invoke();
            hasDropped = false;
        }
        HashSet<IGrabbable> grabbables = (new HashSet<IGrabbable>(intersectingGameObjects));
        grabbables.ExceptWith(grabbedObjects);
        grabbables.ExceptWith(thrownObjects);
        List<IGrabbable> sortedGrabables = new List<IGrabbable>(grabbables);
        sortedGrabables.Sort((a, b) => Vector3.Distance(a.GrabTransform(a.GetRigidBodies()[0]).position, transform.position).CompareTo(Vector3.Distance(b.GrabTransform(b.GetRigidBodies()[0]).position, transform.position)));
        foreach (var t in sortedGrabables) {
            bool grabbed = TryGrab(t);
        }
        grabbing = grabbedObjects.Count>0;
    }
    public void FixedUpdate() {
        Validate();
        if (removeLater.Count > 0) {
            intersectingGameObjects.ExceptWith(removeLater);
            removeLater.Clear();
            if (intersectingGameObjects.Count == 0) {
                OnExitGrabbable.Invoke();
            }
        }

        if (grabbedObjects.Count <= 0) {
            return;
        }
        int weaponCount = 1;
        foreach (IGrabbable g in grabbedObjects) {
            foreach(Rigidbody r in g.GetRigidBodies()) {
                weaponCount += r.GetComponentsInChildren<GenericWeapon>().Length;
            }
        }
        while ( weaponPoints.Count <= weaponCount) {
            weaponPoints.Add(Random.insideUnitSphere);
        }
        for (int x = 0; x < weaponPoints.Count; x++) {
            for(int y = 0;y<weaponPoints.Count;y++) {
                if (x != y) {
                    Vector3 dir = Vector3.Normalize(weaponPoints[y] - weaponPoints[x]);
                    float dist = Vector3.Distance(weaponPoints[x], weaponPoints[y]) - weaponSeparation;
                    weaponPoints[x] += dir * dist * Time.deltaTime * 5f;
                }
            }
        }
        weaponPoints[0] = Vector3.zero;
        int currentWeapon = 0;
        RaycastHit hit;
        float weaponHitDistance = 100f;
        Vector3 hitPos = cam.position + cam.forward * weaponHitDistance;
        if ( weaponCount > 0 && Physics.Raycast(cam.position, cam.forward, out hit, 100f, GameManager.instance.waterSprayHitMask, QueryTriggerInteraction.Ignore)) {
            if (hit.transform.root != transform.root) {
                hitPos = hit.point;
            }
        }
        foreach (IGrabbable g in grabbedObjects) {
            foreach (Rigidbody r in g.GetRigidBodies()) {
                DriverConstraint j = joints.Find(info => info.body == r).constraint;
                j.anchor = r.transform.InverseTransformPoint(g.GrabTransform(r).position);
                GenericWeapon[] weapons = r.GetComponentsInChildren<GenericWeapon>();
                if (weapons.Length != 0) {
                    Vector3 averageForward;
                    Vector3 averageUp;
                    Vector3 averageOffset;
                    GetForwardAndUpVectors(weapons, out averageForward, out averageUp, out averageOffset);
                    //Debug.DrawLine(transform.position, transform.position + averageForward);
                    //Debug.DrawLine(transform.position, transform.position + averageUp);
                    if (j != null && j.body != null) {
                        Quaternion fq = Quaternion.FromToRotation(averageForward, Vector3.Normalize(hitPos-j.body.position))*Quaternion.FromToRotation(averageUp, body.transform.up);
                        //j.forwardVector = fq * Vector3.Normalize(body.transform.forward + body.transform.up * 0.85f - body.transform.right * 0.4f);
                        j.connectedAnchor = averageOffset + weaponPoints[currentWeapon];
                        j.angleSpringStrength = 32f;
                        j.angleDamping = 0.1f;
                        j.angleSpringSoftness = 60f;
                        j.forwardVector = fq*j.body.transform.forward;
                        //Debug.DrawLine(transform.position, transform.position + j.forwardVector, Color.red);
                        j.upVector = fq*j.body.transform.up;
                    }
                    currentWeapon++;
                }
            }
        }
        //while ( weaponPoints.Count > weaponCount) {
            //weaponPoints.RemoveAt(weaponPoints.Count-1);
        //}
    }
    private void TryStopActivate(IGrabbable g) {
        foreach (Rigidbody r in g.GetRigidBodies()) {
            if (r == null) {
                continue;
            }
            foreach (GenericWeapon w in r.GetComponentsInChildren<GenericWeapon>()) {
                w.OnEndFire(player);
            }
        }
    }
    public void TryStopActivate() {
        Validate();
        if (!activating) {
            return;
        }
        activating = false;
        foreach (IGrabbable g in grabbedObjects) {
            TryStopActivate(g);
        }
    }
    public void TryActivate() {
        Validate();
        if (activating) {
            return;
        }
        OnActivate.Invoke();
        activating = true;
        foreach( IGrabbable g in grabbedObjects ) {
            bool hasWeapon = false;
            foreach( Rigidbody r in g.GetRigidBodies() ) {
                if (r == null) { 
                    continue;
                }
                // Only throw non-weapons
                foreach(GenericWeapon w in r.GetComponentsInChildren<GenericWeapon>()) {
                    w.OnFire(player);
                    hasWeapon = true;
                }
                if (hasWeapon) {
                    continue;
                }
                //RecursiveSetLayer(g.transform, LayerMask.NameToLayer("Effects"), LayerMask.NameToLayer("Pickups"));
                r.velocity += transform.forward * throwStrength;
            }
            if (hasWeapon) {
                continue;
            }
            g.OnThrow(kobold);
            thrownObjects.Add(g);
            StartCoroutine(WaitAndClearThrown(thrownUntouchableTime));
        }
        HashSet<IGrabbable> removeObjects = new HashSet<IGrabbable>();
        foreach(IGrabbable g in grabbedObjects) {
            if (thrownObjects.Contains(g)) {
                removeObjects.Add(g);
            }
        }
        foreach(IGrabbable g in removeObjects) {
            TryDrop(g);
        }
        grabbedObjects.ExceptWith(thrownObjects);
    }
    public void OnTriggerEnter(Collider other) {
        //if (((1<<other.transform.root.gameObject.layer) & pickupLayers) == 0 || other.transform.root == transform.root) {
        if (other.transform.root == transform.root) {
            return;
        }
        if (intersectingGameObjects.Count == 0) {
            OnEnterGrabbable.Invoke();
        }
        intersectingGameObjects.Add(other.transform.GetComponentInParent<IGrabbable>());
    }
    public void OnTriggerStay(Collider other) {
        //if (((1<<other.transform.root.gameObject.layer) & pickupLayers) == 0 || other.transform.root == transform.root) {
        if (other.transform.root == transform.root) {
            return;
        }
        if (intersectingGameObjects.Count == 0) {
            OnEnterGrabbable.Invoke();
        }
        intersectingGameObjects.Add(other.transform.GetComponentInParent<IGrabbable>());
    }
    public void OnTriggerExit(Collider other) {
        //if (((1<<other.transform.root.gameObject.layer) & pickupLayers) == 0 || other.transform.root == transform.root) {
        if (other.transform.root == transform.root) {
            return;
        }
        removeLater.Add(other.transform.GetComponentInParent<IGrabbable>());
        //intersectingGameObjects.Remove(other.transform.root.GetComponent<IGrabbable>());
        //if (intersectingGameObjects.Count == 0) {
            //OnExitGrabbable.Invoke();
        //}
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(this.activating);
            stream.SendNext(this.grabbing);
        } else {
            if ((bool)stream.ReceiveNext()) {
                TryActivate();
            } else {
                TryStopActivate();
            }
            if ((bool)stream.ReceiveNext()) {
                TryGrab();
            } else {
                TryDrop();
            }
        }
    }
    public void Save(BinaryWriter writer, string version) {
        writer.Write(activating);
        writer.Write(grabbing);
    }

    public void Load(BinaryReader reader, string version) {
        if (reader.ReadBoolean()) {
            TryActivate();
        } else {
            TryStopActivate();
        }
        if (reader.ReadBoolean()) {
            TryGrab();
        } else {
            TryDrop();
        }
    }
}
