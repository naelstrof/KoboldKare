using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class KoboldGenes {
    public byte maxEnergy = 1;
    public float baseSize = 20f;
    public float fatSize;
    public float ballSize;
    public float dickSize;
    public float breastSize;
    public float bellySize = 20f;
    public float metabolizeCapacitySize = 20f;
    public byte hue;
    public byte brightness = 128;
    public byte saturation = 128;
    public byte dickEquip = byte.MaxValue;
    public float dickThickness;

    public KoboldGenes With(byte? maxEnergy = null, float? baseSize = null, float? fatSize = null,
            float? ballSize = null, float? dickSize = null, float? breastSize = null, float? bellySize = null,
            float? metabolizeCapacitySize = null, byte? hue = null, byte? brightness = null,
            byte? saturation = null, byte? dickEquip = null, float? dickThickness = null) {
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
            dickThickness = dickThickness ?? this.dickThickness
        };
    }

    public KoboldGenes Randomize() {
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

            breastSize = Random.Range(0f, 10f);
            ballSize = Random.Range(10f, 20f);
            dickSize = Random.Range(0f, 20f);
            dickEquip = (byte)equipments.IndexOf(dick);
        } else {
            breastSize = Random.Range(10f, 40f);
            ballSize = Random.Range(5f, 25f);
            dickSize = Random.Range(0f, 20f);
            dickEquip = byte.MaxValue;
        }

        dickThickness = Random.Range(0f, 1f);
        baseSize = Random.Range(14f, 24f);
        hue = (byte)Random.Range(0, 255);
        brightness = (byte)Random.Range(0, 255);
        saturation = (byte)Random.Range(0, 255);
        return this;
    }

    public static KoboldGenes Mix(KoboldGenes a, KoboldGenes b) {
        KoboldGenes c;
        if (UnityEngine.Random.Range(0f, 1f) > 0.5f) {
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
        c.metabolizeCapacitySize = Mathf.Lerp(a.metabolizeCapacitySize, b.metabolizeCapacitySize, 0.5f);
        c.dickSize = Mathf.Lerp(a.dickSize, b.dickSize, 0.5f);
        c.ballSize = Mathf.Lerp(a.ballSize, b.ballSize, 0.5f);
        c.fatSize = Mathf.Lerp(a.fatSize, b.fatSize, 0.5f);
        c.baseSize = Mathf.Lerp(a.baseSize, b.baseSize, 0.5f);
        c.maxEnergy = (byte)Mathf.RoundToInt(Mathf.Lerp(a.maxEnergy, b.maxEnergy, 0.5f));
        c.dickThickness = Mathf.Lerp(a.dickThickness, b.dickThickness, 0.5f);
        
        return c;
    }

    private const short byteCount = sizeof(float) * 8 + sizeof(byte) * 5;
    public static short Serialize(StreamBuffer outStream, object customObject) {
        KoboldGenes genes = (KoboldGenes)customObject;
        byte[] bytes = new byte[byteCount];
        int index = 0;
        bytes[index++] = genes.maxEnergy;
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
            genes.maxEnergy = bytes[index++];
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
            Protocol.Deserialize(out genes.dickThickness, bytes, ref index);
        }
        return genes;
    }

    public void Serialize(BinaryWriter writer) {
        writer.Write(maxEnergy);
        writer.Write(baseSize);
        writer.Write(fatSize);
        writer.Write(ballSize);
        writer.Write(dickSize);
        writer.Write(breastSize);
        writer.Write(bellySize);
        writer.Write(metabolizeCapacitySize);
        writer.Write(hue);
        writer.Write(brightness);
        writer.Write(saturation);
        writer.Write(dickEquip);
        writer.Write(dickThickness);
    }

    public KoboldGenes Deserialize(BinaryReader reader) {
        maxEnergy = reader.ReadByte();
        baseSize = reader.ReadSingle();
        fatSize = reader.ReadSingle();
        ballSize = reader.ReadSingle();
        dickSize = reader.ReadSingle();
        breastSize = reader.ReadSingle();
        bellySize = reader.ReadSingle();
        metabolizeCapacitySize = reader.ReadSingle();
        hue = reader.ReadByte();
        brightness = reader.ReadByte();
        saturation = reader.ReadByte();
        dickEquip = reader.ReadByte();
        dickThickness = reader.ReadSingle();
        return this;
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
           dickThickness: {dickThickness}";
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

