using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using PenetrationTech;
using SimpleJSON;
using UnityEngine;

[CreateAssetMenu(fileName = "New Prefab Database", menuName = "Data/Prefab Database", order = 1)]
public class PrefabDatabase : ScriptableObject {
    private List<PrefabReferenceInfo> prefabReferenceInfos;
    private ReadOnlyCollection<PrefabReferenceInfo> readOnlyPrefabReferenceInfos;

    public delegate void PrefabReferencesChangedAction(ReadOnlyCollection<PrefabReferenceInfo> prefabReferenceInfos);

    private event PrefabReferencesChangedAction prefabReferencesChanged;
    private List<PrefabReferenceInfo> validInfos;
    
    private const string JSONLocation = "modConfiguration.json";
    [System.Serializable]
    public class PrefabReferenceInfo {
        private bool enabled;
        private string key;
        private GameObject prefab;
        private readonly PrefabDatabase parentDatabase;
        public PrefabReferenceInfo(PrefabDatabase database, string primaryKey, GameObject newPrefab, bool enabled = true) {
            this.enabled = enabled;
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
            return enabled && prefab != null;
        }

        public void SetEnabled(bool newValue) {
            enabled = newValue;
            parentDatabase.prefabReferencesChanged?.Invoke(parentDatabase.GetPrefabReferenceInfos());
        }
        //public bool GetEnabled() {
            //return enabled;
        //}
        public void Save(JSONNode n) {
            n["enabled"] = enabled;
            n["key"] = key;
        }
        public void Load(JSONNode n) {
            enabled = n["enabled"];
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
        prefabReferenceInfos = new List<PrefabReferenceInfo>();
        readOnlyPrefabReferenceInfos = prefabReferenceInfos.AsReadOnly();
        LoadPlayerConfiguration();
    }

    private void SavePlayerConfiguration() {
        string jsonLocation = $"{Application.persistentDataPath}/{JSONLocation}";
        if (!File.Exists(jsonLocation)) {
            using FileStream quickWrite = File.Open(jsonLocation, FileMode.CreateNew);
            byte[] write = { (byte)'{', (byte)'}', (byte)'\n' };
            quickWrite.Write(write, 0, write.Length);
            quickWrite.Close();
        }
        using FileStream file = File.Open(jsonLocation, FileMode.Open);
        byte[] b = new byte[file.Length];
        file.Read(b,0,(int)file.Length);
        file.Close();
        string data = Encoding.UTF8.GetString(b);
        JSONNode n = JSON.Parse(data);
        var rootNode = new JSONArray();
        foreach (var info in prefabReferenceInfos) {
            JSONNode node = JSONNode.Parse("{}");
            info.Save(node);
            rootNode.Add(node);
        }
        n[name] = rootNode;
        using FileStream fileWrite = File.Create(jsonLocation);
        fileWrite.Write(Encoding.UTF8.GetBytes(n.ToString(2)),0,n.ToString(2).Length);
        fileWrite.Close();
    }

    private void LoadPlayerConfiguration() {
        string jsonLocation = $"{Application.persistentDataPath}/{JSONLocation}";
        if (!File.Exists(jsonLocation)) {
            using FileStream quickWrite = File.Open(jsonLocation, FileMode.CreateNew);
            byte[] write = { (byte)'{', (byte)'}', (byte)'\n' };
            quickWrite.Write(write, 0, write.Length);
            quickWrite.Close();
        }

        using FileStream file = File.Open(jsonLocation, FileMode.Open);
        byte[] b = new byte[file.Length];
        file.Read(b,0,(int)file.Length);
        file.Close();
        string data = Encoding.UTF8.GetString(b);
        JSONNode n = JSON.Parse(data);
        foreach (var node in n[name]) {
            PrefabReferenceInfo info = new PrefabReferenceInfo(this, "", null);
            info.Load(n);
            AddPrefab(info);
        }
    }

    public void AddPrefab(PrefabReferenceInfo newInfo) {
        foreach (var info in prefabReferenceInfos) {
            if (info.GetKey() == newInfo.GetKey()) {
                return;
            }
        }
        prefabReferenceInfos.Add(newInfo);
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
    }

    public void RemovePrefab(string key) {
        for(int i=0;i<prefabReferenceInfos.Count;i++) {
            if (prefabReferenceInfos[i].GetKey() == key) {
                prefabReferenceInfos.RemoveAt(i);
            }
        }
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
