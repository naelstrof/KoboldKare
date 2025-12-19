using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Database<T> : MonoBehaviour where T : UnityEngine.Object {
    protected static Database<T> instance;
    [SerializeField] private T missingObject;
    
    private List<AssetGroup.AssetLocation.AssetHandle<T>> handles;

    private class StringSorter : IComparer<string> {
        public int Compare(string x, string y) {
            return String.Compare(x, y, StringComparison.InvariantCulture);
        }
    }
    protected internal SortedDictionary<string, T> assets = new(new StringSorter());

    private bool ContainsKey(string name) {
        return assets.ContainsKey(name);
    }
    public void Awake() {
        if (instance && instance != this) {
            Destroy(gameObject);
        } else {
            handles = new();
            instance = this;
        }
    }
    public static bool TryGetAsset(string name, out T match) {
        return instance.assets.TryGetValue(name, out match);
    }
    
    public static bool TryGetAsset(short id, out T match) {
        int i = 0;
        foreach (var pair in instance.assets) {
            if (i == id) {
                match = pair.Value;
                return true;
            }
            i++;
        }
        
        match = null;
        return false;
    }

    public static void AddAsset(string key, T newAsset) {
        if (!instance.ContainsKey(key)) {
            instance.assets.Add(key, newAsset);
        } else {
            instance.assets[key] = newAsset;
        }
    }

    public static void ClearAllAssets() {
        instance.assets.Clear();
    }
    
    public static void RemoveAsset(string key) {
        if (!instance.ContainsKey(key)) {
            return;
        }
        instance.assets.Remove(key);
    }

    public static short GetID(T obj) {
        var key = obj.name;
        if (!instance.ContainsKey(key)) {
            return 0;
        }

        int i = 0;
        foreach (var pair in instance.assets) {
            if (pair.Key == obj.name) {
                return (short)i;
            }
            i++;
        }
        return 0;
    }
    public static List<T> GetAssets() {
        List<T> assets = new();
        foreach (var pair in instance.assets) {
            assets.Add(pair.Value);
        }
        return assets;
    }

    public static List<string> GetAssetKeys() {
        return instance.assets.Keys.ToList();
    }
    
    private void FreeAllHandles() {
        if(handles != null) {
            foreach(var handle in handles) {
                handle.Release();
            }
            handles.Clear();
        }
    }
    
    public static async Task LoadAllAssets(string group, List<string> names) {
        instance.FreeAllHandles();
        ClearAllAssets();
        
        List<Task> tasksToComplete = new List<Task>();
        foreach (var objName in names) {
            var reagentTask = KoboldKareObjectPostProcessor.GetAssetAsync<T>(group, objName, instance.missingObject);
            async Task Consume() {
                var result = await reagentTask;
                AddAsset(objName, result.asset);
                instance.handles.Add(result);
            }
            tasksToComplete.Add(Consume());
        }

        await Task.WhenAll(tasksToComplete);
    }
}
