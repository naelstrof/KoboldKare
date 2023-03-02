using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiePrefab : MonoBehaviour {
    private static DiePrefab instance;
    [SerializeField, SerializeReference, SerializeReferenceButton]
    private OrbitCameraConfiguration dieConfig;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start() {
        OrbitCamera.AddConfiguration(dieConfig);
    }

    public void Remove() {
        if (instance != this) {
            Destroy(gameObject);
            return;
        }
        NetworkManager.instance.SpawnControllablePlayer();
        Destroy(gameObject);
    }
}
