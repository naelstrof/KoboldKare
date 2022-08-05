using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using KoboldKare;
using Photon.Pun;
using PenetrationTech;
using System.IO;
using Naelstrof.Inflatable;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Kobold : GeneHolder, IGrabbable, IAdvancedInteractable, IPunObservable, IPunInstantiateMagicCallback, ISavable, IValuedGood {
    public StatusEffect koboldStatus;
    [System.Serializable]
    public class PenetrableSet {
        public Penetrable penetratable;
        public Rigidbody ragdollAttachBody;
        public bool isFemaleExclusiveAnatomy = false;
    }

    public delegate void EnergyChangedAction(int value, int maxValue);
    public event EnergyChangedAction energyChanged;

    public float nextEggTime;
    public List<PenetrableSet> penetratables = new List<PenetrableSet>();

    public List<Transform> attachPoints = new List<Transform>();

    public AudioClip[] yowls;
    public Animator animator;
    public Rigidbody body;
    public GameEventFloat MetabolizeEvent;
    public GameEventGeneric MidnightEvent;
    

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
    public ReagentContents metabolizedContents;
    
    [SerializeField]
    private byte energy = 1;

    [SerializeField]
    private AudioPack tummyGrumbles;
    [FormerlySerializedAs("gurglePack")] [SerializeField]
    private AudioPack garglePack;
    
    public BodyProportionSimple bodyProportion;
    public TMPro.TMP_Text chatText;
    public float textSpeedPerCharacter, minTextTimeout;
    public UnityEvent OnOrgasm;
    [HideInInspector]
    public List<DickInfo.DickSet> activeDicks = new List<DickInfo.DickSet>();
    private AudioSource gargleSource;
    private AudioSource tummyGrumbleSource;
    public Rigidbody[] grabbableBodies;
    public List<Renderer> koboldBodyRenderers;
    private float internalSex = 0f;
    [SerializeField]
    private List<Transform> nipples;
    public Transform hip;
    public LayerMask playerHitMask;
    public KoboldCharacterController controller;
    public float stimulation = 0f;
    public float stimulationMax = 30f;
    public float stimulationMin = -30f;
    public UnityEvent SpawnEggEvent;
    public float uprightForce = 10f;
    public Animator koboldAnimator;
    private float lastPumpTime = 0f;
    private bool grabbed = false;
    private List<Vector3> savedJointAnchors = new List<Vector3>();
    public float arousal = 0f;
    public GameObject nipplePumps;
    public GameObject nippleBarbells;
    
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

    public List<Transform> GetNipples() {
        return nipples;
    }

    public Coroutine displayMessageRoutine;
    public Ragdoller ragdoller;
    public void AddStimulation(float s) {
        stimulation += s;
        if (photonView.IsMine && stimulation >= stimulationMax && TryConsumeEnergy(1)) {
            photonView.RPC(nameof(Cum), RpcTarget.All);
        }
    }
    
    [PunRPC]
    public void Cum() {
        OnOrgasm.Invoke();
        foreach(var dickSet in activeDicks) {
            float cumAmount = 0.5f+0.1f*GetGenes().baseSize+0.5f*GetGenes().ballSize+0.1f*GetGenes().dickSize; // Bonus!
            ballsContents.AddMix(ReagentDatabase.GetReagent("Cum").GetReagent(cumAmount));
            // TODO: This is a really, really terrible way to make a dick cum lol. Clean this up.
            dickSet.info.StartCoroutine(dickSet.info.CumRoutine(dickSet));
        }
        PumpUpDick(1f);
        stimulation = stimulationMin;
    }

    public ReagentContents GetBallsContents() {
        return ballsContents;
    }

    public bool TryConsumeEnergy(byte amount) {
        if (energy < amount) {
            return false;
        }
        energy -= amount;
        energyChanged?.Invoke(energy, GetGenes().maxEnergy);
        return true;
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
    private void OnBoobContentsChanged(ReagentContents contents) {
        boobs.SetSize(Mathf.Log(1f + (contents.volume + GetGenes().breastSize) / 20f, 2f), this);
    }
    private void OnBallsContentsChanged(ReagentContents contents) {
        foreach (var dickSet in activeDicks) {
            dickSet.ballSizeInflater.SetSize(0.7f+Mathf.Log(1f + (contents.volume + GetGenes().ballSize) / 20f, 2f), dickSet.info);
        }
    }

    public int GetEnergy() {
        return energy;
    }
    public int GetMaxEnergy() {
        if (GetGenes() == null) {
            return 1;
        }

        return GetGenes().maxEnergy;
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

    public override void SetGenes(KoboldGenes newGenes) {
        foreach (var dickSet in activeDicks) {
            dickSet.dickSizeInflater.SetSize(0.7f+Mathf.Log(1f + (newGenes.dickSize) / 20f, 2f), dickSet.info);
        }
        sizeInflater.SetSize(Mathf.Max(Mathf.Log(1f+newGenes.baseSize/20f,2f), 0.2f), this);
        fatnessInflater.SetSize(Mathf.Log(1f + (newGenes.fatSize) / 20f, 2f), this);
        bellyContainer.maxVolume = newGenes.bellySize;
        metabolizedContents.SetMaxVolume(newGenes.metabolizeCapacitySize);
        Vector4 hbcs = new Vector4(newGenes.hue/255f, newGenes.brightness/255f, 0.5f, newGenes.saturation/255f);
        // Set color
        foreach (Renderer r in koboldBodyRenderers) {
            if (r == null) {
                continue;
            }
            foreach (Material m in r.materials) {
                m.SetVector(BrightnessContrastSaturation, hbcs);
            }
            foreach (var dickSet in activeDicks) {
                foreach (var rendererMask in dickSet.dick.GetTargetRenderers()) {
                    foreach (Material m in rendererMask.renderer.materials) {
                        m.SetVector(BrightnessContrastSaturation, hbcs);
                    }
                }
            }
        }
        // Set dick
        var inventory = GetComponent<KoboldInventory>();
        if (GetGenes() == null || newGenes.dickEquip != GetGenes().dickEquip || newGenes.dickEquip == byte.MaxValue) {
            while(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch) != null) {
                inventory.RemoveEquipment(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch),PhotonNetwork.InRoom);
            }
        }

        if (newGenes.dickEquip != byte.MaxValue) {
            if (!inventory.Contains(EquipmentDatabase.GetEquipments()[newGenes.dickEquip])) {
                inventory.PickupEquipment(EquipmentDatabase.GetEquipments()[newGenes.dickEquip], null);
            }
        }

        energyChanged?.Invoke(energy, newGenes.maxEnergy);
        base.SetGenes(newGenes);
        OnBoobContentsChanged(boobContents);
        OnBallsContentsChanged(ballsContents);
    }
    
    void OnMidnight(object ignore) {
        int hitCount = 0;
        foreach (RaycastHit h in Physics.RaycastAll(transform.position + Vector3.up * 400f,
                     Vector3.down, 400f, SpoilableHandler.GetSafeZoneMask(), QueryTriggerInteraction.Collide)) {
            hitCount++;
        }
        if (hitCount % 2 != 0) {
            if (energy != GetGenes().maxEnergy) {
                energy = GetGenes().maxEnergy;
                energyChanged?.Invoke(energy, GetGenes().maxEnergy);
            }
        }
    }

    private void Awake() {
        ballsContents = new ReagentContents();
        ballsContents.changed += OnBallsContentsChanged;
        boobContents = new ReagentContents();
        boobContents.changed += OnBoobContentsChanged;
        bellyContainer = gameObject.AddComponent<GenericReagentContainer>();
        bellyContainer.type = GenericReagentContainer.ContainerType.Mouth;
        metabolizedContents = new ReagentContents(20f);
        bellyContainer.maxVolume = 20f;
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
        
        if (gargleSource == null) {
            gargleSource = gameObject.AddComponent<AudioSource>();
            gargleSource.playOnAwake = false;
            gargleSource.maxDistance = 10f;
            gargleSource.minDistance = 0.2f;
            gargleSource.rolloffMode = AudioRolloffMode.Linear;
            gargleSource.spatialBlend = 1f;
            gargleSource.loop = true;
        }
        belly.AddListener(new InflatableSoundPack(tummyGrumbles, tummyGrumbleSource));
    }

    void Start() {
        lastPumpTime = Time.timeSinceLevelLoad;
        MetabolizeEvent.AddListener(OnEventRaised);
        MidnightEvent.AddListener(OnMidnight);
        bellyContainer.OnChange.AddListener(OnBellyContentsChanged);
    }
    private void OnDestroy() {
        MetabolizeEvent.RemoveListener(OnEventRaised);
        bellyContainer.OnChange.RemoveListener(OnBellyContentsChanged);
        MidnightEvent.RemoveListener(OnMidnight);
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
        photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
        controller.frictionMultiplier = 0.1f;
        controller.enabled = false;
        return true;
    }
    public void PumpUpDick(float amount) {
        if (amount > 0 ) {
            lastPumpTime = Time.timeSinceLevelLoad;
        }
        arousal += amount;
        arousal = Mathf.Clamp01(arousal);
    }
    public IEnumerator ThrowRoutine() {
        photonView.RPC(nameof(Ragdoller.PushRagdoll), RpcTarget.All);
        yield return new WaitForSeconds(3f);
        photonView.RPC(nameof(Ragdoller.PopRagdoll), RpcTarget.All);
    }
    public void OnRelease(Kobold kobold) {
        //animator.updateMode = modeSave;
        animator.SetBool("Carried", false);
        if (body.velocity.magnitude > 3f) {
            StartCoroutine(ThrowRoutine());
        } else {
            if (photonView.IsMine) {
                foreach (Collider c in Physics.OverlapSphere(transform.position, 1f, GameManager.instance.usableHitMask,
                             QueryTriggerInteraction.Collide)) {
                    GenericUsable usable = c.GetComponentInParent<GenericUsable>();
                    if (usable != null && usable.CanUse(this)) {
                        usable.LocalUse(this);
                        break;
                    }
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
            dick.bonerInflater.SetSize(arousal*0.95f + (0.05f * Mathf.Clamp01(Mathf.Sin(Time.time*2f)))*arousal, dick.info);
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
    public ReagentContents SpillMetabolizedContents(float volume) {
        ReagentContents v = metabolizedContents.Spill(volume);
        ProcessReagents(v, -1f);
        return v;
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
        photonView.RPC(nameof(RPCSendChat), RpcTarget.All, new object[]{message});
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
    private float FloorNearestPower(float baseNum, float target) {
        float f = baseNum;
        for(;f<=target;f*=baseNum) {}
        return f/baseNum;
    }

    public void ProcessReagents(ReagentContents contents, float multi) {
        
        float melonJuiceVolume = contents.GetVolumeOf(ReagentDatabase.GetReagent("MelonJuice"));
        float eggplantJuiceVolume = contents.GetVolumeOf(ReagentDatabase.GetReagent("EggplantJuice"));
        float growthSerumVolume = contents.GetVolumeOf(ReagentDatabase.GetReagent("GrowthSerum"));
        float milkShakeVolume = contents.GetVolumeOf(ReagentDatabase.GetReagent("MilkShake"));
        float pineappleJuiceVolume = contents.GetVolumeOf(ReagentDatabase.GetReagent("PineappleJuice"));
        float sum = melonJuiceVolume + eggplantJuiceVolume + growthSerumVolume + milkShakeVolume + pineappleJuiceVolume * multi;

        if (sum != 0f) {
            KoboldGenes genes = GetGenes();
            genes.breastSize += melonJuiceVolume * multi;
            genes.dickSize += eggplantJuiceVolume * multi;
            genes.baseSize += growthSerumVolume * multi;
            genes.fatSize += milkShakeVolume * multi;
            genes.ballSize += pineappleJuiceVolume * multi;
            SetGenes(genes);
        }
    }
    
    private void OnEventRaised(float f) {
        if (!photonView.IsMine) {
            return;
        }
        stimulation = Mathf.MoveTowards(stimulation, 0f, f*0.1f);
        ReagentContents vol = bellyContainer.Metabolize(f);
        // Reagents that don't affect metabolization limits
        bellyContainer.GetContents().AddMix(ReagentDatabase.GetReagent("Egg").GetReagent(vol.GetVolumeOf(ReagentDatabase.GetReagent("Cum"))*3f), bellyContainer);

        vol.DumpNonConsumable();
        
        // Can't over-metabolize, put some back if it doesn't fit
        float maxMetabolization = (metabolizedContents.GetMaxVolume() - metabolizedContents.volume);
        if (vol.volume > maxMetabolization) {
            bellyContainer.GetContents().AddMix(vol.Spill(vol.volume - maxMetabolization), bellyContainer);
        }

        bellyContainer.OnChange.Invoke(bellyContainer.GetContents(), GenericReagentContainer.InjectType.Metabolize);

        if (vol.volume <= 0f) {
            return;
        }
        metabolizedContents.AddMix(vol);
        ProcessReagents(vol, 1f);
    }

    IEnumerator WaitAndThenStopGargling(float time) {
        yield return new WaitForSeconds(time);
        gargleSource.Pause();
        gargleSource.enabled = false;
    }
    public void OnBellyContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        belly.SetSize(Mathf.Log(1f + contents.volume / 80f, 2f), this);
        if (injectType != GenericReagentContainer.InjectType.Spray || bellyContainer.volume >= bellyContainer.maxVolume) {
            return;
        }
        koboldAnimator.SetTrigger("Quaff");
        if (gargleSource.enabled == false || !gargleSource.isPlaying) {
            gargleSource.enabled = true;
            garglePack.Play(gargleSource);
            //gurgleSource.Play();
            gargleSource.pitch = 1f;
            StartCoroutine(WaitAndThenStopGargling(0.25f));
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(GetGenes());
            stream.SendNext(arousal);
        } else {
            SetGenes((KoboldGenes)stream.ReceiveNext());
            arousal = (float)stream.ReceiveNext();
        }
    }

    public void OnThrow(Kobold kobold) {
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData == null) {
            return;
        }

        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is KoboldGenes) {
            SetGenes((KoboldGenes)info.photonView.InstantiationData[0]);
        } else {
            SetGenes(new KoboldGenes().Randomize());
        }

        if (info.photonView.InstantiationData.Length > 1 && info.photonView.InstantiationData[1] is bool) {
            if ((bool)info.photonView.InstantiationData[1] == true) {
                body.rotation = Quaternion.FromToRotation(body.transform.up, Vector3.up) * body.rotation;
                body.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                GetComponentInChildren<KoboldAIPossession>(true).gameObject.SetActive(false);
                info.Sender.TagObject = this;
            } else {
                GetComponentInChildren<KoboldAIPossession>(true).gameObject.SetActive(true);
                FarmSpawnEventHandler.TriggerProduceSpawn(gameObject);
            }
        }
    }

    public void Save(BinaryWriter writer, string version) {
        GetGenes().Serialize(writer);
    }

    public void Load(BinaryReader reader, string version) {
        SetGenes(GetGenes().Deserialize(reader));
    }

    public float GetWorth() {
        KoboldGenes genes = GetGenes();
        return 5f+(Mathf.Log(1f+(genes.baseSize + genes.dickSize + genes.breastSize + genes.fatSize),2)*6f);
    }
    
    
}
