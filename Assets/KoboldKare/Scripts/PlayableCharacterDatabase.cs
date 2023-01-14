using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using SimpleJSON;
using UnityEngine;

public class PlayableCharacterDatabase : MonoBehaviour {
    private static PlayableCharacterDatabase instance;
    private List<PlayableCharcterInfo> playableCharacters;
    private ReadOnlyCollection<PlayableCharcterInfo> readOnlyPlayableCharacters;
    private const string JSONLocation = "modConfiguration.json";
    public class PlayableCharcterInfo {
        public bool enabled;
        public string key;

        public void Save(JSONNode n) {
            n["enabled"] = enabled;
            n["key"] = key;
        }
        public void Load(JSONNode n) {
            enabled = n["enabled"];
            key =n["key"];
        }
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }

        instance = this;
        playableCharacters = new List<PlayableCharcterInfo>();
        readOnlyPlayableCharacters = playableCharacters.AsReadOnly();
    }
    
    private void Start() {
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
        foreach (var info in playableCharacters) {
            JSONNode node = JSONNode.Parse("{}");
            info.Save(node);
            rootNode.Add(node);
        }
        n["PlayableCharacters"] = rootNode;
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
        foreach (var node in n["PlayableCharacters"]) {
            PlayableCharcterInfo info = new PlayableCharcterInfo();
            info.Load(n);
            AddPlayableCharacter(info);
        }
    }

    public static void AddPlayableCharacter(PlayableCharcterInfo newInfo) {
        foreach (var info in instance.playableCharacters) {
            if (info.key == newInfo.key) {
                info.enabled = newInfo.enabled;
                return;
            }
        }

        instance.playableCharacters.Add(newInfo);
    }

    public static void AddPlayableCharacter(string newKey) {
        foreach (var info in instance.playableCharacters) {
            if (info.key == newKey) {
                return;
            }
        }
        AddPlayableCharacter(new PlayableCharcterInfo {
            enabled = true,
            key = newKey
        });
    }

    public static void RemovePlayableCharacter(string key) {
        for(int i=0;i<instance.playableCharacters.Count;i++) {
            if (instance.playableCharacters[i].key == key) {
                instance.playableCharacters.RemoveAt(i);
                return;
            }
        }
    }
    public static ReadOnlyCollection<PlayableCharcterInfo> GetPlayableCharacters() => instance.readOnlyPlayableCharacters;
}
