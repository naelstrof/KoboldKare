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
        var opHandle = Addressables.LoadAssetsAsync<GameObject>(locations, LoadPrefab);
        await opHandle.Task;
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

    public override void UnloadAllAssets() {
        foreach (var obj in addedGameObjects) {
            if (networkedPrefabs) {
                PreparePool.RemovePrefab(obj.name);
            }
            targetDatabase.RemovePrefab(obj.name);
        }
    }
}
