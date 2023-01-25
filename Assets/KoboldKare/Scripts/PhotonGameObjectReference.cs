using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PhotonGameObjectReference {
    [SerializeField]
    private GameObject gameObject;
    [SerializeField]
    private PrefabDatabase optionalDatabase;

    public string photonName {
        get {
            if (gameObject != null) {
                return gameObject.name;
            }

            if (optionalDatabase != null) {
                return optionalDatabase.GetRandom().GetKey();
            }
            
            return null;
        }
    }

    public void OnValidate() {
        //if (gameObject != null) {
            //photonName = gameObject.name;
        //}
    }
}
