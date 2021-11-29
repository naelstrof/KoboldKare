using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scriptable Plant", menuName = "Data/Plant", order = 1)]
public class ScriptablePlant : ScriptableObject {
    [System.Serializable]
    public class Produce {
        public PhotonGameObjectReference prefab;
        public int minProduce;
        public int maxProduce;
    }
    public float fluidNeeded = 1f;
    public GameObject display;
    public ScriptablePlant[] possibleNextGenerations;
    public Produce[] produces;
    void OnValidate() {
        foreach(var produce in produces) {
            produce.prefab.OnValidate();
        }
    }
}
