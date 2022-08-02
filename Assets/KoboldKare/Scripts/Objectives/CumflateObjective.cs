using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

public class CumflateObjective : DragonMailObjective {
    [SerializeField]
    private LocalizedString description;
    [SerializeField]
    private ScriptableReagent reagentType;
    [SerializeField]
    private int cumflatedMax = 5;

    private int cumflated;
    public override void Register() {
        GenericReagentContainer.containerFilled += OnContainerFilled;
    }
    public override void Unregister() {
        GenericReagentContainer.containerFilled -= OnContainerFilled;
    }
    private void OnContainerFilled(GenericReagentContainer container) {
        if (container.GetVolumeOf(reagentType) > container.maxVolume * 0.8f) {
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

    public override void Save(BinaryWriter writer, string version) {
        writer.Write(cumflated);
    }

    public override void Load(BinaryReader reader, string version) {
        cumflated = reader.ReadInt32();
        TriggerUpdate();
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(cumflated);
        } else {
            cumflated = (int)stream.ReceiveNext();
        }
    }
}
