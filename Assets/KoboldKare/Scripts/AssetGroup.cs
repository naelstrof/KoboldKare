using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AssetGroup {
    public class AssetLocation {
        public string key;
        public string primaryKey;
        public ModManager.ModStub? stub;
        private AsyncOperationHandle<UnityEngine.Object> handle;
        private int useCount = 0;
        public bool GetRepresentedByStub(ModManager.ModStub? b) {
            if (b == null && stub == null) {
                return true;
            }
            if (b == null || stub == null) {
                return false;
            }
            return stub.Value.GetRepresentedBy(b.Value);
        }

        public class AssetHandle<T> {
            public T asset;
            private bool released = false;
            private event System.Action onRelease;
            public AssetHandle(T asset, System.Action onRelease) {
                this.asset = asset;
                this.onRelease = onRelease;
            }
            public void Release() {
                if (!released) {
                    released = true;
                    onRelease?.Invoke();
                }
            }
        }

        public async Task<AssetHandle<T>> GetAssetAsync<T>() where T : UnityEngine.Object {
            if (stub == null && !string.IsNullOrEmpty(primaryKey)) {
                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded) {
                    useCount++;
                    return new AssetHandle<T>((T)handle.Result, OnReleasedAsset);
                }
                handle = Addressables.LoadAssetAsync<UnityEngine.Object>(primaryKey);
                UnityEngine.Object obj = await handle.Task;
                useCount++;
                return new AssetHandle<T>((T)obj, OnReleasedAsset);
            }

            if (stub == null) {
                throw new UnityException("No stub or location to load asset from!");
            }

            if (ModManager.TryGetModByStub(stub.Value, out ModManager.Mod match)) {
                if (match is ModManager.ModAddressable modAddressable) {
                    if (handle.IsValid() && handle is { IsDone: true, Status: AsyncOperationStatus.Succeeded }) {
                        useCount++;
                        return new AssetHandle<T>((T)handle.Result, OnReleasedAsset);
                    }
                    handle = Addressables.LoadAssetAsync<UnityEngine.Object>(primaryKey);
                    UnityEngine.Object obj = await handle.Task;
                    useCount++;
                    return new AssetHandle<T>((T)obj, OnReleasedAsset);
                } else if (match is ModManager.ModAssetBundle modAssetBundle) {
                    var bundle = modAssetBundle.bundle;
                    var assetLoadHandle = bundle.LoadAssetAsync<UnityEngine.Object>(key).AsSingleAssetTask<UnityEngine.Object>();
                    UnityEngine.Object obj = await assetLoadHandle;
                    useCount++;
                    return new AssetHandle<T>((T)obj, OnReleasedAsset);
                } else {
                    throw new UnityException("Mod type not supported for asset loading!");
                }
            } else {
                throw new UnityException("Could not find mod for given stub!");
            }
        }

        private void OnReleasedAsset() {
            useCount--;
            if (handle.IsValid() && useCount == 0) {
                Addressables.Release(handle);
            }
            handle = default;
        }

        public void ReleaseAsset() {
            useCount--;
            if (handle.IsValid() && useCount == 0) {
                Addressables.Release(handle);
            }
            handle = default;
        }
    }
    public void RemoveAllRepresentedByStub(ModManager.ModStub? stub) {
        foreach (var pair in assets) {
            pair.value.RemoveAll(location => location.GetRepresentedByStub(stub));
            pair.value.Sort(CompareObjectStubPair);
        }
    }
    private static int CompareObjectStubPair(AssetLocation x, AssetLocation y) {
        if (x.stub == null && y.stub == null) return 0;
        if (x.stub == null) return -1;
        if (y.stub == null) return 1;
        if (x.stub.Value.loadPriority == y.stub.Value.loadPriority) {
            return String.Compare(x.stub.Value.title, y.stub.Value.title, StringComparison.InvariantCulture);
        }
        return x.stub.Value.loadPriority.CompareTo(y.stub.Value.loadPriority);
    }
    
    private static int CompareAssetKeyPair(AssetKeyPair x, AssetKeyPair y) {
        return String.Compare(x.key, y.key, StringComparison.InvariantCulture);
    }
    
    public struct AssetKeyPair {
        public string key;
        public List<AssetLocation> value;
    }
    
    protected internal List<AssetKeyPair> assets = new();

    public bool ContainsKey(string key) {
        foreach (var pair in assets) {
            if (pair.key == key) {
                return true;
            }
        }

        return false;
    }
    
    private bool TryGetList(string key, out List<AssetLocation> list) {
        foreach (var pair in assets) {
            if (pair.key == key) {
                list = pair.value;
                return true;
            }
        }

        list = null;
        return false;
    }
    
    public bool TryGetAssetLocation(int id, out AssetLocation matchLocation) {
        if (id >= 0 && id < assets.Count) {
            matchLocation = assets[id].value[^1];
            return true;
        }
        matchLocation = null;
        return false;
    }
    
    public bool TryGetAssetLocation(string key, out AssetLocation matchLocation) {
        for (int i = 0; i < assets.Count; i++) {
            if (assets[i].key == key) {
                matchLocation = assets[i].value[^1];
                return true;
            }
        }
        matchLocation = null;
        return false;
    }

    public void AddAsset(string key, ModManager.ModStub? stub, string primaryKey) {
        if (!ContainsKey(key)) {
            assets.Add(new AssetKeyPair() {
                key = key,
                value = new List<AssetLocation>()
            });
        }

        if (TryGetList(key, out var list)) {
            var assetLocation = new AssetLocation() {
                key = key,
                stub = stub,
                primaryKey = primaryKey
            };
            list.Add(assetLocation);
            list.Sort(CompareObjectStubPair);
        }
        assets.Sort(CompareAssetKeyPair);
    }
    
    public void RemoveAsset(string key, ModManager.ModStub? stub) {
        if (!ContainsKey(key)) {
            return;
        }
        if (!TryGetList(key, out var list)) {
            return;
        }

        for (int i = 0; i < list.Count; i++) {
            if (list[i].GetRepresentedByStub(stub)) {
                list.RemoveAt(i);
                i--;
            }
        }
        
        if (list.Count == 0) {
            assets.RemoveAll(pair => pair.key == key);
            return;
        }
        list.Sort(CompareObjectStubPair);
        assets.Sort(CompareAssetKeyPair);
    }

    public void GetAssetKeys(List<string> output) {
        output.Clear();
        foreach (var pair in assets) {
            output.Add(pair.value[^1].key);
        }
    }
    
    public List<string> GetAssetKeys() {
        List<string> assetKeys = new();
        foreach (var pair in assets) {
            assetKeys.Add(pair.value[^1].key);
        }
        return assetKeys;
    }
    
    public int GetAssetID(string key) {
        for (int i = 0; i < assets.Count; i++) {
            if (assets[i].key == key) {
                return i;
            }
        }
        return -1;
    }
    
}
