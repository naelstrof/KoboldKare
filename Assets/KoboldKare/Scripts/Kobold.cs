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
    public List<GenericInflatable> boobs = new List<GenericInflatable>();
    public List<GenericInflatable> bellies = new List<GenericInflatable>();
    public List<GenericInflatable> subcutaneousStorage = new List<GenericInflatable>();
    public GenericInflatable sizeInflatable;
    public GenericReagentContainer balls;
    public BodyProportionSimple bodyProportion;
    public TMPro.TMP_Text chatText;
    public float textSpeedPerCharacter, minTextTimeout;
    public UnityEvent OnEggFormed;
    public UnityEvent OnOrgasm;
    [HideInInspector]
    public List<DickInfo.DickSet> activeDicks = new List<DickInfo.DickSet>();

    public Grabber grabber;
    public AudioSource gurgleSource;
    public Rigidbody[] grabbableBodies;
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
    public Ragdoller ragdoller;
    public void AddStimulation(float s) {
        stimulation += s;
        if (stimulation >= stimulationMax) {
            OnOrgasm.Invoke();
            foreach(var dickSet in activeDicks) {
                float cumAmount = 0.5f+0.5f*sizeInflatable.GetContainer().GetVolumeOf(ReagentDatabase.GetReagent("GrowthSerum"))+0.5f*baseBallSize+0.5f*baseDickSize;
                //Debug.Log("Cumming " + cumAmount);
                dickSet.balls.GetContainer().AddMix(ReagentDatabase.GetReagent("Cum"), cumAmount, GenericReagentContainer.InjectType.Inject);
                //dickSet.dick.GetComponentInChildren<IFluidOutput>(true).SetVolumePerSecond(cumAmount/12f);
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

    public void Awake() {
        statblock.StatusEffectsChangedEvent += OnStatusEffectsChanged;
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
            baseBoobSize = Random.Range(0f,15f);
            baseBallSize = Random.Range(20f,40f);
            baseDickSize = Random.Range(0f,1f);
        } else {
            baseBoobSize = Random.Range(6f,30f);
            baseBallSize = 0f;
        }
        bodyProportion.SetTopBottom(Random.Range(-1f,1f));
        bodyProportion.SetThickness(Random.Range(-1f,1f));

        sizeInflatable.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("GrowthSerum"), Random.Range(0.7f,1.2f) * sizeInflatable.reagentVolumeDivisor);
        RegenerateSlowly(1000f);
        PumpUpDick(-1f);
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
        foreach (var b in bellies) {
            b.GetContainer().OnChange.AddListener(OnReagentContainerChanged);
        }
    }
    private void OnDestroy() {
        statblock.StatusEffectsChangedEvent -= OnStatusEffectsChanged;
        MetabolizeEvent.RemoveListener(OnEventRaised);
        foreach (var b in bellies) {
            b.GetContainer().OnChange.RemoveListener(OnReagentContainerChanged);
        }
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
        GetComponent<CharacterControllerAnimator>().OnEndStation();
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
            dick.bonerInflator.baseSize = arousal*0.95f + (0.05f * Mathf.Clamp01(Mathf.Sin(Time.time*2f)))*arousal;
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
    private float FloorNearestPower(float baseNum, float target) {
        float f = baseNum;
        for(;f<=target;f*=baseNum) {}
        return f/baseNum;
    }
    public void OnEventRaised(float f) {
        stimulation = Mathf.MoveTowards(stimulation, 0f, f*0.1f);
        foreach (var belly in bellies) {
            ReagentContents vol = belly.GetContainer().Metabolize(f);
            belly.GetContainer().AddMix(ReagentDatabase.GetReagent("Egg"), vol.GetVolumeOf(ReagentDatabase.GetReagent("Cum"))*3f, GenericReagentContainer.InjectType.Metabolize);
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

            if (Time.timeSinceLevelLoad > nextEggTime && photonView.IsMine) {
                float currentEggVolume = belly.GetContainer().GetVolumeOf(ReagentDatabase.GetReagent("Egg"));
                float eggSize = FloorNearestPower(5f,currentEggVolume);
                if (eggSize >= 5f && eggSize < belly.GetContainer().GetVolumeOf(ReagentDatabase.GetReagent("Cum"))) {
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
                        belly.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("Egg"), currentEggVolume-eggSize);
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
            stream.SendNext(sex);
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.r*255f));
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.g*255f));
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.b*255f));
            stream.SendNext((byte)Mathf.RoundToInt(HueBrightnessContrastSaturation.a*255f));
            stream.SendNext(baseBallSize);
            stream.SendNext(baseBoobSize);
            stream.SendNext(baseDickSize);
        } else {
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
