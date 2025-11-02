using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentDatabase : MonoBehaviour {
    private static EquipmentDatabase instance;
    private static EquipmentDatabase Instance {
        get {
            if (instance == null) {
                instance = (EquipmentDatabase)FindObjectOfType(typeof(EquipmentDatabase));
            }
            return instance;
        }
    }
    private class EquipmentSorter : IComparer<string> {
        public int Compare(string x, string y) {
            return String.Compare(x, y, StringComparison.InvariantCulture);
        }
    }

    private struct EquipmentStubPair {
        public ModManager.ModStub? stub;
        public Equipment equipment;
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
    
    private SortedDictionary<string,List<EquipmentStubPair>> equipments = new(new EquipmentSorter());

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            if (this != instance) {
                Destroy(this.gameObject);
            }
        }
    }

    public static void AddEquipment(Equipment newEquipment, ModManager.ModStub? stub) {
        if (Instance.equipments.Count >= short.MaxValue) {
            throw new UnityException("Too many equipment! Only 32767 unique equipment allowed...");
        }
        
        if (!Instance.equipments.ContainsKey(newEquipment.name)) {
            Instance.equipments.Add(newEquipment.name, new List<EquipmentStubPair>());
        }
        var list = Instance.equipments[newEquipment.name];
        list.Add(new EquipmentStubPair() {
            stub = stub,
            equipment = newEquipment
        });
        list.Sort(CompareEquipmentStubPair);
    }

    private static int CompareEquipmentStubPair(EquipmentStubPair x, EquipmentStubPair y) {
        if (x.stub == null && y.stub == null) return 0;
        if (x.stub == null) return -1;
        if (y.stub == null) return 1;
        if (x.stub.Value.loadPriority == y.stub.Value.loadPriority) {
            return String.Compare(x.stub.Value.title, y.stub.Value.title, StringComparison.InvariantCulture);
        }

        return x.stub.Value.loadPriority.CompareTo(y.stub.Value.loadPriority);
    }

    public static void RemoveEquipment(Equipment equipment, ModManager.ModStub? stub) {
        if (!Instance.equipments.TryGetValue(equipment.name, out var list)) {
            return;
        } else {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].GetRepresentedByStub(stub)) {
                    list.RemoveAt(i);
                    i--;
                }
            }
            if (list.Count == 0) {
                Instance.equipments.Remove(equipment.name);
            }
            list.Sort(CompareEquipmentStubPair);
        }
    }

    public static Equipment GetEquipment(string name) {
        if (Instance.equipments.TryGetValue(name, out var list)) {
            return list[^1].equipment;
        }
        throw new UnityException("Failed to find equipment with name " + name);
    }

    public static Equipment GetEquipment(short id) {
        if (id >= Instance.equipments.Count) {
            Debug.LogError($"Failed to find equipment with id {id}, replaced it with the first available equipment.");
            return Instance.equipments.ElementAt(0).Value[^1].equipment;
        }
        var list = Instance.equipments.ElementAt(id).Value;
        return list[^1].equipment;
    }

    public static short GetID(Equipment equipment) {
        for(int i=0;i<Instance.equipments.Count;i++) {
            if (Instance.equipments.ElementAt(i).Key == equipment.name) {
                return (short)i;
            }
        }
        Debug.LogError($"Failed to find equipment id for {equipment.name}, replaced it with the first available equipment.");
        return 0;
    }

    public static List<Equipment> GetEquipments() {
        List<Equipment> equipments = new();
        for(int i=0;i<Instance.equipments.Count;i++) {
            var list = Instance.equipments.ElementAt(i).Value;
            equipments.Add(list[^1].equipment);
        }
        return equipments;
    }

}




