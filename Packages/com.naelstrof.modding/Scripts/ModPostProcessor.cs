using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

[Serializable]
public class ModPostProcessor {
    protected struct ModStubAddressableHandlePair {
        public ModManager.ModStub stub;
        public AsyncOperationHandle handle;
    }
    [SerializeField]
    protected AssetLabelReference searchLabel;
    public virtual void Awake() {
    }

    public virtual Task UnloadAssets(ModManager.ModInfoData data) {
        return Task.CompletedTask;
    }

    public virtual Task HandleAddressableMod(ModManager.ModInfoData data, IResourceLocator locator) {
        return Task.CompletedTask;
    }

    public virtual Task HandleAssetBundleMod(ModManager.ModInfoData data, AssetBundle assetBundle) {
        return Task.CompletedTask;
    }
}
