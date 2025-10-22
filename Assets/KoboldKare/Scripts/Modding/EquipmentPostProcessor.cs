using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EquipmentPostProcessor : ModPostProcessor {
    private List<Equipment> addedEquipments;
    private AsyncOperationHandle opHandle;
    public override async Task LoadAllAssets()  {
        addedEquipments.Clear();
        var assetsHandle = Addressables.LoadResourceLocationsAsync(searchLabel.RuntimeKey);
        await assetsHandle.Task;
        opHandle = Addressables.LoadAssetsAsync<Equipment>(assetsHandle.Result, LoadEquipment);
        await opHandle.Task;
        Addressables.Release(assetsHandle);
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

    public override Task UnloadAllAssets() {
        foreach (var equipment in addedEquipments) {
            EquipmentDatabase.RemoveEquipment(equipment);
        }

        if (opHandle.IsValid()) {
            Addressables.Release(opHandle);
        }
        return Task.CompletedTask;
    }
}
