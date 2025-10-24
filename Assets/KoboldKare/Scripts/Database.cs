using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class Database<T> where T : UnityEngine.Object {
    private class StringSorter : IComparer<string> {
        public int Compare(string x, string y) {
            return String.Compare(x, y, StringComparison.InvariantCulture);
        }
    }
    private SortedList<string, DatabaseValue<T>> values;

    protected abstract class DatabaseValue<A> where A : UnityEngine.Object {
        public abstract Task<A> GetValueAsync(out bool hasHandle, out AsyncOperationHandle<A> handle);
    }
    private static Task<A> ConvertToTask<A>(AssetBundleRequest request) where A : UnityEngine.Object {
        var tcs = new TaskCompletionSource<A>(TaskCreationOptions.RunContinuationsAsynchronously);
        request.completed += _ => {
            if (request.asset is A t) {
                tcs.SetResult(t);
            } else if (!request.asset) {
                tcs.SetException(new Exception("AssetBundleRequest returned null."));
            } else {
                tcs.SetException(new InvalidCastException($"Asset is {request.asset.GetType()}, cannot cast to {typeof(T)}."));
            }
        };
        return tcs.Task;
    }
    
    protected class DatabaseAddressableValue<A> : DatabaseValue<A> where A : UnityEngine.Object {
        private IResourceLocation location;
        public DatabaseAddressableValue(IResourceLocation location) {
            this.location = location;
        }
        public override Task<A> GetValueAsync(out bool hasHandle, out AsyncOperationHandle<A> handle) {
            handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<A>(location);
            hasHandle = true;
            return handle.Task;
        }
    }
    
    protected class DatabaseAssetBundleValue<A> : DatabaseValue<A> where A : UnityEngine.Object {
        private AssetBundle bundle;
        private string assetName;
        public DatabaseAssetBundleValue(AssetBundle bundle, string assetName) {
            this.bundle = bundle;
            this.assetName = assetName;
        }
        public override Task<A> GetValueAsync(out bool hasHandle, out AsyncOperationHandle<A> handle) {
            handle = default;
            hasHandle = false;
            return ConvertToTask<A>(bundle.LoadAssetAsync<A>(assetName));
        }
    }

    protected Task<T> GetValueAsync(int id, out bool hasHandle, out AsyncOperationHandle<T> handle) {
        if (id < 0 || id >= values.Count) {
            Debug.LogError($"ID {id} is out of range for database of type {typeof(T)}.");
            if (values.Count > 0) {
                return values.Values[0].GetValueAsync(out hasHandle, out handle);
            }
            throw new Exception("Don't have any values in the database.");
        }
        return values.Values[id].GetValueAsync(out hasHandle, out handle);
    }

    protected void GetKeys(List<string> keys) {
        keys.Clear();
        keys.AddRange(values.Keys);
    }

    protected Task<T> GetValueAsync(string key, out bool hasHandle, out AsyncOperationHandle<T> handle, out int id) {
        if (!values.ContainsKey(key)) {
            throw new KeyNotFoundException($"Key {key} not found in database of type {typeof(T)}.");
        }

        if (values.TryGetValue(key, out var value)) {
            id = values.IndexOfKey(key);
            value.GetValueAsync(out hasHandle, out handle);
        }

        Debug.LogError($"Key {key} is not in database of type {typeof(T)}.");
        if (values.Count > 0) {
            id = 0;
            return values.Values[0].GetValueAsync(out hasHandle, out handle);
        }
        throw new Exception("Don't have any values in the database.");
    }
    
    public void AddAddressable(string key, IResourceLocation location) {
        values[key] = new DatabaseAddressableValue<T>(location);
    }
    public void AddAssetBundle(string key, AssetBundle bundle) {
        values[key] = new DatabaseAssetBundleValue<T>(bundle, key);
    }
}
