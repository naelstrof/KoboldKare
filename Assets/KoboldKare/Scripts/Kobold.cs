using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using System;
using UnityEngine.Events;
using KoboldKare;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PenetrationTech;

public class Kobold : MonoBehaviourPun, IGameEventGenericListener<float>, IGrabbable, IAdvancedInteractable, IPunInstantiateMagicCallback, IReagentContainerListener, IPunObservable {
    public StatusEffect koboldStatus;
    [System.Serializable]
    public class PenetrableSet {
        public Penetrable penetratable;
        public Rigidbody ragdollAttachBody;
        public bool isFemaleExclusiveAnatomy = false;
    }

    [System.Serializable]
    public class UnityEventFloat : UnityEvent<float> {}

    [System.Serializable]
    public class StatChangeEvent {
        public Stat changedStat;
        public UnityEventFloat onChange;
    }

    public List<StatChangeEvent> statChangedEvents = new List<StatChangeEvent>();

    public delegate void RagdollEventHandler(bool ragdolled);
    public event RagdollEventHandler RagdollEvent;


    public Task ragdollTask;

    public EquipmentInventory inventory;
    public StatBlock statblock = new StatBlock();

    public List<PenetrableSet> penetratables = new List<PenetrableSet>();

    public List<Transform> attachPoints = new List<Transform>();

    public GenericLODConsumer lodLevel;
    public Transform root;
    public Animator animator;
    public Rigidbody body;
    [HideInInspector]
    public float uprightTimer = 0;
    private float originalUprightTimer = 0;
    public GameEventFloat MetabolizeEvent;
    public List<GenericInflatable> boobs = new List<GenericInflatable>();
    public List<GenericInflatable> bellies = new List<GenericInflatable>();
    public List<GenericInflatable> subcutaneousStorage = new List<GenericInflatable>();
    public GenericInflatable sizeInflatable;
    public List<GenericReagentContainer> balls = new List<GenericReagentContainer>();
    public BodyProportion bodyProportion;
    public UnityEvent OnRagdoll;
    public UnityEvent OnStandup;
    public UnityEvent OnOrgasm;
    [HideInInspector]
    public List<DickInfo.DickSet> activeDicks = new List<DickInfo.DickSet>();

    public Grabber grabber;
    public AudioSource gurgleSource;
    public List<Renderer> koboldBodyRenderers;
    [HideInInspector]
    public float sex;
    public Transform hip;
    public LayerMask playerHitMask;
    [HideInInspector]
    public float topBottom;
    [HideInInspector]
    public float thickness;
    //[HideInInspector]
    //public float inout;
    private CollisionDetectionMode oldCollisionMode = CollisionDetectionMode.Discrete;
    //public PhotonView photonView;
    public KoboldCharacterController controller;
    public float stimulation = 0f;
    public float stimulationMax = 30f;
    public float stimulationMin = -30f;
    public UnityEvent SpawnEggEvent;
    //public KoboldUseEvent onGrabEvent;
    public float uprightForce = 10f;
    public Animator koboldAnimator;
    public List<Rigidbody> ragdollBodies = new List<Rigidbody>();
    private Rigidbody[] allRigidbodies;
    private float lastPumpTime = 0f;
    private bool grabbed = false;
    private List<Vector3> savedJointAnchors = new List<Vector3>();
    private Vector3 networkedRagdollHipPosition;
    public bool isLoaded = false;
    public float arousal = 0f;
    public bool ragdolled {
        get {
            if (ragdollBodies[0] == null) {
                return false;
            }
            return uprightTimer > 0f || (originalUprightTimer > 0f && bodyProportion.running);
        }
    }
    public bool notRagdolled {
        get {
            return !ragdolled;
        }
    }
    public void AddStimulation(float s) {
        stimulation += s;
        if (stimulation >= stimulationMax) {
            OnOrgasm.Invoke();
            foreach(var dickSet in activeDicks) {
                dickSet.dick.Cum();
            }
            PumpUpDick(1f);
            stimulation = stimulationMin;
        }
    }
    private void RecursiveSetLayer(Transform t, int fromLayer, int toLayer) {
        for (int i = 0; i < t.childCount; i++) {
            RecursiveSetLayer(t.GetChild(i), fromLayer, toLayer);
        }
        if (t.gameObject.layer == fromLayer && t.GetComponent<Collider>() != null) {
            t.gameObject.layer = toLayer;
        }
    }
    public Vector4 HueBrightnessContrastSaturation {
        set {
            foreach (Renderer r in koboldBodyRenderers) {
                if (r == null) {
                    continue;
                }
                foreach (Material m in r.materials) {
                    m.SetVector("_HueBrightnessContrastSaturation", value);
                }
            }
        }
        get {
            foreach (Renderer r in koboldBodyRenderers) {
                foreach (Material m in r.materials) {
                    return m.GetVector("_HueBrightnessContrastSaturation");
                }
            }
            return new Vector4(0, 0.5f, 0.5f, 0.5f);
        }
    }
    //private bool incremented = false;
    //public AnimatorUpdateMode modeSave;

