using System;
using SimpleJSON;

[Serializable]
public class PrefabReferenceInfo {
    private string key;
    private string groupName;
    public PrefabReferenceInfo(string groupName, string primaryKey) {
        this.groupName = groupName;
        key = primaryKey;
    }

    public string GetKey() {
        return key;
    }

    public string GetGroupName() {
        if (string.IsNullOrEmpty(groupName)) {
            return "Prefabs";
        }
        return groupName;
    }

    public void SetPrefab(string groupName, string key) {
        this.groupName = groupName;
        this.key = key;
    }
    public bool IsValid() {
        return KoboldKareObjectPostProcessor.HasAssetInGroup(GetGroupName(), GetKey());
    }
    public void Save(JSONNode n) {
        n["key"] = key;
        n["groupName"] = groupName;
    }
    public void Load(JSONNode n) {
        key = n["key"];
        if (n.HasKey("groupName")) {
            groupName = n["groupName"];
        }
    }
}
