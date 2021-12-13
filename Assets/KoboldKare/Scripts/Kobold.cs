using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using KoboldKare;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PenetrationTech;
using TMPro;
using System.IO;

public class Kobold : MonoBehaviourPun, IGrabbable, IAdvancedInteractable, IPunObservable, IPunInstantiateMagicCallback, ISavable {
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
    public float nextEggTime;


    public Task ragdollTask;

    public StatBlock statblock = new StatBlock();

    public List<PenetrableSet> penetratables = new List<PenetrableSet>();

    public List<Transform> attachPoints = new List<Transform>();

    public AudioClip[] yowls;
    public GenericLODConsumer lodLevel;
    public Transform root;
    public EggSpawner eggSpawner;
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
    public GenericReagentContainer balls;
    public BodyProportion bodyProportion;
    public UnityEvent OnRagdoll;
    public UnityEvent OnStandup;
    public TMPro.TMP_Text chatText;
    public UnityEvent OnEggFormed;
    public UnityEvent OnOrgasm;
    [HideInInspector]
    public List<DickInfo.DickSet> activeDicks = new List<DickInfo.DickSet>();

    public Grabber grabber;
    public AudioSource gurgleSource;
    public List<Renderer> koboldBodyRenderers;
    private float internalSex = 0f;
    [HideInInspector]
    public float sex {
        get {
            return internalSex;
        }
        set {
            foreach (Renderer r in koboldBodyRenderers) {
                if (!(r is SkinnedMeshRenderer)) {
                    continue;
                }
                SkinnedMeshRenderer bodyMesh = (SkinnedMeshRenderer)r;
                int index = bodyMesh.sharedMesh.GetBlendShapeIndex("MaleEncode");
                if (index == -1) {
                    continue;
                }
                bodyMesh.SetBlendShapeWeight(index,  Mathf.Clamp01(1f - value * 2f) * 100f);
            }
            internalSex = value;
        }
    }
    public Transform hip;
    public LayerMask playerHitMask;
    private float internalTopBottom;
    public float topBottom {
        get {
            return internalTopBottom;
        }
        set {
            if (Mathf.Approximately(internalTopBottom, value)) {
                return;
            }
            internalTopBottom = value;
            StandUp();
            bodyProportion.Initialize();
        }
    }
    private float internalThickness;
    public float thickness {
        get {
            return internalThickness;
        }
        set {
            if (Mathf.Approximately(internalThickness, value)) {
                return;
            }
            internalThickness = value;
            StandUp();
            bodyProportion.Initialize();
        }
    }
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
    public float arousal = 0f;
    private float internalBaseDickSize;
    public float baseDickSize {
        get {
            return internalBaseDickSize;
        }
        set {
            internalBaseDickSize = value;
            foreach(var dick in activeDicks) {
                dick.dickInflater.baseSize = baseDickSize;
            }
        }
    }
    private float internalBaseBoobSize;
    public float baseBoobSize {
        get {
            return internalBaseBoobSize;
        }
        set {
            internalBaseBoobSize = value;
            foreach(var boob in boobs) {
                boob.baseSize = baseBoobSize;
            }
        }
    }
    private float internalBaseBallSize;
    public float baseBallSize {
        get {
            return internalBaseBallSize;
        }
        set {
            internalBaseBallSize = value;
            foreach(var dick in activeDicks) {
                dick.balls.baseSize = baseBallSize;
            }
        }
    }
    public Coroutine displayMessageRoutine;
    public bool ragdolled {
        get {
            if (ragdollBodies[0] == null) {
                return false;
            }
            return uprightTimer > 0f;
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
    private Color internalHBCS;
    public Color HueBrightnessContrastSaturation {
        set {
            if (internalHBCS == value) {
                return;
            }
            foreach (Renderer r in koboldBodyRenderers) {
                if (r == null) {
                    continue;
                }
                foreach (Material m in r.materials) {
                    m.SetVector("_HueBrightnessContrastSaturation", value);
                }
            }
            internalHBCS = value;
        }
        get {
            return internalHBCS;
        }
    }
    //private bool incremented = false;
    //public AnimatorUpdateMode modeSave;

    //reiikz was here
    //rate of change for arousal when it's set to permanent
    public float arousalSpeed = 0.01f;
    //this is used to be able to tell if certain logic needs to be ran or not, variable gets set by PlayerKoboldLoader.cs
    public bool isPlayer = false;
    //if this is set to 1 the arousal behaves as noraml, if not it hovers between this x number and x - (x * .37)
    public float permanentArousal = 0.99f;
    //welp...
    public float slowUpdateRate = 2f;
    //I added an update that is called every two seconds
    public float nextSlowUpdate = 0f;
    //position vector used to move the player back to the house if they go outside the map
    public static Vector3 FallbackPos = new Vector3(-168.097824f, 200f, 317.367493f);
    //do I need to explain this?
    public float fertility = 1f;
    //maximum amount of cum that your belly can hold at any given time
    public float maximumCum = 100000f;
    //Holds weather arousal is going down or up when it's set to be permanent
    private bool arousalDirection = false;
    //next time the player's dick is going to soften a bit when set to permanent
    private float nextArousalDown = 0f;
    //probability of the player's arousal to go back up when it's going down (only afects it when set to ermanent)
    private static int[] arouseProb  = { 1, 1200 };

    public void Awake() {
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
    public void OnCompleteBodyProportion() {
        sizeInflatable.enabled = true;
        if (originalUprightTimer > 0f) {
            KnockOver(originalUprightTimer);
        }
    }
    public void RandomizeKobold() {
        sex = Random.Range(0f,1f);
        HueBrightnessContrastSaturation = new Vector4(Random.Range(0f,1f), Random.Range(0f,1f), Random.Range(0f,1f), Random.Range(0f,1f));

        if (Random.Range(0f,1f) > 0.5f) {
            Equipment dick = null;
            var equipments = EquipmentDatabase.GetEquipments();
            while (dick == null) {
                foreach(var equipment in equipments) {
                    if (equipment is DickEquipment && UnityEngine.Random.Range(0f,1f) > 0.9f) {
                        dick = equipment;
                    }
                }
            }
            GetComponent<KoboldInventory>().PickupEquipment(dick, null);
            baseBoobSize = Random.Range(0f,0.2f)*30f;
            baseBallSize = Random.Range(0.5f,1f)*40f;
            baseDickSize = Random.Range(0f,1f);
        } else {
            baseBoobSize = Random.Range(0.2f,1f)*30f;
            baseBallSize = 0f;
        }
        topBottom = Random.Range(-1f,1f);
        thickness = Random.Range(-1f,1f);

        sizeInflatable.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("GrowthSerum"), Random.Range(0.7f,1.2f) * sizeInflatable.reagentVolumeDivisor);
        RegenerateSlowly(1000f);
    }
    public void OnStatusEffectsChanged(StatBlock block, StatBlock.StatChangeSource source) {
        foreach (var statEvent in statChangedEvents) {
            statEvent.onChange.Invoke(block.GetStat(statEvent.changedStat));
        }
    }
    private void OnSteamAudioChanged(UnityScriptableSettings.ScriptableSetting setting) {
        foreach(AudioSource asource in GetComponentsInChildren<AudioSource>(true)) {
            asource.spatialize = setting.value > 0f;
        }
        //foreach(SteamAudio.SteamAudioSource source in GetComponentsInChildren<SteamAudio.SteamAudioSource>(true)) {
            //source.enabled = setting.value > 0f;
        //}
    }

    void Start() {
        statblock.AddStatusEffect(koboldStatus, StatBlock.StatChangeSource.Misc);
        lastPumpTime = Time.timeSinceLevelLoad;
        MetabolizeEvent.AddListener(OnEventRaised);
        foreach (var b in bellies) {
            b.GetContainer().OnChange.AddListener(OnReagentContainerChanged);
        }
        bodyProportion.OnComplete += OnCompleteBodyProportion;
        var steamAudioSetting = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("SteamAudio");
        steamAudioSetting.onValueChange -= OnSteamAudioChanged;
        steamAudioSetting.onValueChange += OnSteamAudioChanged;
        OnSteamAudioChanged(steamAudioSetting);
        bodyProportion.Initialize();
    }
    private void OnDestroy() {
        bodyProportion.OnComplete -= OnCompleteBodyProportion;
        statblock.StatusEffectsChangedEvent -= OnStatusEffectsChanged;
        MetabolizeEvent.RemoveListener(OnEventRaised);
        foreach (var b in bellies) {
            b.GetContainer().OnChange.RemoveListener(OnReagentContainerChanged);
        }
        if (photonView.IsMine) {
            PhotonNetwork.CleanRpcBufferIfMine(photonView);
        }
        var steamAudioSetting = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("SteamAudio");
        steamAudioSetting.onValueChange -= OnSteamAudioChanged;
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
        GetComponent<CharacterControllerAnimator>().OnEndStation();
        controller.frictionMultiplier = 0.1f;
        controller.enabled = false;
        return true;
    }
    public IEnumerator KnockOverRoutine() {
        // If we need jigglebones disabled, it takes TWO frames for it to take effect! So... here we wait!
        // Otherwise jigglebones will move rigidbodies and fuck stuff up...
        OnRagdoll.Invoke();
        sizeInflatable.enabled = false;
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
    }
    public void KnockOver(float duration = 3f) {
        //uprightTimer = Mathf.Max(Mathf.Max(0 + duration, uprightTimer + duration), 1f);
        if (bodyProportion.running) {
            originalUprightTimer = Mathf.Max(duration, originalUprightTimer);
            return;
        }
        originalUprightTimer = 0f;
        if (ragdolled) {
            StandUp();
        }
        uprightTimer = duration;
        if (ragdollTask != null && ragdollTask.Running) {
            ragdollTask.Stop();
        }
        ragdollTask = new Task(KnockOverRoutine());
    }
    // This was a huuuUUGE pain, but for somereason joints forget their initial orientation if you switch bodies.
    // I tried a billion different things to try to reset the initial orientation, this was the only thing that worked for me!
    public void StandUp() {
        uprightTimer = 0f;
        if ((!body.isKinematic && ragdollBodies[0].isKinematic)) {
            return;
        }
        sizeInflatable.enabled = true;
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
        //if it's a player and permanent arousal is set lower than one we make sure the player is always aroused but not quite fully aroused
        //if not we just do business as usual
        if(isPlayer && permanentArousal < 1){
            if(permanentArousal == 0f){
                arousal = 0;
            }else{
                if(arousalDirection){
                    arousal += ((permanentArousal * arousalSpeed)/(150*permanentArousal))*UnityEngine.Random.Range(1.3f, 1.7f);
                    arousal = Mathf.Clamp(arousal, (permanentArousal - (permanentArousal * 0.37f)), permanentArousal);
                    if(arousal >= permanentArousal) if(Time.timeSinceLevelLoad > nextArousalDown) arousalDirection = !arousalDirection;
                }else{
                    arousal -= (permanentArousal * arousalSpeed)/(150*permanentArousal) * UnityEngine.Random.Range(1f, 1.157f);
                    arousal = Mathf.Clamp01(arousal);
                    float target = permanentArousal * 0.37f;
                    if((arousal <= (permanentArousal - target)) || ( RandomChoice.WeightedIndex(arouseProb) == 0 ) ){
                        arousalDirection = !arousalDirection;
                        nextArousalDown = Time.timeSinceLevelLoad + UnityEngine.Random.Range(1f, 16f);
                    }
                }
            }
        }else{
            arousal += amount;
            arousal = Mathf.Clamp01(arousal);
        }
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
        //pickedUp = 0;
        //transSpeed = 1f;
    }
    private void Update() {
        foreach(var dick in activeDicks) {
            dick.bonerInflator.baseSize = arousal*0.92f + (0.08f * Mathf.Clamp01(Mathf.Sin(Time.time*2f)))*arousal;
        }
        //we make sure if it's time to call the slow update so unimportant stuff can be handled
        if(Time.timeSinceLevelLoad >= nextSlowUpdate){
            SlowUpdate();
            nextSlowUpdate = Time.timeSinceLevelLoad + slowUpdateRate;
        }
    }
    //update that runs at a fixed time and slow rate
    private void SlowUpdate(){
        //if the player is this far away we must bring them back and make them fat for some reason(?)
        if(Vector3.Distance(root.position, FallbackPos) > 3333333){
            body.velocity = new Vector3(0f, 0f, 0f);
            root.position = FallbackPos;
            var fat = ReagentDatabase.GetReagent("Fat");
            foreach (var ss in subcutaneousStorage) {
                ss.GetContainer().OverrideReagent(fat, 50);
            }
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
            float deflectionForgivenessDegrees = 5f;
            Vector3 cross = Vector3.Cross(body.transform.up, Vector3.up);
            float angleDiff = Mathf.Max(Vector3.Angle(body.transform.up, Vector3.up) - deflectionForgivenessDegrees, 0f);
            body.AddTorque(cross*angleDiff, ForceMode.Acceleration);
        }
        if (Time.timeSinceLevelLoad-lastPumpTime > 10f) {
            PumpUpDick(-Time.deltaTime * 0.01f);
        }
        if (!photonView.IsMine) {
            Vector3 dir = networkedRagdollHipPosition - hip.position;
            hip.GetComponent<Rigidbody>().AddForce(dir, ForceMode.VelocityChange);
        }
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
    public void SendChat(string message) {
        photonView.RPC("RPCSendChat", RpcTarget.All, new object[]{message});
    }
    [PunRPC]
    public void RPCSendChat(string message) {
        GameManager.instance.SpawnAudioClipInWorld(yowls[UnityEngine.Random.Range(0,yowls.Length)], transform.position);
        if (displayMessageRoutine != null) {
            StopCoroutine(displayMessageRoutine);
        }
        displayMessageRoutine = StartCoroutine(DisplayMessage(message,5f));
    }
    IEnumerator DisplayMessage(string message, float duration) {
        chatText.text = message;
        chatText.alpha = 1f;
        yield return new WaitForSeconds(duration);
        float endTime = Time.time + 1f;
        while(Time.time < endTime) {
            chatText.alpha = endTime-Time.time;
            yield return null;
        }
        chatText.alpha = 0f;
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
        controller.frictionMultiplier = 1f;
        //uprightForce = 40f;
    }
    public bool ShowHand() { return true; }
    public bool PhysicsGrabbable() { return true; }
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

    public void RegenerateSlowly(float deltaTime) {
        float wantedBoobVolume = baseBoobSize*0.2f;
        foreach(var boob in boobs) {
            var milk = ReagentDatabase.GetReagent("Milk");
            float currentVolume = boob.GetContainer().GetVolumeOf(milk);
            if (currentVolume < wantedBoobVolume) {
                boob.GetContainer().OverrideReagent(milk, Mathf.MoveTowards(currentVolume, wantedBoobVolume, deltaTime));
            }
        }
        var cum = ReagentDatabase.GetReagent("Cum");
        float wantedCumVolume = baseBallSize*0.2f;
        float currentCumVolume = balls.GetVolumeOf(cum);
        if (currentCumVolume < wantedCumVolume) {
            balls.OverrideReagent(cum, Mathf.MoveTowards(currentCumVolume, wantedBoobVolume, deltaTime));
        }
    }

    public void OnEventRaised(float f) {
        stimulation = Mathf.MoveTowards(stimulation, 0f, f*0.1f);
        foreach (var belly in bellies) {
            ReagentContents vol = belly.GetContainer().Metabolize(f);
            //if fertility is set to 0 we make sure the player doesn't have cum in them otherwise it behaves normally-ish (the cum metabolization gets multiplied by the fertility)
            if(isPlayer){
                if(fertility == 0){
                    belly.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("Egg"), 0f);
                    belly.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("Cum"), 0f);
                }else{
                    //handle if the cum cap
                    if(belly.GetContainer().GetVolumeOf(ReagentDatabase.GetReagent("Cum")) > maximumCum){
                        belly.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("Cum"), maximumCum);
                    }
                    if(belly.GetContainer().GetVolumeOf(ReagentDatabase.GetReagent("Egg")) > maximumCum){
                        belly.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("Egg"), maximumCum);
                    }
                    if(maximumCum > 0){
                        belly.GetContainer().AddMix(ReagentDatabase.GetReagent("Egg"), vol.GetVolumeOf(ReagentDatabase.GetReagent("Cum"))*3f*fertility, GenericReagentContainer.InjectType.Metabolize);
                    }
                }
            }else{
                belly.GetContainer().AddMix(ReagentDatabase.GetReagent("Egg"), vol.GetVolumeOf(ReagentDatabase.GetReagent("Cum"))*3f, GenericReagentContainer.InjectType.Metabolize);
            }
            float melonJuiceVolume = vol.GetVolumeOf(ReagentDatabase.GetReagent("MelonJuice"));
            foreach (var boob in boobs) {
                baseBoobSize += melonJuiceVolume / boobs.Count;
                boob.GetContainer().AddMix(ReagentDatabase.GetReagent("Milk"), melonJuiceVolume / boobs.Count, GenericReagentContainer.InjectType.Metabolize);
            }
            float eggplantJuiceVolume = vol.GetVolumeOf(ReagentDatabase.GetReagent("EggplantJuice"));
            baseDickSize += eggplantJuiceVolume;
            float growthSerumVolume = vol.GetVolumeOf(ReagentDatabase.GetReagent("GrowthSerum"));
            foreach (var ss in subcutaneousStorage) {
                ss.GetContainer().AddMix(ReagentDatabase.GetReagent("GrowthSerum"), growthSerumVolume/subcutaneousStorage.Count, GenericReagentContainer.InjectType.Metabolize);
            }
            float milkShakeVolume = vol.GetVolumeOf(ReagentDatabase.GetReagent("MilkShake"));
            foreach (var ss in subcutaneousStorage) {
                ss.GetContainer().AddMix(ReagentDatabase.GetReagent("Fat"), milkShakeVolume*2f/subcutaneousStorage.Count, GenericReagentContainer.InjectType.Metabolize);
            }
            float pineappleJuiceVolume = vol.GetVolumeOf(ReagentDatabase.GetReagent("PineappleJuice"));
            baseBallSize += pineappleJuiceVolume;
            balls.AddMix(ReagentDatabase.GetReagent("Cum"), pineappleJuiceVolume*1f, GenericReagentContainer.InjectType.Metabolize);

            if (Time.timeSinceLevelLoad > nextEggTime && fertility > 0) {
                float currentEggVolume = belly.GetContainer().GetVolumeOf(ReagentDatabase.GetReagent("Egg"));
                if (currentEggVolume > 8f) {
                    OnEggFormed.Invoke();
                    nextEggTime = Time.timeSinceLevelLoad + 60f;
                    bool spawnedEgg = false;
                    foreach(var penetratableSet in penetratables) {
                        if (penetratableSet.isFemaleExclusiveAnatomy && penetratableSet.penetratable.isActiveAndEnabled) {
                            eggSpawner.targetPenetrable = penetratableSet.penetratable;
                            eggSpawner.spawnAlongLength = 1f;
                            eggSpawner.SpawnEgg();
                            spawnedEgg = true;
                            break;
                        }
                    }
                    if (!spawnedEgg) {
                        foreach(var penetratableSet in penetratables) {
                            if (penetratableSet.penetratable.isActiveAndEnabled) {
                                eggSpawner.targetPenetrable = penetratableSet.penetratable;
                                eggSpawner.spawnAlongLength = 0.5f;
                                eggSpawner.SpawnEgg();
                                spawnedEgg = true;
                                break;
                            } 
                        }
                    }
                    if (spawnedEgg) {
                        belly.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("Egg"), currentEggVolume-8f);
                    }
                }
            }
            RegenerateSlowly(f*0.05f);
        }
    }

    IEnumerator WaitAndThenStopGargling(float time) {
        yield return new WaitForSeconds(time);
        gurgleSource.Pause();
    }
    public void OnReagentContainerChanged(GenericReagentContainer.InjectType injectType) {
        if (injectType != GenericReagentContainer.InjectType.Spray) {
            return;
        }
        koboldAnimator.SetTrigger("Quaff");
        if (!gurgleSource.isPlaying) {
            gurgleSource.Play();
            gurgleSource.pitch = 0.9f + sex*0.4f;
            StartCoroutine(WaitAndThenStopGargling(0.25f));
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(ragdolled);
            stream.SendNext(hip.position);
            stream.SendNext(thickness);
            stream.SendNext(topBottom);
            stream.SendNext(sex);
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.r*255f));
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.g*255f));
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.b*255f));
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.a*255f));
            stream.SendNext(baseBallSize);
            stream.SendNext(baseBoobSize);
            stream.SendNext(baseDickSize);
        } else {
            bool ragged = (bool)stream.ReceiveNext();
            if (!ragdolled && ragged) {
                KnockOver(99999f);
            }
            if (ragdolled && !ragged) {
                StandUp();
            }
            networkedRagdollHipPosition = (Vector3)stream.ReceiveNext();
            thickness = (float)stream.ReceiveNext();
            topBottom = (float)stream.ReceiveNext();
            sex = (float)stream.ReceiveNext();
            byte r = (byte)stream.ReceiveNext();
            byte g = (byte)stream.ReceiveNext();
            byte b = (byte)stream.ReceiveNext();
            byte a = (byte)stream.ReceiveNext();
            var col = new Color((float)r/255f,(float)g/255f,(float)b/255f,(float)a/255f);
            HueBrightnessContrastSaturation = col;
            baseBallSize = (float)stream.ReceiveNext();
            baseBoobSize = (float)stream.ReceiveNext();
            baseDickSize = (float)stream.ReceiveNext();
        }
    }

    public void OnThrow(Kobold kobold) {
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData != null && info.photonView.InstantiationData.Length != 0) {
            info.Sender.TagObject = this;
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(ragdolled);
        writer.Write(hip.position.x);
        writer.Write(hip.position.y);
        writer.Write(hip.position.z);
        writer.Write(thickness);
        writer.Write(topBottom);
        writer.Write(sex);
        writer.Write((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.r*255f));
        writer.Write((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.g*255f));
        writer.Write((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.b*255f));
        writer.Write((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.a*255f));
        writer.Write(baseBallSize);
        writer.Write(baseBoobSize);
        writer.Write(baseDickSize);
    }

    public void Load(BinaryReader reader, string version) {
        bool ragged = reader.ReadBoolean();
        if (!ragdolled && ragged) {
            KnockOver(99999f);
        }
        if (ragdolled && !ragged) {
            StandUp();
        }
        float hipx = reader.ReadSingle();
        float hipy = reader.ReadSingle();
        float hipz = reader.ReadSingle();
        networkedRagdollHipPosition = new Vector3(hipx,hipy,hipz);

        thickness = reader.ReadSingle();
        topBottom = reader.ReadSingle();
        sex = reader.ReadSingle();
        byte r = reader.ReadByte();
        byte g = reader.ReadByte();
        byte b = reader.ReadByte();
        byte a = reader.ReadByte();
        var col = new Color((float)r/255f,(float)g/255f,(float)b/255f,(float)a/255f);
        HueBrightnessContrastSaturation = col;

        baseBallSize = reader.ReadSingle();
        baseBoobSize = reader.ReadSingle();
        baseDickSize = reader.ReadSingle();
    }
}