    public void Awake() {
        inventory = new EquipmentInventory(this);
        inventory.EquipmentChangedEvent += OnEquipmentChanged;
        statblock.StatusEffectsChangedEvent += OnStatusEffectsChanged;
        allRigidbodies = new Rigidbody[2];
        allRigidbodies[0] = body;
        allRigidbodies[1] = ragdollBodies[0];
        /*foreach(var dickGroup in dickGroups) {
            foreach (var dickSet in dickGroup.dicks) {
                dickSet.dickAttachPosition = dickSet.parent.InverseTransformPoint(dickSet.dick.dickTransform.position);
                dickSet.initialDickForwardHipSpace = dickSet.parent.InverseTransformDirection(dickSet.dick.dickTransform.TransformDirection(dickSet.dick.dickForwardAxis));
                dickSet.initialDickUpHipSpace = dickSet.parent.InverseTransformDirection(dickSet.dick.dickTransform.TransformDirection(dickSet.dick.dickUpAxis));
                dickSet.initialBodyLocalRotation = dickSet.dick.body.transform.localRotation;
                dickSet.initialTransformLocalRotation = dickSet.dick.dickTransform.localRotation;
                //dickSet.joint.axis = Vector3.up;
                //dickSet.joint.secondaryAxis = Vector3.forward;
                //dickSet.dick.body.transform.parent = root;
                dickSet.joint.autoConfigureConnectedAnchor = false;
                if (dickSet.joint is ConfigurableJoint) {
                    Debug.LogWarning("Configurable joints will cause problems! They won't get removed properly due to a unity bug, and using a while loop to remove them will sometimes delete freezes. So just don't use them!");
                    dickSet.savedJoint = new ConfigurableJointData((ConfigurableJoint)dickSet.joint);
                } else if (dickSet.joint is CharacterJoint) {
                    dickSet.savedJoint = new CharacterJointData((CharacterJoint)dickSet.joint);
                }
                koboldBodyRenderers.AddRange(dickSet.dick.deformationTargets);
            }
        }*/
        savedJointAnchors.Clear();
        foreach (Rigidbody ragdollBody in ragdollBodies) {
            if (ragdollBody.GetComponent<CharacterJoint>() == null) {
                continue;
            }
            savedJointAnchors.Add(ragdollBody.GetComponent<CharacterJoint>().connectedAnchor);
            ragdollBody.GetComponent<CharacterJoint>().autoConfigureConnectedAnchor = false;
        }
        //for(int i=1;i<ragdollBodies.Count+1;i++) {
            //allRigidbodies[i] = ragdollBodies[i-1];
        //}
    }
    public ExitGames.Client.Photon.Hashtable Save() {
        ExitGames.Client.Photon.Hashtable t = new ExitGames.Client.Photon.Hashtable();
        t["sex"] = sex;
        t["hue"] = HueBrightnessContrastSaturation.x;
        t["brightness"] =  HueBrightnessContrastSaturation.y;
        t["contrast"] = HueBrightnessContrastSaturation.z;
        t["saturation"] = HueBrightnessContrastSaturation.w;
        t["topBottom"] = topBottom;
        t["thickness"] = thickness;
        t["size"] = subcutaneousStorage[0].transformCurves[0].initialScale;
        t["boobSize"] = boobs[0].defaultReagentVolume;
        int[] equipmentList = new int[inventory.equipment.Count];
        for(int i=0;i<inventory.equipment.Count;i++) {
            equipmentList[i] = inventory.equipment[i].equipment.GetID();
        }
        t["equippedItems"] = equipmentList;
        int[] statusList = new int[statblock.activeEffects.Count];
        for(int i=0;i<statblock.activeEffects.Count;i++) {
            statusList[i] = statblock.activeEffects[i].effect.GetID();
        }
        t["activeStatusEffects"] = statusList;
        return t;
    }
    public void OnCompleteBodyProportion() {
        if (originalUprightTimer > 0f) {
            KnockOver(originalUprightTimer);
        }
    }

