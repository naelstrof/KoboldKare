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

public class PrecisionGrabber : MonoBehaviourPun, IPunObservable, ISavable {
    public Rigidbody self;
    public Transform view;
    public GameObject handPrefab;
    [HideInInspector]
    public GameObject hand;
    //private Joint joint;
    private Rigidbody jointRigidbody;
    private Vector3 jointAnchor;
    public float _springStrength = 50;
    [Range(0,1f)]
    public float _dampStrength = 0.5f;
    public bool inputRotation = false;
    public float _breakStrength = 5000;
    public GameObject freezeEffect;
    public UnityEvent OnGrab;
    public UnityEvent OnRelease;
    public UnityEvent OnFreeze;
    public UnityEvent OnUnfreeze;
    public AudioClip unfreezeSound;
    public PlayerPossession possession;
    public List<Rigidbody> ignoreBodies = new List<Rigidbody>();
    private Collider grabbedCollider;
    private float distance;
    private float maxDistance = 2.5f;
    private Vector3 hitNormalObjectSpace;
    private GameObject advancedGameObject;
    private Vector3 colliderLocalAnchor;
    Transform handTransform;
    public GameObject player;
    private Kobold internalKobold;
    public Kobold kobold {
        get {
            if (internalKobold == null) {
                internalKobold = GetComponentInParent<Kobold>();
            }
            return internalKobold;
        }
    }
    private class FrozenGrab {
        public List<IAdvancedInteractable> interactables = new List<IAdvancedInteractable>();
        public List<IFreezeReciever> freezeReceivers = new List<IFreezeReciever>();
        public ConfigurableJoint joint;
        public Vector3 worldHoldPoint;
        public Quaternion worldHoldRotation;
        public Rigidbody body;
        public Rigidbody fallbackBody;
    }
    private List<FrozenGrab> frozenGrabs = new List<FrozenGrab>();
    private Vector3 handPosition;
    private Quaternion handRotation;
    public UnityEngine.InputSystem.PlayerInput controls;
    [HideInInspector]
    public bool grabbing = false;
    private Quaternion savedQuaternion = Quaternion.identity;
    public UnityEvent OnHover;
    public UnityEvent OnExitHover;
    public GameObject crosshair;
    private bool hovering = false;
    [HideInInspector]
    private bool hideHand = true;
    private bool savedRotation = false;
    private bool affectingRotation = false;
    private Rigidbody originalBody;
    private RaycastHit closestHit = new RaycastHit();
    public UnityScriptableSettings.ScriptableSetting mouseSensitivy;
    RaycastHit[] hits = new RaycastHit[6];
    
    public void HideHand(bool hidden) {
        hideHand = hidden;
    }
    public bool HandHidden() {
        return hideHand;
    }

    private void RecursiveSetLayer(Transform t, int fromLayer, int toLayer) {
        for(int i=0;i<t.childCount;i++ ) {
            RecursiveSetLayer(t.GetChild(i), fromLayer, toLayer);
        }
        if (t.gameObject.layer == fromLayer) {
            t.gameObject.layer = toLayer;
        }
    }

