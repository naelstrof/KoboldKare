using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(OcclusionArea))]
public class CloudPool : GenericPool<Cloud> {
    private Bounds cloudBounds;
    private OcclusionArea area;
    private List<Cloud> spawnedClouds;
    
    void Start() {
        area = GetComponent<OcclusionArea>();
        cloudBounds = new Bounds(area.transform.TransformPoint(area.center) + new Vector3(0f,area.size.y*0.25f, 0f), new Vector3(area.size.x, area.size.y*0.25f, area.size.z));
        spawnedClouds = new List<Cloud>();
        for (int i = 0; i < GetPoolSize(); i++) {
            if (!TryInstantiate(out Cloud cloud)) continue;
            cloud.SetBounds(cloudBounds);
            cloud.transform.position = new Vector3(
                UnityEngine.Random.Range(cloudBounds.center.x - cloudBounds.extents.x,
                    cloudBounds.center.x + cloudBounds.extents.x),
                UnityEngine.Random.Range(cloudBounds.center.y - cloudBounds.extents.y,
                    cloudBounds.center.y + cloudBounds.extents.y),
                UnityEngine.Random.Range(cloudBounds.center.z - cloudBounds.extents.z,
                    cloudBounds.center.z + cloudBounds.extents.z));
            spawnedClouds.Add(cloud);
        }
    }
    protected override Cloud PrepareItem() {
        Cloud thing = Instantiate(prefab).GetComponent<Cloud>();
        thing.resetTrigger += ()=>{prefabSet.Enqueue(thing);};
        thing.Reset();
        thing.gameObject.SetActive(false);
        return thing;
    }

    private void OnDestroy() {
        foreach(var thing in allPrefabs) {
            if (thing != null) {
                Destroy(thing.gameObject);
            }
        }
    }
#if UNITY_EDITOR
    private void OnValidate() {
        if (prefab == null) {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                AssetDatabase.GUIDToAssetPath("eecff7b2cef7a554fa2e0ce739b133a3"));
        }
    }

#endif
}
