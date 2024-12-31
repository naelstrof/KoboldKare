using System;
using NetStack.Quantization;
using NetStack.Serialization;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public static class KoboldGenesBitBufferExtension {
    public static void AddKoboldGenes(this BitBuffer buffer, KoboldGenes genes) {
        if (genes == null) {
            buffer.AddBool(false);
            return;
        }
        buffer.AddBool(true);
        ushort maxEnergyQ = HalfPrecision.Quantize(genes.maxEnergy);
        ushort baseSizeQ = HalfPrecision.Quantize(genes.baseSize);
        ushort fatSizeQ = HalfPrecision.Quantize(genes.fatSize);
        ushort ballSizeQ = HalfPrecision.Quantize(genes.ballSize);
        ushort dickSizeQ = HalfPrecision.Quantize(genes.dickSize);
        ushort breastSizeQ = HalfPrecision.Quantize(genes.breastSize);
        ushort bellySizeQ = HalfPrecision.Quantize(genes.bellySize);
        ushort metabolizeCapacitySizeQ = HalfPrecision.Quantize(genes.metabolizeCapacitySize);
        ushort dickThicknessQ = HalfPrecision.Quantize(genes.dickThickness);
        buffer.AddUShort(maxEnergyQ);
        buffer.AddUShort(baseSizeQ);
        buffer.AddUShort(fatSizeQ);
        buffer.AddUShort(ballSizeQ);
        buffer.AddUShort(dickSizeQ);
        buffer.AddUShort(breastSizeQ);
        buffer.AddUShort(bellySizeQ);
        buffer.AddUShort(metabolizeCapacitySizeQ);
        buffer.AddUShort(dickThicknessQ);
        buffer.AddByte(genes.hue);
        buffer.AddByte(genes.brightness);
        buffer.AddByte(genes.saturation);
        buffer.AddByte(genes.dickEquip);
        buffer.AddByte(genes.grabCount);
        buffer.AddByte(genes.species);
    }
    public static KoboldGenes ReadKoboldGenes(this BitBuffer buffer) {
        if (!buffer.ReadBool()) {
            return null;
        }
        return new KoboldGenes {
            maxEnergy = HalfPrecision.Dequantize(buffer.ReadUShort()),
            baseSize = HalfPrecision.Dequantize(buffer.ReadUShort()),
            fatSize = HalfPrecision.Dequantize(buffer.ReadUShort()),
            ballSize = HalfPrecision.Dequantize(buffer.ReadUShort()),
            dickSize = HalfPrecision.Dequantize(buffer.ReadUShort()),
            breastSize = HalfPrecision.Dequantize(buffer.ReadUShort()),
            bellySize = HalfPrecision.Dequantize(buffer.ReadUShort()),
            metabolizeCapacitySize = HalfPrecision.Dequantize(buffer.ReadUShort()),
            dickThickness = HalfPrecision.Dequantize(buffer.ReadUShort()),
            hue = buffer.ReadByte(),
            brightness = buffer.ReadByte(),
            saturation = buffer.ReadByte(),
            dickEquip = buffer.ReadByte(),
            grabCount = buffer.ReadByte(),
            species = buffer.ReadByte()
        };
    }
}

[System.Serializable]
public class KoboldGenes {
    public float maxEnergy = 5f;
    public float baseSize = 20f;
    public float fatSize;
    public float ballSize;
    public float dickSize;
    public float breastSize;
    public float bellySize = 20f;
    public float metabolizeCapacitySize = 20f;
    public float dickThickness;
    public byte hue;
    public byte brightness = 128;
    public byte saturation = 128;
    public byte dickEquip = byte.MaxValue;
    public byte grabCount = 1;
    public byte species = 0;

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

