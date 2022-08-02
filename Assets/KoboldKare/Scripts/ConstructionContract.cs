using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using System.IO;

public class ConstructionContract : GenericUsable {
    public delegate void ConstructionContractPurchaseAction(ConstructionContract contract);
    public static event ConstructionContractPurchaseAction purchasedEvent;
    [SerializeField]
    private Sprite displaySprite;
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
    protected virtual void SetState(bool purchased) {
        bought = purchased;
        foreach (Transform t in transform) {
            if (transform == t) {
                continue;
            }

            t.gameObject.SetActive(!purchased);
        }

        foreach (Renderer r in GetComponents<Renderer>()) {
            r.enabled = !purchased;
        }
    }
    public override void LocalUse(Kobold k) {
        if (k.GetComponent<MoneyHolder>().ChargeMoney(cost)) {
            base.LocalUse(k);
        }
    }
    [PunRPC]
    public override void Use() {
        base.Use();
        SetState(true);
        purchasedEvent?.Invoke(this);
    }

    public override void Save(BinaryWriter writer, string version){   
        writer.Write(bought);
    }

    public override void Load(BinaryReader reader, string version){
        bought = reader.ReadBoolean();
        SetState(bought);
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if (stream.IsWriting) {
            stream.SendNext(bought);
        } else {
            SetState((bool)stream.ReceiveNext());
        }
    }
}
