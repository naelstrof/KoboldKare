using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EquipmentPostProcessor : ModPostProcessor {
    private struct ModStubEquipmentPair {
        public ModManager.ModStub stub;
        public Equipment equipment;
    }
    
    private List<ModStubEquipmentPair> addedEquipments;
    
    private List<ModStubAddressableHandlePair> opHandles;

    private ModManager.ModStub currentStub;
    private AsyncOperationHandle<IList<Equipment>> inherentAssetsHandle;
    
    public override async Task HandleAddressableMod(ModManager.ModInfoData data, IResourceLocator locator) {
        if (locator.Locate(searchLabel.RuntimeKey, typeof(Equipment), out var locations)) {
            currentStub = new ModManager.ModStub(data);
            var opHandle = Addressables.LoadAssetsAsync<Equipment>(locations, LoadEquipment);
            await opHandle.Task;
            opHandles.Add(new ModStubAddressableHandlePair() {
                stub = currentStub,
                handle = opHandle
            });
        }
    }
    public override async Task HandleAssetBundleMod(ModManager.ModInfoData data, AssetBundle assetBundle) {
        var key = searchLabel.labelString;
        List<Task> tasks = new List<Task>();
        var rootNode = data.assets;
        if (rootNode.HasKey(key)) {
            currentStub = new ModManager.ModStub(data);
            var array = rootNode[key].AsArray;
            foreach (var node in array) {
                if (!node.Value.IsString) continue;
                var assetName = node.Value;
                var handle = assetBundle.LoadAssetAsync<Equipment>(assetName);
                handle.completed += (a) => {
                    LoadEquipment(((AssetBundleRequest)a).asset as Equipment);
                };
                tasks.Add(handle.AsSingleAssetTask<Equipment>());
            }
        }
        await Task.WhenAll(tasks);
    }

    public override async Task Awake() {
        await base.Awake();
        addedEquipments = new ();
        opHandles = new();
        inherentAssetsHandle = Addressables.LoadAssetsAsync<Equipment>(searchLabel.RuntimeKey, LoadInherentEquipment);
        await inherentAssetsHandle.Task;
    }
    private void LoadInherentEquipment(Equipment equipment) {
        EquipmentDatabase.AddAsset(equipment, currentStub);
    }

    private void LoadEquipment(Equipment equipment) {
        if (equipment == null) {
            return;
        }
        addedEquipments.Add(new ModStubEquipmentPair() {
            stub = currentStub,
            equipment = equipment
        });
        EquipmentDatabase.AddAsset(equipment, currentStub);
    }

    public override Task UnloadAssets(ModManager.ModInfoData data) {
        for (int i=0;i<addedEquipments.Count;i++) {
            if(addedEquipments[i].stub.GetRepresentedBy(data)) {
                EquipmentDatabase.RemoveAsset(addedEquipments[i].equipment, addedEquipments[i].stub);
                addedEquipments.RemoveAt(i);
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
