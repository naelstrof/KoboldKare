using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;
using KoboldKare;
using SimpleJSON;

[System.Serializable]
public class SellKoboldObjective : ObjectiveWithSpaceBeam {
    [SerializeField]
    private int maxKobolds = 1;
    [SerializeField]
    private LocalizedString kobold;
    private int kobolds = 0;
    [SerializeField]
    private GameEventPhotonView soldObject;
    public override void Register() {
        base.Register();
        soldObject.AddListener(OnEntitySold);
    }
    public override void Unregister() {
        base.Unregister();
        soldObject.RemoveListener(OnEntitySold);
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        kobolds++;
        TriggerUpdate();
        if (kobolds >= maxKobolds) {
            TriggerComplete();
        }
    }

    private void OnEntitySold(PhotonView obj) {
        if (obj.GetComponentInChildren<Kobold>() != null) {
            ObjectiveManager.NetworkAdvance(obj.transform.position, $"{obj.ViewID.ToString()}{kobolds.ToString()}");
        }
    }

    public override string GetTextBody() {
        return $"{kobold.GetLocalizedString()} {kobolds.ToString()}/{maxKobolds.ToString()}";
    }

    public override void Save(JSONNode node) {
        node["kobolds"] = kobolds;
    }

    public override void Load(JSONNode node) {
        kobolds = node["kobolds"];
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(kobolds);
        } else {
            int newKobolds = (int)stream.ReceiveNext();
            if (kobolds != newKobolds) {
                kobolds = newKobolds;
                TriggerUpdate();
            }
            PhotonProfiler.LogReceive(sizeof(int));
        }
    }
}
