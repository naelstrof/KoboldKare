using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Equipment : ScriptableObject {
    [HideInInspector]
    public int guid;
    public Sprite sprite;
    public enum EquipmentSlot {
        Misc = -1,
        Crotch = 0,
        Neck,
        Head,
        Nipples,
        Tail,
        Feet,
        Butt,
        Hands,
    }
    public enum AttachPoint {
        Misc = -1,
        Crotch = 0,
        Neck,
        Head,
        LeftNipple,
        RightNipple,
        TailBase,
        LeftCalf,
        RightCalf,
        LeftHand,
        RightHand,
        LeftForearm,
        RightForearm,
    }
    public EquipmentSlot slot;
    public PhotonGameObjectReference groundPrefab;
    public LocalizedString localizedName;
    public LocalizedString localizedDescription;
    public List<StatusEffect> effectsToApply;
    private static Dictionary<int,Equipment> availableEquipment = new Dictionary<int, Equipment>();

    // Since these aren't referenced anywhere directly, this is the only way to get them to load properly.
    [RuntimeInitializeOnLoadMethod]
    public void OnInitialize() {
        if (!availableEquipment.ContainsKey(guid)) {
            availableEquipment.Add(guid, this);
        }
    }
    public void OnEnable() {
        OnInitialize();
    }
    public int GetID() {
        return guid;
    }
    public static Equipment GetEquipmentFromID(int id) {
        if (availableEquipment.ContainsKey(id)) {
            return availableEquipment[id];
        }
        return null;
    }
    // GameObjects that are returned by this will automatically get destroyed on unequip.
    public virtual GameObject[] OnEquip(Kobold k, GameObject groundPrefab) {
        foreach(StatusEffect effect in effectsToApply) {
            k.statblock.AddStatusEffect(effect, StatBlock.StatChangeSource.Equipment);
        }
        return null;
    }
    public virtual GameObject OnUnequip(Kobold k, bool dropOnGround = true) {
        foreach(StatusEffect effect in effectsToApply) {
            k.statblock.RemoveStatusEffect(effect, StatBlock.StatChangeSource.Equipment);
        }
        if (k.photonView.IsMine && groundPrefab.gameObject != null && dropOnGround) {
            return SaveManager.Instantiate(groundPrefab.photonName, k.transform.position, Quaternion.identity);
        }
        return null;
    }

    public void OnValidate() {
#if UNITY_EDITOR
        OnInitialize();
        while ( GetEquipmentFromID(guid) != this) {
            guid++;
            OnInitialize();
        }
        groundPrefab.OnValidate();
#endif
    }
}
