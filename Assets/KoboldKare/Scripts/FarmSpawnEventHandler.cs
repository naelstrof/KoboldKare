using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmSpawnEventHandler : MonoBehaviour {
    public delegate void ProduceSpawnedAction(GameObject produce);

    private event ProduceSpawnedAction producedSpawned;
    private static FarmSpawnEventHandler instance;
    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public static void TriggerProduceSpawn(GameObject produce) {
        instance.producedSpawned?.Invoke(produce);
    }
    public static void AddListener(ProduceSpawnedAction action) {
        instance.producedSpawned += action;
    }
    public static void RemoveListener(ProduceSpawnedAction action) {
        instance.producedSpawned -= action;
    }
}
