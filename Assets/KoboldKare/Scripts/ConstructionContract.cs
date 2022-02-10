using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using System.IO;

public class ConstructionContract : GenericUsable {
    [SerializeField]
    private Sprite displaySprite;
    [SerializeField]
    private UnityEvent purchased;
    [SerializeField]
    private GameObject[] disableOnPurchase;
    [SerializeField]
    private GameObject[] enableOnPurchase;
    [SerializeField]
    private ScriptableFloat money;
    [SerializeField]
    private float cost;
    [SerializeField]
    private MoneyFloater floater;

    [SerializeField]
    public bool bought;

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
        return money.has(cost) && !bought;
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
    }
    [PunRPC]
    public override void Use() {
        base.Use();
        money.charge(cost);
        purchased.Invoke();
        SetState(true);
        //gameObject.SetActive(false);
    }

    public override void Save(BinaryWriter writer, string version){       
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
}
