using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using NetStack.Serialization;
using SimpleJSON;

public class MovableFurniture : GenericWeapon, IValuedGood, IGrabbable, ISavable, IPunObservable, IPunInstantiateMagicCallback
{
    [SerializeField]
    private Transform center;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private bool needsConsistentViewId;
    private bool isFrozen=false;

    [PunRPC]
    protected override void OnFireRPC(int playerViewID)
    { Freeze();
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
        return !isFrozen;
    }
    public void Freeze(){
        
        rb.constraints = RigidbodyConstraints.FreezePositionX |RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | 
                                RigidbodyConstraints.FreezeRotationX |RigidbodyConstraints.FreezeRotationY |RigidbodyConstraints.FreezeRotationZ;
        isFrozen=true;
    }
    public void Unfreeze()
    {
        rb.constraints=RigidbodyConstraints.None;
        isFrozen=false;

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
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            bool frozen=isFrozen;
            stream.SendNext(frozen);
            
        } else {
            bool frozen = (bool)stream.ReceiveNext();
            if(frozen!=isFrozen)
            {
                if(frozen){
                    Freeze();
                }
                else{
                    Unfreeze();
                }
            }
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        
        if (info.photonView.InstantiationData == null) {
            return;
        }
        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is bool) {
            rb.isKinematic=(bool)info.photonView.InstantiationData[0];
        }
        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is not BitBuffer) {
            throw new UnityException("Unexpected spawn data for container");
        }
    }

    public void Save(JSONNode node) {  
        node["frozen"] = isFrozen;
        if(needsConsistentViewId)
        {node["id"] = photonView.ViewID;}
    }

    public void Load(JSONNode node) {
            isFrozen=node["frozen"];
                if(isFrozen)Freeze();
                else Unfreeze();
            if(needsConsistentViewId)
            {GrabId(node["id"]);}
            }

    private void GrabId(int wantedId){
        if(photonView.ViewID==wantedId)
            return;
        PhotonView tempView=PhotonView.Find(wantedId);
        if(tempView!=null){
            int replaceId=wantedId;
            while(PhotonView.Find(replaceId)!=null){
                replaceId=replaceId+5;
            }
            tempView.ViewID=0;
            tempView.ViewID=replaceId;
            }
        photonView.ViewID=0;
        photonView.ViewID=wantedId;
    }
}
