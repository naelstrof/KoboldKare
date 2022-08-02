using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using KoboldKare;

public class GenericLODConsumer : MonoBehaviour {
    [HideInInspector]
    public bool isClose = false;
    [HideInInspector]
    public bool isVeryFar = false;
    public List<Rigidbody> trackedRigidbodies;
    public enum ConsumerType {
        Kobold,
        PhysicsItem,
    }
    public ConsumerType resource;
    void Start() {
        LODManager.instance.RegisterConsumer(this, resource);
    }
    private void OnDestroy() {
        LODManager.instance.UnregisterConsumer(this, resource);
    }

    public void SetLOD(bool close) {
        foreach(var body in trackedRigidbodies) {
            body.interpolation = close ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
        }
    }
}
