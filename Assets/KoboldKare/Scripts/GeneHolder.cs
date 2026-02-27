using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NetStack.Serialization;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

[System.Serializable]
public class GeneHolder : NetworkBehaviour {
    public readonly SyncVar<float> maxEnergy = new SyncVar<float>(5f);
    public readonly SyncVar<float> baseSize = new SyncVar<float>(20f);
    public readonly SyncVar<float> fatSize = new SyncVar<float>();
    public readonly SyncVar<float> ballSize = new SyncVar<float>();
    public readonly SyncVar<float> dickSize = new SyncVar<float>();
    public readonly SyncVar<float> breastSize = new SyncVar<float>();
    public readonly SyncVar<float> bellySize = new SyncVar<float>(20f);
    public readonly SyncVar<float> metabolizeCapacitySize = new SyncVar<float>(20f);
    public readonly SyncVar<float> dickThickness = new SyncVar<float>();
    public readonly SyncVar<byte> hue = new SyncVar<byte>();
    public readonly SyncVar<byte> clothingHue = new SyncVar<byte>();
    public readonly SyncVar<byte> brightness = new SyncVar<byte>(128);
    public readonly SyncVar<byte> saturation = new SyncVar<byte>(128);
    public readonly SyncVar<string> dickEquip = new SyncVar<string>("None");
    public readonly SyncVar<byte> grabCount = new SyncVar<byte>(1);
    public readonly SyncVar<string> species = new SyncVar<string>("Kobold");
    public readonly SyncVar<ReagentContents> reagentContents = new SyncVar<ReagentContents>(new ReagentContents(20f));

    
    protected KoboldEntitySpawner.NetworkedEntityInstantiationData instantiationData;
    
    public virtual void SetInstantiationData(KoboldEntitySpawner.NetworkedEntityInstantiationData data) {
        instantiationData = data;
        maxEnergy.Value = data.maxEnergy;
        baseSize.Value = data.baseSize;
        fatSize.Value = data.fatSize;
        ballSize.Value = data.ballSize;
        dickSize.Value = data.dickSize;
        breastSize.Value = data.breastSize;
        bellySize.Value = data.bellySize;
        metabolizeCapacitySize.Value = data.metabolizeCapacitySize;
        dickThickness.Value = data.dickThickness;
        hue.Value = data.hue;
        clothingHue.Value = data.clothingHue;
        brightness.Value = data.brightness;
        saturation.Value = data.saturation;
        dickEquip.Value = data.dickEquip;
        grabCount.Value = data.grabCount;
        species.Value = data.species;
        reagentContents.Value = data.reagentContents;
    }
    
    [ServerRpc]
    public void SetMaxEnergy(float value) {
        maxEnergy.Value = value;
    }
    
    [ServerRpc]
    public void SetBaseSize(float value) {
        baseSize.Value = value;
    }
    
    [ServerRpc]
    public void SetFatSize(float value) {
        fatSize.Value = value;
    }
    
    [ServerRpc]
    public void SetBallSize(float value) {
        ballSize.Value = value;
    }
    
    [ServerRpc]
    public void SetDickSize(float value) {
        dickSize.Value = value;
    }
    
    [ServerRpc]
    public void SetBreastSize(float value) {
        breastSize.Value = value;
    }
    
    [ServerRpc]
    public void SetBellySize(float value) {
        bellySize.Value = value;
        reagentContents.Value.SetMaxVolume(value);
    }

    [ServerRpc]
    public void SetMetabolizeCapacitySize(float value) {
        metabolizeCapacitySize.Value = value;
    }
    
    [ServerRpc]
    public void SetDickThickness(float value) {
        dickThickness.Value = value;
    }
    
    [ServerRpc]
    public void SetHue(byte value) {
        hue.Value = value;
    }
    
    [ServerRpc]
    public void SetClothingHue(byte value) {
        clothingHue.Value = value;
    }
    
    [ServerRpc]
    public void SetBrightness(byte value) {
        brightness.Value = value;
    }
    
    [ServerRpc]
    public void SetSaturation(byte value) {
        saturation.Value = value;
    }
    
    [ServerRpc]
    public void SetDickEquip(string value) {
        dickEquip.Value = value;
    }
    
    [ServerRpc]
    public void SetGrabCount(byte value) {
        grabCount.Value = value;
    }
    
    [ServerRpc]
    public void SetSpecies(string value) {
        species.Value = value;
    }

