using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;

public class PreparePool : MonoBehaviour {
    private static PreparePool instance;
    [FormerlySerializedAs("Prefabs")] [SerializeField]
    private List<GameObject> builtInPrefabs;
    
    private Dictionary<string,GameObject> dynamicPrefabs = new Dictionary<string, GameObject>();
    private DefaultPool pool => (DefaultPool)PhotonNetwork.PrefabPool;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
        } else {
            instance = this;
            foreach (GameObject prefab in builtInPrefabs) {
                pool.ResourceCache.Add(prefab.name, prefab);
            }
        }
    }

    public static void AddPrefab(string assetName, GameObject prefab) => instance.InternalAddPrefab(assetName, prefab);
    public static void RemovePrefab(string assetName) => instance.InternalRemovePrefab(assetName);
    public static bool HasPrefab(string assetName) => instance.dynamicPrefabs.ContainsKey(assetName);
    
    private void InternalAddPrefab(string assetName, GameObject prefab) {
        if (pool.ResourceCache.ContainsKey(prefab.name)) {
            throw new UnityException($"Failed to add {assetName} to Photon Prefab pool, its already there!");
        }
        dynamicPrefabs.Add(assetName, prefab);
        pool.ResourceCache.Add(prefab.name, prefab);
    }

    private void InternalRemovePrefab(string assetName) {
        if (!dynamicPrefabs.TryGetValue(assetName, out var prefab)) return;
        pool.ResourceCache.Remove(assetName);
        dynamicPrefabs.Remove(assetName);
    }

}