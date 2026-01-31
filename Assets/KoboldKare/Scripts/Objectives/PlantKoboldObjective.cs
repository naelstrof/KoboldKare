using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FishNet;
using FishNet.Managing;
using NetStack.Serialization;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class PlantKoboldObjective : ObjectiveWithSpaceBeam {
    [SerializeField] private ScriptablePlant targetPlant;
    [SerializeField] private int maxPlants = 1;
    [SerializeField] private LocalizedString description;
    [SerializeField] private PhotonGameObjectReference eggPrefab;
    [SerializeField] private Transform mailBox;
    
    private NetworkManager networkManager;
    
    private int plants = 0;
    public override void Register() {
        base.Register();
        Plant.planted += OnPlant;
        networkManager = InstanceFinder.NetworkManager;
        if (networkManager.ServerManager.Started) {
            var data = KoboldEntitySpawner.NetworkedEntityInstantiationData.Default();
            data.assetName = "Egg";
            data.groupName = "NetworkedPrefab";
            data.position = mailBox.position;
            data.rotation = Quaternion.identity;
            networkManager.GetComponent<KoboldEntitySpawner>().SpawnAsServer(data, true);
        }
    }
    public override void Unregister() {
        base.Unregister();
        Plant.planted -= OnPlant;
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        plants++;
        TriggerUpdate();
        if (plants >= maxPlants) {
            TriggerComplete();
        }
    }

    private void OnPlant(GameObject obj, ScriptablePlant plant) {
        // FIXME FISHNET
        /*
        if (plant == targetPlant) {
            ObjectiveManager.NetworkAdvance(obj.transform.position, $"PlantKoboldObjective{obj.GetPhotonView().ViewID.ToString()}");
        }*/
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} {plants.ToString()}/{maxPlants.ToString()}";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }

    public override void Save(JSONNode node) {
        node["plants"] = plants;
    }

    public override Task Load(JSONNode node) {
        plants = node["plants"];
        return Task.CompletedTask;
    }

    // FIXME FISHNET
    /*
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(plants);
        } else {
            int newPlants = (int)stream.ReceiveNext();
            if (newPlants != plants) {
                plants = newPlants;
                TriggerUpdate();
            }
            PhotonProfiler.LogReceive(sizeof(int));
        }
    }*/
}
