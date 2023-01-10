using System.Collections;
using System.Collections.Generic;
using KoboldKare;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Localization;

public class DeliverMicroKoboldsObjective : ObjectiveWithSpaceBeam {
    [SerializeField]
    private LocalizedString description;
    private int koboldCount = 0;
    [SerializeField]
    private int maxKobolds = 5;

    [SerializeField] private GameEventPhotonView soldGameObjectEvent;
    
    public override void Register() {
        base.Register();
        soldGameObjectEvent.AddListener(OnSoldObject);
    }
    
    public override void Unregister() {
        base.Unregister();
        soldGameObjectEvent.RemoveListener(OnSoldObject);
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        koboldCount++;
        TriggerUpdate();
        if (koboldCount >= maxKobolds) {
            TriggerComplete();
        }
    }

    private void OnSoldObject(PhotonView view) {
        Kobold k = view.GetComponent<Kobold>();
        if (k == null) {
            return;
        }
        if (k.GetGenes().baseSize < 14f) {
            ObjectiveManager.NetworkAdvance(spaceBeamTarget.position, view.ViewID.ToString());
        }
    }
    
    public override void Save(JSONNode node) {
        node["koboldCount"] = koboldCount;
    }

    public override void Load(JSONNode node) {
        if (node.HasKey("koboldCount")) {
            koboldCount = node["koboldCount"];
            TriggerUpdate();
        }
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} {koboldCount.ToString()}/{maxKobolds.ToString()}";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(koboldCount);
        } else {
            int newKoboldCount = (int)stream.ReceiveNext();
            if (newKoboldCount != koboldCount) {
                koboldCount = newKoboldCount;
                TriggerUpdate();
            }
            PhotonProfiler.LogReceive(sizeof(int));
        }
    }
}
