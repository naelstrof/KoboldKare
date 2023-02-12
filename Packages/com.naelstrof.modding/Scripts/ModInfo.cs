using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;

public class ModInfo {
    public enum ModSource {
        LocalModFolder,
        SteamWorkshop,
    }

    public ModInfo(JSONNode node) {
        Load(node);
    }
    

    public ModInfo(string modPath, ModSource source) {
        enabled = false;
        this.modPath = modPath;
        modSource = source;
        LoadMetaData($"{modPath}{Path.DirectorySeparatorChar}info.json");
        LoadPreview($"{modPath}{Path.DirectorySeparatorChar}preview.png");
    }

    public bool enabled;
    public string title;
    private ulong publishedFileId;
    public string description;
    public string modPath;
    public Texture2D preview;

    public string cataloguePath {
        get {
            string searchDir = $"{modPath}{Path.DirectorySeparatorChar}{ModManager.runningPlatform}";
            foreach (var file in Directory.EnumerateFiles(searchDir)) {
                if (file.EndsWith(".json")) {
                    return file;
                }
            }
            return null;
        }
    }

    public IResourceLocator locator;
    public ModSource modSource;

    private void LoadMetaData(string jsonPath) {
		using FileStream file = new FileStream(jsonPath, FileMode.Open, FileAccess.Read);
        using StreamReader reader = new StreamReader(file);
        var rootNode = JSONNode.Parse(reader.ReadToEnd());
        if (rootNode.HasKey("publishedFileId")) {
	        if (ulong.TryParse(rootNode["publishedFileId"], out ulong output)) {
                publishedFileId = output;
	        }
        }
        if (rootNode.HasKey("description")) {
	        description =rootNode["description"];
        }
        
        if (rootNode.HasKey("title")) {
            title = rootNode["title"];
        }
    }

    private void LoadPreview(string previewPngPath) {
        preview = new Texture2D(16, 16);
        preview.LoadImage(File.ReadAllBytes(previewPngPath));
    }

    public void Save(JSONNode node) {
        node["enabled"] = enabled;
        node["modName"] = title;
        node["modPath"] = modPath;
    }

    public void Load(JSONNode node) {
        enabled = node["enabled"];
        modPath = node["modPath"];
        LoadMetaData($"{modPath}{Path.DirectorySeparatorChar}info.json");
        LoadPreview($"{modPath}{Path.DirectorySeparatorChar}preview.png");
    }
}
