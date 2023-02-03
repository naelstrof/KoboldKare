using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;

public class ModInfo {
    public bool enabled;
    public string modName;
    public string cataloguePath;
    public IResourceLocator locator;

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
}
