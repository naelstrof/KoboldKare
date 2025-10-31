using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlantPostProcessor : ModPostProcessor {
    private struct ModStubPlantPair {
        public ModManager.ModStub stub;
        public ScriptablePlant plant;
    }
    
    private List<ModStubPlantPair> addedPlants;
    
    private List<ModStubAddressableHandlePair> opHandles;

    private ModManager.ModStub currentStub;
    
    private AsyncOperationHandle inherentAssetsHandle;
    public override async Task HandleAddressableMod(ModManager.ModInfoData data, IResourceLocator locator) {
        if (locator.Locate(searchLabel.RuntimeKey, typeof(ScriptablePlant), out var locations)) {
            currentStub = new ModManager.ModStub(data);
            var opHandle = Addressables.LoadAssetsAsync<ScriptablePlant>(locations, LoadPlant);
            await opHandle.Task;
            opHandles.Add(new ModStubAddressableHandlePair() {
                stub = currentStub,
                handle = opHandle
            });
        }
    }

    public override async Task Awake() {
        await base.Awake();
        addedPlants = new ();
        opHandles = new();
        inherentAssetsHandle = Addressables.LoadAssetsAsync<ScriptablePlant>(searchLabel.RuntimeKey, LoadInherentPlant);
        await inherentAssetsHandle.Task;
    }
    
    private void LoadInherentPlant(ScriptablePlant plant) {
        if (!plant) {
            return;
        }
        addedPlants.Add(new ModStubPlantPair() {
            stub = currentStub,
            plant = plant
        });
        PlantDatabase.AddPlant(plant);
    }

    private void LoadPlant(ScriptablePlant plant) {
        if (!plant) {
            return;
        }
        addedPlants.Add(new ModStubPlantPair() {
            stub = currentStub,
            plant = plant
        });
        PlantDatabase.AddPlant(plant);
    }
    public override Task UnloadAssets(ModManager.ModInfoData data) {
        for (int i=0;i<addedPlants.Count;i++) {
            if(addedPlants[i].stub.GetRepresentedBy(data)) {
                PlantDatabase.RemovePlant(addedPlants[i].plant);
                addedPlants.RemoveAt(i);
                i--;
            }
        }
        for (int i=0;i<opHandles.Count;i++) {
            if(opHandles[i].stub.GetRepresentedBy(data)) {
                if (opHandles[i].handle.IsValid()) {
                    Addressables.Release(opHandles[i].handle);
                }
                opHandles.RemoveAt(i);
                i--;
            }
        }
        return base.UnloadAssets(data);
    }
}