    private static double NextGaussian (double mean, double standard_deviation, double min, double max) {
        // While this is technically possible, don't want to churn numbers till the end of the universe to continue...
        Assert.IsTrue(mean + standard_deviation * 3f > min && mean - standard_deviation * 3f < max);
        double x;
        do {
            x = NextGaussian(mean, standard_deviation);
        } while (x < min || x > max);
        return x;
    }
    private static double NextGaussian(double mean, double standard_deviation) {
        return mean + RandomGaussian() * standard_deviation;
    }
    private static double RandomGaussian() {
        double u, v, s;
        do {
            u = 2.0f * Random.Range(0f,1f) - 1.0f;
            v = 2.0f * Random.Range(0f,1f) - 1.0f;
            s = u * u + v * v;
        } while (s >= 1.0 || s == 0f);

        double fac = Math.Sqrt(-2.0f * Math.Log(s) / s);
        return u * fac;
    }

    private string GetRandomDick() {
        if (KoboldKareObjectPostProcessor.TryGetRandomAssetKey("Penis", out var infoKey)) {
            return infoKey;
        }
        Debug.LogError("Failed to get a penis, penis database is probably empty.");
        return "None";
    }
    private short GetDickIndex(string name) {
        return (short)KoboldKareObjectPostProcessor.GetAssetID("Penis", name);
    }
    private string GetDickName(short id){
        List<string> penises = new();
        KoboldKareObjectPostProcessor.GetAllAssetNamesInGroup("Penis", penises);
        return penises[id];
    }
    
    private byte GetPlayerIndex(string name) {
        return (byte)KoboldKareObjectPostProcessor.GetAssetID("PlayableCharacter", name);
    }

    private string GetPlayerName(byte id){
        List<string> players = new();
        KoboldKareObjectPostProcessor.GetAllAssetNamesInGroup("PlayableCharacter", players);
        return players[id];
    }

    public void RandomizeGenes(string koboldName=null, float meanMultiplier=1f, float standardDeviationMultiplier=1f) {
        // Slight bias for kobolds with dicks, as they have more variety.
        if (Random.Range(0f,1f) > 0.4f) {
            breastSize.Value = (float)NextGaussian(2.5f*meanMultiplier,2.5f*standardDeviationMultiplier,0f, float.MaxValue);
            dickEquip.Value = GetRandomDick();
        } else {
            breastSize.Value = (float)NextGaussian(15f*meanMultiplier,5.5f*standardDeviationMultiplier,0f, float.MaxValue);
            dickEquip.Value = "None";
        }

        ballSize.Value = (float)NextGaussian(10f*meanMultiplier,5.5f*standardDeviationMultiplier,5f, float.MaxValue);
        dickSize.Value = (float)NextGaussian(10f*meanMultiplier, 5.5f*standardDeviationMultiplier, 5f, float.MaxValue);
        // Since fatness only goes one way (and we have no skinniness factor), we only do half a gaussian curve.
        fatSize.Value = (float)NextGaussian(0f,3.2f*standardDeviationMultiplier,0f, float.MaxValue);
        dickThickness.Value = (float)NextGaussian(0.5f, 0.12f*standardDeviationMultiplier, 0f, float.MaxValue);
        baseSize.Value = (float)NextGaussian(20f*meanMultiplier, 2.5f*standardDeviationMultiplier, 0f, float.MaxValue);//Random.Range(14f, 24f)*multiplier;
        hue.Value = (byte)Random.Range(0, 255);
        clothingHue.Value = hue.Value;  // Let's not randomize this as the results might be weird more often than not
        brightness.Value = (byte)Mathf.RoundToInt((float)NextGaussian(128f,35f*standardDeviationMultiplier, 0f,255f));
        saturation.Value = (byte)Mathf.RoundToInt((float)NextGaussian(128f,35f*standardDeviationMultiplier, 0f,255f));
        if (string.IsNullOrEmpty(koboldName) && KoboldKareObjectPostProcessor.TryGetRandomAssetKey("PlayableCharacter", out var info)) {
            koboldName = info;
        }
        species.Value = koboldName;
    }

