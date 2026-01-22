using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using System.IO;
using System.Threading.Tasks;
using SimpleJSON;

public class ConstructionContract : MonoBehaviour {
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

    private NetworkedKobold networkedKobold;

    void Start() {
        Bounds bound = new Bounds(transform.position, Vector3.one);
        foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
            bound.Encapsulate(r.bounds);
        }
        floater.SetBounds(bound);
        floater.SetText(cost.ToString());
        networkedKobold = GetComponentInParent<NetworkedKobold>();
        if (networkedKobold) {
            networkedKobold.SetSprite(displaySprite);
            networkedKobold.useRequested += OnUseRequested;
            networkedKobold.used += OnUse;
        }
    }
    
    private bool OnUseRequested(NetworkedKobold k) {
        return !bought && (k.TryGetKobold(out var kobold) && kobold.GetComponent<MoneyHolder>().HasMoney(cost) && ObjectiveManager.GetStars() > starRequirement || (ObjectiveManager.GetStars() == starRequirement && ObjectiveManager.GetCurrentObjective() != null));
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
    
    private void OnUse(NetworkedKobold k) {
        if (k.TryGetKobold(out var kobold) && kobold.GetComponent<MoneyHolder>().ChargeMoney(cost)) {
            SetState(true);
            GameManager.instance.SpawnAudioClipInWorld(purchaseSound, transform.position);
            purchasedEvent?.Invoke(this);
        }
    }

    // FIXME FISHNET
    /*public override void Save(JSONNode node) {
        node["bought"] = bought;
    }

    public override Task Load(JSONNode node){
        bought = node["bought"];
        SetState(bought);
        return Task.CompletedTask;
    }*/

    
    // FIXME FISHNET
    /*
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if (stream.IsWriting) {
            stream.SendNext(bought);
        } else {
            SetState((bool)stream.ReceiveNext());
            PhotonProfiler.LogReceive(sizeof(bool));
        }
    }*/
}
