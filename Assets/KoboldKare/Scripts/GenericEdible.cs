using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GenericEdible : GenericUsable, IPunObservable, IOnPhotonViewOwnerChange {
    [SerializeField]
    private Sprite eatSymbol;
    [SerializeField]
    private GenericReagentContainer container;
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
            k.bellies[0].GetContainer().TransferMix(container, container.volume*0.5f, GenericReagentContainer.InjectType.Spray);
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }

    public void OnOwnerChange(Player newOwner, Player previousOwner) {
        if (newOwner == PhotonNetwork.LocalPlayer && tryingToEat != null) {
            tryingToEat.bellies[0].GetContainer().TransferMix(container, container.volume*0.5f, GenericReagentContainer.InjectType.Spray);
            PhotonNetwork.Destroy(photonView.gameObject);
        }
        // Someone else must've won the handshake, so we clear our attempt to equip.
        tryingToEat = null;
    }
}
