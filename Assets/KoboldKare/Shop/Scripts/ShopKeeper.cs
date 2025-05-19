using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using SimpleJSON;
using NetStack.Serialization;

public class ShopKeeper : GenericUsable, IPunObservable ,ISavable
{   
    [SerializeField]
    private ShopHolder shop;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private GameObject spawnPoint;
    [SerializeField]
    private string talkAnimation;

    public override void LocalUse(Kobold k) {
        animator.SetTrigger(talkAnimation);
        ShowShopMenu(k);
    }
    
    public override void Use() { 
        base.Use();
        animator.SetTrigger(talkAnimation);
    }

    private void ShowShopMenu(Kobold player)
    {    
        GameObject tempObject = player.transform.GetComponentInChildren<CameraSwitcher>().FPSCanvas;

        if(shop.HasInstance())
        {
            shop.GetInstance().transform.SetParent(tempObject.transform);
        }
        else
        {    
            shop.SetInstance(Instantiate(shop.GetPrefab(),tempObject.transform));
        }

        shop.GetInstance().SetActive(true);
        shop.GetInstance().GetComponent<ShopUI>().SetUser(player,this);
    }

   
    public void Spawn(string photonName){
        PhotonNetwork.Instantiate(photonName, spawnPoint.transform.position, Quaternion.identity,0);
    }

}
