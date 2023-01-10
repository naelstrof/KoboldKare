using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using SimpleJSON;
using UnityEngine;

public class MoneyPile : GenericUsable, IPunInstantiateMagicCallback, IOnPhotonViewControllerChange {
    private float internalWorth;
    private Kobold tryingToEquip;
    [SerializeField]
    private GameObject[] displays;
    [SerializeField]
    private AnimationCurve moneyMap;
    [SerializeField]
    private Sprite useSprite;
    private float worth {
        get {
            return internalWorth;
        }
        set {
            internalWorth = value;
            //int targetIndex = Mathf.RoundToInt(moneyMap.Evaluate(value/maxMoney)*(displays.Length-1));
            int targetIndex = (int)Mathf.Log(value, 5f);
            //Debug.Log("Got " + Mathf.Log(value, 5f) + ", rounded to " + targetIndex);
            targetIndex = Mathf.Clamp(targetIndex, 0, displays.Length-1);
            for (int i=0;i<displays.Length;i++) {
                displays[i].SetActive(i==targetIndex);
            }
        }
    }
    public override Sprite GetSprite(Kobold k) {
        return useSprite;
    }
    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData != null && info.photonView.InstantiationData.Length != 0) {
            worth = (float)info.photonView.InstantiationData[0];
            PhotonProfiler.LogReceive(sizeof(float));
        }
    }
    public override void LocalUse(Kobold k) {
        // Try to take control of the equipment, if we don't have permission.
        if (k.photonView.IsMine && !photonView.IsMine && tryingToEquip == null) {
            tryingToEquip = k;
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
        // Only successfully equip if we own both the equipment, and the kobold. Otherwise, wait for ownership to successfully transfer
        if (k.photonView.IsMine && photonView.IsMine) {
            k.GetComponent<MoneyHolder>().AddMoney(worth);
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(worth);
        } else {
            worth = (float)stream.ReceiveNext();
            PhotonProfiler.LogReceive(sizeof(float));
        }
    }
    public override void Save(JSONNode node) {
        base.Save(node);
        node["worth"] = worth;
    }
    public override void Load(JSONNode node) {
        base.Load(node);
        worth = node["worth"];
    }

    public void OnControllerChange(Player newController, Player previousController) {
        if (tryingToEquip.photonView.IsMine && newController == PhotonNetwork.LocalPlayer) {
            tryingToEquip.GetComponent<MoneyHolder>().AddMoney(worth);
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }
}
