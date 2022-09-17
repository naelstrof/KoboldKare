using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardObject : MonoBehaviour {
    private Coroutine routine;
    private int nextUpdate;
    private const float fadeDistance = 10f;
    void OnEnable() {
        routine = StartCoroutine(UpdateRoutine());
    }
    void OnDisable() {
        if (routine != null) {
            StopCoroutine(routine);
        }
    }
    IEnumerator UpdateRoutine() {
        while(isActiveAndEnabled) {
            // Every few frames we update
            for(int i=0;i<nextUpdate;i++) {
                yield return null;
            }
            Camera check = Camera.main;
            // Skip if camera is null
            if (check == null) {
                nextUpdate = 64;
                continue;
            }
            float distance = transform.DistanceTo(check.transform);
            Vector3 diff = check.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-diff.normalized, Vector3.up);
            nextUpdate = distance < fadeDistance+1f ? 1 : 64;
        }
    }
}
