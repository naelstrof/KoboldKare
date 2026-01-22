using System;
using System.Collections;
using FishNet;
using Photon.Pun;
using UnityEngine;

public class GenericEquipment : MonoBehaviour {
    public Equipment representedEquipment;
    [SerializeField]
    private Sprite displaySprite;
    protected Kobold tryingToEquip;

    private NetworkedEntity networkedEntity;
    // Trying to match the Use pattern, so we can just use a GenericUsable to equip. Though technically we can call this from anything. A button that equips you with a status effect or whatever.
    private void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity != null) {
            networkedEntity.SetSprite(displaySprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    private void OnUse(NetworkedKobold by) {
        if (networkedEntity.IsOwner) {
            networkedEntity.Equip(by, representedEquipment.ToString());
        }
    }

    private bool OnUseRequested(NetworkedKobold by) {
        return true;
    }
}
