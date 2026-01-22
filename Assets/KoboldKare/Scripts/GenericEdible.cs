using System;
using System.Collections;
using System.Collections.Generic;
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
            return container.volume > 0.01f && kobold.bellyContainer.volume < kobold.bellyContainer.maxVolume;
        }

        return false;
    }

    private void OnUse(NetworkedKobold k) {
        if (k && k.TryGetKobold(out var kobold)) {
            // Only successfully eat if we own both the edible, and the kobold. Otherwise, wait for ownership to successfully transfer
            float spillAmount = Mathf.Min(10f, kobold.bellyContainer.maxVolume - kobold.bellyContainer.volume);
            ReagentContents spill = container.Spill(spillAmount);
            // FIXME FISHNET
            /*photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.Others, spillAmount);
            BitBuffer buffer = new BitBuffer(4);
            buffer.AddReagentContents(spill);
            k.bellyContainer.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All, buffer, photonView.ViewID, (byte)GenericReagentContainer.InjectType.Spray);*/
            /*if (destroyOnEat) {
                PhotonNetwork.Destroy(photonView.gameObject);
            }*/
            GameManager.instance.SpawnAudioClipInWorld(eatSoundPack, transform.position);
        }
    }
}
