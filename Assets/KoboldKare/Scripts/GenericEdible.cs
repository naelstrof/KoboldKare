using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GenericEdible : GenericUsable, IOnPhotonViewOwnerChange {
    [SerializeField]
    private Sprite eatSymbol;
    [SerializeField]
    private GenericReagentContainer container;

    [SerializeField] private bool destroyOnEat = true;
    [SerializeField] private AudioPack eatSoundPack;
    public override Sprite GetSprite(Kobold k) {
        return eatSymbol;
    }
    private Kobold tryingToEat;
    public override void LocalUse(Kobold k) {
        // If kobold got deleted
        if (k == null) {
            return;
        }
        // Try to take control of the edible, if we don't have permission.
        if (k.photonView.IsMine && !photonView.IsMine && tryingToEat == null) {
            tryingToEat = k;
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
        // Only successfully eat if we own both the edible, and the kobold. Otherwise, wait for ownership to successfully transfer
        if (k.photonView.IsMine && photonView.IsMine) {
            Eat(k);
        }
    }

    private void Eat(Kobold kobold) {
        kobold.bellyContainer.TransferMix(container, container.volume*0.5f, GenericReagentContainer.InjectType.Spray);
        if (destroyOnEat) {
            PhotonNetwork.Destroy(photonView.gameObject);
        }

        GameManager.instance.SpawnAudioClipInWorld(eatSoundPack, transform.position);
    }

    public void OnOwnerChange(Player newOwner, Player previousOwner) {
        if (newOwner == PhotonNetwork.LocalPlayer && tryingToEat != null) {
            Eat(tryingToEat);
        }
        // Someone else must've won the handshake, so we clear our attempt to equip.
        tryingToEat = null;
    }
}
