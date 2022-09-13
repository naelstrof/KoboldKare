using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class PlantKoboldObjective : ObjectiveWithSpaceBeam {
    [SerializeField] private ScriptablePlant targetPlant;
    [SerializeField] private int maxPlants = 1;
    [SerializeField] private LocalizedString description;
    [SerializeField] private PhotonGameObjectReference eggPrefab;
    [SerializeField] private Transform mailBox;
    
    private int plants = 0;
    public override void Register() {
        base.Register();
        PlantSpawnEventHandler.AddListener(OnPlant);
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.InstantiateRoomObject(eggPrefab.photonName, mailBox.transform.position, Quaternion.identity,
                0, new object[] { new KoboldGenes().Randomize() });
        }
    }
    public override void Unregister() {
        base.Unregister();
        PlantSpawnEventHandler.RemoveListener(OnPlant);
    }

    protected override void Advance(Vector3 position) {
        base.Advance(position);
        plants++;
        TriggerUpdate();
        if (plants >= maxPlants) {
            TriggerComplete();
        }
    }

    private void OnPlant(GameObject obj, ScriptablePlant plant) {
        if (plant == targetPlant) {
            Advance(obj.transform.position);
        }
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} {plants.ToString()}/{maxPlants.ToString()}";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }

    public override void Save(BinaryWriter writer) {
        writer.Write(plants);
    }

    public override void Load(BinaryReader reader) {
        plants = reader.ReadInt32();
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(plants);
        } else {
            plants = (int)stream.ReceiveNext();
        }
    }

    public override void OnValidate() {
        base.OnValidate();
        eggPrefab.OnValidate();
    }
}