    public KoboldGenes With(float? maxEnergy = null, float? baseSize = null, float? fatSize = null,
            float? ballSize = null, float? dickSize = null, float? breastSize = null, float? bellySize = null,
            float? metabolizeCapacitySize = null, byte? hue = null, byte? brightness = null,
            byte? saturation = null, byte? dickEquip = null, float? dickThickness = null, byte? grabCount = null, byte? species = null) {
        return new KoboldGenes() {
            maxEnergy = maxEnergy ?? this.maxEnergy,
            baseSize = baseSize ?? this.baseSize,
            fatSize = fatSize ?? this.fatSize,
            ballSize = ballSize ?? this.ballSize,
            dickSize = dickSize ?? this.dickSize,
            breastSize = breastSize ?? this.breastSize,
            bellySize = bellySize ?? this.bellySize,
            metabolizeCapacitySize = metabolizeCapacitySize ?? this.metabolizeCapacitySize,
            hue = hue ?? this.hue,
            brightness = brightness ?? this.brightness,
            saturation = saturation ?? this.saturation,
            dickEquip = dickEquip ?? this.dickEquip,
            dickThickness = dickThickness ?? this.dickThickness,
            grabCount = grabCount ?? this.grabCount,
            species = species ?? this.species
        };
    }

    private byte GetRandomDick() {
        var penisDatabase = GameManager.GetPenisDatabase();
        var penises = penisDatabase.GetValidPrefabReferenceInfos();
        var selectedPenis = penisDatabase.GetRandom();
        if (selectedPenis == null) {
            throw new UnityException("Failed to get a penis, penis database is probably empty.");
        }
        return (byte)penises.IndexOf(selectedPenis);
    }
    private byte GetDickIndex(string name){
        var penisDatabase = GameManager.GetPenisDatabase();
        var dicks = penisDatabase.GetValidPrefabReferenceInfos();
        foreach (var info in dicks) {
            if (name.Contains(info.GetKey())) {
                return (byte)dicks.IndexOf(info);
            }
        }
        return GetRandomDick();// Get random dick if can't find the correct one
    }
    private string GetDickName(byte id){
        var penisDatabase = GameManager.GetPenisDatabase();
        var dicks = penisDatabase.GetValidPrefabReferenceInfos();
        return dicks[id].GetKey();
    }
    private byte GetPlayerIndex(string name) {
        var playerDatabase = GameManager.GetPlayerDatabase();
        var players = playerDatabase.GetValidPrefabReferenceInfos();
        foreach (var info in players) {
            if (name.Contains(info.GetKey())) {
                return (byte)players.IndexOf(info);
            }
        }
        return 0;
    }

    private string GetPlayerName(byte id){
        var playerDatabase = GameManager.GetPlayerDatabase();
        var players = playerDatabase.GetValidPrefabReferenceInfos();
        return players[id].GetKey();
    }

    public KoboldGenes Randomize(string koboldName=null, float meanMultiplier=1f, float standardDeviationMultiplier=1f) {
        // Slight bias for kobolds with dicks, as they have more variety.
        if (Random.Range(0f,1f) > 0.4f) {
            breastSize = (float)NextGaussian(2.5f*meanMultiplier,2.5f*standardDeviationMultiplier,0f, float.MaxValue);
            dickEquip = GetRandomDick();
        } else {
            breastSize = (float)NextGaussian(15f*meanMultiplier,5.5f*standardDeviationMultiplier,0f, float.MaxValue);
            dickEquip = byte.MaxValue;
        }

        ballSize = (float)NextGaussian(10f*meanMultiplier,5.5f*standardDeviationMultiplier,5f, float.MaxValue);
        dickSize = (float)NextGaussian(10f*meanMultiplier, 5.5f*standardDeviationMultiplier, 5f, float.MaxValue);
        // Since fatness only goes one way (and we have no skinniness factor), we only do half a gaussian curve.
        fatSize = (float)NextGaussian(0f,3.2f*standardDeviationMultiplier,0f, float.MaxValue);
        dickThickness = (float)NextGaussian(0.5f, 0.12f*standardDeviationMultiplier, 0f, float.MaxValue);
        baseSize = (float)NextGaussian(20f*meanMultiplier, 2.5f*standardDeviationMultiplier, 0f, float.MaxValue);//Random.Range(14f, 24f)*multiplier;
        hue = (byte)Random.Range(0, 255);
        brightness = (byte)Mathf.RoundToInt((float)NextGaussian(128f,35f*standardDeviationMultiplier, 0f,255f));
        saturation = (byte)Mathf.RoundToInt((float)NextGaussian(128f,35f*standardDeviationMultiplier, 0f,255f));
        if (string.IsNullOrEmpty(koboldName)) {
            koboldName = GameManager.GetPlayerDatabase().GetRandom().GetKey();
        }
        species = GetPlayerIndex(koboldName);
        return this;
    }

