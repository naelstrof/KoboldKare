using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CloudPool))]
[RequireComponent(typeof(OcclusionArea))]
public class CloudSpawner : MonoBehaviour {
    private CloudPool pool;
    private Bounds cloudBounds;
    private OcclusionArea area;

    void Start() {
        area = GetComponent<OcclusionArea>();
        cloudBounds = new Bounds(area.transform.TransformPoint(area.center) + new Vector3(0f,area.size.y*0.25f, 0f), new Vector3(area.size.x, area.size.y*0.25f, area.size.z));
        pool = GetComponent<CloudPool>();
        for (int i = 0; i < pool.GetPoolSize(); i++) {
            if (pool.TryInstantiate(out Cloud cloud)) {
                cloud.SetBounds(cloudBounds);
                cloud.transform.position = new Vector3(
                    UnityEngine.Random.Range(cloudBounds.center.x - cloudBounds.extents.x,
                        cloudBounds.center.x + cloudBounds.extents.x),
                    UnityEngine.Random.Range(cloudBounds.center.y - cloudBounds.extents.y,
                        cloudBounds.center.y + cloudBounds.extents.y),
                    UnityEngine.Random.Range(cloudBounds.center.z - cloudBounds.extents.z,
                        cloudBounds.center.z + cloudBounds.extents.z));
                cloud.resetTrigger -= OnCloudRemove;
                cloud.resetTrigger += OnCloudRemove;
            }
        }
    }

    void OnCloudRemove() {
        if (pool.TryInstantiate(out Cloud cloud)) {
            cloud.resetTrigger -= OnCloudRemove;
            cloud.resetTrigger += OnCloudRemove;
        }
    }
}
