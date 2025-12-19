using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
           species: {species}";
    }
}