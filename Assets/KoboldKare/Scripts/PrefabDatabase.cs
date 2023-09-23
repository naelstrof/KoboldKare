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
    [NonSerialized]
    private List<PrefabReferenceInfo> prefabReferenceInfos = new List<PrefabReferenceInfo>();
    [NonSerialized]
    private ReadOnlyCollection<PrefabReferenceInfo> readOnlyPrefabReferenceInfos;

    public delegate void PrefabReferencesChangedAction(ReadOnlyCollection<PrefabReferenceInfo> prefabReferenceInfos);

    private event PrefabReferencesChangedAction prefabReferencesChanged;
    private List<PrefabReferenceInfo> validInfos;

    [System.Serializable]
    public class PrefabReferenceInfo {
        private string key;
        private GameObject prefab;
        private readonly PrefabDatabase parentDatabase;
        public PrefabReferenceInfo(PrefabDatabase database, string primaryKey, GameObject newPrefab) {
            key = primaryKey;
            prefab = newPrefab;
            parentDatabase = database;
        }

        public string GetKey() {
            return key;
        }
        public void SetPrefab(GameObject newPrefab) {
            prefab = newPrefab;
        }

        public GameObject GetPrefab() {
            return prefab;
        }
        public bool IsValid() {
            return prefab != null;
        }
        public void Save(JSONNode n) {
            n["key"] = key;
        }
        public void Load(JSONNode n) {
            key =n["key"];
        }
    }

    public PrefabReferenceInfo GetRandom() {
        float count = 0f;
        foreach(var prefab in prefabReferenceInfos) {
            if (prefab.IsValid()) {
                count++;
            }
        }
        float selection = Random.Range(0f,count);
        float current = 0f;
        foreach(var prefab in prefabReferenceInfos) {
            if (!prefab.IsValid()) continue;
            current++;
            if (current >= selection) {
                return prefab;
            }
        }

        #if UNITY_EDITOR
        // Return an empty object to avoid errors in scene play mode.
        if (!ModManager.GetReady()) {
            GameObject temp = new GameObject("fake prefab");
            Destroy(temp,1f);
            temp.SetActive(false);
            return new PrefabReferenceInfo(this, "fake", temp);
        }
#endif
        return null;
    }

    public void AddPrefabReferencesChangedListener(PrefabReferencesChangedAction action) {
        prefabReferencesChanged += action;
    }
    public void RemovePrefabReferencesChangedListener(PrefabReferencesChangedAction action) {
        prefabReferencesChanged -= action;
    }

    private void OnEnable() {
        readOnlyPrefabReferenceInfos = prefabReferenceInfos.AsReadOnly();
    }

    public void AddPrefab(PrefabReferenceInfo newInfo) {
        foreach (var info in prefabReferenceInfos) {
            if (info.GetKey() == newInfo.GetKey()) {
                return;
            }
        }

        prefabReferenceInfos.Add(newInfo);
        prefabReferenceInfos.Sort((a, b) => String.Compare(a.GetKey(), b.GetKey(), StringComparison.InvariantCulture));
        prefabReferencesChanged?.Invoke(GetPrefabReferenceInfos());
    }

    public void AddPrefab(string newKey, GameObject prefabReference) {
        foreach (var info in prefabReferenceInfos) {
            if (info.GetKey() == newKey) {
                info.SetPrefab(prefabReference);
                return;
            }
        }
        AddPrefab(new PrefabReferenceInfo(this, newKey, prefabReference)); 
        prefabReferenceInfos.Sort((a, b) => String.Compare(a.GetKey(), b.GetKey(), StringComparison.InvariantCulture));
        prefabReferencesChanged?.Invoke(GetPrefabReferenceInfos());
    }

    public void RemovePrefab(string key) {
        foreach (var t in prefabReferenceInfos) {
            if (t.GetKey() == key) {
                t.SetPrefab(null);
            }
        }
        prefabReferenceInfos.Sort((a, b) => String.Compare(a.GetKey(), b.GetKey(), StringComparison.InvariantCulture));
        prefabReferencesChanged?.Invoke(GetPrefabReferenceInfos());
    }
    public ReadOnlyCollection<PrefabReferenceInfo> GetPrefabReferenceInfos() => readOnlyPrefabReferenceInfos;

    public List<PrefabReferenceInfo> GetValidPrefabReferenceInfos() {
        validInfos ??= new List<PrefabReferenceInfo>();
        validInfos.Clear();
        foreach(var info in prefabReferenceInfos) {
            if (info.IsValid()) {
                validInfos.Add(info);
            }
        }
        return validInfos;
    }

    public PrefabReferenceInfo GetInfoByName(string key) {
        foreach(var info in prefabReferenceInfos) {
            if (info.IsValid() && info.GetKey() == key) {
                return info;
            }
        }
        return null;
    }
}
