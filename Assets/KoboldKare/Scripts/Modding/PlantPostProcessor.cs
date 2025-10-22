using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlantPostProcessor : ModPostProcessor {
    private List<ScriptablePlant> addedPlants;
    private AsyncOperationHandle opHandle;
    public override async Task LoadAllAssets()  {
        addedPlants.Clear();
        var assetsHandle = Addressables.LoadResourceLocationsAsync(searchLabel.RuntimeKey);
        await assetsHandle.Task;
        opHandle = Addressables.LoadAssetsAsync<ScriptablePlant>(assetsHandle.Result, LoadPlant);
        await opHandle.Task;
        Addressables.Release(assetsHandle);
    }

    public override void Awake() {
        base.Awake();
        addedPlants = new List<ScriptablePlant>();
    }

    private void LoadPlant(ScriptablePlant plant) {
        if (!plant) {
            return;
        }
        addedPlants.Add(plant);
        PlantDatabase.AddPlant(plant);
    }

    public override Task UnloadAllAssets() {
        foreach (var plant in addedPlants) {
            PlantDatabase.RemovePlant(plant);
        }

        if (opHandle.IsValid()) {
            Addressables.Release(opHandle);
        }

        return Task.CompletedTask;
    }
}
