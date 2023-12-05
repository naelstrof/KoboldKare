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
        
        if(shop.HasInstance())
        {
            GameObject tempObject = player.transform.GetComponentInChildren<CameraSwitcher>().FPSCanvas;
            shop.GetInstance().transform.SetParent(tempObject.transform);
            shop.GetInstance().SetActive(true);
            shop.GetInstance().GetComponent<ShopUI>().SetUser(player,this);
        }
        else
        {    
            GameObject tempObject = player.transform.GetComponentInChildren<CameraSwitcher>().FPSCanvas;
            
            shop.SetInstance(Instantiate(shop.GetPrefab(),tempObject.transform));
            shop.GetInstance().SetActive(true);
            shop.GetInstance().GetComponent<ShopUI>().SetUser(player,this);

        }

    }
     [PunRPC]
    public void Spawn(string photonName){
        if (PhotonNetwork.IsMasterClient) {
        PhotonNetwork.InstantiateRoomObject(photonName,spawnPoint.transform.position,Quaternion.identity);
        }
    }
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            Vector3 spawnPosition=spawnPoint.transform.position;
            stream.SendNext(spawnPoint);
            
        } else {
            Vector3 spawnPosition = (Vector3)stream.ReceiveNext();
            spawnPoint.transform.position=spawnPosition;
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        
        if (info.photonView.InstantiationData == null) {
            return;
        }
        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is bool) {
            Vector3 spawnPosition = (Vector3)info.photonView.InstantiationData[0];
            spawnPoint.transform.position=spawnPosition;
        }
        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is not BitBuffer) {
            throw new UnityException("Unexpected spawn data for container");
        }
    }
     public void Save(JSONNode node) {  
        node["spx"] = spawnPoint.transform.position.x;
        node["spy"] = spawnPoint.transform.position.y;
        node["spz"] = spawnPoint.transform.position.z;

    }

    public void Load(JSONNode node) {
        float spx=node["spx"];
        float spy=node["spy"];
        float spz=node["spz"];
        spawnPoint.transform.position=new Vector3(spx,spy,spz);
        
        }
}
