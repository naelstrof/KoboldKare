using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
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

    protected override void Advance(Vector3 position) {
        base.Advance(position);
        eggs++;
        TriggerUpdate();
        if (eggs >= maxEggs) {
            TriggerComplete();
        }
    }

    protected virtual void OnOviposit(GameObject egg) {
        Advance(egg.transform.position);
    }
    public override string GetTextBody() {
        return $"{description.GetLocalizedString()} {eggs.ToString()}/{maxEggs.ToString()}";
    }

    public override void Save(BinaryWriter writer) {
        writer.Write(eggs);
    }

    public override void Load(BinaryReader reader) {
        eggs = reader.ReadInt32();
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(eggs);
        } else {
            eggs = (int)stream.ReceiveNext();
        }
    }
}
