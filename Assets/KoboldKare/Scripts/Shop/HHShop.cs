using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
public class HHShop : GenericWeapon, IValuedGood, IGrabbable
{
    [SerializeField]
    private Transform center;
    [SerializeField]
    private ShopHolder shop;


    [PunRPC]
    protected override void OnFireRPC(int playerViewID)
    { 
        if (photonView.IsMine) {
        ShowShopMenu(playerViewID); }
    }

        public bool ShouldSave()
    {
        return true;
    }
    public float GetWorth()
    {
        return 15f;
    }

    public bool CanGrab(Kobold kobold)
    {
        return true;
    }

    [PunRPC]
    public void OnGrabRPC(int koboldID)
    {    
        //animator.SetBool("Open", true);
    }

    [PunRPC]
    public void OnReleaseRPC(int koboldID, Vector3 velocity)
    {
        //animator.SetBool("Open", false);
    }

    public Transform GrabTransform()
    {
        return center;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void ShowShopMenu(int playerViewID)
    {    
        
        Kobold player =PhotonNetwork.GetPhotonView(playerViewID).GetComponentInParent<Kobold>();
        
        if(shop.HasInstance())
        {
            GameObject tempObject = player.transform.GetComponentInChildren<CameraSwitcher>().FPSCanvas;
            shop.GetInstance().transform.SetParent(tempObject.transform);
            shop.GetInstance().SetActive(true);
            shop.GetInstance().GetComponent<ShopUI>().SetUser(player);
        }
        else
        {    
            GameObject tempObject = player.transform.GetComponentInChildren<CameraSwitcher>().FPSCanvas;
            
            shop.SetInstance(Instantiate(shop.GetPrefab(),tempObject.transform));
            shop.GetInstance().SetActive(true);
            shop.GetInstance().GetComponent<ShopUI>().SetUser(player);

        }

    }
}
