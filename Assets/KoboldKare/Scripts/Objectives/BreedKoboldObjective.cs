using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class BreedKoboldObjective : ObjectiveWithSpaceBeam {
    [SerializeField]
    private int maxEggs = 5;
    [SerializeField]
    private LocalizedString description;
    private int eggs = 0;
    public override void Register() {
        base.Register();
        OvipositionSpot.oviposition += OnOviposit;
    }
    public override void Unregister() {
        base.Unregister();
        OvipositionSpot.oviposition -= OnOviposit;
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        eggs++;
        TriggerUpdate();
        if (eggs >= maxEggs) {
            TriggerComplete();
        }
    }

    protected virtual void OnOviposit(int koboldID, int eggID) {
        PhotonView view = PhotonNetwork.GetPhotonView(eggID);
        
        ObjectiveManager.NetworkAdvance(view == null ? Vector3.zero : view.transform.position, $"{koboldID.ToString()}{eggID.ToString()}");
    }
    public override string GetTextBody() {
        return $"{description.GetLocalizedString()} {eggs.ToString()}/{maxEggs.ToString()}";
    }

    public override void Save(JSONNode node) {
        node["eggs"] = eggs;
    }

    public override void Load(JSONNode node) {
        eggs = node["eggs"];
        TriggerUpdate();
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(eggs);
        } else {
            int newEggs = (int)stream.ReceiveNext();
            if (eggs != newEggs) {
                eggs = newEggs;
                TriggerUpdate();
            }
            PhotonProfiler.LogReceive(sizeof(int));
        }
    }
}
