using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

[System.Serializable]
public class ModPostProcessor {
    [SerializeField]
    protected UnityEngine.AddressableAssets.AssetLabelReference searchLabel;

    public virtual UnityEngine.AddressableAssets.AssetLabelReference GetSearchLabel() => searchLabel;
    public virtual void LoadAsset(IResourceLocation location, object asset) {
    }
    public virtual void UnloadAsset(IResourceLocation location, object asset) {
    }
}
