using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class PrefabPostProcessor : ModPostProcessor {
    [SerializeField]
    private PrefabDatabase targetDatabase;

    private List<GameObject> addedGameObjects;

    public override void Awake() {
        base.Awake();
        addedGameObjects = new List<GameObject>();
    }

    public override IEnumerator LoadAllAssets(IList<IResourceLocation> locations) {
        addedGameObjects.Clear();
        var opHandle = Addressables.LoadAssetsAsync<GameObject>(locations, LoadPrefab);
        yield return opHandle;
    }
    
    private void LoadPrefab(GameObject obj) {
        PreparePool.AddPrefab(obj.name, obj);
        targetDatabase.AddPrefab(obj.name);
        addedGameObjects.Add(obj);
    }

    public override void UnloadAllAssets(IList<IResourceLocation> locations) {
        foreach (var obj in addedGameObjects) {
            PreparePool.RemovePrefab(obj.name);
            targetDatabase.RemovePrefab(obj.name);
        }
    }
}
