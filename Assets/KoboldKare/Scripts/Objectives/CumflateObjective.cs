using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Localization;

public class CumflateObjective : DragonMailObjective {
    [SerializeField]
    private LocalizedString description;
    [SerializeField]
    private ScriptableReagent reagentTypeA;
    [SerializeField]
    private ScriptableReagent reagentTypeB;
    [SerializeField]
    private int cumflatedMax = 5;

    private int cumflated;
    public override void Register() {
        base.Register();
        GeneHolder.containerFilled += OnContainerFilled;
    }
    public override void Unregister() {
        base.Unregister();
        GeneHolder.containerFilled -= OnContainerFilled;
    }
    private void OnContainerFilled(GeneHolder container) {
        if (container.GetVolumeOf(reagentTypeA) + container.GetVolumeOf(reagentTypeB) > container.maxVolume * 0.8f) {
            cumflated++;
            TriggerUpdate();
        }

        if (cumflated >= cumflatedMax) {
            TriggerComplete();
        }
    }
    public override string GetTextBody() {
        return description.GetLocalizedString();
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} {cumflated.ToString()}/{cumflatedMax.ToString()}";
    }

    public override void Save(JSONNode node) {
        node["cumflated"] = cumflated;
    }

    public override Task Load(JSONNode node) {
        cumflated = node["cumflated"];
        TriggerUpdate();
        return Task.CompletedTask;
    }

    // FIXME FISHNET
    /*
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream,info);
        if (stream.IsWriting) {
            stream.SendNext(cumflated);
        } else {
            int newCumflated = (int)stream.ReceiveNext();
            if (cumflated != newCumflated) {
                cumflated = newCumflated;
                TriggerUpdate();
            }
            PhotonProfiler.LogReceive(sizeof(int));
        }
    } */
}
