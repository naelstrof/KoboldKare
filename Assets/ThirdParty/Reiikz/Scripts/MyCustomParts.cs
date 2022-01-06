using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MyCustomParts : MonoBehaviour
{
    public UnityScriptableSettings.ScriptableSetting[] settings;
    public List<GameObject> PunPrefabs;
    void Start ()
    {
        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        if (pool != null && this.PunPrefabs != null)
        {
            foreach (GameObject prefab in this.PunPrefabs)
            {
                pool.ResourceCache.Add(prefab.name, prefab);
            }
        }
    }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
}