    public void RefreshPrompts() {
        if (hovering) {
            OnHover.Invoke();
        } else {
            OnExitHover.Invoke();
        }
    }
    /*public void Unfreeze(IAdvancedInteractable interactable, bool keepBodyFreezes = false) {
        HashSet<FrozenGrab> matches = new HashSet<FrozenGrab>();
        for(int i=0;i<frozenGrabs.Count;i++) {
            if (keepBodyFreezes && frozenGrabs[i].joint != null) {
                continue;
            }
            for(int o=0;o<frozenGrabs[i].interactables.Count;o++) {
                if (frozenGrabs[i].interactables[o] == interactable && frozenGrabs[i].interactables.Count == 1) {
                    matches.Add(frozenGrabs[i]);
                    frozenGrabs[i].interactables[o].OnEndInteract();
                    break;
                }
            }
        }
        foreach(FrozenGrab f in matches) {
            if (f.joint != null) {
                f.body?.WakeUp();
                Destroy(f.joint);
            }
            foreach(IAdvancedInteractable inter in f.interactables) {
                inter?.OnEndInteract();
            }
        }
        foreach(FrozenGrab f in matches) {
            frozenGrabs.Remove(f);
        }
    }*/
    public void Unfreeze(bool shouldNetwork) {
        if (frozenGrabs.Count > 0) {
            GameManager.instance.SpawnAudioClipInWorld(unfreezeSound, transform.position);
        }
        for(int i=0;i<frozenGrabs.Count;) {
            if (frozenGrabs[0].joint != null) {
                frozenGrabs[0].body?.WakeUp();
                Destroy(frozenGrabs[0].joint);
            }
            foreach(IFreezeReciever freezeReciever in frozenGrabs[0].freezeReceivers) {
                freezeReciever.OnEndFreeze();
            }
            foreach(IAdvancedInteractable interactable in frozenGrabs[0].interactables) {
                if (((Component)interactable) != null) {
                    interactable.OnEndInteract(kobold);
                }
            }
            frozenGrabs.RemoveAt(0);
        }
        OnUnfreeze.Invoke();

        if (shouldNetwork && photonView.IsMine) {
            // Implemented in Kobold.cs
            photonView.RPC("RPCUnfreezeAll", RpcTarget.OthersBuffered, null);
        }
    }
    public SpringJoint AddSpringJoint(Rigidbody hitBody, Vector3 worldAnchor) {
        if (ignoreBodies.Contains(hitBody)) {
            return null;
        }
        //hit.rigidbody.rotation = Quaternion.identity;
        SpringJoint joint = hitBody.gameObject.AddComponent<SpringJoint>();
        //joint.axis = view.forward;
        //hit.rigidbody.rotation = initialRotation;
        //joint.axis = view.up;
        //joint.secondaryAxis = view.right;
        //savedQuaternion = initialRotation = joint.transform.rotation;
        joint.connectedBody = self;
        joint.autoConfigureConnectedAnchor = false;
        joint.breakForce = _breakStrength;
        joint.massScale = 1f;
        joint.connectedMassScale = 1f;
        joint.enablePreprocessing = false;
        joint.spring = _springStrength;
        joint.minDistance = Vector3.Distance(worldAnchor, view.position);
        joint.maxDistance = joint.minDistance;
        joint.anchor = hitBody.transform.InverseTransformPoint(worldAnchor);
        joint.connectedAnchor = joint.connectedBody.transform.InverseTransformPoint(view.position);
        //joint.anchor = hit.point;
        //joint.connectedAnchor = hit.point;
        return joint;
    }

    public ConfigurableJoint AddJoint(Rigidbody hitBody, Vector3 worldAnchor) {
        if (ignoreBodies.Contains(hitBody)) {
            return null;
        }
        //hit.rigidbody.rotation = Quaternion.identity;
        ConfigurableJoint joint = hitBody.gameObject.AddComponent<ConfigurableJoint>();
        joint.axis = view.up;
        joint.secondaryAxis = view.right;
        //hit.rigidbody.rotation = initialRotation;
        //joint.axis = view.up;
        //joint.secondaryAxis = view.right;
        //savedQuaternion = initialRotation = joint.transform.rotation;
        joint.connectedBody = self;
        joint.autoConfigureConnectedAnchor = false;
        joint.breakForce = _breakStrength;
        JointDrive drive = joint.xDrive;
        SoftJointLimit sjl = joint.linearLimit;
        sjl.limit = 0f;
        joint.linearLimit = sjl;
        SoftJointLimitSpring sjls = joint.linearLimitSpring;
        sjls.spring = _springStrength;
        joint.linearLimitSpring = sjls;
        joint.linearLimit = sjl;
        drive.positionSpring = _springStrength;
        drive.positionDamper = 2f;
        joint.xDrive = drive;
        joint.yDrive = drive;
        joint.zDrive = drive;
        //joint.projectionMode = JointProjectionMode.PositionAndRotation;
        //joint.projectionDistance = 0.1f;
        joint.massScale = 8f;
        joint.connectedMassScale = 1f;
        joint.enablePreprocessing = false;
        joint.configuredInWorldSpace = true;
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        //joint.swapBodies = true;
        //joint.configuredInWorldSpace = true;
        joint.anchor = hitBody.transform.InverseTransformPoint(worldAnchor);
        joint.connectedAnchor = joint.connectedBody.transform.InverseTransformPoint(view.position);
        //joint.anchor = hit.point;
        //joint.connectedAnchor = hit.point;
        return joint;
    }


