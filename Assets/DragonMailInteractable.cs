using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class DragonMailInteractable : GenericUsable, IPunObservable{
    public AudioSource src;
    public Canvas tgt;
    public DragonMailHandler dmHandler;
    public Sprite displaySprite;

    public override Sprite GetSprite(Kobold k){
        return displaySprite;
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        //Serialize event
    }

    public override void Use(){ 
        //Pulling the screen up/down is entirely local so don't broadcast this part of the set of behaviors
        if(photonView.IsMine){
            base.Use();
            DragonMailHandler.inst.Toggle();
        }
    }
}
