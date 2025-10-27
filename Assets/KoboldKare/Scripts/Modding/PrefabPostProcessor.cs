using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PrefabPostProcessor : ModPostProcessor {
    [SerializeField] private PrefabDatabase targetDatabase;
    [SerializeField] private bool networkedPrefabs;
    private struct ModStubGameObjectPair {
        public ModManager.ModStub stub;
        public GameObject obj;
    }
    
    private List<ModStubGameObjectPair> addedGameObjects;
    
    private List<ModStubAddressableHandlePair> opHandles;

    private ModManager.ModStub currentStub;

    public override void Awake() {
        base.Awake();
        addedGameObjects = new ();
        opHandles = new();
    }
    
    private void LoadPrefab(GameObject obj) {
        if (obj == null) {
            return;
        }

        for (int i = 0; i < addedGameObjects.Count; i++) {
            if (addedGameObjects[i].obj.name != obj.name) continue;
            if (networkedPrefabs) {
                PreparePool.RemovePrefab(obj.name);
            }
            targetDatabase.RemovePrefab(obj.name);
            addedGameObjects.RemoveAt(i);
            break;
        }
        
        // If something is a part of multiple groups (Say, an EquipmentStoreItem and a Cosmetic, it'll get double added to the networked prefabs.)
        // This is to prevent it being added multiple times.
        if (PreparePool.HasPrefab(obj.name)) {
            PreparePool.RemovePrefab(obj.name);
        }

        if (networkedPrefabs) {
            PreparePool.AddPrefab(obj.name, obj);
        }
        targetDatabase.AddPrefab(obj.name, obj);
        addedGameObjects.Add(new ModStubGameObjectPair() {
            stub = currentStub,
            obj = obj
        });
    }

    public override async Task HandleAssetBundleMod(ModManager.ModInfoData data, AssetBundle assetBundle) {
        var key = searchLabel.labelString;
        List<Task> tasks = new List<Task>();
        var rootNode = data.assets;
        if (rootNode.HasKey(key)) {
            var array = rootNode[key].AsArray;
            foreach (var node in array) {
                if (!node.Value.IsString) continue;
                var assetName = node.Value;
                var handle = assetBundle.LoadAssetAsync<GameObject>(assetName);
                handle.completed += (a) => {
                    LoadPrefab(handle.asset as GameObject);
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
            var obj = addedGameObjects[i].obj;
            if (networkedPrefabs) {
                PreparePool.RemovePrefab(obj.name);
            }
            targetDatabase.RemovePrefab(obj.name);
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
