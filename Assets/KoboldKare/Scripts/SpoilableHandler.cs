using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using KoboldKare;

public class SpoilableHandler : MonoBehaviour {
    [SerializeField]
    private GameEventGeneric midnightEvent;
    [SerializeField]
    private LayerMask safeZoneMask;
    private List<ISpoilable> spoilables = new List<ISpoilable>();
    private static SpoilableHandler instance;
    public static LayerMask GetSafeZoneMask() => instance.safeZoneMask;
    private void Awake() {
        if (instance != null) {
            Destroy(gameObject);
        } else {
            instance = this;
        }
    }
    void Start() {
        midnightEvent.AddListener(OnMidnight);
    }
    void OnDestroy() {
        midnightEvent.RemoveListener(OnMidnight);
    }
    void OnMidnight(object nothing) {
        foreach (var spoilable in spoilables) {
            int hitCount = 0;
            foreach (RaycastHit h in Physics.RaycastAll(spoilable.transform.position + Vector3.up * 400f,
                         Vector3.down, 400f, safeZoneMask, QueryTriggerInteraction.Collide)) {
                hitCount++;
            }

            if (hitCount % 2 == 0) {
                spoilable.OnSpoil();
            }
        }
    }
    public static void AddSpoilable(ISpoilable spoilable) {
        instance.spoilables.Add(spoilable);
    }
    public static void RemoveSpoilable(ISpoilable spoilable) {
        if (instance.spoilables.Contains(spoilable)) {
            instance.spoilables.Remove(spoilable);
        }
    }
}
