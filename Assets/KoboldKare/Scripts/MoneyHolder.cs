using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;

public class MoneyHolder : MonoBehaviourPun, ISavable, IPunObservable, IValuedGood {
    public delegate void MoneyChangedAction(float newMoney);
    public MoneyChangedAction moneyChanged;
#if UNITY_EDITOR
    private float money = 150f;
#else
    private float money = 5f;
#endif
    public float GetMoney() => money;

    public void SetMoney(float amount) {
        money = amount;
    }

    [PunRPC]
    public void AddMoney(float add) {
        if (add <= 0) {
            return;
        }
        money += add;
        moneyChanged.Invoke(money);
        PhotonProfiler.LogReceive(sizeof(float));
    }
    public bool ChargeMoney(float amount) {
        if (money < amount) {
            return false;
        }
        money -= amount;
        moneyChanged.Invoke(money);
        return true;
    }
    public bool HasMoney(float amount) {
        return money >= amount;
    }
    public void Load(JSONNode node) {
        money = node["money"];
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(money);
        } else {
            money = (float)stream.ReceiveNext();
            PhotonProfiler.LogReceive(sizeof(float));
        }
    }

    public void Save(JSONNode node) {
        node["money"] = money;
    }

    public float GetWorth() {
        return money;
    }
}
