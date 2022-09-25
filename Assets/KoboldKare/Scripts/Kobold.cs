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
using Naelstrof.Mozzarella;
using SkinnedMeshDecals;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Kobold : GeneHolder, IGrabbable, IPunObservable, IPunInstantiateMagicCallback, ISavable, IValuedGood {
    public StatusEffect koboldStatus;
    [System.Serializable]
    public class PenetrableSet {
        public Penetrable penetratable;
        public Rigidbody ragdollAttachBody;
        public bool isFemaleExclusiveAnatomy = false;
        public bool canLayEgg = true;
    }

    public delegate void EnergyChangedAction(float value, float maxValue);
    public delegate void KoboldSpawnAction(Kobold kobold);
    public event EnergyChangedAction energyChanged;
    public static event KoboldSpawnAction spawned;

    public List<PenetrableSet> penetratables = new List<PenetrableSet>();

    public List<Transform> attachPoints = new List<Transform>();

    public AudioClip[] yowls;
    public Animator animator;
    public Rigidbody body;
    public GameEventFloat MetabolizeEvent;
    

    public GenericReagentContainer bellyContainer { get; private set; }
    [SerializeField]
    private Inflatable belly;
    private Grabber grabber;
    [SerializeField]
    private Inflatable fatnessInflater;
    [SerializeField]
    private Inflatable sizeInflater;
    [SerializeField]
    private Inflatable boobs;
    [SerializeField]
    private Material milkSplatMaterial;

    private UsableColliderComparer usableColliderComparer;
    public ReagentContents metabolizedContents;
    
    [SerializeField]
    private float energy = 1f;

    [SerializeField]
    private AudioPack tummyGrumbles;
    [FormerlySerializedAs("gurglePack")] [SerializeField]
    private AudioPack garglePack;
    
    public BodyProportionSimple bodyProportion;
    public TMPro.TMP_Text chatText;
    public float textSpeedPerCharacter, minTextTimeout;
    [SerializeField] private PhotonGameObjectReference heartPrefab;
    [HideInInspector]
    public List<DickInfo.DickSet> activeDicks = new List<DickInfo.DickSet>();
    private AudioSource gargleSource;
    private AudioSource tummyGrumbleSource;
    public List<Renderer> koboldBodyRenderers;
    //private float internalSex = 0f;
    [SerializeField]
    private List<Transform> nipples;
    public Transform hip;
    public KoboldCharacterController controller;
    public float stimulation = 0f;
    public float stimulationMax = 30f;
    public float stimulationMin = -30f;
    public Animator koboldAnimator;
    private float lastPumpTime = 0f;
    public bool grabbed { get; private set; }
    private List<Vector3> savedJointAnchors = new List<Vector3>();
    public float arousal = 0f;
    public GameObject nippleBarbells;
    private ReagentContents consumedReagents;
    private ReagentContents addbackReagents;
    private static Collider[] colliders = new Collider[32];
    private WaitForSeconds waitSpurt;
    private bool milking = false;
    
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
    private class UsableColliderComparer : IComparer<Collider> {
        private Vector3 checkPoint;

        public void SetCheckPoint(Vector3 position) {
            checkPoint = position;
        }

        public int Compare(Collider x, Collider y) {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            float closestX = Vector3.Distance(checkPoint, x.ClosestPointOnBounds(checkPoint));
            float closestY =  Vector3.Distance(checkPoint,y.ClosestPointOnBounds(checkPoint));
            return closestX.CompareTo(closestY);
        }
    }
    
    [PunRPC]
    public IEnumerator MilkRoutine() {
        while (milking) {
            yield return null;
        }
        milking = true;
        int pulses = 12;
        // Now do some milk stuff.
        for (int i = 0; i < pulses; i++) {
            foreach (Transform t in GetNipples()) {
                if (MozzarellaPool.instance.TryInstantiate(out Mozzarella mozzarella)) {
                    mozzarella.SetFollowTransform(t);
                    ReagentContents alloc = new ReagentContents();
                    alloc.AddMix(ReagentDatabase.GetReagent("Milk").GetReagent(GetGenes().breastSize/(pulses*GetNipples().Count)));
                    mozzarella.SetVolumeMultiplier(alloc.volume);
                    mozzarella.SetLocalForward(Vector3.up);
                    Color color = alloc.GetColor();
                    mozzarella.hitCallback += (hit, startPos, dir, length, volume) => {
                        if (photonView.IsMine) {
                            GenericReagentContainer container =
                                hit.collider.GetComponentInParent<GenericReagentContainer>();
                            if (container != null && this != null) {
                                container.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All,
                                    alloc.Spill(alloc.volume * 0.1f), photonView.ViewID);
                            }
                        }
                        milkSplatMaterial.color = color;
                        PaintDecal.RenderDecalForCollider(hit.collider, milkSplatMaterial,
                            hit.point - hit.normal * 0.1f, Quaternion.LookRotation(hit.normal, Vector3.up)*Quaternion.AngleAxis(UnityEngine.Random.Range(-180f,180f), Vector3.forward),
                            Vector2.one * (volume * 4f), length);
                    };
                }
            }
            yield return waitSpurt;
        }
        milking = false;
    }

    public List<Transform> GetNipples() {
        return nipples;
    }

    private Coroutine displayMessageRoutine;
    public Ragdoller ragdoller;
    public void AddStimulation(float s) {
        stimulation += s;
        if (photonView.IsMine && stimulation >= stimulationMax && TryConsumeEnergy(1)) {
            photonView.RPC(nameof(Cum), RpcTarget.All);
        }
    }
    
    [PunRPC]
    public void Cum() {
        if (photonView.IsMine && activeDicks.Count == 0) {
            PhotonNetwork.Instantiate(heartPrefab.photonName, hip.transform.position, Quaternion.identity, 0, new object[] { GetGenes() });
        }
        foreach(var dickSet in activeDicks) {
            // TODO: This is a really, really terrible way to make a dick cum lol. Clean this up.
            dickSet.info.StartCoroutine(dickSet.info.CumRoutine(dickSet));
        }
        PumpUpDick(1f);
        stimulation = stimulationMin;
    }

    public bool TryConsumeEnergy(byte amount) {
        if (energy < amount) {
            return false;
        }
        SetEnergyRPC(energy - amount);
        if (!photonView.IsMine) {
            photonView.RPC(nameof(Kobold.SetEnergyRPC), RpcTarget.Others, energy);
        }
        return true;
    }

    [PunRPC]
    public void SetEnergyRPC(float newEnergy) {
        float diff = newEnergy - energy;
        if (diff < 0f && photonView.IsMine) {
            SetGenes(GetGenes().With(fatSize: Mathf.Max(GetGenes().fatSize + diff, 0f)));
        }

        energy = newEnergy;
        energy = Mathf.Max(0, energy);
        energyChanged?.Invoke(energy, GetGenes().maxEnergy);
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
    private static readonly int Carried = Animator.StringToHash("Carried");
    private static readonly int Quaff = Animator.StringToHash("Quaff");

    public float GetEnergy() {
        return energy;
    }
    public float GetMaxEnergy() {
        if (GetGenes() == null) {
            return 1f;
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

    [PunRPC]
    public void SetDickRPC(short dickID) {
        SetGenes(GetGenes().With(dickEquip: (byte)dickID));
    }

    public override void SetGenes(KoboldGenes newGenes) {
        // Set dick
        var inventory = GetComponent<KoboldInventory>();
        Equipment crotchEquipment = inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch);
        if (crotchEquipment != null && EquipmentDatabase.GetID(crotchEquipment) != newGenes.dickEquip) {
            inventory.RemoveEquipment(crotchEquipment,PhotonNetwork.InRoom);
        }

        if (newGenes.dickEquip != byte.MaxValue) {
            if (!inventory.Contains(EquipmentDatabase.GetEquipments()[newGenes.dickEquip])) {
                inventory.PickupEquipment(EquipmentDatabase.GetEquipments()[newGenes.dickEquip], null);
            }
        }
        foreach (var dickSet in activeDicks) {
            foreach (var inflater in dickSet.dickSizeInflater.GetInflatableListeners()) {
                if (inflater is InflatableDick inflatableDick) {
                    inflatableDick.SetDickThickness(newGenes.dickThickness);
                }
            }
            dickSet.dickSizeInflater.SetSize(0.7f+Mathf.Log(1f + newGenes.dickSize / 20f, 2f), dickSet.info);
            dickSet.ballSizeInflater.SetSize(0.7f+Mathf.Log(1f + newGenes.ballSize / 20f, 2f), dickSet.info);
        }
        grabber.SetMaxGrabCount(newGenes.grabCount);
        if (ragdoller.ragdolled) {
            sizeInflater.SetSizeInstant(Mathf.Max(Mathf.Log(1f + newGenes.baseSize / 20f, 2f), 0.2f));
        } else {
            sizeInflater.SetSize(Mathf.Max(Mathf.Log(1f + newGenes.baseSize / 20f, 2f), 0.2f), this);
        }

        fatnessInflater.SetSize(Mathf.Log(1f + newGenes.fatSize / 20f, 2f), this);
        boobs.SetSize(Mathf.Log(1f + newGenes.breastSize / 20f, 2f), this);
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
                    if (rendererMask.renderer == null) {
                        continue;
                    }
                    foreach (Material m in rendererMask.renderer.materials) {
                        m.SetVector(BrightnessContrastSaturation, hbcs);
                    }
                }
            }
        }

        energyChanged?.Invoke(energy, newGenes.maxEnergy);
        base.SetGenes(newGenes);
    }

    private void Awake() {
        waitSpurt = new WaitForSeconds(1f);
        usableColliderComparer = new UsableColliderComparer();
        grabber = GetComponentInChildren<Grabber>(true);
        consumedReagents = new ReagentContents();
        addbackReagents = new ReagentContents();
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
            tummyGrumbleSource = hip.gameObject.AddComponent<AudioSource>();
            tummyGrumbleSource.playOnAwake = false;
            tummyGrumbleSource.maxDistance = 10f;
            tummyGrumbleSource.minDistance = 0.2f;
            tummyGrumbleSource.rolloffMode = AudioRolloffMode.Linear;
            tummyGrumbleSource.spatialBlend = 1f;
            tummyGrumbleSource.loop = false;
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
        belly.AddListener(new InflatableSoundPack(tummyGrumbles, tummyGrumbleSource, this));
    }

    void Start() {
        lastPumpTime = Time.timeSinceLevelLoad;
        MetabolizeEvent.AddListener(OnEventRaised);
        bellyContainer.OnChange.AddListener(OnBellyContentsChanged);
        PlayAreaEnforcer.AddTrackedObject(photonView);
        if (GetGenes() == null) {
            SetGenes(new KoboldGenes().Randomize());
        }
    }
    private void OnDestroy() {
        MetabolizeEvent.RemoveListener(OnEventRaised);
        bellyContainer.OnChange.RemoveListener(OnBellyContentsChanged);
        PlayAreaEnforcer.RemoveTrackedObject(photonView);
    }
    [PunRPC]
    public void OnGrabRPC(int koboldID) {
        grabbed = true;
        animator.SetBool(Carried, true);
        controller.frictionMultiplier = 0.1f;
        controller.enabled = false;
        
        if (photonView.IsMine) {
            photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
        }
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

    private void OnValidate() {
        heartPrefab.OnValidate();
    }

    public bool CanGrab(Kobold kobold) {
        return !controller.inputJump;
    }

    [PunRPC]
    public void OnReleaseRPC(int koboldID, Vector3 velocity) {
        animator.SetBool(Carried, false);
        controller.frictionMultiplier = 1f;
        grabbed = false;
        controller.enabled = true;
        
        if (!photonView.IsMine) {
            return;
        }
        
        foreach (Rigidbody b in ragdoller.GetRagdollBodies()) {
            b.velocity = velocity;
        }

        if (velocity.magnitude > 3f) {
            StartCoroutine(ThrowRoutine());
        } else {
            int hits = Physics.OverlapSphereNonAlloc(transform.position, Mathf.Max(1f, Mathf.Log(1f+transform.localScale.x,2f)),
                colliders, GameManager.instance.usableHitMask, QueryTriggerInteraction.Collide);
            usableColliderComparer.SetCheckPoint(transform.position);
            Array.Sort(colliders, 0, hits, usableColliderComparer);
            for (int i=0;i<hits;i++) {
                Collider c = colliders[i];
                GenericUsable usable = c.GetComponentInParent<GenericUsable>();
                if (usable != null && usable.CanUse(this)) {
                    usable.LocalUse(this);
                    break;
                }
            }
        }
    }
    private void Update() {
        // Throbbing!
        foreach(var dick in activeDicks) {
            dick.bonerInflater.SetSize(arousal*0.95f + (0.05f * Mathf.Clamp01(Mathf.Sin(Time.time*2f)))*arousal, dick.info);
        }
    }
    private void FixedUpdate() {
        if (grabbed) {
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
    public void SendChat(string message) {
        photonView.RPC(nameof(RPCSendChat), RpcTarget.All, message);
    }
    [PunRPC]
    public void RPCSendChat(string message) {
        GameManager.instance.SpawnAudioClipInWorld(yowls[UnityEngine.Random.Range(0,yowls.Length)], transform.position);
        if (displayMessageRoutine != null) {
            StopCoroutine(displayMessageRoutine);
        }

        foreach (var player in PhotonNetwork.PlayerList) {
            if ((Kobold)player.TagObject != this) continue;
            CheatsProcessor.AppendText($"{player.NickName}: {message}\n");
            CheatsProcessor.ProcessCommand(this, message);
            displayMessageRoutine = StartCoroutine(DisplayMessage(message,minTextTimeout));
            break;
        }
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
    }
    public bool IsPenetrating(Kobold k) {
        return false;
    }

    public bool PhysicsGrabbable() { return true; }
    public Transform GrabTransform() {
        return hip;
    }
    private float FloorNearestPower(float baseNum, float target) {
        float f = baseNum;
        for(;f<=target;f*=baseNum) {}
        return f/baseNum;
    }

    public void ProcessReagents(ReagentContents contents) {
        addbackReagents.Clear();
        KoboldGenes genes = GetGenes();
        float newEnergy = energy;
        float passiveEnergyGeneration = 0.025f;
        if (newEnergy < 1f) {
            if (ragdoller.ragdolled) {
                passiveEnergyGeneration *= 4f;
            }
            newEnergy = Mathf.MoveTowards(newEnergy, 1.1f, passiveEnergyGeneration);
        }
        foreach (var pair in contents) {
            ScriptableReagent reagent = ReagentDatabase.GetReagent(pair.id);
            float processedAmount = pair.volume;
            reagent.GetConsumptionEvent().OnConsume(this, reagent, ref processedAmount, ref consumedReagents, ref addbackReagents, ref genes, ref newEnergy);
            pair.volume -= processedAmount;
        }
        bellyContainer.AddMixRPC(contents, -1); 
        bellyContainer.AddMixRPC(addbackReagents, -1);
        float overflowEnergy = Mathf.Max(newEnergy - GetMaxEnergy(), 0f);
        if (overflowEnergy != 0f) {
            genes = genes.With(fatSize: genes.fatSize + overflowEnergy);
        }
        SetGenes(genes);
        
        if (Math.Abs(energy - newEnergy) > 0.001f) {
            energy = Mathf.Clamp(newEnergy, 0f, GetMaxEnergy());
            energyChanged?.Invoke(energy, GetMaxEnergy());
        }
    }
    
    private void OnEventRaised(float f) {
        if (!photonView.IsMine) {
            return;
        }
        stimulation = Mathf.MoveTowards(stimulation, 0f, f*0.08f);
        ReagentContents vol = bellyContainer.Metabolize(f);
        ProcessReagents(vol);
    }

    IEnumerator WaitAndThenStopGargling(float time) {
        yield return new WaitForSeconds(time);
        gargleSource.Pause();
        gargleSource.enabled = false;
    }
    private void OnBellyContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        belly.SetSize(Mathf.Log(1f + contents.volume / 80f, 2f), this);
        if (injectType != GenericReagentContainer.InjectType.Spray || bellyContainer.volume >= bellyContainer.maxVolume) {
            return;
        }
        koboldAnimator.SetTrigger(Quaff);
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
            stream.SendNext(metabolizedContents);
            stream.SendNext(consumedReagents);
            stream.SendNext(energy);
        } else {
            SetGenes((KoboldGenes)stream.ReceiveNext());
            arousal = (float)stream.ReceiveNext();
            metabolizedContents.Copy((ReagentContents)stream.ReceiveNext());
            consumedReagents.Copy((ReagentContents)stream.ReceiveNext());
            float newEnergy = (float)stream.ReceiveNext();
            if (Math.Abs(newEnergy - energy) > 0.01f) {
                energy = newEnergy;
                energyChanged?.Invoke(energy, GetGenes().maxEnergy);
            }
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData == null) {
            spawned?.Invoke(this);
            return;
        }

        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is KoboldGenes) {
            SetGenes((KoboldGenes)info.photonView.InstantiationData[0]);
        } else {
            SetGenes(new KoboldGenes().Randomize());
        }

        if (info.photonView.InstantiationData.Length > 1 && info.photonView.InstantiationData[1] is bool) {
            if ((bool)info.photonView.InstantiationData[1] == true) {
                GetComponentInChildren<KoboldAIPossession>(true).gameObject.SetActive(false);
                if (info.Sender != null) { // Possible for instantiated kobold's owner to have disconnected. (late join instantiate).
                    info.Sender.TagObject = this;
                }
            } else {
                GetComponentInChildren<KoboldAIPossession>(true).gameObject.SetActive(true);
                FarmSpawnEventHandler.TriggerProduceSpawn(gameObject);
            }
        }
        spawned?.Invoke(this);
    }

    public void Save(BinaryWriter writer) {
        GetGenes().Save(writer);
        writer.Write(arousal);
        metabolizedContents.Save(writer);
        consumedReagents.Save(writer);
        bool isPlayerControlled = (Kobold)PhotonNetwork.LocalPlayer.TagObject == this;
        writer.Write(isPlayerControlled);
    }

    public void Load(BinaryReader reader) {
        KoboldGenes loadedGenes = new KoboldGenes();
        loadedGenes.Load(reader);
        SetGenes(loadedGenes);
        arousal = reader.ReadSingle();
        metabolizedContents.Load(reader);
        consumedReagents.Load(reader);
        bool isPlayerControlled = reader.ReadBoolean();
        if (isPlayerControlled) {
            PhotonNetwork.LocalPlayer.TagObject = this;
            GetComponentInChildren<KoboldAIPossession>(true).gameObject.SetActive(false);
            GetComponentInChildren<PlayerPossession>(true).gameObject.SetActive(true);
        }
    }

    public float GetWorth() {
        KoboldGenes genes = GetGenes();
        return 5f+(Mathf.Log(1f+(genes.baseSize + genes.dickSize + genes.breastSize + genes.fatSize),2)*6f);
    }
}