    [PunRPC]
    public void Load(ExitGames.Client.Photon.Hashtable s) {
        isLoaded = true;
        if (s.ContainsKey("sex")) {
            sex = (float)s["sex"];
            foreach (Renderer r in koboldBodyRenderers) {
                if (r is SkinnedMeshRenderer) {
                    SkinnedMeshRenderer bodyMesh = (SkinnedMeshRenderer)r;
                    for (int o = 0; o < bodyMesh.sharedMesh.blendShapeCount; o++) {
                        if (bodyMesh.sharedMesh.GetBlendShapeName(o) == "MaleEncode") {
                            bodyMesh.SetBlendShapeWeight(o, Mathf.Clamp01(1f - sex * 2f) * 100f);
                        }
                    }
                }
            }
        }
        bool changedBodyShape = false;
        if (s.ContainsKey("topBottom")) {
            topBottom = (float)s["topBottom"];
            changedBodyShape = true;
        }
        if (s.ContainsKey("thickness")) {
            thickness = (float)s["thickness"];
            changedBodyShape = true;
        }
        if (changedBodyShape) {
            originalUprightTimer = uprightTimer;
            StandUp();
            bodyProportion.Initialize();
        }
        Vector4 hbcs = HueBrightnessContrastSaturation;
        if (s.ContainsKey("brightness")) {
            hbcs.y = (float)s["brightness"];
        }
        if (s.ContainsKey("contrast")) {
            hbcs.z = (float)s["contrast"];
        }
        if (s.ContainsKey("saturation")) {
            hbcs.w = (float)s["saturation"];
        }
        if (s.ContainsKey("hue")) {
            hbcs.x = (float)s["hue"];
        }
        HueBrightnessContrastSaturation = hbcs;

        if (s.ContainsKey("size")) {
            Reagent r = new Reagent();
            r.volume = (float)s["size"] * sizeInflatable.reagentVolumeDivisor;
            sizeInflatable.container.contents[ReagentData.ID.GrowthSerum] = r;
            sizeInflatable.container.contents.InvokeListenerUpdate(ReagentContents.ReagentInjectType.Inject);
        }

        if (s.ContainsKey("boobSize")) {
            foreach (var boob in boobs) {
                Reagent f = new Reagent();
                f.volume = (float)s["boobSize"] * boob.reagentVolumeDivisor * 0.7f;
                Reagent m = new Reagent();
                m.volume = (float)s["boobSize"] * boob.reagentVolumeDivisor * 0.3f;
                boob.container.contents[ReagentData.ID.Fat] = f;
                boob.container.contents[ReagentData.ID.Milk] = m;
                boob.container.contents.InvokeListenerUpdate(ReagentContents.ReagentInjectType.Inject);
            }
        }

        if (s.ContainsKey("equippedItems")) {
            int[] equipped = (int[])s["equippedItems"];
            bool isSame = equipped.Length == inventory.equipment.Count;
            for (int i=0;isSame&&i<equipped.Length&&i<inventory.equipment.Count;i++) {
                if (inventory.equipment[i].equipment.GetID() != equipped[i]) {
                    isSame = false;
                }
            }
            if (!isSame) {
                inventory.Clear(EquipmentInventory.EquipmentChangeSource.Network, false);
                foreach (var id in equipped) {
                    if (Equipment.GetEquipmentFromID(id) != null) {
                        inventory.AddEquipment(Equipment.GetEquipmentFromID(id), EquipmentInventory.EquipmentChangeSource.Network);
                    } else {
                        Debug.LogError("Equipment with id " + id + " doesn't exist!");
                    }
                }
            }
        }
        if (s.ContainsKey("activeStatusEffects")) {
            int[] activeEffects = (int[])s["activeStatusEffects"];
            bool isSame = activeEffects.Length == statblock.activeEffects.Count;
            for (int i=0;isSame&&i<activeEffects.Length&&i<statblock.activeEffects.Count;i++) {
                if (statblock.activeEffects[i].effect.GetID() != activeEffects[i]) {
                    isSame = false;
                }
            }
            if (!isSame) {
                statblock.Clear();
                foreach (var id in activeEffects) {
                    statblock.AddStatusEffect(StatusEffect.GetFromID(id), StatBlock.StatChangeSource.Network);
                }
            }
        }
    }
    public void OnEquipmentChanged(EquipmentInventory inv, EquipmentInventory.EquipmentChangeSource source) {
        /*if (source != EquipmentInventory.EquipmentChangeSource.Network && photonView.IsMine) {
            Hashtable sendInfo = new Hashtable();
            int[] equipmentList = new int[inventory.equipment.Count];
            for(int i=0;i<inventory.equipment.Count;i++) {
                equipmentList[i] = inventory.equipment[i].equipment.GetID();
            }
            sendInfo["equippedItems"] = equipmentList;
            photonView.RPC("Load", RpcTarget.OthersBuffered, new object[] { sendInfo });
        }*/
    }
    public void OnStatusEffectsChanged(StatBlock block, StatBlock.StatChangeSource source) {
        foreach (var statEvent in statChangedEvents) {
            statEvent.onChange.Invoke(block.GetStat(statEvent.changedStat));
        }
        // Equipment is already synced, and we don't want updates triggered from the network.
        /*if (source != StatBlock.StatChangeSource.Equipment && source != StatBlock.StatChangeSource.Network && photonView.IsMine) {
            Hashtable sendInfo = new Hashtable();
            int[] statusList = new int[statblock.activeEffects.Count];
            for (int i = 0; i < statblock.activeEffects.Count; i++) {
                statusList[i] = statblock.activeEffects[i].effect.GetID();
            }
            sendInfo["activeStatusEffects"] = statusList;
            photonView.RPC("Load", RpcTarget.OthersBuffered, new object[] { sendInfo });
        }*/
    }