    public void CopyGenesFrom(GeneHolder other) {
        maxEnergy.Value = other.maxEnergy.Value;
        baseSize.Value = other.baseSize.Value;
        fatSize.Value = other.fatSize.Value;
        ballSize.Value = other.ballSize.Value;
        dickSize.Value = other.dickSize.Value;
        breastSize.Value = other.breastSize.Value;
        bellySize.Value = other.bellySize.Value;
        metabolizeCapacitySize.Value = other.metabolizeCapacitySize.Value;
        dickThickness.Value = other.dickThickness.Value;
        hue.Value = other.hue.Value;
        clothingHue.Value = other.clothingHue.Value;
        brightness.Value = other.brightness.Value;
        saturation.Value = other.saturation.Value;
        dickEquip.Value = other.dickEquip.Value;
        grabCount.Value = other.grabCount.Value;
        species.Value = other.species.Value;
    }

    public void MixFrom(GeneHolder a, GeneHolder b) {
        if (Random.Range(0f, 1f) > 0.5f) {
            CopyGenesFrom(a);
        } else {
            CopyGenesFrom(b);
        }

        // Blend hue, hue is angle-based, so it loops around. 
        float hueAngA = a.hue.Value / 255f;
        float hueAngB = b.hue.Value / 255f;
        hue.Value = (byte)Mathf.RoundToInt(FloatExtensions.CircularLerp(hueAngA, hueAngB, 0.5f) * 255f);
        clothingHue.Value = hue.Value;
        brightness.Value = (byte)Mathf.RoundToInt(Mathf.Lerp(a.brightness.Value / 255f, b.brightness.Value / 255f, 0.5f)*255f);
        saturation.Value = (byte)Mathf.RoundToInt(Mathf.Lerp(a.saturation.Value / 255f, b.saturation.Value / 255f, 0.5f)*255f);
        bellySize.Value = Mathf.Lerp(a.bellySize.Value, b.bellySize.Value, 0.5f);
        metabolizeCapacitySize.Value = Mathf.Lerp(a.metabolizeCapacitySize.Value, b.metabolizeCapacitySize.Value, 0.5f);
        dickSize.Value = Mathf.Lerp(a.dickSize.Value, b.dickSize.Value, 0.5f);
        ballSize.Value = Mathf.Lerp(a.ballSize.Value, b.ballSize.Value, 0.5f);
        fatSize.Value = Mathf.Lerp(a.fatSize.Value, b.fatSize.Value, 0.5f);
        baseSize.Value = Mathf.Lerp(a.baseSize.Value, b.baseSize.Value, 0.5f);
        maxEnergy.Value = Mathf.Lerp(a.maxEnergy.Value, b.maxEnergy.Value, 0.5f);
        dickThickness.Value = Mathf.Lerp(a.dickThickness.Value, b.dickThickness.Value, 0.5f);
        grabCount.Value = (byte)Mathf.Max(Mathf.RoundToInt(Mathf.Lerp(a.grabCount.Value, b.grabCount.Value, 0.5f)),1);
        // If species don't match, we have a 30% chance to mutate to a new species!
        if (a.species.Value != b.species.Value && Random.Range(0f, 1f) > 0.7f) {
            int maxSpecies = Mathf.Max(KoboldKareObjectPostProcessor.GetAssetID("PlayableCharacter", a.species.Value), KoboldKareObjectPostProcessor.GetAssetID("PlayableCharacter",b.species.Value));
            List<string> possibleMutations = new();
            KoboldKareObjectPostProcessor.GetAllAssetNamesInGroup("PlayableCharacter", possibleMutations);
            species.Value = possibleMutations[(maxSpecies+1)%possibleMutations.Count];
        }
    }

    public void SaveGenes(JSONNode node, string key) {
        JSONNode rootNode = JSONNode.Parse("{}");
        rootNode["maxEnergy"] = maxEnergy.Value;
        rootNode["baseSize"] = baseSize.Value;
        rootNode["fatSize"] = fatSize.Value;
        rootNode["ballSize"] = ballSize.Value;
        rootNode["dickSize"] = dickSize.Value;
        rootNode["breastSize"] = breastSize.Value;
        rootNode["bellySize"] = bellySize.Value;
        rootNode["metabolizeCapacitySize"] = metabolizeCapacitySize.Value;
        rootNode["hue"] = (int)hue.Value;
        rootNode["clothingHue"] = (int)clothingHue.Value;  // FIXME default value should be hue if clothingHue does not exist
        rootNode["brightness"] = (int)brightness.Value;
        rootNode["saturation"] = (int)saturation.Value;
        rootNode["dickEquip"] = dickEquip.Value;
        rootNode["grabCount"] = grabCount.Value;
        rootNode["dickThickness"] = dickThickness.Value;
        rootNode["species"] = species.Value;
        node[key] = rootNode;
    }

