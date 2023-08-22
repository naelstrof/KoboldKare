using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetStack.Serialization;
using SimpleJSON;
using Photon.Pun;

public class Freezable : MonoBehaviourPun,ISavable, IPunObservable, IPunInstantiateMagicCallback
{   [SerializeField]
    private Rigidbody rb;
    private bool frozen=false;
    public bool IsFrozen
    {
        get { return frozen; }
        private set { frozen = value; }
    }

    
    // Start is called before the first frame update
    public void Freeze(){
        
        rb.constraints = RigidbodyConstraints.FreezePositionX |RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | 
                                RigidbodyConstraints.FreezeRotationX |RigidbodyConstraints.FreezeRotationY |RigidbodyConstraints.FreezeRotationZ;
        frozen=true;
    }
    public void Unfreeze()
    {
        rb.constraints=RigidbodyConstraints.None;
        frozen=false;

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            bool frozenTemp=frozen;
            stream.SendNext(frozenTemp);
            
        } else {
            bool frozenTemp = (bool)stream.ReceiveNext();
            if(frozenTemp!=frozen)
            {
                if(frozenTemp){
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
            bool frozen = (bool)info.photonView.InstantiationData[0];
            if(frozen!=IsFrozen)
            {
                if(frozen){
                    Freeze();
                }
                else{
                    Unfreeze();
                }
            }
        }
        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is not BitBuffer) {
            throw new UnityException("Unexpected spawn data for container");
        }
    }
    public bool ShouldSave()
    {
        return true;
    }
    public void Save(JSONNode node) {  
        node["frozen"] = IsFrozen;

    }

    public void Load(JSONNode node) {
        bool frozenTemp=node["frozen"];
        if(frozenTemp)Freeze();
        else Unfreeze();
        }
}
