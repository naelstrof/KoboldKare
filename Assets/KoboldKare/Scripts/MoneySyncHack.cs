using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class MoneySyncHack : MonoBehaviourPun, ISavable, IPunObservable {
    private static MoneySyncHack instance;
    public static PhotonView view => instance.photonView;
    public void Awake() {
        if (instance != null) {
            Destroy(this);
        } else {
            instance = this;
        }
    }
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
    [PunRPC]
    void RPCGiveMoney(float amount) {
        if (photonView.IsMine) {
            money.give(amount);
        }
    }
}
