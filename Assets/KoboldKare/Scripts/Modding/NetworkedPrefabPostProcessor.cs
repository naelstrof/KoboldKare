using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

public class NetworkedPrefabPostProcessor : ModPostProcessor {
    public override void LoadAsset(IResourceLocation location, object asset) {
        base.LoadAsset(location, asset);
        if (asset is not GameObject obj) {
            throw new UnityException("Asset marked as networked object is not required type `GameObject`.");
        }
        PreparePool.AddPrefab(obj.name, obj);
    }

    public override void UnloadAsset(IResourceLocation location, object asset) {
        if (asset is GameObject obj) {
            PreparePool.RemovePrefab(obj.name);
        }
    }
}