    public static KoboldGenes Mix(KoboldGenes a, KoboldGenes b) {
        KoboldGenes c;
        // This should never happen.
        if (a == null && b == null) {
            Debug.LogError("Tried to mix two null gene pools, how does this happen?");
            return new KoboldGenes().Randomize("Kobold");
        }
        
        // Single parent? Also shouldn't happen.
        if (a == null) {
            return b;
        }
        if (b == null) {
            return a;
        }

        if (Random.Range(0f, 1f) > 0.5f) {
            c = (KoboldGenes)a.MemberwiseClone();
        } else {
            c = (KoboldGenes)b.MemberwiseClone();
        }

        // Blend hue, hue is angle-based, so it loops around. 
        float hueAngA = a.hue / 255f;
        float hueAngB = b.hue / 255f;
        c.hue = (byte)Mathf.RoundToInt(FloatExtensions.CircularLerp(hueAngA, hueAngB, 0.5f) * 255f);
        c.brightness = (byte)Mathf.RoundToInt(Mathf.Lerp(a.brightness / 255f, b.brightness / 255f, 0.5f)*255f);
        c.saturation = (byte)Mathf.RoundToInt(Mathf.Lerp(a.saturation / 255f, b.saturation / 255f, 0.5f)*255f);
        c.bellySize = Mathf.Lerp(a.bellySize, b.bellySize, 0.5f);
        c.metabolizeCapacitySize = Mathf.Lerp(a.metabolizeCapacitySize, b.metabolizeCapacitySize, a.metabolizeCapacitySize>b.metabolizeCapacitySize ? 0.2f : 0.8f);
        c.dickSize = Mathf.Lerp(a.dickSize, b.dickSize, 0.5f);
        c.ballSize = Mathf.Lerp(a.ballSize, b.ballSize, 0.5f);
        c.fatSize = Mathf.Lerp(a.fatSize, b.fatSize, 0.5f);
        c.baseSize = Mathf.Lerp(a.baseSize, b.baseSize, 0.5f);
        c.maxEnergy = Mathf.Lerp(a.maxEnergy, b.maxEnergy, a.maxEnergy>b.maxEnergy ? 0.2f : 0.8f );
        c.dickThickness = Mathf.Lerp(a.dickThickness, b.dickThickness, 0.5f);
        c.grabCount = (byte)Mathf.Max(Mathf.RoundToInt(Mathf.Lerp(a.grabCount, b.grabCount, a.grabCount>b.grabCount? 0.2f: 0.8f )),1);
        // If species don't match, we have a 30% chance to mutate to a new species!
        if (a.species != b.species && Random.Range(0f, 1f) > 0.7f) {
            int maxSpecies = Mathf.Max(a.species, b.species);
            c.species = (byte)((maxSpecies+1)%GameManager.GetPlayerDatabase().GetValidPrefabReferenceInfos().Count);
        }
        return c;
    }

    /*public const short byteCount = sizeof(float) * 9 + sizeof(byte) * 5;
    public static short Serialize(StreamBuffer outStream, object customObject) {
        KoboldGenes genes = (KoboldGenes)customObject;
        byte[] bytes = new byte[byteCount];
        int index = 0;
        Protocol.Serialize(genes.maxEnergy, bytes, ref index);
        Protocol.Serialize(genes.baseSize, bytes, ref index);
        Protocol.Serialize(genes.fatSize, bytes, ref index);
        Protocol.Serialize(genes.ballSize, bytes, ref index);
        Protocol.Serialize(genes.dickSize, bytes, ref index);
        Protocol.Serialize(genes.breastSize, bytes, ref index);
        Protocol.Serialize(genes.bellySize, bytes, ref index);
        Protocol.Serialize(genes.metabolizeCapacitySize, bytes, ref index);
        bytes[index++] = genes.hue;
        bytes[index++] = genes.brightness;
        bytes[index++] = genes.saturation;
        bytes[index++] = genes.dickEquip;
        bytes[index++] = genes.grabCount;
        Protocol.Serialize(genes.dickThickness, bytes, ref index);
        outStream.Write(bytes, 0, byteCount);
        return byteCount;
    }
    public static object Deserialize(StreamBuffer inStream, short length) {
        KoboldGenes genes = new KoboldGenes();
        byte[] bytes = new byte[length];
        inStream.Read(bytes, 0, length);
        int index = 0;
        while (index < length) {
            Protocol.Deserialize(out genes.maxEnergy, bytes, ref index);
            Protocol.Deserialize(out genes.baseSize, bytes, ref index);
            Protocol.Deserialize(out genes.fatSize, bytes, ref index);
            Protocol.Deserialize(out genes.ballSize, bytes, ref index);
            Protocol.Deserialize(out genes.dickSize, bytes, ref index);
            Protocol.Deserialize(out genes.breastSize, bytes, ref index);
            Protocol.Deserialize(out genes.bellySize, bytes, ref index);
            Protocol.Deserialize(out genes.metabolizeCapacitySize, bytes, ref index);
            genes.hue = bytes[index++];
            genes.brightness = bytes[index++];
            genes.saturation = bytes[index++];
            genes.dickEquip = bytes[index++];
            genes.grabCount = bytes[index++];
            Protocol.Deserialize(out genes.dickThickness, bytes, ref index);
        }
        return genes;
    }*/

