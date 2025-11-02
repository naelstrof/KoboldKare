using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;

public class PreparePool : MonoBehaviour {
    private static PreparePool instance;

    private struct GameObjectWithStubPair {
        public GameObject obj;
        public ModManager.ModStub? stub;
    }
    
    private Dictionary<string,List<GameObjectWithStubPair>> dynamicPrefabs = new();
    private DefaultPool pool => (DefaultPool)PhotonNetwork.PrefabPool;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
        } else {
            instance = this;
        }
    }

    public static void AddPrefab(string assetName, GameObject prefab, ModManager.ModStub? stub) => instance.InternalAddPrefab(assetName, prefab, stub);
    public static void RemovePrefab(string assetName, ModManager.ModStub? stub) => instance.InternalRemovePrefab(assetName, stub);
    public static bool HasPrefab(string assetName) => instance.dynamicPrefabs.ContainsKey(assetName);
    
    private void InternalAddPrefab(string assetName, GameObject prefab, ModManager.ModStub? stub) {
        pool.ResourceCache.Remove(assetName);
        if (!dynamicPrefabs.ContainsKey(assetName)) {
            dynamicPrefabs.Add(assetName, new List<GameObjectWithStubPair>());
        }
        var list = dynamicPrefabs[assetName];
        list.Add(new GameObjectWithStubPair() {
            obj = prefab,
            stub = stub
        });
        list.Sort(CompareModdedPrefab);
        pool.ResourceCache.Add(assetName, list[^1].obj);
    }

    private int CompareModdedPrefab(GameObjectWithStubPair a, GameObjectWithStubPair b) {
        if (a.stub == null && b.stub == null) return 0;
        if (a.stub == null) return -1;
        if (b.stub == null) return 1;
        if (a.stub.Value.loadPriority == b.stub.Value.loadPriority) {
            return String.Compare(a.stub.Value.title, b.stub.Value.title, StringComparison.InvariantCulture);
        }

        return a.stub.Value.loadPriority.CompareTo(b.stub.Value.loadPriority);
    }

    private bool CheckStubMatch(ModManager.ModStub? a, ModManager.ModStub? b) {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.Value.GetRepresentedBy(b.Value);
    }

    private void InternalRemovePrefab(string assetName, ModManager.ModStub? stub) {
        if (!dynamicPrefabs.TryGetValue(assetName, out var prefabList)) return;
        for (int i = 0; i < prefabList.Count; i++) {
            var stubCheck = prefabList[i].stub;
            if (CheckStubMatch(stubCheck, stub)) {
                prefabList.RemoveAt(i);
                i--;
            }
        }
        if (prefabList.Count == 0) {
            dynamicPrefabs.Remove(assetName);
            pool.ResourceCache.Remove(assetName);
        } else {
            prefabList.Sort(CompareModdedPrefab);
            pool.ResourceCache.Remove(assetName);
            pool.ResourceCache.Add(assetName, prefabList[^1].obj);
        }
    }

}