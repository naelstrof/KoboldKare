using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class BreedKoboldObjective : DragonMailObjective {
    [SerializeField]
    private int maxEggs = 5;
    [SerializeField]
    private LocalizedString description;
    private int eggs = 0;
    public override void Register() {
        OvipositionSpot.oviposition += OnOviposit;
    }
    public override void Unregister() {
        OvipositionSpot.oviposition -= OnOviposit;
    }
    
    private void OnOviposit(GameObject egg) {
        eggs++;
        TriggerUpdate();
        if (eggs >= maxEggs) {
            TriggerComplete();
        }
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