    public void LoadGenes(JSONNode node, string key) {
        JSONNode rootNode = node[key];
        maxEnergy.Value = rootNode["maxEnergy"];
        baseSize.Value = rootNode["baseSize"];
        fatSize.Value = rootNode["fatSize"];
        ballSize.Value = rootNode["ballSize"];
        dickSize.Value = rootNode["dickSize"];
        breastSize.Value = rootNode["breastSize"];
        bellySize.Value = rootNode["bellySize"];
        metabolizeCapacitySize.Value = rootNode["metabolizeCapacitySize"];
        hue.Value = (byte)rootNode["hue"].AsInt;
        if (!rootNode.HasKey("clothingHue")) {
            clothingHue.Value = hue.Value;  // Fallback to hue
        } else {
            clothingHue.Value = (byte)rootNode["clothingHue"].AsInt;
        }
        brightness.Value = (byte)rootNode["brightness"].AsInt;
        saturation.Value = (byte)rootNode["saturation"].AsInt;
        dickEquip.Value = rootNode["dickEquip"];
        grabCount.Value = (byte)rootNode["grabCount"].AsInt;
        species.Value = rootNode["species"];
        dickThickness.Value = rootNode["dickThickness"];
    }

    public override string ToString() {
        string blah = "[";
        foreach(var reagent in ReagentDatabase.GetAssets()) {
            if (GetContents().GetVolumeOf(reagent) != 0f) {
                blah += reagent.name + ": " + GetContents().GetVolumeOf(reagent) + ", ";
            }
        }
        blah += "]";
        return $@"{base.ToString()}: With Genes: 
            maxEnergy: {maxEnergy}
            baseSize: {baseSize}
            fatSize: {fatSize}
            ballSize: {ballSize}
            dickSize: {dickSize}
            breastSize: {breastSize}
            bellySize: {bellySize}
            metabolizeCapacitySize: {metabolizeCapacitySize}
            hue: {hue}
            clothingHue: {clothingHue}
            brightness: {brightness}
            saturation: {saturation}
            dickEquip: {dickEquip}
            grabCount: {grabCount}
            dickThickness: {dickThickness}
            species: {species}
            With reagents: {blah}";
    }
    
    public delegate void ContainerFilledAction(GeneHolder container);
    public static event ContainerFilledAction containerFilled;
    public static event ContainerFilledAction containerInflated;
    public ReagentContents GetContents() {
        return reagentContents.Value;
    }
    [System.Serializable]
    public class InspectorReagent {
        public ScriptableReagent reagent;
        public float volume;
    }
    public enum ContainerType : byte {
        OpenTop,
        Sealed,
        Mouth,
    }
    public enum InjectType : byte {
        Inject,
        Spray,
        Flood,
        Metabolize,
        Vacuum,
    }
    private static bool[,] ReagentMixMatrix = new bool[,]{
        // OpenTop, Sealed, Mouth
        {  true,   true,    true }, // Inject
        {  true,   false,   true }, // Spray
        {  true,   false,   false }, // Flood
        {  true,   true,   true }, // Metabolize
        {  true,   true,   true }, // Vacuum
    };
    public delegate void ReagentContainerChangedEvent(ReagentContents c);
    public static bool IsMixable(ContainerType container, InjectType injectionType) {
        return ReagentMixMatrix[(int)injectionType,(int)container];
    }
    public float volume => GetContents().volume;

    public float maxVolume {
        get => GetContents().GetMaxVolume();
        set {
            GetContents().SetMaxVolume(value);
        }
    }

    public Color GetColor() => GetContents().GetColor();
    public ContainerType type;
    
    public event ReagentContainerChangedEvent OnFilled, OnEmpty;
    
    public bool isFull => Mathf.Approximately(GetContents().volume, GetContents().GetMaxVolume());
    public bool isEmpty => Mathf.Approximately(GetContents().volume,0f);
    public bool IsCleaningAgent() => GetContents().IsCleaningAgent();
    public float GetVolumeOf(ScriptableReagent reagent) => GetContents().GetVolumeOf(reagent);
    public float GetVolumeOf(byte id) => GetContents().GetVolumeOf(id);
    public InspectorReagent[] startingReagents;

    private bool hasGenes = false;