    void Start() {
        statblock.AddStatusEffect(koboldStatus, StatBlock.StatChangeSource.Misc);
        lastPumpTime = Time.timeSinceLevelLoad;
        MetabolizeEvent.RegisterListener(this);
        foreach (var b in bellies) {
            b.container.contents.AddListener(this);
        }
        bodyProportion.OnComplete += OnCompleteBodyProportion;
    }
    private void OnDestroy() {
        bodyProportion.OnComplete -= OnCompleteBodyProportion;
        statblock.StatusEffectsChangedEvent -= OnStatusEffectsChanged;
        inventory.EquipmentChangedEvent -= OnEquipmentChanged;
        MetabolizeEvent.UnregisterListener(this);
        foreach (var b in bellies) {
            b.container.contents.RemoveListener(this);
        }
        if (photonView.IsMine) {
            PhotonNetwork.CleanRpcBufferIfMine(photonView);
        }
    }
    public bool OnGrab(Kobold kobold) {
        //onGrabEvent.Invoke(kobold, transform.position);
        grabbed = true;
        //KnockOver(999999f);
        //modeSave = animator.updateMode;
        //animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        animator.SetBool("Carried", true);
        //pickedUp = 1;
        //transSpeed = 5.0f;
        controller.frictionMultiplier = 0.1f;
        controller.enabled = false;
        return true;
    }
    [PunRPC]
    public void RPCKnockOver() {
        KnockOver(9999f);
    }
    [PunRPC]
    public void RPCStandUp() {
        StandUp();
    }
    public IEnumerator KnockOverRoutine() {
        // If we need jigglebones disabled, it takes TWO frames for it to take effect! So... here we wait!
        // Otherwise jigglebones will move rigidbodies and fuck stuff up...
        OnRagdoll.Invoke();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        //RecursiveSetLayer(transform, LayerMask.NameToLayer("Hitbox"), LayerMask.NameToLayer("PlayerHitbox"));
        if (koboldAnimator == null) {
            // Oh dear, guess we got removed already. Just quit out.
            yield return null;
        }
        koboldAnimator.enabled = false;
        controller.enabled = false;
        //foreach(var penSet in penetratables) {
            //penSet.penetratable.SwitchBody(penSet.ragdollAttachBody);
        //}
        foreach (Rigidbody b in ragdollBodies) {
            b.velocity = body.velocity;
            b.isKinematic = false;
            //b.interpolation = RigidbodyInterpolation.Interpolate;
            if (lodLevel.isClose) {
                b.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }
        oldCollisionMode = body.collisionDetectionMode;
        body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        body.isKinematic = true;
        body.GetComponent<Collider>().enabled = false;
        foreach(JigglePhysics.JiggleBone j in GetComponentsInChildren<JigglePhysics.JiggleBone>()) {
            j.updateMode = JigglePhysics.JiggleBone.UpdateType.FixedUpdate;
        }
        foreach(JigglePhysics.JiggleSoftbody s in GetComponentsInChildren<JigglePhysics.JiggleSoftbody>()) {
            s.updateMode = JigglePhysics.JiggleSoftbody.UpdateType.FixedUpdate;
        }

        // We need to know the final result of our ragdoll before we update the anchors.
        bodyProportion.ScaleSkeleton();
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
    }
    public void KnockOver(float duration = 3f) {
        //uprightTimer = Mathf.Max(Mathf.Max(0 + duration, uprightTimer + duration), 1f);
        uprightTimer = duration;
        if (bodyProportion.running) {
            originalUprightTimer = Mathf.Max(duration, originalUprightTimer);
            return;
        }
        if (body.isKinematic) {
            return;
        }
        if (ragdollTask != null && ragdollTask.Running) {
            ragdollTask.Stop();
        }
        ragdollTask = new Task(KnockOverRoutine());
    }
    // This was a huuuUUGE pain, but for somereason joints forget their initial orientation if you switch bodies.
    // I tried a billion different things to try to reset the initial orientation, this was the only thing that worked for me!
    public void StandUp() {
        // FIXME: If we've got some queued ragdoll commands, then we let it at least ragdoll a little. Otherwise our body rigidbody gets unconstrained???
        if (originalUprightTimer > 0f) {
            originalUprightTimer = 0.5f;
        }
        if ((!body.isKinematic && ragdollBodies[0].isKinematic) || koboldAnimator.enabled) {
            return;
        }
        //foreach(var penSet in penetratables) {
            //penSet.penetratable.SwitchBody(body);
        //}
        Vector3 diff = hip.position - body.transform.position;
        body.transform.position += diff;
        hip.position -= diff;
        body.transform.position += Vector3.up*0.5f;
        body.isKinematic = false;
        body.GetComponent<Collider>().enabled = true;
        body.collisionDetectionMode = oldCollisionMode;
        Vector3 averageVel = Vector3.zero;
        foreach (Rigidbody b in ragdollBodies) {
            averageVel += b.velocity;
        }
        averageVel /= ragdollBodies.Count;
        body.velocity = averageVel;
        uprightTimer = 0f;
        controller.enabled = true;
        //RecursiveSetLayer(transform, LayerMask.NameToLayer("PlayerHitbox"), LayerMask.NameToLayer("Hitbox"));
        foreach (Rigidbody b in ragdollBodies) {
            //b.interpolation = RigidbodyInterpolation.None;
            b.collisionDetectionMode = CollisionDetectionMode.Discrete;
            b.isKinematic = true;
        }
        foreach(JigglePhysics.JiggleBone j in GetComponentsInChildren<JigglePhysics.JiggleBone>()) {
            j.updateMode = JigglePhysics.JiggleBone.UpdateType.LateUpdate;
        }
        foreach(JigglePhysics.JiggleSoftbody s in GetComponentsInChildren<JigglePhysics.JiggleSoftbody>()) {
            s.updateMode = JigglePhysics.JiggleSoftbody.UpdateType.LateUpdate;
        }
        //foreach(var penSet in penetratables) {
            //penSet.penetratable.SwitchBody(body);
        //}
        koboldAnimator.enabled = true;
        controller.enabled = true;
        OnStandup.Invoke();
        RagdollEvent?.Invoke(false);
    }
    public void PumpUpDick(float amount) {
        if (amount > 0 ) {
            lastPumpTime = Time.timeSinceLevelLoad;
        }
        arousal += amount;
        arousal = Mathf.Clamp01(arousal);
    }
    public void OnRelease(Kobold kobold) {
        //animator.updateMode = modeSave;
        animator.SetBool("Carried", false);
        if (body.velocity.magnitude > 3f) {
            KnockOver(3f);
        } else {
            foreach(Collider c in Physics.OverlapSphere(transform.position, 1f, playerHitMask, QueryTriggerInteraction.Collide)) {
                Kobold k = c.GetComponentInParent<Kobold>();
                if (k!=this && k!=kobold) {
                    k.GetComponentInChildren<GenericUsable>().Use(this);
                }
            }
            controller.enabled = true;
        }
        controller.frictionMultiplier = 1f;
        grabbed = false;
        // If someone released us, and we own us, re-aquire ownership
        if (NetworkManager.instance.localPlayerInstance == this.photonView && !GetComponentInParent<PhotonView>().IsMine) {
            GetComponentInParent<PhotonView>().RequestOwnership();
        }
        //pickedUp = 0;
        //transSpeed = 1f;
    }
    private void Update() {
        foreach(var dickSet in activeDicks) {
            if (!dickSet.container.contents.ContainsKey(ReagentData.ID.Blood)) {
                dickSet.container.contents.Mix(ReagentData.ID.Blood, 0.01f);
            }
            // Pulse the dicks a little, so they still look "lively"
            dickSet.container.contents[ReagentData.ID.Blood].volume = arousal*0.92f + (0.08f * Mathf.Clamp01(Mathf.Sin(Time.time*2f)))*arousal;
            dickSet.container.contents.TriggerChange();
        }
    }
    private void FixedUpdate() {
        if (!grabbed) {
            uprightForce = Mathf.MoveTowards(uprightForce, 10f, Time.deltaTime * 10f);
        } else {
            uprightForce = Mathf.MoveTowards(uprightForce, 0f, Time.deltaTime * 2f);
            PumpUpDick(Time.deltaTime*0.1f);
        }
        if (uprightTimer > 0f) {
            uprightTimer -= Time.fixedDeltaTime;
            if (uprightTimer < 0f) {
                StandUp();
            }
        }
        if (uprightTimer <= 0) {
            body.angularVelocity -= body.angularVelocity*0.2f;
            float penetrationAmount = 0f;
            //foreach(var penSet in penetratables) {
                //if (penSet.penetratable.isActiveAndEnabled) {
                    //penetrationAmount += penSet.penetratable.realGirth;
                //}
            //}
            float deflectionForgivenessDegrees = 5f;
            Vector3 cross = Vector3.Cross(body.transform.up, Vector3.up);
            float angleDiff = Mathf.Max(Vector3.Angle(body.transform.up, Vector3.up) - deflectionForgivenessDegrees, 0f);
            body.AddTorque(cross*angleDiff, ForceMode.Acceleration);
            //body.angularVelocity += new Vector3(rot.x, rot.y, rot.z).MagnitudeClamped(0f, 1f) * Mathf.Max((1f - penetrationAmount * 2f) * uprightForce, 0f);
        }
        //dick.dickTransform.GetComponent<CharacterJoint>().connectedAnchor = body.transform.InverseTransformPoint(hip.TransformPoint(dickAttachPosition));
        //ConfigurableJoint dickJoint = dick.body.GetComponent<ConfigurableJoint>();
        /*if (activeDicks != null) {
            foreach (var dickSet in activeDicks) {
                if (dickSet.joint == null) {
                    continue;
                }
                Vector3 dickForward = dickSet.dick.dickTransform.TransformDirection(dickSet.dick.dickForwardAxis);
                Vector3 dickUp = dickSet.dick.dickTransform.TransformDirection(dickSet.dick.dickUpAxis);
                Vector3 dickRight = Vector3.Cross(dickUp, dickForward);
                Vector3 hipUp = dickSet.parent.TransformDirection(dickSet.initialDickUpHipSpace);
                Vector3 hipForward = dickSet.parent.TransformDirection(dickSet.initialDickForwardHipSpace);
                Vector3 hipRight = Vector3.Cross(hipUp, hipForward);

                //FIXME
                // Force the dick to be oriented correctly.
                //dickSet.dick.dickTransform.rotation = Quaternion.FromToRotation(dickUp, Vector3.ProjectOnPlane(hipUp, dickForward).normalized) * dickSet.dick.dickTransform.rotation;
                //dickSet.joint.autoConfigureConnectedAnchor = false; //dickJoint.axis = body.transform.right;
                //if (dickSet.joint.connectedBody == body || dickSet.joint.connectedBody.isKinematic) {
                if (uprightTimer <= 0) { // If we're not ragdolled
                    dickSet.dick.body.interpolation = body.interpolation;
                    dickSet.joint.connectedAnchor = dickSet.joint.connectedBody.transform.InverseTransformPoint(dickSet.parent.TransformPoint(dickSet.dickAttachPosition));
                } else {
                    dickSet.dick.body.interpolation = dickSet.joint.connectedBody.interpolation;
                }
                //dick.dickTransform.position = hip.TransformPoint(dickAttachPosition);
                if (dickSet.container.contents.ContainsKey(ReagentData.ID.Blood)) {
                    Quaternion ro = Quaternion.FromToRotation(dickForward, hipForward);
                    dickSet.dick.body.angularVelocity *= 0.8f;
                    dickSet.dick.body.AddTorque(new Vector3(ro.x, ro.y, ro.z) * 30f * dickSet.container.contents[ReagentData.ID.Blood].volume);
                }
                //dickSet.joint.axis = dickSet.dick.dickTransform.TransformDirection(dickSet.dick.dickUpAxis);
                //dickSet.dick.body.position = dickSet.parent.TransformPoint(dickSet.dickAttachPosition);
                //dickSet.dick.body.MovePosition(dickSet.parent.TransformPoint(dickSet.dickAttachPosition));
            }
        }*/
        if (Time.timeSinceLevelLoad-lastPumpTime > 10f) {
            PumpUpDick(-Time.deltaTime * 0.01f);
        }
        if (!photonView.IsMine) {
            Vector3 dir = networkedRagdollHipPosition - hip.position;
            hip.GetComponent<Rigidbody>().AddForce(dir, ForceMode.VelocityChange);
        }
    }

    [PunRPC]
    public void RPCGrab(int photonID) {
        PhotonView view = PhotonView.Find(photonID);
        if (view == null) {
            return;
        }
        IGrabbable g = view.GetComponentInChildren<IGrabbable>();
        if (g != null) {
            GetComponentInChildren<Grabber>().TryGrab(g);
        }
    }
    [PunRPC]
    public void RPCDrop() {
        GetComponentInChildren<Grabber>().TryDrop();
    }

    [PunRPC]
    public void RPCPrecisionGrab(int grabberViewID, int colliderID, Vector3 lHitPoint) {
        PhotonView view = PhotonView.Find(grabberViewID);
        if (view != null) {
            Collider[] colliders = view.GetComponentsInChildren<Collider>();
            if (colliderID >= 0 && colliderID < colliders.Length) {
                GetComponentInChildren<PrecisionGrabber>().Grab(colliders[colliderID], lHitPoint, Vector3.up);
            }
        }
    }

    [PunRPC]
    public void RPCFreeze(int grabberViewID, int colliderID, Vector3 localPosition, Vector3 worldPosition, Quaternion rotation, bool affRotation) {
        PhotonView view = PhotonView.Find(grabberViewID);
        if (view != null) {
            Collider[] colliders = view.GetComponentsInChildren<Collider>();
            if (colliderID >= 0 && colliderID < colliders.Length) {
                GetComponentInChildren<PrecisionGrabber>().Freeze(colliders[colliderID], localPosition, worldPosition, rotation, affRotation);
            }
        }
    }

    [PunRPC]
    public void RPCUnfreezeAll() {
        GetComponentInChildren<PrecisionGrabber>().Unfreeze(false);
    }
    [PunRPC]
    public void PickupEquipment(int id) {
        inventory.AddEquipment(Equipment.GetEquipmentFromID(id), EquipmentInventory.EquipmentChangeSource.Network);
    }
    [PunRPC]
    public void DropEquipment(int slot) {
        if (slot == -1 ) {
            inventory.Clear(EquipmentInventory.EquipmentChangeSource.Network, true);
            return;
        }
        inventory.RemoveEquipment(slot, EquipmentInventory.EquipmentChangeSource.Network, true);
    }
    public void InteractTo(Vector3 worldPosition, Quaternion worldRotation) {
        PumpUpDick(Time.deltaTime * 0.02f);
        uprightForce = Mathf.MoveTowards(uprightForce, 1f, Time.deltaTime*10f);
    }
    public void OnInteract(Kobold k) {
        grabbed = true;
        if (k != this) {
            controller.frictionMultiplier = 0.1f;
        }
    }
    public bool IsPenetrating(Kobold k) {
        foreach(var penetratable in k.penetratables) {
            foreach(var dickset in activeDicks) {
                if (penetratable.penetratable.ContainsPenetrator(dickset.dick)) {
                    return true;
                }
            }
        }
        return false;
    }
    public void OnEndInteract(Kobold k) {
        grabbed = false;
        if (NetworkManager.instance.localPlayerInstance == photonView && photonView && !photonView.IsMine) {
            photonView.RequestOwnership();
        }
        controller.frictionMultiplier = 1f;
        //uprightForce = 40f;
    }
    public bool ShowHand() { return true; }
    public bool PhysicsGrabbable() { return true; }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData != null && info.photonView.InstantiationData[0] is Hashtable) {
            //Debug.Log(info.photonView.InstantiationData[0]);
            Load((Hashtable)(info.photonView.InstantiationData[0]));
        }
    }

    public Rigidbody[] GetRigidBodies()
    {
        return allRigidbodies;
    }

    public Renderer[] GetRenderers() {
        return new Renderer[]{};
    }

    public Transform GrabTransform(Rigidbody b) {
        if (!body.isKinematic) {
            return hip;
        } else {
            return b.transform;
        }
    }

    public GrabbableType GetGrabbableType() {
        return GrabbableType.Kobold;
    }

    public void OnEventRaised(GameEventGeneric<float> e, float f) {
        stimulation = Mathf.MoveTowards(stimulation, 0f, f*0.1f);
        foreach (var belly in bellies) {
            ReagentContents vol = belly.container.contents.Metabolize(ReagentDatabase.instance, f);
            foreach (KeyValuePair<ReagentData.ID, Reagent> r in vol) {
                switch (r.Key) {
                    case ReagentData.ID.Cum:
                        //if (sex >= 0.5f) {
                        belly.container.contents.Mix(ReagentData.ID.Egg, r.Value.volume*4f, 1f, 310f);
                        //}
                        break;
                    case ReagentData.ID.MelonJuice:
                        foreach (var boob in boobs) {
                            boob.container.contents.Mix(ReagentData.ID.Fat, r.Value.volume*4f / boobs.Count, 1f, 310f, ReagentContents.ReagentInjectType.Metabolize);
                            boob.container.contents.Mix(ReagentData.ID.Milk, r.Value.volume*4f*0.33f / boobs.Count, 1f, 310f, ReagentContents.ReagentInjectType.Metabolize);
                            //boob.baseVolume += r.Value.volume*0.8f;
                        }
                        break;
                    //case ReagentData.ID.Blood:
                        //belly.container.contents[ReagentData.ID.Blood].volume = Mathf.Max(belly.container.contents[ReagentData.ID.Blood].volume, 10f);
                        //break;
                    case ReagentData.ID.EggplantJuice:
                        if (activeDicks.Count == 0) {
                            break;
                        }
                        foreach(var dickSet in activeDicks) {
                            dickSet.container.contents.Mix(ReagentData.ID.Fat, r.Value.volume*2f / activeDicks.Count, 1f, 310);
                        }
                        //desiredDickSize += (r.Value.volume * 0.8f) / (desiredDickSize + 1f);
                        break;
                    case ReagentData.ID.GrowthSerum:
                        foreach (var ss in subcutaneousStorage) {
                            ss.container.contents.Mix(ReagentData.ID.GrowthSerum, r.Value.volume/subcutaneousStorage.Count, r.Value.potentcy, r.Value.heat, ReagentContents.ReagentInjectType.Metabolize);
                        }
                        break;
                    case ReagentData.ID.MilkShake:
                        foreach (var ss in subcutaneousStorage) {
                            ss.container.contents.Mix(ReagentData.ID.Fat, r.Value.volume*2f/subcutaneousStorage.Count, r.Value.potentcy, r.Value.heat, ReagentContents.ReagentInjectType.Metabolize);
                        }
                        //foreach( var boob in boobs ) {
                        //if (boob.baseVolume < 4f) {
                        //boob.baseVolume = Mathf.MoveTowards(boob.baseVolume, 6f, r.Value.volume*3f);
                        //}
                        //}
                        break;
                    case ReagentData.ID.PineappleJuice:
                        foreach (var b in balls) {
                            b.contents.Mix(ReagentData.ID.Fat, r.Value.volume*3f/balls.Count, r.Value.potentcy, r.Value.heat, ReagentContents.ReagentInjectType.Metabolize);
                            b.contents.Mix(ReagentData.ID.Cum, r.Value.volume*1f/balls.Count, r.Value.potentcy, r.Value.heat, ReagentContents.ReagentInjectType.Metabolize);
                        }
                        break;
                }
            }
            if (belly.container.contents.ContainsKey(ReagentData.ID.Egg)) {
                if (belly.container.contents[ReagentData.ID.Egg].volume > 4f) {
                    foreach(var penetratableSet in penetratables) {
                        //if (penetratableSet.isFemaleExclusiveAnatomy && penetratableSet.penetratable.dickTarget == null) {
                        //if (penetratableSet.penetratable.dickTarget == null) {
                            //belly.container.contents[ReagentData.ID.Egg].volume -= 4f;
                            //belly.container.contents.TriggerChange();
                            //SpawnEggEvent.Invoke();
                        //}
                    }
                }
            }
        }
    }

    IEnumerator WaitAndThenStopGargling(float time) {
        yield return new WaitForSeconds(time);
        gurgleSource.Pause();
    }
    public void OnReagentContainerChanged(ReagentContents contents, ReagentContents.ReagentInjectType injectType) {
        if (injectType == ReagentContents.ReagentInjectType.Spray) {
            koboldAnimator.SetTrigger("Quaff");
            if (!gurgleSource.isPlaying) {
                gurgleSource.Play();
                gurgleSource.pitch = 0.9f + sex*0.4f;
                StartCoroutine(WaitAndThenStopGargling(0.25f));
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(ragdolled);
            stream.SendNext(hip.position);
        } else {
            bool ragged = (bool)stream.ReceiveNext();
            if (!ragdolled && ragged) {
                KnockOver(99999f);
            }
            if (ragdolled && !ragged) {
                StandUp();
            }
            networkedRagdollHipPosition = (Vector3)stream.ReceiveNext();
        }
    }
}
