using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GenericEdible : GenericUsable {
    [SerializeField]
    private Sprite eatSymbol;
    [SerializeField]
    private GenericReagentContainer container;

    [SerializeField] private bool destroyOnEat = true;
    [SerializeField] private AudioPack eatSoundPack;
    public override Sprite GetSprite(Kobold k) {
        return eatSymbol;
    }

    public override bool CanUse(Kobold k) {
        return container.volume > 0.01f && k.bellyContainer.volume < k.bellyContainer.maxVolume;
    }

    public override void LocalUse(Kobold k) {
        base.LocalUse(k);
        // Only successfully eat if we own both the edible, and the kobold. Otherwise, wait for ownership to successfully transfer
        float spillAmount = Mathf.Min(container.maxVolume * 0.5f, k.bellyContainer.maxVolume - k.bellyContainer.volume);
        ReagentContents spill = container.Spill(spillAmount);
        container.photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.Others, spillAmount);
        k.bellyContainer.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All, spill, photonView.ViewID);
        photonView.RequestOwnership();
    }

    public override void Use() {
        if (destroyOnEat) {
            PhotonNetwork.Destroy(photonView.gameObject);
        }
        GameManager.instance.SpawnAudioClipInWorld(eatSoundPack, transform.position);
    }
}
