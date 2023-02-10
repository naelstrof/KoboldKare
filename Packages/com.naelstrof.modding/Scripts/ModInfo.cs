using System.Collections;
using System.Collections.Generic;
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

    public ModInfo(string name, string filePath, ModSource source) {
        enabled = false;
        modName = name;
        cataloguePath = filePath;
        modSource = source;
    }

    public bool enabled;
    public string modName;
    public string cataloguePath;
    public IResourceLocator locator;
    public ModSource modSource;

    public void Save(JSONNode node) {
        node["enabled"] = enabled;
        node["modName"] = modName;
        node["cataloguePath"] = cataloguePath;
    }

    public void Load(JSONNode node) {
        enabled = node["enabled"];
        modName = node["modName"];
        cataloguePath = node["cataloguePath"];
    }

    public void GetSource() {
    }
}
