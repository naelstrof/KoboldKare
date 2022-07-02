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
    public Sprite sprite;
    public bool hideInFirstPersonWhenWorn;
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
    public virtual GameObject[] OnEquip(Kobold k, GameObject groundPrefab) {
        foreach(StatusEffect effect in effectsToApply) {
            k.statblock.AddStatusEffect(effect, StatBlock.StatChangeSource.Equipment);
        }
        // If we take up a slot, and we're actually being picked up off the ground, then we should unequip all the same slot.
        if (slot != EquipmentSlot.Misc && groundPrefab != null) {
            while(k.GetComponent<KoboldInventory>().GetEquipmentInSlot(slot) != null) {
                k.GetComponent<KoboldInventory>().RemoveEquipment(slot, true);
            }
        }
        return null;
    }
    public virtual GameObject OnUnequip(Kobold k, bool dropOnGround = true) {
        foreach(StatusEffect effect in effectsToApply) {
            k.statblock.RemoveStatusEffect(effect, StatBlock.StatChangeSource.Equipment);
        }
        if (k.photonView.IsMine && groundPrefab.gameObject != null && dropOnGround) {
            return PhotonNetwork.Instantiate(groundPrefab.photonName, k.transform.position, Quaternion.identity);
        }
        return null;
    }

    private void OnValidate() {
        groundPrefab.OnValidate();
    }
}
