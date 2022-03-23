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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        //Serialize event
    }

    public override void Use(){ 
        //Pulling the screen up/down is entirely local so don't network this part of the set of behaviors
        if(photonView.IsMine){
            base.Use();
            tgt.enabled = !tgt.enabled;
            if(tgt.enabled){
                tgt.GetComponent<Animator>().SetBool("Open", true);
                tgt.GetComponent<CanvasGroup>().blocksRaycasts = true;
                tgt.GetComponent<CanvasGroup>().interactable = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else{
                tgt.GetComponent<Animator>().SetBool("Open", false);
                tgt.GetComponent<CanvasGroup>().blocksRaycasts = true;
                tgt.GetComponent<CanvasGroup>().interactable = true;
                Cursor.lockState = CursorLockMode.Locked;
            }
            dmHandler.SwitchToMain();
        }
    }
}
