using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using NetStack.Serialization;
using Photon.Pun;
using UnityEngine;

public class GenericEdible : MonoBehaviour {
    [SerializeField]
    private Sprite eatSymbol;
    [SerializeField]
    private GenericReagentContainer container;

    [SerializeField] private bool destroyOnEat = true;
    [SerializeField] private AudioPack eatSoundPack;
    private NetworkedEntity networkedEntity;

    private void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(eatSymbol);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    private bool OnUseRequested(NetworkedKobold k) {
        if (k && k.TryGetKobold(out var kobold)) {
            return networkedEntity.reagentContents.Value.volume > 0.01f && k.reagentContents.Value.volume < k.reagentContents.Value.GetMaxVolume();
        }

        return false;
    }

    private void OnUse(NetworkedKobold k) {
        if (k && k.TryGetKobold(out var kobold)) {
            // Only successfully eat if we own both the edible, and the kobold. Otherwise, wait for ownership to successfully transfer
            float spillAmount = Mathf.Min(10f, k.reagentContents.Value.GetMaxVolume() - k.reagentContents.Value.volume);
            ReagentContents spill = networkedEntity.Spill(spillAmount);
            k.AddMix(spill, GeneHolder.InjectType.Spray);
            if (destroyOnEat) {
                networkedEntity.Despawn();
            }
            GameManager.instance.SpawnAudioClipInWorld(eatSoundPack, transform.position);
        }
    }
}
