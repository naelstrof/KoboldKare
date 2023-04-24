using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericPool<T> : MonoBehaviour where T : PooledItem {
    protected static GenericPool<T> instance;
    [SerializeField][Range(0,1000)]
    protected int poolSize = 10;
    public GameObject prefab;
    protected Queue<T> prefabSet;
    protected List<T> allPrefabs;
    public static int GetPoolSize() => instance.poolSize;
    void Awake() {
        prefabSet = new Queue<T>();
        allPrefabs = new List<T>();

        instance = this;
        for(int i=0;i<poolSize;i++) {
            allPrefabs.Add(PrepareItem());
        }
    }
    protected virtual T PrepareItem() {
        T thing = Instantiate(prefab).GetComponent<T>();
        DontDestroyOnLoad(thing.gameObject);
        thing.resetTrigger += ()=>{prefabSet.Enqueue(thing);};
        thing.Reset();
        thing.gameObject.SetActive(false);
        return thing;
    }
    public bool TryInstantiate(out T thing) {
        if (prefabSet.Count > 0) {
            thing = prefabSet.Dequeue();
            thing.gameObject.SetActive(true);
            return true;
        }
        thing = null;
        return false;
    }

    public static bool StaticTryInstantiate(out T thing) {
        if (instance.prefabSet.Count > 0) {
            thing = instance.prefabSet.Dequeue();
            thing.gameObject.SetActive(true);
            return true;
        }
        thing = null;
        return false;
    }
}
