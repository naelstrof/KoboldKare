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
using NetStack.Quantization;
using NetStack.Serialization;
using SimpleJSON;
using SkinnedMeshDecals;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Kobold : GeneHolder, IGrabbable, IPunObservable, IPunInstantiateMagicCallback, ISavable, IValuedGood {
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

    [SerializeField]
    private List<Equipment.AttachPointReference> attachPoints;

    public Transform GetAttachPointTransform(Equipment.AttachPoint attachPoint) {
        foreach (var point in attachPoints) {
            if (point.attachPoint == attachPoint) {
                return point.targetTransform;
            }
        }

        return null;
    }

    [HideInInspector]
    public Rigidbody body;
    

    public GenericReagentContainer bellyContainer { get; private set; }
    [FormerlySerializedAs("belly")] [SerializeField]
    private Inflatable bellyInflater;
    private Grabber grabber => GetComponent<Grabber>();
    [SerializeField]
    private Inflatable fatnessInflater;
    public Inflatable sizeInflater;
    [FormerlySerializedAs("boobs")] [SerializeField]
    private Inflatable boobsInflater;
    [SerializeField]
    private LayerMask heartHitMask;
    [SerializeField] private PhotonGameObjectReference heartPrefab;
    
    private UsableColliderComparer usableColliderComparer;
    public ReagentContents metabolizedContents;
    
    private float energy = 1f;

    [SerializeField]
    private AudioPack tummyGrumbles;
    [FormerlySerializedAs("gurglePack")] [SerializeField]
    private AudioPack garglePack;
    
    [HideInInspector]
    public List<DickDescriptor.DickSet> activeDicks = new List<DickDescriptor.DickSet>();
    private AudioSource gargleSource;
    private AudioSource tummyGrumbleSource;
    public List<Renderer> koboldBodyRenderers;
    //private float internalSex = 0f;
    
    [SerializeField]
    private MilkLactator milkLactator;
    
    public Transform hip;
    private KoboldCharacterController controller;
    
    [HideInInspector]
    public float stimulation = 0f;
    [HideInInspector]
    public float stimulationMax = 10f;
    [HideInInspector]
    public float stimulationMin = -20f;
    
    private float lastPumpTime = 0f;
    public bool grabbed { get; private set; }
    private List<Vector3> savedJointAnchors = new List<Vector3>();
    private float arousal = 0f;
    
    private ReagentContents consumedReagents;
    private ReagentContents addbackReagents;
    private static Collider[] colliders = new Collider[32];
    public delegate void CarriedAction(bool carried);
    public delegate void QuaffAction();

    public event CarriedAction carriedChanged;
    public event QuaffAction quaff;
    private GameObject dickObject;
    private bool initialized = false;
    
    public IEnumerable<InflatableListener> GetAllInflatableListeners() {
        foreach (var listener in bellyInflater.GetInflatableListeners()) {
            yield return listener;
        }
        foreach (var listener in fatnessInflater.GetInflatableListeners()) {
            yield return listener;
        }
        foreach (var listener in boobsInflater.GetInflatableListeners()) {
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
    public void MilkRoutine() {
        milkLactator.StartMilking(this);
    }

    private Ragdoller ragdoller => GetComponent<Ragdoller>();
    public void AddStimulation(float s) {
        stimulation += s;
        if (photonView.IsMine && stimulation >= stimulationMax && TryConsumeEnergy(1)) {
            photonView.RPC(nameof(Cum), RpcTarget.All);
        }
    }
    
    [PunRPC]
    public void Cum() {
        if (photonView.IsMine && activeDicks.Count == 0) {
            bool foundHeart = false;
            int hits = Physics.OverlapSphereNonAlloc(hip.position, 5f, colliders, heartHitMask);
            for (int i = 0; i < hits; i++) {
                // Found a nearby heart!
                PhotonView fruitView = colliders[i].GetComponentInParent<PhotonView>();
                if (fruitView != null && fruitView.name.Contains(heartPrefab.photonName)) {
                    BitBuffer reagentBuffer = new BitBuffer(16);
                    ReagentContents loveContents = new ReagentContents();
                    loveContents.AddMix(ReagentDatabase.GetReagent("Love").GetReagent(10f));
                    reagentBuffer.AddReagentContents(loveContents);
                    fruitView.RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, reagentBuffer,
                        photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
                    foundHeart = true;
                    break;
                }
            }

            // No nearby hearts, spawn a new one.
            if (!foundHeart) {
                BitBuffer buffer = new BitBuffer(16);
                buffer.AddKoboldGenes(GetGenes());
                PhotonNetwork.Instantiate(heartPrefab.photonName, hip.transform.position, Quaternion.identity, 0, new object[] { buffer });
            }
        }
        foreach(var dickSet in activeDicks) {
            // TODO: This is a really, really terrible way to make a dick cum lol. Clean this up.
            dickSet.descriptor.StartCoroutine(dickSet.descriptor.CumRoutine(dickSet));
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
        if (diff < 0f && GetGenes().fatSize > 0f && photonView.IsMine) {
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

    public float GetEnergy() {
        return energy;
    }

    public Ragdoller GetRagdoller() => ragdoller;
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
    public void SetDickRPC(byte dickID) {
        SetGenes(GetGenes().With(dickEquip: dickID));
    }

    public override void SetGenes(KoboldGenes newGenes) {
        if (newGenes == null) {
            return;
        }
        // Set dick
        if (newGenes.dickEquip == byte.MaxValue || GetGenes() == null || newGenes.dickEquip != GetGenes().dickEquip) {
            if (dickObject != null) {
                dickObject.GetComponentInChildren<DickDescriptor>().RemoveFrom(this);
                Destroy(dickObject);
            }
        }
        
        if ((GetGenes() == null || newGenes.dickEquip != GetGenes().dickEquip) && newGenes.dickEquip != byte.MaxValue) {
            var dickDatabase = GameManager.GetPenisDatabase().GetValidPrefabReferenceInfos();
            PrefabDatabase.PrefabReferenceInfo selectedDick;
            if (newGenes.dickEquip <= dickDatabase.Count) {
                selectedDick = dickDatabase[newGenes.dickEquip];
            } else {
                Debug.LogWarning($"Couldn't find dick with id {newGenes.dickEquip}, replacing with default dick.");
                selectedDick = dickDatabase[0];
            }

            dickObject = Instantiate(selectedDick.GetPrefab(), GetAttachPointTransform(Equipment.AttachPoint.Crotch));
            dickObject.GetComponentInChildren<DickDescriptor>().AttachTo(this);
        }

        foreach (var dickSet in activeDicks) {
            foreach (var inflater in dickSet.dickSizeInflater.GetInflatableListeners()) {
                if (inflater is InflatableDick inflatableDick) {
                    inflatableDick.SetDickThickness(newGenes.dickThickness);
                }
            }
            dickSet.dickSizeInflater.SetSize(0.5f+Mathf.Log(1f + newGenes.dickSize / 20f, 2f), dickSet.descriptor);
            dickSet.ballSizeInflater.SetSize(0.5f+Mathf.Log(1f + newGenes.ballSize / 20f, 2f), dickSet.descriptor);
        }
        grabber.SetMaxGrabCount(newGenes.grabCount);
        if (ragdoller.ragdolled) {
            sizeInflater.SetSizeInstant(Mathf.Max(Mathf.Log(1f + newGenes.baseSize / 20f, 2f), 0.2f));
        } else {
            sizeInflater.SetSize(Mathf.Max(Mathf.Log(1f + newGenes.baseSize / 20f, 2f), 0.2f), this);
        }

        fatnessInflater.SetSize(Mathf.Log(1f + newGenes.fatSize / 20f, 2f), this);
        boobsInflater.SetSize(Mathf.Log(1f + newGenes.breastSize / 20f, 2f), this);
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
        if (initialized) {
            return;
        }

        initialized = true;
        usableColliderComparer = new UsableColliderComparer();
        consumedReagents = new ReagentContents();
        addbackReagents = new ReagentContents();
        bellyContainer = gameObject.AddComponent<GenericReagentContainer>();
        bellyContainer.type = GenericReagentContainer.ContainerType.Mouth;
        metabolizedContents = new ReagentContents(20f);
        bellyContainer.maxVolume = 20f;
        photonView.ObservedComponents.Add(bellyContainer);
        bellyInflater.OnEnable();
        sizeInflater.OnEnable();
        boobsInflater.OnEnable();
        fatnessInflater.OnEnable();
        milkLactator.Awake();

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
        bellyInflater.AddListener(new InflatableSoundPack(tummyGrumbles, tummyGrumbleSource, this));
    }

    void Start() {
        controller = GetComponent<KoboldCharacterController>();
        body = GetComponent<Rigidbody>();
        lastPumpTime = Time.timeSinceLevelLoad;
        DayNightCycle.AddMetabolizationListener(OnMetabolizationEvent);
        bellyContainer.OnChange.AddListener(OnBellyContentsChanged);
        PlayAreaEnforcer.AddTrackedObject(photonView);
        if (GetGenes() == null) {
            SetGenes(new KoboldGenes().Randomize(gameObject.name));
        }
    }
    private void OnDestroy() {
        DayNightCycle.RemoveMetabolizationListener(OnMetabolizationEvent);
        bellyContainer.OnChange.RemoveListener(OnBellyContentsChanged);
        PlayAreaEnforcer.RemoveTrackedObject(photonView);
    }
    [PunRPC]
    public void OnGrabRPC(int koboldID) {
        grabbed = true;
        carriedChanged?.Invoke(true);
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
        heartPrefab?.OnValidate();
    }

    public bool CanGrab(Kobold kobold) {
        return !controller.inputJump;
    }

    [PunRPC]
    public void OnReleaseRPC(int koboldID, Vector3 velocity) {
        carriedChanged?.Invoke(false);
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
            dick.bonerInflater.SetSize(arousal*0.95f + (0.05f * Mathf.Clamp01(Mathf.Sin(Time.time*2f)))*arousal, dick.descriptor);
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
        bellyContainer.AddMix(contents, GenericReagentContainer.InjectType.Inject); 
        bellyContainer.AddMix(addbackReagents, GenericReagentContainer.InjectType.Inject);
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
    
    private void OnMetabolizationEvent(float f) {
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
        bellyInflater.SetSize(Mathf.Log(1f + contents.volume / 80f, 2f), this);
        if (injectType != GenericReagentContainer.InjectType.Spray || bellyContainer.volume >= bellyContainer.maxVolume*0.99f) {
            return;
        }

        quaff?.Invoke();
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
            BitBuffer sendBuffer = new BitBuffer(32);
            sendBuffer.AddByte((byte)Mathf.RoundToInt(arousal * 255f));
            sendBuffer.AddReagentContents(metabolizedContents);
            sendBuffer.AddReagentContents(consumedReagents);
            ushort quantizedEnergy = HalfPrecision.Quantize(energy);
            sendBuffer.AddUShort(quantizedEnergy);
            sendBuffer.AddKoboldGenes(GetGenes());
            stream.SendNext(sendBuffer);
        } else {
            BitBuffer data = (BitBuffer)stream.ReceiveNext();
            arousal = data.ReadByte()/255f;
            metabolizedContents.Copy(data.ReadReagentContents());
            consumedReagents.Copy(data.ReadReagentContents());
            float newEnergy = HalfPrecision.Dequantize(data.ReadUShort());
            if (Math.Abs(newEnergy - energy) > 0.01f) {
                energy = newEnergy;
                energyChanged?.Invoke(energy, GetGenes().maxEnergy);
            }
            SetGenes(data.ReadKoboldGenes());
            PhotonProfiler.LogReceive(data.Length);
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        Awake();
        if (info.photonView.InstantiationData == null) {
            SetGenes(new KoboldGenes().Randomize(gameObject.name));
            spawned?.Invoke(this);
            return;
        }

        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is BitBuffer) {
            BitBuffer buffer = (BitBuffer)info.photonView.InstantiationData[0];
            // Might be a shared buffer
            buffer.SetReadPosition(0);
            SetGenes(buffer.ReadKoboldGenes());
            PhotonProfiler.LogReceive(buffer.Length);
        } else {
            SetGenes(new KoboldGenes().Randomize(gameObject.name));
        }
        
        spawned?.Invoke(this);
    }

    public void Save(JSONNode node) {
        GetGenes().Save(node, "genes");
        node["arousal"] = arousal;
        metabolizedContents.Save(node, "metabolizedContents");
        consumedReagents.Save(node, "consumedReagents");
        bool isPlayerControlled = (Kobold)PhotonNetwork.LocalPlayer.TagObject == this;
        node["isPlayerControlled"] = isPlayerControlled;
    }

    public void Load(JSONNode node) {
        KoboldGenes loadedGenes = new KoboldGenes();
        loadedGenes.Load(node, "genes");
        arousal = node["arousal"];
        metabolizedContents.Load(node, "metabolizedContents");
        consumedReagents.Load(node, "consumedReagents");
        bool isPlayerControlled = node["isPlayerControlled"];
        if (isPlayerControlled) {
            PhotonNetwork.LocalPlayer.TagObject = this;
            GetComponent<CharacterDescriptor>().SetPlayerControlled(CharacterDescriptor.ControlType.LocalPlayer);
        }
        SetGenes(loadedGenes);
    }

    public float GetWorth() {
        KoboldGenes genes = GetGenes();
        return 5f+(Mathf.Log(1f+(genes.baseSize + genes.dickSize + genes.breastSize + genes.fatSize),2)*6f);
    }
    [PunRPC]
    public void Spawn(string photonName,Vector3 position,Quaternion rotation){
        if (PhotonNetwork.IsMasterClient) {
        PhotonNetwork.InstantiateRoomObject(photonName,position,rotation);
        }
    }
}
