using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

[System.Serializable]
public class ModPostProcessor {
    [SerializeField]
    protected UnityEngine.AddressableAssets.AssetLabelReference searchLabel;
    public virtual void Awake() {
    }
    public virtual AssetLabelReference GetSearchLabel() => searchLabel;
    public virtual IEnumerator LoadAllAssets(IList<IResourceLocation> locations) {
        var opHandle = Addressables.LoadAssetsAsync<object>(locations, (object o) => { });
        yield return opHandle;
    }
    public virtual void UnloadAllAssets(IList<IResourceLocation> locations) {
        Addressables.Release(locations);
    }
}
