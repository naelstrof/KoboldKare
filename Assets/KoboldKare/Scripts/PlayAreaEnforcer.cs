using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayAreaEnforcer : MonoBehaviour {
    private static PlayAreaEnforcer instance;
    private WaitForSeconds wait;
    private Bounds bounds;
    private List<PhotonView> trackedObjects;
    
    public static void AddTrackedObject(PhotonView obj) {
        if (instance == null) {
            return;
        }

        if (!instance.trackedObjects.Contains(obj)) {
            instance.trackedObjects.Add(obj);
        }
    }
    
    public static void RemoveTrackedObject(PhotonView obj) {
        if (instance == null) {
            return;
        }
        if (instance.trackedObjects.Contains(obj)) {
            instance.trackedObjects.Remove(obj);
        }
    }

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
        } else {
            instance = this;
        }

        trackedObjects = new List<PhotonView>();
    }

    void Start() {
        wait = new WaitForSeconds(5f);
        OcclusionArea area = GetComponent<OcclusionArea>();
        bounds = new Bounds(area.transform.TransformPoint(area.center), area.transform.TransformVector(area.size));
        StartCoroutine(CheckForViolations());
    }

    IEnumerator CheckForViolations() {
        while(true) {
            yield return wait;
            for (int i = 0; i < trackedObjects.Count; i++) {
                if (!bounds.Contains(trackedObjects[i].transform.position) && trackedObjects[i].IsMine) {
                    PhotonNetwork.Destroy(trackedObjects[i]);
                    trackedObjects.RemoveAt(i--);
                }
                yield return null;
            }
        }
    }
}
