using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

public class PreparePool : MonoBehaviour {
    private static PreparePool instance;
    [FormerlySerializedAs("Prefabs")] [SerializeField]
    private List<GameObject> builtInPrefabs;
    
    private Dictionary<string,GameObject> dynamicPrefabs;
    private DefaultPool pool;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this);
        } else {
            instance = this;
        }
    }

    void Start () {
        if (PhotonNetwork.PrefabPool is DefaultPool checkPool) {
            pool = checkPool;
        } else {
            throw new UnityException("Unexpected Photon pool type.");
        }
        dynamicPrefabs = new Dictionary<string,GameObject>();

        foreach (GameObject prefab in builtInPrefabs) {
            pool.ResourceCache.Add(prefab.name, prefab);
        }
    }

    public static void AddPrefab(string assetName, GameObject prefab) => instance.InternalAddPrefab(assetName, prefab);
    public static void RemovePrefab(string assetName) => instance.InternalRemovePrefab(assetName);
    
    private void InternalAddPrefab(string assetName, GameObject prefab) {
        if (pool.ResourceCache.ContainsKey(prefab.name)) {
            throw new UnityException($"Failed to add {assetName} to Photon Prefab pool, its already there!");
        }
        dynamicPrefabs.Add(assetName, prefab);
        pool.ResourceCache.Add(prefab.name, prefab);
    }

    private void InternalRemovePrefab(string assetName) {
        if (!dynamicPrefabs.ContainsKey(assetName)) return;
        
        dynamicPrefabs.Remove(assetName);
        if (pool.ResourceCache.ContainsKey(dynamicPrefabs[assetName].name)) {
            pool.ResourceCache.Remove(dynamicPrefabs[assetName].name);
        }
    }

}