    public void Grab(Collider collider, Vector3 localColliderPosition, Vector3 localHitNormal) {
        ValidateHand();
        if (grabbing) {
            return;
        }
        if ( jointRigidbody != null || advancedGameObject != null ) {
            return;
        }
        grabbing = true;
        Rigidbody body = collider.GetComponentInParent<Rigidbody>();
        Vector3 hitPoint = collider.transform.TransformPoint(localColliderPosition);
        colliderLocalAnchor = localColliderPosition;
        if (photonView.IsMine) {
            collider.GetComponentInParent<PhotonView>()?.TransferOwnership(PhotonNetwork.LocalPlayer);
        }

        savedQuaternion = Quaternion.Inverse(view.rotation);
        OnGrab.Invoke();
        hand.GetComponentInChildren<Animator>().SetBool("Grabbing", true);
        handPosition = hitPoint + handRotation * Vector3.down * 0.1f;
        hitNormalObjectSpace = localHitNormal;
        distance = Vector3.Distance(view.transform.position, collider.transform.TransformPoint(localColliderPosition));
        Vector3 holdPoint = view.position + view.forward * distance;
        advancedGameObject = collider.gameObject;
        grabbedCollider = collider;
        bool needsJoint = true;
        if (advancedGameObject.GetComponentInParent<IAdvancedInteractable>() != null) {
            needsJoint = advancedGameObject.GetComponentInParent<IAdvancedInteractable>().PhysicsGrabbable();
            if (!needsJoint) {
                savedQuaternion = advancedGameObject.transform.rotation * savedQuaternion;
            }
            IAdvancedInteractable a = advancedGameObject.GetComponentInParent<IAdvancedInteractable>();
            if (a!=null) {
                bool canFire = true;
                foreach (FrozenGrab grab in frozenGrabs) {
                    foreach (IAdvancedInteractable i in grab.interactables) {
                        // This must be frozen, we shouldn't grab it.
                        if (a == i) {
                            canFire = false;
                            advancedGameObject = null;
                        }
                    }
                }
                if (canFire) {
                    a.OnInteract(kobold);
                }
            }
        } else {
            advancedGameObject = null;
        }
        GetFallbackBody(ref body, out originalBody);
        if (body != null && needsJoint) {
            if (ignoreBodies.Contains(body))  {
                return;
            }
            savedQuaternion = body.rotation * savedQuaternion;
            //joint = AddSpringJoint(body, hitPoint);
            jointRigidbody = body;
            jointAnchor = body.transform.InverseTransformPoint(hitPoint);
        }
    }

