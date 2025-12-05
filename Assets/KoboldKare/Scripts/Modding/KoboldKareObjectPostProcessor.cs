using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class KoboldKareObjectPostProcessor : ModPostProcessor {
    [SerializeField] protected AssetLabelReference equipmentLabel;
    [SerializeField] protected AssetLabelReference reactionsLabel;
    [SerializeField] protected AssetLabelReference reagentsLabel;
    [SerializeField] protected AssetLabelReference koboldLabel;
    [SerializeField] protected AssetLabelReference plantLabel;
    [SerializeField] protected AssetLabelReference dicksLabel;
    [SerializeField] protected AssetLabelReference cosmeticItemsLebel;
    [SerializeField] protected AssetLabelReference seedsLabel;
    [SerializeField] protected AssetLabelReference networkedPrefabsLabel;
    
    private struct ModStubPair<T> {
        public ModManager.ModStub stub;
        public string objKey;
        public T obj;
    }

    public override async Task Awake() {
        await base.Awake();
        var locationHandle = Addressables.LoadResourceLocationsAsync(equipmentLabel.RuntimeKey);
    }
    
    private void LoadInherentPrefab(GameObject obj) {
        // FIXME FISHNET
        /*if (PreparePool.HasPrefab(obj.name)) {
            return;
        }
        if (networkedPrefabs) {
            PreparePool.AddPrefab(obj.name, obj, null);
        }*/
        targetDatabase.AddPrefab(obj.name, obj, null);
    }
    
    private void LoadPrefab(GameObject obj) {
        if (obj == null) {
            return;
        }
        // FIXME FISHNET
        /*
        if (networkedPrefabs) {
            PreparePool.AddPrefab(obj.name, obj, currentStub);
        }
        */
        targetDatabase.AddPrefab(obj.name, obj, currentStub);
        addedGameObjects.Add(new ModStubGameObjectPair() {
            stub = currentStub,
            obj = obj,
            objName = obj.name
        });
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
                var handle = assetBundle.LoadAssetAsync<GameObject>(assetName);
                handle.completed += (a) => {
                    LoadPrefab(((AssetBundleRequest)a).asset as GameObject);
                };
                tasks.Add(handle.AsSingleAssetTask<GameObject>());
            }
        }
        await Task.WhenAll(tasks);
    }

    public override async Task HandleAddressableMod(ModManager.ModInfoData data, IResourceLocator locator) {
        if (locator.Locate(searchLabel.RuntimeKey, typeof(GameObject), out var locations)) {
            currentStub = new ModManager.ModStub(data);
            var opHandle = Addressables.LoadAssetsAsync<GameObject>(locations, LoadPrefab);
            await opHandle.Task;
            opHandles.Add(new ModStubAddressableHandlePair() {
                stub = currentStub,
                handle = opHandle
            });
        }
    }

    public override Task UnloadAssets(ModManager.ModInfoData data) {
        for (int i=0;i<addedGameObjects.Count;i++) {
            if (!addedGameObjects[i].stub.GetRepresentedBy(data)) continue;
            var obj = addedGameObjects[i];
            /*
            if (networkedPrefabs) {
                PreparePool.RemovePrefab(obj.objName, obj.stub);
            }
            */
            targetDatabase.RemovePrefab(obj.objName, obj.stub);
            addedGameObjects.RemoveAt(i);
            i--;
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
