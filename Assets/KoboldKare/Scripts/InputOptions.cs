using UnityEngine;
using System.IO;
using System.Text;
using SimpleJSON;
using System;
using Steamworks;
using UnityEngine.InputSystem;

public class InputOptions : MonoBehaviour {
    [SerializeField]
    private InputActionAsset controls;
    private static InputOptions instance;
    private static string runningPlatform {
        get {
            switch (Application.platform) {
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxServer:
                case RuntimePlatform.LinuxEditor:
                    return "StandaloneLinux64";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsServer:
                    return IntPtr.Size == 8 ? "StandaloneWindows64" : "StandaloneWindows";
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXServer:
                    return "StandaloneOSX";
                default: return "Unknown";
            }
        }
    }

    private string savePath {
        get {
            var path = $"{Application.persistentDataPath}/defaultUser/inputBindings_{runningPlatform}.json";
            if (SteamManager.Initialized) {
                path = $"{Application.persistentDataPath}/{SteamUser.GetSteamID().ToString()}/inputBindings_{runningPlatform}.json";
            }
            return path;
        }
    }

    public static void SaveControls() {
        instance.Save();
    }

    private void Save() {
        FileStream file = File.Create(savePath);
        //string json = JsonUtility.ToJson(overrides, true);
        JSONNode n = JSON.Parse("{}");
        foreach (var map in controls.actionMaps) {
            foreach (var binding in map.bindings) {
                if (!string.IsNullOrEmpty(binding.overridePath)) {
                    n[binding.id.ToString()] = binding.overridePath;
                }
            }
        }
        file.Write(Encoding.UTF8.GetBytes(n.ToString(2)),0,n.ToString(2).Length);
        file.Close();
        Debug.Log("Saved input bindings to " + savePath);
    }
    private void Load() {
        try {
            if (NeedsUpgrade()) {
                PerformUpgrade();
            }
            FileStream file = File.Open(savePath, FileMode.Open);
            byte[] b = new byte[file.Length];
            file.Read(b,0,(int)file.Length);
            file.Close();

            string data = Encoding.UTF8.GetString(b);
            JSONNode n = JSON.Parse(data);
            
            foreach (var map in controls.actionMaps) {
                var bindings = map.bindings;
                for (var i = 0; i < bindings.Count; ++i) {
                    if (n.HasKey(bindings[i].id.ToString())) {
                        map.ApplyBindingOverride(i, new InputBinding { overridePath = n[bindings[i].id.ToString()] });
                    }
                }
            }
        } catch (Exception e) {
            if (e is FileNotFoundException) {
                return;
            }
            Debug.LogException(e);
        }
    }
    
    private bool NeedsUpgrade() {
        var oldPath = $"{Application.persistentDataPath}/inputBindings.json";
        return File.Exists(oldPath);
    }

    private void PerformUpgrade() {
        if (!NeedsUpgrade()) {
            return;
        }
        var oldPath = $"{Application.persistentDataPath}/inputBindings.json";
        File.Move(oldPath, savePath);
    }

    private void Start() {
        //Check if instance already exists
        if (instance == null) {
            //if not, set instance to this
            instance = this;
        } else if (instance != this) {
            //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }

        Load();
    }
}