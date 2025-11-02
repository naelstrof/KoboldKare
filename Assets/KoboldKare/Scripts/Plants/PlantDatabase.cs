using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlantDatabase : MonoBehaviour {
    private static PlantDatabase instance;

    private struct PlantStubPair {
        public ScriptablePlant plant;
        public ModManager.ModStub? stub;
        public bool GetRepresentedByStub(ModManager.ModStub? b) {
            if (b == null && stub == null) {
                return true;
            }
            if (b == null || stub == null) {
                return false;
            }
            return stub.Value.GetRepresentedBy(b.Value);
        }
    }
    private static int ComparePlantStubPair(PlantStubPair x, PlantStubPair y) {
        if (x.stub == null && y.stub == null) return 0;
        if (x.stub == null) return -1;
        if (y.stub == null) return 1;
        if (x.stub.Value.loadPriority == y.stub.Value.loadPriority) {
            return String.Compare(x.stub.Value.title, y.stub.Value.title, StringComparison.InvariantCulture);
        }
        return x.stub.Value.loadPriority.CompareTo(y.stub.Value.loadPriority);
    }
    
    private class PlantSorter : IComparer<string> {
        public int Compare(string x, string y) {
            return String.Compare(x, y, StringComparison.InvariantCulture);
        }
    }
    
    
    private SortedDictionary<string, List<PlantStubPair>> plants = new(new PlantSorter());
    public void Awake() {
        if (instance != null) {
            Destroy(this);
        } else {
            instance = this;
        }
    }
    public static ScriptablePlant GetPlant(string name) {
        if (instance.plants.TryGetValue(name, out var plantList)) {
            return plantList[^1].plant;
        }

        if (instance.plants.Count > 0) {
            return instance.plants.ElementAt(0).Value[^1].plant;
        }

        return null;
    }
    public static ScriptablePlant GetPlant(short id) {
        return instance.plants.ElementAt(id).Value[^1].plant;
    }

    public static void AddPlant(ScriptablePlant newPlant, ModManager.ModStub? stub) {
        if (!instance.plants.ContainsKey(newPlant.name)) {
            instance.plants.Add(newPlant.name, new List<PlantStubPair>());
        }
        var list = instance.plants[newPlant.name];
        list.Add(new PlantStubPair() {
            plant = newPlant,
            stub = stub
        });
        list.Sort(ComparePlantStubPair);
    }
    
    public static void RemovePlant(ScriptablePlant newPlant, ModManager.ModStub? stub) {
        if (!instance.plants.TryGetValue(newPlant.name, out var list)) {
            return;
        }
        for (int i = 0; i < list.Count; i++) {
            if (list[i].GetRepresentedByStub(stub)) {
                list.RemoveAt(i);
                i--;
            }
        }
        if (list.Count == 0) {
            instance.plants.Remove(newPlant.name);
        }
        list.Sort(ComparePlantStubPair);
    }

    public static short GetID(ScriptablePlant plant) {
        if (!instance.plants.ContainsKey(plant.name)) {
            return 0;
        }
        for(int i=0;i<instance.plants.Count;i++) {
            if (instance.plants.ElementAt(i).Key == plant.name) {
                return (short)i;
            }
        }
        return 0;
    }
}
