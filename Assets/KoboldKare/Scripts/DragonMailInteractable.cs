using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class DragonMailInteractable : MonoBehaviour {
    public AudioSource src;
    public Canvas tgt;
    public DragonMailHandler dmHandler;
    public Sprite displaySprite;
    private NetworkedEntity networkedEntity;

    void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity != null) {
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.SetSprite(displaySprite);
            networkedEntity.used += OnUse;
        }
    }

    private bool OnUseRequested(NetworkedKobold by) {
        return true;
    }


    // FIXME FISHNET
    /*public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        //Serialize event
    }*/

    private void OnUse(NetworkedKobold by) { 
        // FIXME FISHNET
        // Should only trigger on client who used it.
        DragonMailHandler.inst.Toggle();
    }
}
