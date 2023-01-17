using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

[Serializable]
public class ModPostProcessor {
    [SerializeField]
    protected AssetLabelReference searchLabel;
    public virtual void Awake() {
    }
    public virtual AssetLabelReference GetSearchLabel() => searchLabel;
    public virtual async Task LoadAllAssets(IList<IResourceLocation> locations) {
    }
    public virtual void UnloadAllAssets(IList<IResourceLocation> locations) {
        Addressables.Release(locations);
    }
}