    public void Grab() {
        ValidateHand();
        if (grabbing) {
            return;
        }
        if ( jointRigidbody != null || advancedGameObject != null ) {
            //hand.GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_BaseColor", new Color(1, 1, 1, 0f));
            return;
        }
        savedQuaternion = Quaternion.Inverse(view.rotation) ;
        float tdistance = float.MaxValue;
        RaycastHit hit = new RaycastHit();
        bool valid = false;
        int numHits = Physics.RaycastNonAlloc(view.position, view.forward, hits, maxDistance, GameManager.instance.precisionGrabMask, QueryTriggerInteraction.Ignore);
        for(int i=0;i<numHits;i++) {
            //foreach(RaycastHit hit in Physics.RaycastAll(view.position, view.forward, maxDistance, hitMask, QueryTriggerInteraction.Collide)) {
            RaycastHit thit = hits[i];
            if (ignoreBodies.Contains(thit.rigidbody)) {
                continue;
            }
            if (thit.collider.CompareTag("IgnoreAdvancedInteraction")) {
                continue;
            }
            if (thit.distance > tdistance) {
                continue;
            }
            hit = thit;
            tdistance = thit.distance;
            valid = true;
        }
        if (!valid) {
            return;
        }
        PhotonView oview = hit.collider.GetComponentInParent<PhotonView>();
        if (oview) {
            Collider[] colliders = oview.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i] == hit.collider) {
                    photonView.RPC("RPCPrecisionGrab", RpcTarget.AllBuffered, new object[] { oview.ViewID, i, hit.collider.transform.InverseTransformPoint(hit.point) });
                }
            }
        }
    }

    public void GetFallbackBody(ref Rigidbody body, out Rigidbody fallback) {
        fallback = body;
        if (body != null && body.isKinematic) {
            foreach(Rigidbody b in body.GetComponentsInParent<Rigidbody>()) {
                if (!b.isKinematic) {
                    body = b;
                    break;
                }
            }
        }
        /*if (body != null && body.isKinematic) {
            foreach(Rigidbody b in body.GetComponentsInParent<Rigidbody>()) {
                if (b.isKinematic) {
                    fallback = b;
                    break;
                }
            }
        }*/
    }
    public void Freeze() {
        if (grabbedCollider == null) {
            return;
        }
        Collider c = grabbedCollider;
        Vector3 holdPoint = view.position + view.forward * distance;
        Rigidbody r = c.GetComponentInParent<Rigidbody>();
        bool affRotation = affectingRotation;
        PhotonView v = grabbedCollider.GetComponentInParent<PhotonView>();
        Collider[] colliders = v.GetComponentsInChildren<Collider>();
        int colliderID = 0;
        for (int i=0;i<colliders.Length;i++) {
            if (colliders[i] == grabbedCollider) {
                colliderID = i;
                break;
            }
        }
        if (r) {
            photonView.RPC("RPCFreeze", RpcTarget.AllBuffered, new object[] { v.ViewID, colliderID, colliderLocalAnchor, holdPoint, r.rotation, affRotation });
        } else {
            photonView.RPC("RPCFreeze", RpcTarget.AllBuffered, new object[] { v.ViewID, colliderID, colliderLocalAnchor, holdPoint, c.transform.rotation, affRotation });
        }
    }


    public void Freeze(Collider collider, Vector3 localPosition, Vector3 worldPosition, Quaternion rotation, bool affRotation) {
        Rigidbody otherBody = collider.GetComponentInParent<Rigidbody>();
        bool needsJoint = true;
        if (collider.GetComponentInParent<IAdvancedInteractable>() != null) {
            needsJoint = collider.GetComponentInParent<IAdvancedInteractable>().PhysicsGrabbable();
        }

        FrozenGrab fgrab = new FrozenGrab();
        if (needsJoint && otherBody != null) {
            Quaternion startRotation = otherBody.rotation;

            fgrab.body = otherBody;
            GetFallbackBody(ref fgrab.body, out fgrab.fallbackBody);

            ConfigurableJoint createdJoint = AddJoint(fgrab.body, worldPosition);
            createdJoint.anchor = fgrab.body.transform.InverseTransformPoint(collider.transform.TransformPoint(localPosition));
            createdJoint.configuredInWorldSpace = true;
            createdJoint.connectedBody = null;
            createdJoint.connectedAnchor = worldPosition;
            createdJoint.SetTargetRotation(rotation, startRotation);
            if (affRotation) {
                createdJoint.angularXMotion = ConfigurableJointMotion.Locked;
                createdJoint.angularYMotion = ConfigurableJointMotion.Locked;
                createdJoint.angularZMotion = ConfigurableJointMotion.Locked;
            }

            fgrab.joint = createdJoint;
        }
        foreach (IFreezeReciever f in collider.GetComponentsInParent<IFreezeReciever>()) {
            f.OnFreeze(kobold);
            fgrab.freezeReceivers.Add(f);
        }
        foreach (IAdvancedInteractable a in collider.GetComponentsInParent<IAdvancedInteractable>()) {
            // This only runs if you lagged out and missed the grab event, happens frequently for people quickly grabbing and freezing items.
            if (!grabbing || grabbedCollider != collider) {
                a.OnInteract(kobold);
            }
            fgrab.interactables.Add(a);
        }
        fgrab.worldHoldPoint = worldPosition - collider.transform.TransformVector(localPosition);
        fgrab.worldHoldRotation = rotation;
        // We can now clear our grab without notifying anything, this is because the frozen gameobject is still grabbed!
        if (grabbing && grabbedCollider == collider) {
            grabbing = false;
            hand.GetComponentInChildren<Animator>().SetBool("Grabbing", false);
            hand.transform.parent = self.transform;
            jointRigidbody = null;
            advancedGameObject = null;
            grabbedCollider = null;
            affectingRotation = false;
        }
        Destroy(GameObject.Instantiate(freezeEffect, worldPosition, Quaternion.identity), 3f);
        frozenGrabs.Add(fgrab);
        OnFreeze.Invoke();
    }

    public void Ungrab() {
        ValidateHand();
        if (!grabbing) {
            return;
        }
        OnRelease.Invoke();
        grabbing = false;
        hand.GetComponentInChildren<Animator>().SetBool("Grabbing", false);
        hand.transform.parent = self.transform;
        //hand.GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_BaseColor", new Color(1, 1, 1, 0f));
        if (jointRigidbody) {
            if (advancedGameObject == null) {
                foreach (IAdvancedInteractable a in jointRigidbody.gameObject.GetComponentsInParent<IAdvancedInteractable>()) {
                    a.OnEndInteract(kobold);
                }
            }
            //joint.GetComponent<Rigidbody>().SendMessageUpwards("OnRelease", SendMessageOptions.DontRequireReceiver);
            //Destroy(joint);
            jointRigidbody = null;
            //RecursiveSetLayer(joint.gameObject.transform.root, LayerMask.NameToLayer("Effects"), LayerMask.NameToLayer("Pickups"));
        }
        if (advancedGameObject != null) {
            if (advancedGameObject.GetComponentInParent<IAdvancedInteractable>() != null) {
                foreach (IAdvancedInteractable a in advancedGameObject.GetComponentsInParent<IAdvancedInteractable>()) {
                    a.OnEndInteract(kobold);
                }
            }
            advancedGameObject = null;
        }
        grabbedCollider = null;
        affectingRotation = false;
    }
    void ValidateHand() {
        if (hand == null) {
            hand = GameObject.Instantiate(handPrefab);
            handTransform = hand.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
        }
    }
    public void LateUpdate() {
        ValidateHand();
        //distance += Input.mouseScrollDelta.y * 0.08f;
        distance += controls.actions["Grab Push and Pull"].ReadValue<float>() * 0.002f;
        hovering = false;
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivy.value;
        bool ranAdvancedObjectStuff = false;
        if (advancedGameObject != null) {
            hovering = true;
            Vector3 worldNormal = grabbedCollider.transform.TransformDirection(hitNormalObjectSpace);
            handRotation = Quaternion.Lerp(handRotation, Quaternion.LookRotation(-worldNormal, Vector3.up) * Quaternion.AngleAxis(90f, new Vector3(0.0f, 1.0f, 0.0f)), Time.deltaTime*50f);
            Vector3 objHoldPoint = grabbedCollider.transform.TransformPoint(colliderLocalAnchor);
            //Vector3 holdPointOffset = objHoldPoint - advancedGameObject.transform.position;
            Vector3 holdPoint = view.position + view.forward * distance;
            handPosition = objHoldPoint + handRotation * Vector3.down*0.1f;
            foreach (IAdvancedInteractable a in advancedGameObject.GetComponentsInParent<IAdvancedInteractable>()) {
                a.InteractTo(holdPoint, savedQuaternion * view.rotation);
            }
            ranAdvancedObjectStuff = true;
            if (inputRotation) {
                savedQuaternion = Quaternion.AngleAxis(-mouseDelta.x*3f, view.up)*savedQuaternion;
                savedQuaternion = Quaternion.AngleAxis(mouseDelta.y*3f, view.right)*savedQuaternion;
            }
        }
        if (jointRigidbody) {
            hovering = true;
            Vector3 holdPoint = view.position + view.forward * distance;
            Vector3 objHoldPoint = jointRigidbody.transform.TransformPoint(jointAnchor);
            //joint.connectedAnchor = self.transform.InverseTransformPoint(holdPoint);
            //(joint as SpringJoint).minDistance = distance;
            //(joint as SpringJoint).maxDistance = distance;
            Vector3 worldNormal = jointRigidbody.transform.TransformDirection(hitNormalObjectSpace);
            handRotation = Quaternion.Lerp(handRotation, Quaternion.LookRotation(-worldNormal, Vector3.up) * Quaternion.AngleAxis(90f, new Vector3(0.0f, 1.0f, 0.0f)), Time.deltaTime*50f);
            handPosition = objHoldPoint + handRotation * Vector3.down*0.1f;
            if (advancedGameObject != null && !ranAdvancedObjectStuff) {
                foreach (IAdvancedInteractable a in advancedGameObject.GetComponentsInParent<IAdvancedInteractable>()) {
                    a.InteractTo(holdPoint, savedQuaternion * view.rotation);
                }
            }
            if (inputRotation) {
                if (!savedRotation) {
                    savedQuaternion = jointRigidbody.transform.rotation;
                    savedRotation = true;
                }
                Quaternion q = Quaternion.AngleAxis(mouseDelta.y*3f, view.right);
                q = Quaternion.AngleAxis(-mouseDelta.x*3f, view.up) * q;
                savedQuaternion = q * savedQuaternion;
                Quaternion sub = Quaternion.Inverse(jointRigidbody.transform.rotation) * savedQuaternion;
                float angle;
                Vector3 axis;
                sub.ToAngleAxis(out angle, out axis);
                if (angle >= 180) {
                    angle = 360 - angle;
                    axis = -axis;
                }
                savedQuaternion = jointRigidbody.transform.rotation * Quaternion.AngleAxis(Mathf.Clamp(angle,-90f,90f), axis);
                affectingRotation = true;
            } else {
                savedRotation = false;
            }
        } else if (advancedGameObject == null) {
            float closestHitDistance = float.MaxValue;
            bool validHit = false;
            int numHits = Physics.RaycastNonAlloc(view.position, view.forward, hits, maxDistance, GameManager.instance.precisionGrabMask, QueryTriggerInteraction.Ignore);
            for(int i=0;i<numHits;i++) {
                //foreach(RaycastHit hit in Physics.RaycastAll(view.position, view.forward, maxDistance, hitMask, QueryTriggerInteraction.Collide)) {
                RaycastHit hit = hits[i];
                if (ignoreBodies.Contains(hit.rigidbody)) {
                    continue;
                }
                if (hit.distance < closestHitDistance) {
                    closestHit = hit;
                    closestHitDistance = hit.distance;
                    validHit = true;
                }
                //hand.GetComponentInChildren<SkinnedMeshRenderer>().material = handIdle;
                break;
            }
            if (validHit) {
                handRotation = Quaternion.LookRotation(-closestHit.normal, Vector3.up) * Quaternion.AngleAxis(90f, new Vector3(0.0f, 1.0f, 0.0f));
                handPosition = closestHit.point + handRotation*Vector3.down*0.1f;
                hovering = true;
            }
        }
        if (hovering && !hand.activeInHierarchy && !HandHidden()) {
            hand.SetActive(true);
            OnHover.Invoke();
        }
        if ((!hovering || HandHidden()) && hand.activeInHierarchy) {
            hand.SetActive(false);
            OnExitHover.Invoke();
        }
        foreach(FrozenGrab fgrab in frozenGrabs) {
            foreach(IAdvancedInteractable interactable in fgrab.interactables) {
                interactable.InteractTo(fgrab.worldHoldPoint, fgrab.worldHoldRotation);
            }
        }
        // We set this late, so that animations can fuck off.
        handTransform.rotation = handRotation;
        handTransform.position = handPosition;
    }
    public void OnDestroy() {
        if (hand) {
            Destroy(hand);
        }
        Unfreeze(false);
        if (photonView.IsMine) {
            PhotonNetwork.CleanRpcBufferIfMine(photonView);
        }
    }
    public void FixedUpdate() {
        ValidateHand();
        // This code here is for when a character we're grabbing gets ragdolled.
        // When we spawn the joint, we search for a NON-kinematic rigidbody, but save the initial rigidbody we hit.
        // We can flip between the two since we know it gets flipped when ragdolled
        for(int i=0;i<frozenGrabs.Count;i++) {
            FrozenGrab grab = frozenGrabs[i];
            //if (grab.joint == null) {
                //frozenGrabs.RemoveAt(i);
                //continue;
            //}
            if (grab.body == null) {
                continue;
            }
            if (grab.body.isKinematic && grab.joint != null) {
                bool affRotation = grab.joint.angularXMotion == ConfigurableJointMotion.Locked;
                Vector3 worldAnchor = grab.joint.transform.TransformPoint(grab.joint.anchor);
                Destroy(grab.joint);
                grab.joint = AddJoint(grab.fallbackBody,worldAnchor);
                if (grab.joint == null) {
                    frozenGrabs.RemoveAt(i);
                    continue;
                }
                Rigidbody save = grab.body;
                grab.body = grab.fallbackBody;
                grab.fallbackBody = save;
                grab.joint.connectedBody = null;
                grab.joint.connectedAnchor = worldAnchor;
                grab.joint.configuredInWorldSpace = true;
                if (affRotation) {
                    grab.joint.angularXMotion = ConfigurableJointMotion.Locked;
                    grab.joint.angularYMotion = ConfigurableJointMotion.Locked;
                    grab.joint.angularZMotion = ConfigurableJointMotion.Locked;
                }
            }
        }
        //  ---
        if (jointRigidbody) {
            if (affectingRotation) {
                Vector3 forward = savedQuaternion * Vector3.forward;
                Vector3 up = savedQuaternion * Vector3.up;
                Quaternion rotAdjustment = Quaternion.FromToRotation(jointRigidbody.transform.forward, forward);
                rotAdjustment *= Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(jointRigidbody.transform.up, up), 0.5f);

                jointRigidbody.angularVelocity -= jointRigidbody.angularVelocity * _dampStrength;
                jointRigidbody.AddTorque(new Vector3(rotAdjustment.x, rotAdjustment.y, rotAdjustment.z) * 32f, ForceMode.VelocityChange);
            }
            Vector3 holdPoint = view.position + view.forward * distance;
            Vector3 objHoldPoint = jointRigidbody.transform.TransformPoint(jointAnchor);
            //joint.connectedAnchor = self.transform.InverseTransformPoint(holdPoint);
            //(joint as SpringJoint).minDistance = distance;
            //(joint as SpringJoint).maxDistance = distance;
            Vector3 worldNormal = jointRigidbody.transform.TransformDirection(hitNormalObjectSpace);

            //Vector3 springForward = objHoldPoint - head.position;
            //Vector3 springRight = 

            //joint.axis = view.forward;
            //joint.secondaryAxis = view.right;
            //Debug.Log(joint.axis);
            //Vector3 springForward = objHoldPoint - view.position;
            //Vector3 springRight = objHoldPoint - view.position;


            //joint.targetPosition = view.forward * distance;//holdPoint-objHoldPoint;

            // Manual axis alignment, for pole jumps!
            bool isPenetrating = false;
            Kobold k = jointRigidbody.GetComponentInParent<Kobold>();
            if (k) {
                isPenetrating = kobold.IsPenetrating(k);
            }
            if (!isPenetrating && jointRigidbody.transform.root != self.transform.root && !jointRigidbody.isKinematic) {
                jointRigidbody.velocity -= jointRigidbody.velocity*_dampStrength;
                Vector3 axis = view.forward;
                Vector3 jointPos = jointRigidbody.transform.TransformPoint(jointAnchor);
                Vector3 center = (view.position + jointPos) / 2f;
                Vector3 wantedPosition1 = center - axis * distance / 2f;
                //Vector3 wantedPosition2 = center + axis * distance / 2f;
                float ratio = Mathf.Clamp((jointRigidbody.mass / self.mass), 0.75f, 1.25f);
                Vector3 force = (wantedPosition1 - view.position) * _springStrength;
                self.AddForce(force * ratio);
                jointRigidbody.AddForce(-force * (1f / ratio));
            }

            // Manual velocity to keep the prop where the user wants

            Vector3 towardGoal = holdPoint - objHoldPoint;
            jointRigidbody.AddForce(towardGoal * _springStrength);

            //joint.gameObject.GetComponent<Rigidbody>().angularVelocity *= 0.5f;
            //self.angularVelocity *= 0.5f;



            //Vector3 velocityDiff = joint.gameObject.GetComponent<Rigidbody>().velocity - self.velocity;
            //joint.gameObject.GetComponent<Rigidbody>().velocity -= velocityDiff * _dampStrength; 
            //Vector3 angleVelDiff = joint.gameObject.GetComponent<Rigidbody>().angularVelocity - self.angularVelocity;
            //joint.gameObject.GetComponent<Rigidbody>().angularVelocity -= angleVelDiff;

            //if (advancedGameObject) {
                //advancedGameObject.GetComponent<AdvancedInteracter>().InteractTo(holdPoint, view.rotation);
            //}
            //hand.GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_BaseColor", new Color(1, 1, 1, 1f));
        }
        /*if (Input.GetMouseButtonDown(0)) {
            Grab();
        }
        if (Input.GetMouseButtonUp(0)) {
            Ungrab();
        }*/

        if (jointRigidbody != null) {
            if (jointRigidbody.isKinematic) {
                Vector3 worldAnchor = jointRigidbody.transform.TransformPoint(jointAnchor);
                //Destroy(joint);
                //joint = AddJoint(originalBody,worldAnchor);
                Rigidbody copy = jointRigidbody;
                jointRigidbody = originalBody;
                originalBody = copy;
                jointAnchor = jointRigidbody.transform.InverseTransformPoint(worldAnchor);
            }
            // To prevent feedback loops, we lessen the self-effect of the joint when we're trying to fuck other kobolds
            //if (kobold.dick != null && kobold.dick.holeTarget != null && kobold.dick.holeTarget.body == jointRigidbody) {
                //joint.massScale = 20f;
            //} else {
                //joint.massScale = 10f;
            //}
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(this.inputRotation);
            stream.SendNext(this.distance);
            stream.SendNext(this.savedQuaternion);
            stream.SendNext(this.grabbing);
        } else {
            inputRotation = (bool)stream.ReceiveNext();
            distance = (float)stream.ReceiveNext();
            savedQuaternion = (Quaternion)stream.ReceiveNext();
            if (!(bool)stream.ReceiveNext()) {
                Ungrab();
            }
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(inputRotation);
        writer.Write(distance);
        writer.Write(savedQuaternion.x);
        writer.Write(savedQuaternion.y);
        writer.Write(savedQuaternion.z);
        writer.Write(savedQuaternion.w);
        writer.Write(grabbing);
    }

    public void Load(BinaryReader reader, string version) {
        inputRotation = reader.ReadBoolean();
        distance = reader.ReadSingle();
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        float w = reader.ReadSingle();
        Quaternion newRot = new Quaternion(x,y,z,w);
        savedQuaternion = newRot;
        if (!reader.ReadBoolean()) {
            Ungrab();
        }
    }
}
