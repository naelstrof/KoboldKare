using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class KoboldCreateObjective : DragonMailObjective {
    [SerializeField]
    private int maxKobolds = 1;

    [SerializeField]
    private LocalizedString description;
    
    private int kobolds = 0;
    public override void Register() {
        base.Register();
        FarmSpawnEventHandler.AddListener(OnEntitySpawn);
    }

    public override void Unregister() {
        base.Unregister();
        FarmSpawnEventHandler.RemoveListener(OnEntitySpawn);
    }

    private void OnEntitySpawn(GameObject obj) {
        Kobold k = obj.GetComponentInChildren<Kobold>();
        if (k != null) {
            ObjectiveManager.NetworkAdvance(obj.transform.position, k.photonView.ViewID.ToString());
        }
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        kobolds++;
        TriggerUpdate();
        if (kobolds >= maxKobolds) {
            TriggerComplete();
        }
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} {kobolds.ToString()}/{maxKobolds.ToString()}";
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
            if (newKobolds != kobolds) {
                kobolds = newKobolds;
                TriggerUpdate();
            }
            PhotonProfiler.LogReceive(sizeof(int));
        }
    }
}
