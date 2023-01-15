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
    
    private const string JSONLocation = "modConfiguration.json";
    [System.Serializable]
    public class PrefabReferenceInfo {
        private bool enabled;
        private string key;
        private readonly PrefabDatabase parentDatabase;
        public PrefabReferenceInfo(PrefabDatabase database, string primaryKey, bool enabled = true) {
            this.enabled = enabled;
            key = primaryKey;
            parentDatabase = database;
        }

        public string GetKey() {
            return key;
        }

        public void SetEnabled(bool newValue) {
            enabled = newValue;
            parentDatabase.prefabReferencesChanged?.Invoke(parentDatabase.GetPrefabReferenceInfos());
        }
        public bool GetEnabled() {
            return enabled;
        }
        public void Save(JSONNode n) {
            n["enabled"] = enabled;
            n["key"] = key;
        }
        public void Load(JSONNode n) {
            enabled = n["enabled"];
            key =n["key"];
        }
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
            PrefabReferenceInfo info = new PrefabReferenceInfo(this, "");
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

    public void AddPrefab(string newKey) {
        foreach (var info in prefabReferenceInfos) {
            if (info.GetKey() == newKey) {
                return;
            }
        }
        AddPrefab(new PrefabReferenceInfo(this, newKey)); 
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
}
