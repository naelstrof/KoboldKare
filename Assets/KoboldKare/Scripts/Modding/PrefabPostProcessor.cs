using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PrefabPostProcessor : ModPostProcessor {
    AsyncOperationHandle opHandle;
    [SerializeField] private PrefabDatabase targetDatabase;
    [SerializeField] private bool networkedPrefabs;
    private List<GameObject> addedGameObjects;

    public override void Awake() {
        base.Awake();
        addedGameObjects = new List<GameObject>();
    }

    public override async Task LoadAllAssets() {
        var assetsHandle = Addressables.LoadResourceLocationsAsync(searchLabel.RuntimeKey);
        await assetsHandle.Task;
        opHandle = Addressables.LoadAssetsAsync<GameObject>(assetsHandle.Result, LoadPrefab);
        await opHandle.Task;
        Addressables.Release(assetsHandle);
    }
    
    private void LoadPrefab(GameObject obj) {
        if (obj == null) {
            return;
        }

        addedGameObjects ??= new List<GameObject>();

        for (int i = 0; i < addedGameObjects.Count; i++) {
            if (addedGameObjects[i] == null) {
                addedGameObjects.RemoveAt(i--);
            }
        }

        for (int i = 0; i < addedGameObjects.Count; i++) {
            if (addedGameObjects[i].name != obj.name) continue;
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
        addedGameObjects.Add(obj);
    }

    public override async Task HandleAssetBundleMod(ModManager.ModAssetBundle mod) {
        var key = searchLabel.labelString;
        List<Task> tasks = new List<Task>();
        var rootNode = mod.info.assets;
        if (rootNode.HasKey(key)) {
            var array = rootNode[key].AsArray;
            foreach (var node in array) {
                if (!node.Value.IsString) continue;
                var assetName = node.Value;
                var handle = mod.bundle.LoadAssetAsync<GameObject>(assetName);
                handle.completed += (a) => {
                    LoadPrefab(handle.asset as GameObject);
                };
                tasks.Add(handle.AsSingleAssetTask<GameObject>());
            }
        }
        await Task.WhenAll(tasks);
    }

    public override Task UnloadAllAssets() {
        foreach (var obj in addedGameObjects) {
            if (!obj) {
            }
            if (networkedPrefabs) {
                PreparePool.RemovePrefab(obj.name);
            }
            targetDatabase.RemovePrefab(obj.name);
        }
        if (opHandle.IsValid()) {
            Addressables.Release(opHandle);
        }
        return Task.CompletedTask;
    }
}
