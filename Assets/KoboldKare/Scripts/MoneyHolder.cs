using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class MoneyHolder : MonoBehaviourPun, ISavable, IPunObservable {
    public delegate void MoneyChangedAction(float newMoney);
    public MoneyChangedAction moneyChanged;
    private float money = 150f;
    public float GetMoney() => money;
    public void AddMoney(float add) {
        if (add <= 0) {
            return;
        }
        money += add;
        moneyChanged.Invoke(money);
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
    public void Load(BinaryReader reader, string version) {
        money = reader.ReadSingle();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(money);
        } else {
            money = (float)stream.ReceiveNext();
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(money);
    }
}
