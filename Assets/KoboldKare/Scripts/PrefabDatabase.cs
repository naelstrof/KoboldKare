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
    
    private const string JsonLocation = "modConfiguration.json";

    private static string jsonFolder {
        get {
            var path = $"{Application.persistentDataPath}/defaultUser/";
            if (SteamManager.Initialized) {
                path = $"{Application.persistentDataPath}/{SteamUser.GetSteamID().ToString()}/";
            }
            return path;
        }
    }

    private static string jsonLocation {
        get {
            var path = $"{jsonFolder}/{JsonLocation}";
            if (SteamManager.Initialized) {
                path = $"{jsonFolder}/{JsonLocation}";
            }
            return path;
        }
    }

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

        public bool GetEnabled() {
            return enabled;
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
        readOnlyPrefabReferenceInfos = prefabReferenceInfos.AsReadOnly();
    }

    public string SavePlayerConfiguration() {
        JSONNode n = GetJsonConfiguration();
        var rootNode = new JSONArray();
        foreach (var info in prefabReferenceInfos) {
            if (info.GetEnabled()) {
                continue;
            }
            JSONNode node = JSONNode.Parse("{}");
            info.Save(node);
            rootNode.Add(node);
        }

        if (n.HasKey(name)) {
            n.Remove(name);
        }
        n[name] = rootNode;
        using FileStream fileWrite = File.Open(jsonLocation, FileMode.Truncate);
        string writeString = n.ToString(2);
        fileWrite.Write(Encoding.UTF8.GetBytes(writeString),0,writeString.Length);
        fileWrite.Close();
        return writeString;
    }

    public static JSONNode GetJsonConfiguration() {
        if (!Directory.Exists(jsonFolder)) {
            Directory.CreateDirectory(jsonFolder);
        }
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
        JSONNode n;
        try {
            n = JSON.Parse(Encoding.UTF8.GetString(b));
        } catch {
            n = JSON.Parse("{}");
        }
        return n;
    }

    public void LoadPlayerConfiguration(string json) {
        JSONNode n = JSON.Parse(json);
        foreach (var node in n[name]) {
            if (node.Value.IsNull) {
                continue;
            }
            PrefabReferenceInfo info = new PrefabReferenceInfo(this, "", null);
            info.Load(node);
            AddPrefab(info);
        }
    }
    public void LoadPlayerConfiguration() {
        JSONNode n = GetJsonConfiguration();
        foreach (var search in prefabReferenceInfos) {
            search.SetEnabled(true);
        }
        
        if (!n.HasKey(name)) {
            prefabReferencesChanged?.Invoke(GetPrefabReferenceInfos());
            return;
        }

        foreach (var node in n[name]) {
            if (node.Value.IsNull) {
                continue;
            }
            bool foundInfo = false;
            PrefabReferenceInfo info = new PrefabReferenceInfo(this, "", null);
            info.Load(node);
            foreach (var search in prefabReferenceInfos) {
                if (search.GetKey() != info.GetKey()) continue;
                search.SetEnabled(info.GetEnabled());
                foundInfo = true;
                break;
            }

            if (!foundInfo) {
                AddPrefab(info);
            }
        }
        prefabReferencesChanged?.Invoke(GetPrefabReferenceInfos());
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
        foreach (var t in prefabReferenceInfos) {
            if (t.GetKey() == key) {
                t.SetPrefab(null);
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
