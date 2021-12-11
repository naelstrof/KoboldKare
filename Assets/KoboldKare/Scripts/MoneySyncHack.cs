using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class MoneySyncHack : MonoBehaviourPun, ISavable, IPunObservable {
    [SerializeField]
    private ScriptableFloat money;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(money.value);
        } else {
            money.set((float)stream.ReceiveNext());
        }
    }
    public void Save(BinaryWriter writer, string version) {
        writer.Write(money.value);
    }
    public void Load(BinaryReader reader, string version) {
        money.set((float)reader.ReadSingle());
    }
}
