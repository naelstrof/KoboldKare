using Photon.Pun;
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

    protected override void Advance(Vector3 position) {
        base.Advance(position);
        fruitCount++;
        TriggerUpdate();
        if (fruitCount >= maxFruit) {
            TriggerComplete();
        }
    }

    private void OnGrindedObject(ReagentContents contents) {
        if (successSpawnLocation == null) {
            Advance(Vector3.zero);
        } else {
            Advance(successSpawnLocation.position);
        }
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
}
