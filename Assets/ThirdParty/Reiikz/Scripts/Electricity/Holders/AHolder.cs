using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Reiikz.UnityUtils;

public class AHolder : GenericUsable, IHolder
{
    public Sprite icon;
    public Transform spotA;
    public Transform spotB;
    public List<Squash2> squashersA;
    public List<Squash2> squashersB;
    public AudioClip snapIn;
    public AudioClip snapOut;
    public AudioSource audioSource;
    public PhotonView photonView;

    private void OnTriggerEnter(Collider other) {
        GameObject collidingObject = other.gameObject;
        ABattery bat = collidingObject.GetComponent<ABattery>();
        if(bat == null) return;
        if(bat.photonView.IsMine) {
            if((spotA.childCount != 0) && (spotB.childCount != 0)) return;
            if(collidingObject.transform.IsChildOf(spotA) || collidingObject.transform.IsChildOf(spotB)) return;
            if(spotA.childCount == 0){
                SquashSquashers(squashersA);
            }else{
                SquashSquashers(squashersB);
            }
            bat.attachable.mainPV.RPC("RPC_Attach", RpcTarget.AllBuffered, bat.photonView.ViewID, photonView.ViewID);
            audioSource.clip = snapIn;
            audioSource.Play();
        }
    }

    private void SquashSquashers(List<Squash2> squashers){
        foreach(Squash2 squasher in squashers){
            squasher.Squash();
        }
    }

    public override Sprite GetSprite(Kobold k) {
        return icon;
    }

    public override void Use(Kobold k) {
        base.Use(k);
        if(photonView.IsMine) {
            photonView.RPC("RPC_PopFromSpot", RpcTarget.AllBuffered, photonView.ViewID);
            photonView.RPC("RPC_PopFromSpot", RpcTarget.AllBuffered, photonView.ViewID);
            SquashSquashers(squashersA);
            SquashSquashers(squashersB);
            audioSource.clip = snapOut;
            audioSource.Play();
        }
        // popFromSpot(spotA)
        // popFromSpot(spotB)
    }

    [PunRPC]
    public void RPC_PopFromSpot(int id){
        PhotonView ov = PhotonView.Find(id);
        if(ov == null) return;
        Transform t = ov.gameObject.GetComponent<IHolder>().GetUsedAttachPoint();
        if(t == null) return;
        GameObject batteryObject = t.GetChild(0).gameObject;
        ABattery bat = batteryObject.GetComponent<ABattery>();
        bat.interactor.SetActive(true);
        bat.itemTearDrop.SetActive(true);
        batteryObject.GetComponent<MeshCollider>().enabled = true;
        batteryObject.GetComponent<Rigidbody>().isKinematic = false;
        batteryObject.transform.position += (Vector3.right * 2f) + (Vector3.up * 1.5f);
        if(bat.attachable.rigidbodyView != null) bat.attachable.rigidbodyView.enabled = true;
        t.DetachChildren();
    }

    public Transform GetAttachPoint(){
        if(spotA.childCount == 0) return spotA;
        if(spotB.childCount == 0) return spotB;
        return null;
    }

    public Transform GetUsedAttachPoint(){
        if(spotA.childCount > 0)  return spotA;
        if(spotB.childCount > 0)  return spotB;
        return null;
    }

    // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
}
