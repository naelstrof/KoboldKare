using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using System.IO;
using SimpleJSON;

public class ConstructionContract : GenericUsable {
    public delegate void ConstructionContractPurchaseAction(ConstructionContract contract);
    public static event ConstructionContractPurchaseAction purchasedEvent;
    [SerializeField]
    private Sprite displaySprite;
    [SerializeField]
    private float cost;
    [SerializeField]
    private MoneyFloater floater;

    [SerializeField] private AudioPack purchaseSound;

    [SerializeField] private int starRequirement = 1;
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
        return k.GetComponent<MoneyHolder>().HasMoney(cost) && !bought && ObjectiveManager.GetStars() > starRequirement || (ObjectiveManager.GetStars() == starRequirement && ObjectiveManager.GetCurrentObjective() != null);
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
        GameManager.instance.SpawnAudioClipInWorld(purchaseSound, transform.position);
        purchasedEvent?.Invoke(this);
    }

    public override void Save(JSONNode node) {
        node["bought"] = bought;
    }

    public override void Load(JSONNode node){
        bought = node["bought"];
        SetState(bought);
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if (stream.IsWriting) {
            stream.SendNext(bought);
        } else {
            SetState((bool)stream.ReceiveNext());
            PhotonProfiler.LogReceive(sizeof(bool));
        }
    }
}
