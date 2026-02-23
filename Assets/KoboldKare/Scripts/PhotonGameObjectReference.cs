using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PhotonGameObjectReference {
    [SerializeField, HideInInspector]
    private GameObject gameObject;

    [SerializeField] private string assetGroup;
    [SerializeField] private string assetName;
    
    [SerializeField] private PrefabDatabase optionalDatabase;

    public GameObject GetGameObject() => gameObject;

    public bool TryGetAssetGroupAndKey(out string group, out string key) {
        if (optionalDatabase != null) {
            if (optionalDatabase.TryGetGroupName(out group)) {
                if (KoboldKareObjectPostProcessor.TryGetRandomAssetKey(group, out key)) {
                    return true;
                }
            }
        }
        group = assetGroup;
        key = assetName;
        return !string.IsNullOrEmpty(group) && !string.IsNullOrEmpty(key);
    }
}
