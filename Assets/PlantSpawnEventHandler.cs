using System.Collections;
using System.Collections.Generic;
using AmplifyShaderEditor;
using UnityEngine;

public class PlantSpawnEventHandler : MonoBehaviour {
    private static PlantSpawnEventHandler instance;
    void Awake() {
        instance = this;
    }
    public delegate void PlantSpawnEventAction(GameObject obj, ScriptablePlant plant);
    private event PlantSpawnEventAction planted;
    public static void AddListener(PlantSpawnEventAction action) {
        instance.planted += action;
    }
    public static void RemoveListener(PlantSpawnEventAction action) {
        instance.planted -= action;
    }
    public static void TriggerPlantSpawnEvent(GameObject obj, ScriptablePlant plant) {
        instance.planted?.Invoke(obj, plant);
    }
}
