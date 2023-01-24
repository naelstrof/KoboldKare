using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class GrindFruitObjective : ObjectiveWithSpaceBeam {
    [SerializeField]
    private LocalizedString description;
    [SerializeField]
    private PhotonGameObjectReference fruit;
    [SerializeField]
    private Transform fruitSpawnLocation;
    [SerializeField]
    private Transform successSpawnLocation;
    
    private int fruitCount = 0;
    [SerializeField]
    private int maxFruit = 4;
    
    public override void Register() {
        base.Register();
        GrinderManager.grindedObject += OnGrindedObject;
        ElectricBlender.grindedObject += OnGrindedObject;
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.InstantiateRoomObject(fruit.photonName, fruitSpawnLocation.position, fruitSpawnLocation.rotation);
        }
    }
    
    public override void Unregister() {
        base.Unregister();
        GrinderManager.grindedObject -= OnGrindedObject;
        ElectricBlender.grindedObject -= OnGrindedObject;
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        fruitCount++;
        TriggerUpdate();
        if (fruitCount >= maxFruit) {
            TriggerComplete();
        }
    }

    private void OnGrindedObject(int viewID, ReagentContents contents) {
        ObjectiveManager.NetworkAdvance(successSpawnLocation == null ? Vector3.zero : successSpawnLocation.position, viewID.ToString());
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} {fruitCount.ToString()}/{maxFruit.ToString()}";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }

    public override void OnValidate() {
        base.OnValidate();
        fruit.OnValidate();
    }
    public override void Save(JSONNode node) {
        node["fruitCount"] = fruitCount;
    }

    public override void Load(JSONNode node) {
        if (node.HasKey("fruitCount")) {
            fruitCount = node["fruitCount"];
            TriggerUpdate();
        }
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(fruitCount);
        } else {
            int newFruitCount = (int)stream.ReceiveNext();
            if (newFruitCount != fruitCount) {
                fruitCount = newFruitCount;
                TriggerUpdate();
            }
            PhotonProfiler.LogReceive(sizeof(int));
        }
    }
}