    private bool filled = false;
    private bool emptied = false;
    protected virtual void Awake() {
        if (startingReagents != null) {
            foreach (var reagent in startingReagents) {
                AddMix(reagent.reagent, reagent.volume, InjectType.Inject);
            }
            foreach (var reagentContainer in GetComponentsInChildren<GenericReagentContainer>()) {
                foreach (var reagent in reagentContainer.startingReagents) {
                    AddMix(reagent.reagent, reagent.volume, InjectType.Inject);
                }
            }
        }
    }
    
    protected virtual void Start() {
        reagentContents.OnChange += OnReagentsChanged;
    }

    private void OnReagentsChanged(ReagentContents prev, ReagentContents next, bool asServer) {
        bool newFilled = Mathf.Approximately(next.volume, next.maxVolume);
        if (!filled && newFilled) {
            OnFilled?.Invoke(next);
        }
        filled = newFilled;

        bool newEmptied = next.volume <= 0f;
        if (!emptied && newEmptied) {
            OnEmpty?.Invoke(next);
        }
        emptied = newEmptied;
    }

    // FIXME FISHNET
    //[PunRPC]
    public ReagentContents Spill(float spillVolume) {
        ReagentContents spillContents = GetContents().Spill(spillVolume);
        SetReagentContents(GetContents());
        reagentContents.DirtyAll();
        return spillContents;
    }

    [ObserversRpc]
    public void SetReagentContents(ReagentContents contents) {
        reagentContents.Value = contents;
        reagentContents.DirtyAll();
    }
    
    private void TransferMix(GeneHolder injector, float amount, InjectType injectType) {
        if (!IsMixable(this.type, injectType)) {
            return;
        }
        ReagentContents spill = injector.Spill(amount);
        AddMix(spill, injectType);
        CopyGenesFrom(injector);
        reagentContents.DirtyAll();
    }
    private bool AddMix(ScriptableReagent incomingReagent, float volume, InjectType injectType) {
        if (!IsMixable(type, injectType)) {
            return false;
        }
        GetContents().AddMix((byte)ReagentDatabase.GetID(incomingReagent), volume, this);
        reagentContents.DirtyAll();
        return true;
    }
    
    [ObserversRpc]
    public void AddMix(ReagentContents incomingReagents, InjectType injectType) {
        if (!IsMixable(type, injectType)) {
            return;
        }
        GetContents().AddMix(incomingReagents, this);
        reagentContents.DirtyAll();
    }
    public void AddMix(Reagent reagent, InjectType injectType, GeneHolder worldContainer = null) {
        if (!IsMixable(type, injectType)) {
            return;
        }
        GetContents().AddMix(reagent.id, reagent.volume, worldContainer);
        reagentContents.DirtyAll();
    }

    public ReagentContents Peek() => new(GetContents());
    public ReagentContents Metabolize(float deltaTime) => GetContents().Metabolize(deltaTime);
    public void OverrideReagent(Reagent r) => GetContents().OverrideReagent(r.id, r.volume);
    public void OverrideReagent(ScriptableReagent r, float volume) => GetContents().OverrideReagent((byte)ReagentDatabase.GetID(r), volume);

    public void RefillToFullWithDefaultContents(){
        if(startingReagents.Length != 0){
            foreach (var item in startingReagents){
                AddMix(item.reagent,item.volume,InjectType.Spray);
            }
        }
    }

    public float GetWorth() {
        return GetContents().GetValue();
    }

    
    // FIXME FISHNET
    /*
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            BitBuffer bitBuffer = new BitBuffer(8);
            bitBuffer.AddReagentContents(GetContents());
            stream.SendNext(bitBuffer);
        } else {
            BitBuffer data = (BitBuffer)stream.ReceiveNext();
            ReagentContents newContents = data.ReadReagentContents();
            GetContents().Copy(newContents);
            OnReagentContentsChanged(InjectType.Metabolize);
            PhotonProfiler.LogReceive(data.Length);
        }
    }*/

    // FIXME FISHNET
    /*
    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData == null) {
            return;
        }
        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is BitBuffer) {
            BitBuffer buffer = (BitBuffer)info.photonView.InstantiationData[0];
            // This buffer might be shared.
            buffer.SetReadPosition(0);
            SetGenes(buffer.ReadKoboldGenes());
            PhotonProfiler.LogReceive(buffer.Length);
        }
        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is not BitBuffer) {
            throw new UnityException("Unexpected spawn data for container");
        }
    }*/
}