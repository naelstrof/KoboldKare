using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FishNet;
using JigglePhysics;
using UnityEngine;
using PenetrationTech;
using Naelstrof.Inflatable;
using SimpleJSON;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Kobold : MonoBehaviour, ISavable, IValuedGood {
    private static Collider[] colliders = new Collider[32];
    [System.Serializable]
    public class PenetrableSet {
        public Penetrable penetratable;
        public Rigidbody ragdollAttachBody;
        public bool isFemaleExclusiveAnatomy = false;
        public bool canLayEgg = true;
        public bool isSelfPenetrableOnRagdoll = false;
    }

    public delegate void EnergyChangedAction(float value, float maxValue);
    public delegate void KoboldSpawnAction(Kobold kobold);
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

    private NetworkedKobold networkedKobold;
    

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
    public PhotonGameObjectReference GetHeartPrefab() => heartPrefab;

    public LayerMask GetHeartHitMask() => heartHitMask;
    
    private UsableColliderComparer usableColliderComparer;
    public ReagentContents metabolizedContents;
    
    [SerializeField]
    private AudioPack tummyGrumbles;
    [FormerlySerializedAs("gurglePack")] [SerializeField]
    private AudioPack garglePack;
    
    [HideInInspector]
    public List<DickDescriptor.DickSet> activeDicks = new List<DickDescriptor.DickSet>();
    private AudioSource gargleSource;
    private AudioSource tummyGrumbleSource;
    
    [SerializeField]
    private List<Renderer> koboldBodyRenderers;

    public void AddKoboldBodyRenderer(Renderer renderer) {
        if (!renderer) {
            return;
        }
        if (koboldBodyRenderers.Contains(renderer)) {
            return;
        }
        
        koboldBodyRenderers.Add(renderer);
        for (int i = koboldBodyRenderers.Count-1; i >= 0; i--) {
            if (!koboldBodyRenderers[i]) {
                koboldBodyRenderers.RemoveAt(i);
            }
        }
        
        var array = koboldBodyRenderers.ToArray();
        foreach (JiggleRigRendererLOD lod in GetComponentsInChildren<JiggleRigRendererLOD>()) {
            if (lod) {
                lod.SetRenderers(array);
            }
        }
        foreach (var inflater in GetAllInflatableListeners()) {
            if (inflater is InflatableBreast inflatableBreast) {
                inflatableBreast.AddTargetRenderer((SkinnedMeshRenderer)renderer);
            }

            if (inflater is InflatableBelly belly) {
                belly.AddTargetRenderer((SkinnedMeshRenderer)renderer);
            }

            if (inflater is InflatableBlendShape inflatableBlendShape) {
                inflatableBlendShape.AddTargetRenderer((SkinnedMeshRenderer)renderer);
            }
        }

    }
    
    public void RemoveKoboldBodyRenderer(Renderer renderer) {
        if (!koboldBodyRenderers.Contains(renderer)) {
            return;
        }
        koboldBodyRenderers.Remove(renderer);
        for (int i = 0; i < koboldBodyRenderers.Count; i++) {
            if (!koboldBodyRenderers[i]) {
                koboldBodyRenderers.RemoveAt(i);
            }
        }
        var array = koboldBodyRenderers.ToArray();
        foreach (JiggleRigRendererLOD lod in GetComponentsInChildren<JiggleRigRendererLOD>()) {
            lod.SetRenderers(array);
        }
        foreach (var inflater in GetAllInflatableListeners()) {
            if (inflater is InflatableBreast inflatableBreast) {
                inflatableBreast.RemoveTargetRenderer((SkinnedMeshRenderer)renderer);
            }

            if (inflater is InflatableBelly belly) {
                belly.RemoveTargetRenderer((SkinnedMeshRenderer)renderer);
            }

            if (inflater is InflatableBlendShape inflatableBlendShape) {
                inflatableBlendShape.RemoveTargetRenderer((SkinnedMeshRenderer)renderer);
            }
        }
    }

    public List<Renderer> GetKoboldBodyRenderers() => koboldBodyRenderers;
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
    
    public void Lactate() {
        milkLactator.StartMilking(this);
    }

    private Ragdoller ragdoller => GetComponent<Ragdoller>();
    public void AddStimulation(float s) {
        stimulation += s;
        // FIXME FISHNET
        /*if (photonView.IsMine && stimulation >= stimulationMax && TryConsumeEnergy(1)) {
            photonView.RPC(nameof(Cum), RpcTarget.All);
        }*/
    }
    
    // FIXME FISHNET
    //[PunRPC]
    public void Cum() {
        foreach(var dickSet in activeDicks) {
            // TODO: This is a really, really terrible way to make a dick cum lol. Clean this up.
            dickSet.descriptor.StartCoroutine(dickSet.descriptor.CumRoutine(dickSet));
        }
        PumpUpDick(1f);
        stimulation = stimulationMin;
    }

    // FIXME FISHNET
    //[PunRPC]
    public void SetEnergyRPC(float newEnergy) {
        /*
        float diff = newEnergy - energy;
        if (diff < 0f && GetGenes().fatSize > 0f && photonView.IsMine) {
            SetGenes(GetGenes().With(fatSize: Mathf.Max(GetGenes().fatSize + diff, 0f)));
        }

        energy = newEnergy;
        energy = Mathf.Max(0, energy);
        energyChanged?.Invoke(energy, GetGenes().maxEnergy);*/
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

    public Ragdoller GetRagdoller() => ragdoller;
    public float GetMaxEnergy() {
        return networkedKobold.maxEnergy.Value;
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

    // FIXME FISHNET
    //[PunRPC]
    public void SetDickRPC(short dickID) {
        //SetGenes(GetGenes().With(dickEquip: dickID));
    }

    private AssetGroup.AssetLocation.AssetHandle<GameObject> penisHandle;
    
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
        
        // FIXME FISHNET
        //photonView.ObservedComponents.Add(bellyContainer);
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

    private void OnColorChanged(byte prev, byte next, bool asServer) {
        Vector4 hbcs = new Vector4(networkedKobold.hue.Value/255f, networkedKobold.brightness.Value/255f, 0.5f, networkedKobold.saturation.Value/255f);
        Vector4 chbcs = new Vector4(networkedKobold.clothingHue.Value/255f, networkedKobold.brightness.Value/255f, 0.5f, networkedKobold.saturation.Value/255f);
        // Set color
        foreach (Renderer r in koboldBodyRenderers) {
            if (r == null) {
                continue;
            }
            foreach (Material m in r.materials) {
                // If it's an equipment, it will have the EquipmentComponent
                if (r.gameObject.GetComponent<EquipmentSkinnedMesh.EquipmentComponent>() != null) {
                    m.SetVector(BrightnessContrastSaturation, chbcs);
                } else {
                    m.SetVector(BrightnessContrastSaturation, hbcs);
                }
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
    }

    private void OnMetabolizeCapacitySizeChanged(float prev, float next, bool asServer) {
        metabolizedContents.SetMaxVolume(next);
    }

    private void OnBellySizeChanged(float prev, float next, bool asServer) {
        bellyContainer.maxVolume = next;
    }

    private void OnBreastSizeChanged(float prev, float next, bool asServer) {
        boobsInflater.SetSize(Mathf.Log(1f + next / 20f, 2f), this);
    }

    private void OnFatSizeChanged(float prev, float next, bool asServer) {
        fatnessInflater.SetSize(Mathf.Log(1f + next / 20f, 2f), this);
    }

    private void OnBaseSizeChanged(float prev, float next, bool asServer) {
        if (ragdoller.ragdolled) {
            sizeInflater.SetSizeInstant(Mathf.Max(Mathf.Log(1f + next / 20f, 2f), 0.2f));
        } else {
            sizeInflater.SetSize(Mathf.Max(Mathf.Log(1f + next / 20f, 2f), 0.2f), this);
        }
    }

    private void OnGrabCountChanged(byte prev, byte next, bool asServer) {
        grabber.SetMaxGrabCount(next);
    }

    private void OnDickChanged(string prev, string next, bool asServer) {
        _ = OnDickChangedRoutine(prev, next, asServer);
    }

    private bool changingDick = false;

    private async Task OnDickChangedRoutine(string prev, string next, bool asServer) {
        if (!asServer && InstanceFinder.ServerManager.Started) {
            return;
        }

        while (changingDick) {
            await Task.Delay(1000);
        }

        changingDick = true;
        try {
            penisHandle?.Release();
            if (dickObject) {
                dickObject.GetComponentInChildren<DickDescriptor>().RemoveFrom(this);
                Destroy(dickObject);
            }

            if (next == "None") {
                return;
            }

            penisHandle = await KoboldKareObjectPostProcessor.GetAssetAsync("Penis", next, GameManager.GetErrorPenis());
            dickObject = Instantiate(penisHandle.asset, GetAttachPointTransform(Equipment.AttachPoint.Crotch));
            dickObject.GetComponentInChildren<DickDescriptor>().AttachTo(this);
        } finally {
            changingDick = false;
        }
    }

    void Start() {
        controller = GetComponent<KoboldCharacterController>();
        lastPumpTime = Time.timeSinceLevelLoad;
        DayNightCycle.AddMetabolizationListener(OnMetabolizationEvent);
        bellyContainer.OnChange += OnBellyContentsChanged;
        
        networkedKobold = GetComponentInParent<NetworkedKobold>();
        networkedKobold.dickEquip.OnChange += OnDickChanged;
        OnDickChanged(networkedKobold.dickEquip.Value, networkedKobold.dickEquip.Value, true);
        networkedKobold.grabCount.OnChange += OnGrabCountChanged;
        OnGrabCountChanged(networkedKobold.grabCount.Value, networkedKobold.grabCount.Value, true);
        networkedKobold.baseSize.OnChange += OnBaseSizeChanged;
        OnBaseSizeChanged(networkedKobold.baseSize.Value, networkedKobold.baseSize.Value, true);
        networkedKobold.fatSize.OnChange += OnFatSizeChanged;
        OnFatSizeChanged(networkedKobold.fatSize.Value, networkedKobold.fatSize.Value, true);
        networkedKobold.breastSize.OnChange += OnBreastSizeChanged;
        OnBreastSizeChanged(networkedKobold.breastSize.Value, networkedKobold.breastSize.Value, true);
        networkedKobold.bellySize.OnChange += OnBellySizeChanged;
        OnBellySizeChanged(networkedKobold.bellySize.Value, networkedKobold.bellySize.Value, true);
        networkedKobold.metabolizeCapacitySize.OnChange += OnMetabolizeCapacitySizeChanged;
        OnMetabolizeCapacitySizeChanged(networkedKobold.metabolizeCapacitySize.Value, networkedKobold.metabolizeCapacitySize.Value, true);
        networkedKobold.brightness.OnChange += OnColorChanged;
        networkedKobold.hue.OnChange += OnColorChanged;
        networkedKobold.saturation.OnChange += OnColorChanged;
        networkedKobold.clothingHue.OnChange += OnColorChanged;
        OnColorChanged(0, 0, true);
        
        networkedKobold.grabbed += OnGrab;
        networkedKobold.released += OnRelease;
        networkedKobold.grabRequested += OnGrabRequest;
        networkedKobold.SetGrabTransform(hip);
        
        // FIXME FISHNET
        //PlayAreaEnforcer.AddTrackedObject(photonView);
    }
    private void OnDestroy() {
        DayNightCycle.RemoveMetabolizationListener(OnMetabolizationEvent);
        bellyContainer.OnChange -= OnBellyContentsChanged;
        if (networkedKobold) {
            networkedKobold.dickEquip.OnChange -= OnDickChanged;
            networkedKobold.grabCount.OnChange -= OnGrabCountChanged;
            networkedKobold.baseSize.OnChange -= OnBaseSizeChanged;
            networkedKobold.fatSize.OnChange -= OnFatSizeChanged;
            networkedKobold.breastSize.OnChange -= OnBreastSizeChanged;
            networkedKobold.bellySize.OnChange -= OnBellySizeChanged;
            networkedKobold.metabolizeCapacitySize.OnChange -= OnMetabolizeCapacitySizeChanged;
            networkedKobold.brightness.OnChange -= OnColorChanged;
            networkedKobold.hue.OnChange -= OnColorChanged;
            networkedKobold.saturation.OnChange -= OnColorChanged;
            networkedKobold.clothingHue.OnChange -= OnColorChanged;
            networkedKobold.grabbed -= OnGrab;
            networkedKobold.released -= OnRelease;
            networkedKobold.grabRequested -= OnGrabRequest;
        }
        // FIXME FISHNET
        //PlayAreaEnforcer.RemoveTrackedObject(photonView);
    }
    public void OnGrab(NetworkedKobold by) {
        grabbed = true;
        carriedChanged?.Invoke(true);
        controller.frictionMultiplier = 0.1f;
        controller.enabled = false;
        
        // FIXME FISHNET
        /*if (photonView.IsMine) {
            photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
        }*/
    }
    public void PumpUpDick(float amount) {
        if (amount > 0 ) {
            lastPumpTime = Time.timeSinceLevelLoad;
        }
        arousal += amount;
        arousal = Mathf.Clamp01(arousal);
    }
    public IEnumerator ThrowRoutine() {
        // FIXME FISHNET
        //photonView.RPC(nameof(Ragdoller.PushRagdoll), RpcTarget.All);
        yield return new WaitForSeconds(3f);
        //photonView.RPC(nameof(Ragdoller.PopRagdoll), RpcTarget.All);
    }

    private bool OnGrabRequest(NetworkedKobold kobold) {
        return !controller.inputJump;
    }

    public void OnRelease(NetworkedKobold by, Vector3 velocity) {
        carriedChanged?.Invoke(false);
        controller.frictionMultiplier = 1f;
        grabbed = false;
        controller.enabled = true;

        if (!networkedKobold.IsOwner) {
            return;
        }
        
        foreach (Rigidbody b in ragdoller.GetRagdollBodies()) {
            b.velocity = velocity;
        }

        if (velocity.magnitude > 3f) {
            StartCoroutine(ThrowRoutine());
        } else {
            int hits = Physics.OverlapSphereNonAlloc(transform.position, Mathf.Max(1f, Mathf.Log(1f+transform.localScale.x,2f)), colliders, GameManager.instance.usableHitMask, QueryTriggerInteraction.Collide);
            usableColliderComparer.SetCheckPoint(transform.position);
            Array.Sort(colliders, 0, hits, usableColliderComparer);
            for (int i=0;i<hits;i++) {
                Collider c = colliders[i];
                NetworkedEntity usable = c.GetComponentInParent<NetworkedEntity>();
                if (usable != null && usable.CanUse(networkedKobold)) {
                    usable.OnUse(networkedKobold);
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
    private float FloorNearestPower(float baseNum, float target) {
        float f = baseNum;
        for(;f<=target;f*=baseNum) {}
        return f/baseNum;
    }

    public void ProcessReagents(ReagentContents contents) {
        addbackReagents.Clear();
        float newEnergy = networkedKobold.energy.Value;
        float passiveEnergyGeneration = 0.025f;
        if (newEnergy < 1f) {
            if (ragdoller.ragdolled) {
                passiveEnergyGeneration *= 4f;
            }
            newEnergy = Mathf.MoveTowards(newEnergy, 1.1f, passiveEnergyGeneration);
        }
        foreach (var pair in contents) {
            if (ReagentDatabase.TryGetAsset(pair.id, out var reagent)) {
                float processedAmount = pair.volume;
                reagent.GetConsumptionEvent().OnConsume(networkedKobold, reagent, ref processedAmount, ref consumedReagents, ref addbackReagents, ref newEnergy);
                pair.volume -= processedAmount;
            }
        }
        bellyContainer.AddMix(contents, GenericReagentContainer.InjectType.Inject); 
        bellyContainer.AddMix(addbackReagents, GenericReagentContainer.InjectType.Inject);
        float overflowEnergy = Mathf.Max(newEnergy - GetMaxEnergy(), 0f);
        if (overflowEnergy != 0f) {
            networkedKobold.SetFatSize(networkedKobold.fatSize.Value + overflowEnergy);
        }
        
        if (Math.Abs(networkedKobold.energy.Value - newEnergy) > 0.001f) {
            networkedKobold.SetEnergy(Mathf.Clamp(newEnergy, 0f, GetMaxEnergy()));
        }
    }
    
    private void OnMetabolizationEvent(float f) {
        // FIXME FISHNET
        /*if (!photonView.IsMine) {
            return;
        }*/
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

    // FIXME FISHNET
    /*public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
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
    }*/

    // FIXME FISHNET
    /*public void OnPhotonInstantiate(PhotonMessageInfo info) {
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
    }*/

    public void Save(JSONNode node) {
        networkedKobold.SaveGenes(node, "genes");
        node["arousal"] = arousal;
        metabolizedContents.Save(node, "metabolizedContents");
        consumedReagents.Save(node, "consumedReagents");
        
        // FIXME FISHNET
        // bool isPlayerControlled = (Kobold)PhotonNetwork.LocalPlayer.TagObject == this;
        // node["isPlayerControlled"] = isPlayerControlled;
    }

    public Task Load(JSONNode node) {
        networkedKobold.LoadGenes(node, "genes");
        arousal = node["arousal"];
        metabolizedContents.Load(node, "metabolizedContents");
        consumedReagents.Load(node, "consumedReagents");
        bool isPlayerControlled = node["isPlayerControlled"];
        if (isPlayerControlled) {
            // FIXME FISHNET
            //PhotonNetwork.LocalPlayer.TagObject = this;
            GetComponent<NetworkedKobold>().SetControlType(NetworkedKobold.ControlType.NetworkedPlayer);
        }
        return Task.CompletedTask;
    }

    public float GetWorth() {
        return 5f+(Mathf.Log(1f+(networkedKobold.baseSize.Value + networkedKobold.dickSize.Value + networkedKobold.breastSize.Value + networkedKobold.fatSize.Value),2)*6f);
    }
}