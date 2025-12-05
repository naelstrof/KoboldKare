using System;
using System.Collections.Generic;
using UnityEngine;

public class Database<T> : MonoBehaviour where T : UnityEngine.Object {
    protected static Database<T> instance;

    public struct ObjectStubPair {
        public object key;
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
    
    public struct AssetKeyPair {
        public object key;
        public List<ObjectStubPair> value;
    }
    
    protected internal List<AssetKeyPair> assets = new();

    private bool ContainsKey(object key) {
        foreach (var pair in assets) {
            if (pair.key == key) {
                return true;
            }
        }

        return false;
    }
    
    private bool TryGetList(object key, out List<ObjectStubPair> list) {
        foreach (var pair in assets) {
            if (pair.key == key) {
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
    public static bool TryGetAsset(object key, out object matchKey, out ModManager.ModStub? matchStub) {
        for (int i = 0; i < instance.assets.Count; i++) {
            if (instance.assets[i].key == key) {
                matchKey = instance.assets[i].value[^1].key;
                matchStub = instance.assets[i].value[^1].stub;
                return true;
            }
        }
        
        if (instance.assets.Count > 0) {
            matchKey = instance.assets[0].value[^1].key;
            matchStub = instance.assets[0].value[^1].stub;
        } else {
            matchKey = null;
            matchStub = null;
        }
        return false;
    }
    
    public static bool TryGetAsset(short id, out object matchKey, out ModManager.ModStub? matchStub) {
        if (id < 0 || id >= instance.assets.Count) {
            if (instance.assets.Count > 0) {
                matchKey = instance.assets[0].value[^1].key;
                matchStub = instance.assets[0].value[^1].stub;
            } else {
                matchKey = null;
                matchStub = null;
            }
            return false;
        }
        matchKey = instance.assets[id].value[^1].key;
        matchStub = instance.assets[id].value[^1].stub;
        return true;
    }

    public static void AddAsset(object key, ModManager.ModStub? stub) {
        if (!instance.ContainsKey(key)) {
            instance.assets.Add(new AssetKeyPair() {
                key = key,
                value = new List<ObjectStubPair>()
            });
        }

        if (instance.TryGetList(key, out var list)) {
            list.Add(new ObjectStubPair() {
                key = key,
                stub = stub
            });
            list.Sort(CompareObjectStubPair);
        }
    }
    
    public static void RemoveAsset(object key, ModManager.ModStub? stub) {
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
    public static List<object> GetAssets() {
        List<object> assets = new();
        foreach (var pair in instance.assets) {
            assets.Add(pair.value[^1].key);
        }
        return assets;
    }
}
