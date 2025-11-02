using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using PenetrationTech;
using SimpleJSON;
using Steamworks;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Prefab Database", menuName = "Data/Prefab Database", order = 1)]
public class PrefabDatabase : ScriptableObject {
    private class SortedDictionaryComparer : IComparer<string> {
        public int Compare(string x, string y) {
            return String.Compare(x, y, StringComparison.InvariantCulture);
        }
    }
    
    [NonSerialized]
    private SortedDictionary<string,List<PrefabReferenceInfo>> prefabReferenceInfos = new(new SortedDictionaryComparer());

    public delegate void PrefabReferencesChangedAction(ReadOnlyCollection<PrefabReferenceInfo> prefabReferenceInfos);

    private event PrefabReferencesChangedAction prefabReferencesChanged;
    private List<PrefabReferenceInfo> validInfos;

    [System.Serializable]
    public class PrefabReferenceInfo {
        private string key;
        private GameObject prefab;
        private readonly PrefabDatabase parentDatabase;
        private ModManager.ModStub? stub;
        public PrefabReferenceInfo(PrefabDatabase database, string primaryKey, GameObject newPrefab, ModManager.ModStub? stub) {
            key = primaryKey;
            this.stub = stub;
            prefab = newPrefab;
            parentDatabase = database;
        }

        public string GetKey() {
            return key;
        }

        public int CompareTo(PrefabReferenceInfo y) {
            if (stub == null && y.stub == null) return 0;
            if (stub == null) return -1;
            if (y.stub == null) return 1;
            if (stub.Value.loadPriority == y.stub.Value.loadPriority) {
                return String.Compare(stub.Value.title, y.stub.Value.title, StringComparison.InvariantCulture);
            }
            return stub.Value.loadPriority.CompareTo(y.stub.Value.loadPriority);
        }

        public bool GetRepresentedByStub(ModManager.ModStub? b) {
            if (b == null && stub == null) {
                return true;
            }
            return stub != null && b != null && stub.Value.GetRepresentedBy(b.Value);
        }
        public void SetPrefab(GameObject newPrefab) {
            prefab = newPrefab;
        }

        public GameObject GetPrefab() {
            return prefab;
        }
        public bool IsValid() {
            return prefab;
        }
        public void Save(JSONNode n) {
            n["key"] = key;
        }
        public void Load(JSONNode n) {
            key = n["key"];
        }
    }

    public bool TryGetRandom(out PrefabReferenceInfo info) {
#if UNITY_EDITOR
        if (!ModManager.GetReady()) {
            info = null;
            return false;
        }
#endif
        float count = 0f;
        foreach(var pair in prefabReferenceInfos) {
            if (pair.Value[^1].IsValid()) {
                count++;
            }
        }
        float selection = Random.Range(0f,count);
        float current = 0f;
        foreach(var pair in prefabReferenceInfos) {
            if (!pair.Value[^1].IsValid()) continue;
            current++;
            if (current >= selection) {
                info = pair.Value[^1];
                return true;
            }
        }

        info = null;
        return false;
    }

    public void AddPrefabReferencesChangedListener(PrefabReferencesChangedAction action) {
        prefabReferencesChanged += action;
    }
    public void RemovePrefabReferencesChangedListener(PrefabReferencesChangedAction action) {
        prefabReferencesChanged -= action;
    }
    
    public void AddPrefab(string newKey, GameObject prefabReference, ModManager.ModStub? stub) {
        if (!prefabReferenceInfos.ContainsKey(newKey)) {
            prefabReferenceInfos.Add(newKey, new List<PrefabReferenceInfo>());
        }
        var list = prefabReferenceInfos[newKey];
        list.Add(new PrefabReferenceInfo(this, newKey, prefabReference, stub));
        list.Sort(ComparePrefabReferenceInfo);
        prefabReferencesChanged?.Invoke(GetPrefabReferenceInfos());
    }

    private int ComparePrefabReferenceInfo(PrefabReferenceInfo a, PrefabReferenceInfo b) {
        return a.CompareTo(b);
    }

    public void RemovePrefab(string key, ModManager.ModStub? stub) {
        if (!prefabReferenceInfos.TryGetValue(key, out var list)) {
            return;
        } else {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].GetRepresentedByStub(stub)) {
                    list.RemoveAt(i);
                    i--;
                }
            }
            if (list.Count == 0) {
                prefabReferenceInfos.Remove(key);
            }
            list.Sort(ComparePrefabReferenceInfo);
            prefabReferencesChanged?.Invoke(GetPrefabReferenceInfos());
        }
    }

    public ReadOnlyCollection<PrefabReferenceInfo> GetPrefabReferenceInfos() {
        List<PrefabReferenceInfo> infos = new List<PrefabReferenceInfo>();
        foreach(var info in prefabReferenceInfos) {
            if (info.Value[^1].IsValid()) {
                infos.Add(info.Value[^1]);
            }
        }
        return infos.AsReadOnly();
    }

    public List<PrefabReferenceInfo> GetValidPrefabReferenceInfos() {
        validInfos ??= new List<PrefabReferenceInfo>();
        validInfos.Clear();
        foreach(var info in prefabReferenceInfos) {
            if (info.Value[^1].IsValid()) {
                validInfos.Add(info.Value[^1]);
            }
        }
        return validInfos;
    }

    public PrefabReferenceInfo GetInfoByName(string key) {
        if (prefabReferenceInfos.TryGetValue(key, out var result)) {
            return result[^1];
        }
        return null;
    }
}
