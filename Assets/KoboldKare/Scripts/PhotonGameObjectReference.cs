using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[System.Serializable]
public class PhotonGameObjectReference {
    public GameObject gameObject;
    public string photonName;
    public void OnValidate() {
        if (gameObject != null) {
            photonName = gameObject.name;
        }
    }
}
