using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[System.Serializable]
public class PhotonGameObjectReference : ISerializationCallbackReceiver {
    public GameObject gameObject;
    public string filepath;
    public string photonName;
    public void OnBeforeSerialize() {
#if UNITY_EDITOR
        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/" + photonName + ".prefab");
        if (obj == null) {
            obj = AssetDatabase.LoadAssetAtPath<GameObject>(filepath);
        }
        if (obj != null) {
            gameObject = obj;
        }
#endif
    }

    // This doesn't get called automatically, classes that use this class *must* call it!!
    public void OnValidate() {
#if UNITY_EDITOR
        if (gameObject == null) {
            filepath = "Assets/Resources/Unknown.prefab";
            photonName = "Unknown";
            return;
        }
        string path = AssetDatabase.GetAssetPath(gameObject);
        if (!path.StartsWith("Assets/Resources/")) {
            Debug.LogError("Prefab " + path + " is located outside the resources folder, Photon won't be able to instantiate it!");
            filepath = "Assets/Resources/Unknown.prefab";
            photonName = "Unknown";
            return;
        }
        filepath = path;
        string filename = Path.GetFileNameWithoutExtension(path);
        photonName = filename;
#endif
    }
    public void OnAfterDeserialize() {
#if UNITY_EDITOR
        photonName = Path.GetFileNameWithoutExtension(filepath);
#endif
    }
}
