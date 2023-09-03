using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class EquipmentPostProcessor : ModPostProcessor {
    private List<Equipment> addedEquipments;
    public override async Task LoadAllAssets(IList<IResourceLocation> locations)  {
        addedEquipments.Clear();
        var opHandle = Addressables.LoadAssetsAsync<Equipment>(locations, LoadEquipment);
        await opHandle.Task;
    }

    public override void Awake() {
        base.Awake();
        addedEquipments = new List<Equipment>();
    }

    private void LoadEquipment(Equipment equipment) {
        if (equipment == null) {
            return;
        }
        addedEquipments.Add(equipment);
        EquipmentDatabase.AddEquipment(equipment);
    }

    public override void UnloadAllAssets() {
        foreach (var equipment in addedEquipments) {
            EquipmentDatabase.RemoveEquipment(equipment);
        }
    }
}
