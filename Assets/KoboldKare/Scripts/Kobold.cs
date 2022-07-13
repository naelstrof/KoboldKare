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
using Naelstrof.BodyProportion;
using Naelstrof.Inflatable;

public class Kobold : MonoBehaviourPun, IGrabbable, IAdvancedInteractable, IPunObservable, IPunInstantiateMagicCallback, ISavable, IValuedGood {
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

    public float nextEggTime;
    public StatBlock statblock = new StatBlock();

    public List<PenetrableSet> penetratables = new List<PenetrableSet>();

    public List<Transform> attachPoints = new List<Transform>();

    public AudioClip[] yowls;
    public Transform root;
    public EggSpawner eggSpawner;
    public Animator animator;
    public Rigidbody body;
    public GameEventFloat MetabolizeEvent;
    
    private float baseBoobSize;

    public GenericReagentContainer bellyContainer { get; private set; }
    [SerializeField]
    private Inflatable belly;
    [SerializeField]
    private Inflatable fatnessInflater;
    [SerializeField]
    private Inflatable sizeInflater;
    [SerializeField]
    private Inflatable boobs;
    private ReagentContents boobContents;
    private ReagentContents ballsContents;
    
    [SerializeField]
    private AudioPack tummyGrumbles;
    
    public BodyProportionSimple bodyProportion;
    public TMPro.TMP_Text chatText;
    public float textSpeedPerCharacter, minTextTimeout;
    public UnityEvent OnEggFormed;
    public UnityEvent OnOrgasm;
    [HideInInspector]
    public List<DickInfo.DickSet> activeDicks = new List<DickInfo.DickSet>();

