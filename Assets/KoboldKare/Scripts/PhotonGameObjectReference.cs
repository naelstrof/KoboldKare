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
                var prefabs = optionalDatabase.GetPrefabReferenceInfos();
                float count = 0f;
                foreach (var prefab in prefabs) {
                    if (!prefab.GetEnabled()) {
                        continue;
                    }
                    count += 1f;
                }

                float current = 0f;
                float target = UnityEngine.Random.Range(0f, count);
                foreach (var prefab in prefabs) {
                    if (!prefab.GetEnabled()) {
                        continue;
                    }

                    current += 1f;
                    if (current >= target) {
                        return prefab.GetKey();
                    }
                }
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
