using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

[Serializable]
public class ModPostProcessor {
    [SerializeField]
    protected AssetLabelReference searchLabel;
    public virtual void Awake() {
    }
    public virtual Task LoadAllAssets() {
        return Task.CompletedTask;
    }
    public virtual Task UnloadAllAssets() {
        return Task.CompletedTask;
    }
    public virtual Task HandleAssetBundleMod(ModManager.ModAssetBundle mod) {
        return Task.CompletedTask;
    }
}