    public Grabber grabber;
    private AudioSource gurgleSource;
    private AudioSource tummyGrumbleSource;
    public Rigidbody[] grabbableBodies;
    public List<Renderer> koboldBodyRenderers;
    private float internalSex = 0f;
    [SerializeField]
    private AnimationCurve bounceCurve;
    [HideInInspector]
    public float sex {
        get => internalSex;
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
    public KoboldCharacterController controller;
    public float stimulation = 0f;
    public float stimulationMax = 30f;
    public float stimulationMin = -30f;
    public UnityEvent SpawnEggEvent;
    //public KoboldUseEvent onGrabEvent;
    public float uprightForce = 10f;
    public Animator koboldAnimator;
    private float lastPumpTime = 0f;
    private bool grabbed = false;
    private List<Vector3> savedJointAnchors = new List<Vector3>();
    public float arousal = 0f;
    public GameObject nipplePumps;
    public GameObject nippleBarbells;
    public float baseSize { get; private set; }
    public float baseFatness { get; private set; }
    public float baseDickSize { get; private set; }
    public float baseBallsSize { get; private set; }

    public IEnumerable<InflatableListener> GetAllInflatableListeners() {
        foreach (var listener in belly.GetInflatableListeners()) {
            yield return listener;
        }
        foreach (var listener in fatnessInflater.GetInflatableListeners()) {
            yield return listener;
        }
        foreach (var listener in boobs.GetInflatableListeners()) {
            yield return listener;
        }
    }

    public Coroutine displayMessageRoutine;
    public Ragdoller ragdoller;
    public void AddStimulation(float s) {
        stimulation += s;
        if (stimulation >= stimulationMax) {
            OnOrgasm.Invoke();
            foreach(var dickSet in activeDicks) {
                float cumAmount = 0.5f+0.1f*baseSize+0.5f*baseBallsSize+0.1f*baseDickSize; // Bonus!
                ballsContents.AddMix(ReagentDatabase.GetReagent("Cum").GetReagent(cumAmount));
                // TODO: This is a really, really terrible way to make a dick cum lol. Clean this up.
                StartCoroutine(dickSet.info.CumRoutine(dickSet));
            }
            PumpUpDick(1f);
            stimulation = stimulationMin;
        }
    }

    public ReagentContents GetBallsContents() {
        return ballsContents;
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
    private static readonly int BrightnessContrastSaturation = Shader.PropertyToID("_HueBrightnessContrastSaturation");

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
                    m.SetVector(BrightnessContrastSaturation, value);
                }

                foreach (var dickSet in activeDicks) {
                    foreach (var rendererMask in dickSet.dick.GetTargetRenderers()) {
                        foreach (Material m in rendererMask.renderer.materials) {
                            m.SetVector(BrightnessContrastSaturation, value);
                        }
                    }
                }
            }
            internalHBCS = value;
        }
        get {
            return internalHBCS;
        }
    }

    private void OnBoobContentsChanged(ReagentContents contents) {
        boobs.SetSize(Mathf.Log(1f + (contents.volume + baseBoobSize) / 20f, 2f), this);
    }
    private void OnBallsContentsChanged(ReagentContents contents) {
        foreach (var dickSet in activeDicks) {
            dickSet.ballSizeInflater.SetSize(1f+Mathf.Log(1f + (contents.volume + baseBallsSize) / 20f, 2f), this);
        }
    }

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
    private static int[] arouseProb  = { 1, 2000 };
    public bool gay = false;
    public float metabolizationRate = 1;

    public void Awake() {
        statblock.StatusEffectsChangedEvent += OnStatusEffectsChanged;
        ballsContents = new ReagentContents();
        ballsContents.changed += OnBallsContentsChanged;
        boobContents = new ReagentContents();
        boobContents.changed += OnBoobContentsChanged;
        bellyContainer = gameObject.AddComponent<GenericReagentContainer>();
        bellyContainer.type = GenericReagentContainer.ContainerType.Mouth;
        photonView.ObservedComponents.Add(bellyContainer);
        belly.OnEnable();
        sizeInflater.OnEnable();
        boobs.OnEnable();
        fatnessInflater.OnEnable();

        if (tummyGrumbleSource == null) {
            tummyGrumbleSource = gameObject.AddComponent<AudioSource>();
            tummyGrumbleSource.playOnAwake = false;
            tummyGrumbleSource.maxDistance = 10f;
            tummyGrumbleSource.minDistance = 0.2f;
            tummyGrumbleSource.rolloffMode = AudioRolloffMode.Linear;
            tummyGrumbleSource.spatialBlend = 1f;
            tummyGrumbleSource.loop = true;
        }
        
        if (gurgleSource == null) {
            gurgleSource = gameObject.AddComponent<AudioSource>();
            gurgleSource.playOnAwake = false;
            gurgleSource.maxDistance = 10f;
            gurgleSource.minDistance = 0.2f;
            gurgleSource.rolloffMode = AudioRolloffMode.Linear;
            gurgleSource.spatialBlend = 1f;
            gurgleSource.loop = true;
        }
        belly.AddListener(new InflatableSoundPack(tummyGrumbles, tummyGrumbleSource));
    }
    private float[] GetRandomProperties(float totalBudget, int count) {
        float[] properties = new float[count];
        float sum = 0f;
        for (int i=0;i<count;i++) {
            properties[i] = Random.Range(0f,totalBudget);
            sum += properties[i];
        }
        float x = totalBudget/sum;
        for (int i=0;i<count;i++) {
            properties[i] *= x;
        }
        return properties;
    }

    public void RandomizeKobold() {
        sex = Random.Range(0f,1f);
        HueBrightnessContrastSaturation = new Vector4(Random.Range(0f,1f), Random.Range(0.1f,1f), Random.Range(0f,1f), Random.Range(0f,1f));


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
            SetBaseBoobSize(Random.Range(0f, 15f));
            SetBaseBallsSize(Random.Range(20f,40f));
            SetBaseDickSize(Random.Range(0f, 1f));
        } else {
            SetBaseBoobSize(Random.Range(6f, 30f));
            SetBaseBallsSize(0f);
            SetBaseDickSize(0f);
        }
        bodyProportion.SetTopBottom(Random.Range(-1f,1f));
        bodyProportion.SetThickness(Random.Range(-1f,1f));

        SetBaseSize(Random.Range(14f, 24f));
        RegenerateSlowly(1000f);
        PumpUpDick(-1f);
    }

    public void SetBaseBoobSize(float baseSize) {
        baseBoobSize = baseSize;
        OnBoobContentsChanged(boobContents);
    }
    public void SetBaseBallsSize(float baseSize) {
        baseBallsSize = baseSize;
        OnBallsContentsChanged(ballsContents);
    }

    public void SetBaseDickSize(float baseSize) {
        baseDickSize = baseSize;
        foreach (var dickSet in activeDicks) {
            dickSet.dickSizeInflater.SetSize(0.7f+Mathf.Log(1f + (baseSize) / 20f, 2f), this);
        }
    }
    public void SetBaseSize(float newSize) {
        baseSize = newSize;
        sizeInflater.SetSize(Mathf.Max(Mathf.Log(1f+baseSize/20f,2f), 0.2f), this);
    }
    public void SetBaseFatness(float newFatness) {
        baseFatness = newFatness;
        fatnessInflater.SetSize(Mathf.Log(1f + (baseFatness) / 20f, 2f), this);
    }

    public void OnStatusEffectsChanged(StatBlock block, StatBlock.StatChangeSource source) {
        foreach (var statEvent in statChangedEvents) {
            statEvent.onChange.Invoke(block.GetStat(statEvent.changedStat));
        }
    }

    void Start() {
        statblock.AddStatusEffect(koboldStatus, StatBlock.StatChangeSource.Misc);
        lastPumpTime = Time.timeSinceLevelLoad;
        MetabolizeEvent.AddListener(OnEventRaised);
        bellyContainer.OnChange.AddListener(OnBellyContentsChanged);
    }
    private void OnDestroy() {
        statblock.StatusEffectsChangedEvent -= OnStatusEffectsChanged;
        MetabolizeEvent.RemoveListener(OnEventRaised);
        bellyContainer.OnChange.RemoveListener(OnBellyContentsChanged);
        if (photonView.IsMine && PhotonNetwork.InRoom) {
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
        //GetComponent<CharacterControllerAnimator>().OnEndStation();
        GetComponent<CharacterControllerAnimator>().StopAnimation();
        controller.frictionMultiplier = 0.1f;
        controller.enabled = false;
        return true;
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
    public IEnumerator ThrowRoutine() {
        ragdoller.PushRagdoll();
        yield return new WaitForSeconds(3f);
        ragdoller.PopRagdoll();
    }
    public void OnRelease(Kobold kobold) {
        //animator.updateMode = modeSave;
        animator.SetBool("Carried", false);
        if (body.velocity.magnitude > 3f) {
            StartCoroutine(ThrowRoutine());
        } else {
            foreach(Collider c in Physics.OverlapSphere(transform.position, 1f, playerHitMask, QueryTriggerInteraction.Collide)) {
                Kobold k = c.GetComponentInParent<Kobold>();
                if (k!=this && k!=kobold) {
                    k.GetComponentInChildren<GenericUsable>().LocalUse(this);
                }
            }
            controller.enabled = true;
        }
        controller.frictionMultiplier = 1f;
        grabbed = false;
    }
    private void Update() {
        // Throbbing!
        foreach(var dick in activeDicks) {
            dick.bonerInflater.SetSize(arousal*0.95f + (0.05f * Mathf.Clamp01(Mathf.Sin(Time.time*2f)))*arousal, this);
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
        if((Vector3.Distance(root.position, FallbackPos) > 950000) && (!grabber.grabbing) && (!grabbed)){
            body.velocity = new Vector3(0f, 0f, 0f);
            root.position = FallbackPos;
            var fat = ReagentDatabase.GetReagent("Fat");
            foreach (var ss in subcutaneousStorage) {
                ss.GetContainer().OverrideReagent(fat, 500);
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
        if (!ragdoller.ragdolled) {
            body.angularVelocity -= body.angularVelocity*0.2f;
            float deflectionForgivenessDegrees = 5f;
            Vector3 cross = Vector3.Cross(body.transform.up, Vector3.up);
            float angleDiff = Mathf.Max(Vector3.Angle(body.transform.up, Vector3.up) - deflectionForgivenessDegrees, 0f);
            body.AddTorque(cross*angleDiff, ForceMode.Acceleration);
        }
        if (Time.timeSinceLevelLoad-lastPumpTime > 10f) {
            PumpUpDick(-Time.deltaTime * 0.01f);
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
        displayMessageRoutine = StartCoroutine(DisplayMessage(message,minTextTimeout));
    }
    IEnumerator DisplayMessage(string message, float duration) {
        chatText.text = message;
        chatText.alpha = 1f;
        duration += message.Length * textSpeedPerCharacter; // Add additional seconds per character

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
        //TODO: add functionality so we can determine if which dick is penetrated where.
        /*foreach(var penetratable in k.penetratables) {
            foreach(var dickset in activeDicks) {
                if (penetratable.penetratable.ContainsPenetrator(dickset.dick)) {
                    return true;
                }
            }
        }*/
        return false;
    }
    public void OnEndInteract(Kobold k) {
        grabbed = false;
        controller.frictionMultiplier = 1f;
        //uprightForce = 40f;
    }
    public bool ShowHand() { return true; }
    public bool PhysicsGrabbable() { return true; }
    public Rigidbody[] GetRigidBodies() {
        return grabbableBodies;
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

    /*public GrabbableType GetGrabbableType() {
        return GrabbableType.Kobold;
    }*/

    public void RegenerateSlowly(float deltaTime) {
        float wantedBoobVolume = baseBoobSize*0.2f;
        var milk = ReagentDatabase.GetReagent("Milk");
        float currentVolume = boobContents.GetVolumeOf(milk);
        if (currentVolume < wantedBoobVolume) {
            boobContents.OverrideReagent(ReagentDatabase.GetID(milk), Mathf.MoveTowards(currentVolume, wantedBoobVolume, deltaTime));
        }
        var cum = ReagentDatabase.GetReagent("Cum");
        
        float wantedCumVolume = baseBallsSize*0.2f;
        float currentCumVolume = ballsContents.GetVolumeOf(cum);
        if (currentCumVolume < wantedCumVolume) {
            ballsContents.OverrideReagent(ReagentDatabase.GetID(cum), Mathf.MoveTowards(currentCumVolume, wantedCumVolume, deltaTime));
        }
    }
    private float FloorNearestPower(float baseNum, float target) {
        float f = baseNum;
        for(;f<=target;f*=baseNum) {}
        return f/baseNum;
    }
    public void OnEventRaised(float f) {
        stimulation = Mathf.MoveTowards(stimulation, 0f, f*0.1f);
        ReagentContents vol = bellyContainer.Metabolize(f);
        bellyContainer.AddMix(ReagentDatabase.GetReagent("Egg"), vol.GetVolumeOf(ReagentDatabase.GetReagent("Cum"))*3f, GenericReagentContainer.InjectType.Metabolize);
        float melonJuiceVolume = vol.GetVolumeOf(ReagentDatabase.GetReagent("MelonJuice"));
        
        SetBaseBoobSize(baseBoobSize+melonJuiceVolume);
        
        float eggplantJuiceVolume = vol.GetVolumeOf(ReagentDatabase.GetReagent("EggplantJuice"));
        SetBaseDickSize(baseDickSize+eggplantJuiceVolume);
        float growthSerumVolume = vol.GetVolumeOf(ReagentDatabase.GetReagent("GrowthSerum"));
        SetBaseSize(baseSize + growthSerumVolume);
        float milkShakeVolume = vol.GetVolumeOf(ReagentDatabase.GetReagent("MilkShake"));
        SetBaseFatness(baseFatness + milkShakeVolume);
        float pineappleJuiceVolume = vol.GetVolumeOf(ReagentDatabase.GetReagent("PineappleJuice"));
        SetBaseBallsSize(baseBallsSize+pineappleJuiceVolume);

        if (Time.timeSinceLevelLoad > nextEggTime && photonView.IsMine) {
            float currentEggVolume = bellyContainer.GetVolumeOf(ReagentDatabase.GetReagent("Egg"));
            float eggSize = FloorNearestPower(5f,currentEggVolume);
            if (eggSize >= 5f && eggSize < bellyContainer.GetVolumeOf(ReagentDatabase.GetReagent("Cum"))) {
                OnEggFormed.Invoke();
                nextEggTime = Time.timeSinceLevelLoad + 30f;
                bool spawnedEgg = false;
                foreach(var penetratableSet in penetratables) {
                    if (penetratableSet.isFemaleExclusiveAnatomy && penetratableSet.penetratable.isActiveAndEnabled) {
                        eggSpawner.targetPenetrable = penetratableSet.penetratable;
                        eggSpawner.spawnAlongLength = 1f;
                        eggSpawner.SpawnEgg(eggSize);
                        spawnedEgg = true;
                        break;
                    }
                }
                if (!spawnedEgg) {
                    foreach(var penetratableSet in penetratables) {
                        if (penetratableSet.penetratable.isActiveAndEnabled) {
                            eggSpawner.targetPenetrable = penetratableSet.penetratable;
                            eggSpawner.spawnAlongLength = 0.5f;
                            eggSpawner.SpawnEgg(eggSize);
                            spawnedEgg = true;
                            break;
                        } 
                    }
                }
                if (spawnedEgg) {
                    bellyContainer.OverrideReagent(ReagentDatabase.GetReagent("Egg"), currentEggVolume-eggSize);
                }
            }
        }
        RegenerateSlowly(f*0.05f);
    }

    IEnumerator WaitAndThenStopGargling(float time) {
        yield return new WaitForSeconds(time);
        gurgleSource.Pause();
        gurgleSource.enabled = false;
    }
    public void OnBellyContentsChanged(GenericReagentContainer.InjectType injectType) {
        belly.SetSize(Mathf.Log(1f + bellyContainer.volume / 80f, 2f), this);
        if (injectType != GenericReagentContainer.InjectType.Spray) {
            return;
        }
        koboldAnimator.SetTrigger("Quaff");
        if (!gurgleSource.isPlaying) {
            gurgleSource.enabled = true;
            gurgleSource.Play();
            gurgleSource.pitch = 0.9f + sex*0.4f;
            StartCoroutine(WaitAndThenStopGargling(0.25f));
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(sex);
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.r*255f));
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.g*255f));
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.b*255f));
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.a*255f));
            stream.SendNext(baseBallsSize);
            stream.SendNext(baseBoobSize);
            stream.SendNext(baseDickSize);
            stream.SendNext(baseFatness);
            stream.SendNext(baseSize);
        } else {
            sex = (float)stream.ReceiveNext();
            byte r = (byte)stream.ReceiveNext();
            byte g = (byte)stream.ReceiveNext();
            byte b = (byte)stream.ReceiveNext();
            byte a = (byte)stream.ReceiveNext();
            var col = new Color((float)r/255f,(float)g/255f,(float)b/255f,(float)a/255f);
            HueBrightnessContrastSaturation = col;
            SetBaseBallsSize((float)stream.ReceiveNext());
            SetBaseBoobSize((float)stream.ReceiveNext());
            SetBaseDickSize((float)stream.ReceiveNext());
            SetBaseFatness((float)stream.ReceiveNext());
            SetBaseSize((float)stream.ReceiveNext());
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
        writer.Write(sex);
        writer.Write((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.r*255f));
        writer.Write((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.g*255f));
        writer.Write((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.b*255f));
        writer.Write((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.a*255f));
        writer.Write(baseBallsSize);
        writer.Write(baseBoobSize);
        writer.Write(baseDickSize);
        writer.Write(baseFatness);
        writer.Write(baseSize);
    }

    public void Lactate() {
    }

    private void LactateRoutine() {
    }

    public void Load(BinaryReader reader, string version) {
        sex = reader.ReadSingle();
        byte r = reader.ReadByte();
        byte g = reader.ReadByte();
        byte b = reader.ReadByte();
        byte a = reader.ReadByte();
        var col = new Color((float)r/255f,(float)g/255f,(float)b/255f,(float)a/255f);
        HueBrightnessContrastSaturation = col;

        SetBaseBallsSize(reader.ReadSingle());
        SetBaseBoobSize(reader.ReadSingle());
        SetBaseDickSize(reader.ReadSingle());
        SetBaseFatness(reader.ReadSingle());
        SetBaseSize(reader.ReadSingle());
    }

    public float GetWorth() {
        return Mathf.Log(1f+(baseSize + baseDickSize + baseBoobSize + baseFatness),2);
    }
}
