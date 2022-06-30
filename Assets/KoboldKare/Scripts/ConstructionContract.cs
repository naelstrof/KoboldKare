using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using System.IO;

public class ConstructionContract : GenericUsable, ISavable, IPunObservable {
    [SerializeField]
    private Sprite displaySprite;
    [SerializeField]
    private UnityEvent purchased;
    [SerializeField]
    private GameObject[] disableOnPurchase;
    [SerializeField]
    private GameObject[] enableOnPurchase;
    //[SerializeField]
    //private ScriptableFloat money;
    [SerializeField]
    private float cost;
    [SerializeField]
    private MoneyFloater floater;
    private bool bought;

    void Start() {
        Bounds bound = new Bounds(transform.position, Vector3.one);
        foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
            bound.Encapsulate(r.bounds);
        }
        floater.SetBounds(bound);
        floater.SetText(cost.ToString());
    }
    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }
    public override bool CanUse(Kobold k) {
        return k.GetComponent<MoneyHolder>().HasMoney(cost) && !bought;
    }
    private void SetState(bool purchased) {
        foreach(GameObject obj in enableOnPurchase) {
            obj.SetActive(purchased);
        }
        foreach(GameObject obj in disableOnPurchase) {
            obj.SetActive(!purchased);
        }
        foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
            r.enabled = !purchased;
        }
        bought = purchased;
        //Debug.Log(gameObject.name+":: Bought is set to: "+bought);
    }
    public override void LocalUse(Kobold k) {
        if (k.GetComponent<MoneyHolder>().ChargeMoney(cost)) {
            base.LocalUse(k);
        }
    }
    [PunRPC]
    public override void Use() {
        base.Use();
        purchased.Invoke();
        SetState(true);
        //gameObject.SetActive(false);
    }

    public override void Save(BinaryWriter writer, string version){   
        //Debug.Log(bought);    
        writer.Write(bought);
    }

    public override void Load(BinaryReader reader, string version){
        bought = reader.ReadBoolean();
        if(bought){
            photonView.RPC("Use",RpcTarget.AllBufferedViaServer);
        }
        else{
            PhotonNetwork.CleanRpcBufferIfMine(photonView);
        }
        SetState(bought);
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
    }
}
