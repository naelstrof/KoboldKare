using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class PlantPostProcessor : ModPostProcessor {
    private List<ScriptablePlant> addedPlants;
    public override async Task LoadAllAssets(IList<IResourceLocation> locations)  {
        addedPlants.Clear();
        var opHandle = Addressables.LoadAssetsAsync<ScriptablePlant>(locations, LoadPlant);
        await opHandle.Task;
    }

    public override void Awake() {
        base.Awake();
        addedPlants = new List<ScriptablePlant>();
    }

    private void LoadPlant(ScriptablePlant plant) {
        if (plant == null) {
            return;
        }
        addedPlants.Add(plant);
        PlantDatabase.AddPlant(plant);
    }

    public override void UnloadAllAssets() {
        foreach (var plant in addedPlants) {
            PlantDatabase.RemovePlant(plant);
        }
    }
}
