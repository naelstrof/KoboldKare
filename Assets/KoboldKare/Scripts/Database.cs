using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Database<T> : MonoBehaviour where T : UnityEngine.Object {
    protected static Database<T> instance;

    public struct ObjectStubPair {
        public T obj;
        public ModManager.ModStub? stub;
        public bool GetRepresentedByStub(ModManager.ModStub? b) {
            if (b == null && stub == null) {
                return true;
            }
            if (b == null || stub == null) {
                return false;
            }
            return stub.Value.GetRepresentedBy(b.Value);
        }
    }
    private static int CompareObjectStubPair(ObjectStubPair x, ObjectStubPair y) {
        if (x.stub == null && y.stub == null) return 0;
        if (x.stub == null) return -1;
        if (y.stub == null) return 1;
        if (x.stub.Value.loadPriority == y.stub.Value.loadPriority) {
            return String.Compare(x.stub.Value.title, y.stub.Value.title, StringComparison.InvariantCulture);
        }
        return x.stub.Value.loadPriority.CompareTo(y.stub.Value.loadPriority);
    }
    
    private class StringSorter : IComparer<string> {
        public int Compare(string x, string y) {
            return String.Compare(x, y, StringComparison.InvariantCulture);
        }
    }

    public struct AssetKeyPair {
        public string key;
        public List<ObjectStubPair> value;
    }
    
    protected internal List<AssetKeyPair> assets = new();

    private bool ContainsKey(string name) {
        foreach (var pair in assets) {
            if (pair.key == name) {
                return true;
            }
        }

        return false;
    }
    
    private bool TryGetList(string name, out List<ObjectStubPair> list) {
        foreach (var pair in assets) {
            if (pair.key == name) {
                list = pair.value;
                return true;
            }
        }

        list = null;
        return false;
    }
    public void Awake() {
        if (instance && instance != this) {
            Destroy(gameObject);
        } else {
            instance = this;
        }
    }
    public static bool TryGetAsset(string name, out T match) {
        for (int i = 0; i < instance.assets.Count; i++) {
            if (instance.assets[i].key == name) {
                match = instance.assets[i].value[^1].obj;
                return true;
            }
        }
        
        if (instance.assets.Count > 0) {
            match = instance.assets[0].value[^1].obj;
        } else {
            match = null;
        }
        return false;
    }
    
    public static bool TryGetAsset(short id, out T match) {
        if (id < 0 || id >= instance.assets.Count) {
            if (instance.assets.Count > 0) {
                match = instance.assets[0].value[^1].obj;
            } else {
                match = null;
            }
            return false;
        }
        match = instance.assets[id].value[^1].obj;
        return true;
    }

    public static void AddAsset(T newAsset, ModManager.ModStub? stub) {
        var key = newAsset.name;
        if (!instance.ContainsKey(key)) {
            instance.assets.Add(new AssetKeyPair() {
                key = key,
                value = new List<ObjectStubPair>()
            });
            instance.assets.Sort((a,b) => String.Compare(a.key, b.key, StringComparison.InvariantCulture));
        }

        if (instance.TryGetList(key, out var list)) {
            list.Add(new ObjectStubPair() {
                obj = newAsset,
                stub = stub
            });
            list.Sort(CompareObjectStubPair);
        }
    }
    
    public static void RemoveAsset(T newAsset, ModManager.ModStub? stub) {
        var key = newAsset.name;
        if (!instance.ContainsKey(key)) {
            return;
        }
        if (!instance.TryGetList(key, out var list)) {
            return;
        }

        for (int i = 0; i < list.Count; i++) {
            if (list[i].GetRepresentedByStub(stub)) {
                list.RemoveAt(i);
                i--;
            }
        }
        
        if (list.Count == 0) {
            instance.assets.RemoveAll(pair => pair.key == key);
            instance.assets.Sort((a,b) => String.Compare(a.key, b.key, StringComparison.InvariantCulture));
            return;
        }
        list.Sort(CompareObjectStubPair);
    }

    public static short GetID(T obj) {
        var key = obj.name;
        if (!instance.ContainsKey(key)) {
            return 0;
        }

        int i = 0;
        foreach (var pair in instance.assets) {
            if (pair.key == obj.name) {
                return (short)i;
            }
            i++;
        }
        return 0;
    }
    public static List<T> GetAssets() {
        List<T> assets = new();
        return assets;
    }
}
