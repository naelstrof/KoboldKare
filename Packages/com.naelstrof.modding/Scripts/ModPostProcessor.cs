using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class ModPostProcessor {
    protected struct ModStubAddressableHandlePair {
        public ModManager.ModStub stub;
        public AsyncOperationHandle handle;
    }
    public virtual Task Awake() {
        return Task.CompletedTask;
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
