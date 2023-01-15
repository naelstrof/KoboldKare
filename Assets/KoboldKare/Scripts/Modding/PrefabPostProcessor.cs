using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

public class PrefabPostProcessor : ModPostProcessor {
    [SerializeField]
    private PrefabDatabase targetDatabase;
    public override void LoadAsset(IResourceLocation location, object asset) {
        base.LoadAsset(location, asset);
        if (asset is not GameObject obj) {
            throw new UnityException($"Asset marked as {searchLabel} is not required type `GameObject`.");
        }
        targetDatabase.AddPrefab(location.PrimaryKey);
    }

    public override void UnloadAsset(IResourceLocation location, object asset) {
        base.UnloadAsset(location, asset);
        targetDatabase.RemovePrefab(location.PrimaryKey);
    }
}
