using System.Collections;
using System.Collections.Generic;
using System.IO;
using KoboldKare;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class GenericPurchasable : GenericUsable, IPunObservable, ISavable {

    [SerializeField]
    private Sprite displaySprite;

    [SerializeField]
    private ScriptablePurchasable purchasable;
    [SerializeField]
    private AudioPack purchaseSoundPack;
    private bool inStock {
        get {
            return display.activeInHierarchy;
        }
    }
    private GameObject display;
    private AudioSource source;
    [SerializeField]
    private MoneyFloater floater;

    public ScriptablePurchasable GetPurchasable() => purchasable;
    public delegate void PurchasableChangedAction(ScriptablePurchasable newPurchasable);
    public PurchasableChangedAction purchasableChanged;
    public virtual void Start() {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Custom;
        source.minDistance = 0f;
        source.maxDistance = 25f;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, GameManager.instance.volumeCurve);
        source.outputAudioMixerGroup = GameManager.instance.soundEffectGroup;
        SwapTo(purchasable, true);
    }
    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }
    protected void SwapTo(ScriptablePurchasable newPurchasable, bool forceRefresh = false) {
        if (purchasable == newPurchasable && !forceRefresh) {
            return;
        }
        if (display != null) {
            Destroy(display);
        }
        purchasable = newPurchasable;
        display = GameObject.Instantiate(purchasable.display, transform);
        Bounds centerBounds = ScriptablePurchasable.DisableAllButGraphics(display);
        floater.SetBounds(centerBounds);
        display.SetActive(inStock);
        floater.SetText(purchasable.cost.ToString());
        purchasableChanged?.Invoke(purchasable);
    }
    public virtual void OnDestroy() {
    }
    public virtual void OnRestock(object nothing) {
        if (!display.activeInHierarchy) {
            display.SetActive(true);
            floater.gameObject.SetActive(true);
        }
    }
    public override void LocalUse(Kobold k) {
        //base.LocalUse(k);
        photonView.RPC("RPCUse", RpcTarget.All);
        k.GetComponent<MoneyHolder>().ChargeMoney(purchasable.cost);
    }
    public override bool CanUse(Kobold k) {
        return display.activeInHierarchy && (k == null || k.GetComponent<MoneyHolder>().HasMoney(purchasable.cost));
    }
    [PunRPC]
    public override void Use() {
        purchaseSoundPack.Play(source);
        floater.gameObject.SetActive(false);
        display.SetActive(false);
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.InstantiateRoomObject(purchasable.spawnPrefab.photonName, transform.position, Quaternion.identity);
            StartCoroutine(Restock());
        }
        PhotonProfiler.LogReceive(1);
    }
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(inStock);
            stream.SendNext(PurchasableDatabase.GetID(purchasable));
        } else {
            display.SetActive((bool)stream.ReceiveNext());
            short currentPurchasable = (short)stream.ReceiveNext();
            SwapTo(PurchasableDatabase.GetPurchasable(currentPurchasable));
            PhotonProfiler.LogReceive(sizeof(bool)+sizeof(short));
        }
    }
    public override void Save(JSONNode node) {
        base.Save(node);
        node["inStock"] = inStock;
        node["purchaseID"] = (int)PurchasableDatabase.GetID(purchasable);
    }

    public override void Load(JSONNode node) {
        base.Load(node);
        display.SetActive(node["inStock"]);
        short currentPurchasable = (short)(node["purchaseID"].AsInt);
        SwapTo(PurchasableDatabase.GetPurchasable(currentPurchasable));
    }

    private IEnumerator Restock() {
        yield return new WaitForSeconds(30f);
        OnRestock(null);
    }
}