    public void Save(JSONNode node, string key) {
        JSONNode rootNode = JSONNode.Parse("{}");
        rootNode["maxEnergy"] = maxEnergy;
        rootNode["baseSize"] = baseSize;
        rootNode["fatSize"] = fatSize;
        rootNode["ballSize"] = ballSize;
        rootNode["dickSize"] = dickSize;
        rootNode["breastSize"] = breastSize;
        rootNode["bellySize"] = bellySize;
        rootNode["metabolizeCapacitySize"] = metabolizeCapacitySize;
        rootNode["hue"] = (int)hue;
        rootNode["brightness"] = (int)brightness;
        rootNode["saturation"] = (int)saturation;
        rootNode["dickEquip"] = dickEquip==byte.MaxValue?"None":GetDickName(dickEquip);
        rootNode["grabCount"] = (int)grabCount;
        rootNode["dickThickness"] = dickThickness;
        rootNode["species"] = GetPlayerName(species);
        node[key] = rootNode;
    }

    public void Load(JSONNode node, string key) {
        JSONNode rootNode = node[key];
        maxEnergy = rootNode["maxEnergy"];
        baseSize = rootNode["baseSize"];
        fatSize = rootNode["fatSize"];
        ballSize = rootNode["ballSize"];
        dickSize = rootNode["dickSize"];
        breastSize = rootNode["breastSize"];
        bellySize = rootNode["bellySize"];
        metabolizeCapacitySize = rootNode["metabolizeCapacitySize"];
        hue = (byte)rootNode["hue"].AsInt;
        brightness = (byte)rootNode["brightness"].AsInt;
        saturation = (byte)rootNode["saturation"].AsInt;
        dickEquip = rootNode["dickEquip"]=="None"? byte.MaxValue: (byte)GetDickIndex(rootNode["dickEquip"]);
        grabCount = (byte)rootNode["grabCount"].AsInt;
        species = (byte)GetPlayerIndex(rootNode["species"]);
        dickThickness = rootNode["dickThickness"];
    }

    public override string ToString() {
        return $@"Kobold Genes: 
           maxEnergy: {maxEnergy}
           baseSize: {baseSize}
           fatSize: {fatSize}
           ballSize: {ballSize}
           dickSize: {dickSize}
           breastSize: {breastSize}
           bellySize: {bellySize}
           metabolizeCapacitySize: {metabolizeCapacitySize}
           hue: {hue}
           brightness: {brightness}
           saturation: {saturation}
           dickEquip: {dickEquip}
           grabCount: {grabCount}
           dickThickness: {dickThickness}
           species: {species}";
    }
}

public class GeneHolder : MonoBehaviourPun {
    private KoboldGenes genes;

    public delegate void GenesChangedAction(KoboldGenes newGenes);

    public event GenesChangedAction genesChanged;

    public KoboldGenes GetGenes() {
        return genes;
    }
    public virtual void SetGenes(KoboldGenes newGenes) {
        genes = newGenes;
        genesChanged?.Invoke(newGenes);
    }
}

