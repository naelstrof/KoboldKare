using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantDatabase : MonoBehaviour {
    private static PlantDatabase instance;
    public void Awake() {
        if (instance != null) {
            Destroy(this);
        } else {
            instance = this;
        }
    }
    public static ScriptablePlant GetPlant(string name) {
        foreach(var plant in instance.plants) {
            if (plant.name == name) {
                return plant;
            }
        }
        return null;
    }
    public static ScriptablePlant GetPlant(short id) {
        return instance.plants[id];
    }
    public static short GetID(ScriptablePlant plant) {
        return (short)instance.plants.IndexOf(plant);
    }
    public List<ScriptablePlant> plants;
}
