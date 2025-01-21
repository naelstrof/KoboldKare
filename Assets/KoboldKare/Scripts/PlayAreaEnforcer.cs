using System.Collections;
using System.Collections.Generic;
using NetStack.Quantization;
using Photon.Pun;
using UnityEngine;

public class PlayAreaEnforcer : MonoBehaviour {
    private static PlayAreaEnforcer instance;
    private WaitForSeconds wait;
    private Bounds bounds;
    private List<PhotonView> trackedObjects;
    private BoundedRange[] worldBounds;

    public static BoundedRange[] GetWorldBounds() {
        return instance.worldBounds;
    }

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
        //Check if instance already exists
        if (instance == null) {
            //if not, set instance to this
            instance = this;
        } else if (instance != this) {
            //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
            return;
        }

        OcclusionArea area = GetComponent<OcclusionArea>();
        bounds = new Bounds(area.transform.TransformPoint(area.center), area.transform.TransformVector(area.size));
        worldBounds = new BoundedRange[] {
            new(bounds.min.x, bounds.max.x, 0.05f),
            new(bounds.min.y, bounds.max.y, 0.05f),
            new(bounds.min.z, bounds.max.z, 0.05f),
        };
        Debug.Log($"Using {worldBounds[0].GetRequiredBits()}:{worldBounds[1].GetRequiredBits()}:{worldBounds[2].GetRequiredBits()} bits for X:Y:Z position data.");
        trackedObjects = new List<PhotonView>();
    }

    void Start() {
        wait = new WaitForSeconds(5f);
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
