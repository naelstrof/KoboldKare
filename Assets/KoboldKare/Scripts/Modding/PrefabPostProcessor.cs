using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class PrefabPostProcessor : ModPostProcessor {
    [SerializeField] private PrefabDatabase targetDatabase;
    [SerializeField] private bool networkedPrefabs;
    private List<GameObject> addedGameObjects;

    public override void Awake() {
        base.Awake();
        addedGameObjects = new List<GameObject>();
    }

    public override async Task LoadAllAssets(IList<IResourceLocation> locations) {
        addedGameObjects.Clear();
        var opHandle = Addressables.LoadAssetsAsync<GameObject>(locations, LoadPrefab);
        await opHandle.Task;
    }
    
    private void LoadPrefab(GameObject obj) {
        if (networkedPrefabs) {
            PreparePool.AddPrefab(obj.name, obj);
        }

        targetDatabase.AddPrefab(obj.name, obj);
        addedGameObjects.Add(obj);
    }

    public override void UnloadAllAssets(IList<IResourceLocation> locations) {
        foreach (var obj in addedGameObjects) {
            if (networkedPrefabs) {
                PreparePool.RemovePrefab(obj.name);
            }

            targetDatabase.RemovePrefab(obj.name);
        }
    }
}
