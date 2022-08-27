using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;
using KoboldKare;

[System.Serializable]
public class SellKoboldObjective : DragonMailObjective {
    [SerializeField]
    private int maxKobolds = 1;
    [SerializeField]
    private LocalizedString kobold;
    private int kobolds = 0;
    [SerializeField]
    private GameEventPhotonView soldObject;
    public override void Register() {
        soldObject.AddListener(OnEntitySold);
    }
    public override void Unregister() {
        soldObject.RemoveListener(OnEntitySold);
    }
    private void OnEntitySold(PhotonView obj) {
        if (obj.GetComponentInChildren<Kobold>() != null) {
            kobolds++;
        }
        if (kobolds >= maxKobolds) {
            TriggerComplete();
        }
    }

    public override string GetTextBody() {
        return $"{kobold.GetLocalizedString()} {kobolds.ToString()}/{maxKobolds.ToString()}";
    }

    public override void Save(BinaryWriter writer) {
        writer.Write(kobolds);
    }

    public override void Load(BinaryReader reader) {
        kobolds = reader.ReadInt32();
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(kobolds);
        } else {
            kobolds = (int)stream.ReceiveNext();
        }
    }
}
