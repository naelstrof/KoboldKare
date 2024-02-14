using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentDatabase : MonoBehaviour {
    private static EquipmentDatabase instance;
    public static EquipmentDatabase Instance {
        get {
            if (instance == null) {
                instance = (EquipmentDatabase)FindObjectOfType(typeof(EquipmentDatabase));
                if (instance == null) {
                    GameObject go = new GameObject("EquipmentDatabase");
                    instance = go.AddComponent<EquipmentDatabase>();
                }
            }
            return instance;
        }
    }

    private class EquipmentSorter : IComparer<Equipment> {
        public int Compare(Equipment x, Equipment y) {
            return String.Compare(x.name, y.name, StringComparison.InvariantCulture);
        }
    }

    private EquipmentSorter equipmentSorter;

    public EquipmentDatabase() {
        equipments = new List<Equipment>();
    }

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            if (this != instance) {
                Destroy(this.gameObject);
            }
        }

        equipmentSorter = new EquipmentSorter();
    }

    public static void AddEquipment(Equipment newEquipment) {
        if (Instance.equipments.Count >= short.MaxValue) {
            throw new UnityException("Too many equipment! Only 32767 unique equipment allowed...");
        }

        for (int i = 0; i < Instance.equipments.Count; i++) {
            var reagent = Instance.equipments[i];
            if (reagent.name == newEquipment.name) {
                Instance.equipments[i] = newEquipment;
                Instance.equipments.Sort(Instance.equipmentSorter);
                return;
            }
        }

        Instance.equipments.Add(newEquipment);
        Instance.equipments.Sort(Instance.equipmentSorter);
    }

    public static void RemoveEquipment(Equipment equipment) {
        if (Instance.equipments.Contains(equipment)) {
            Instance.equipments.Remove(equipment);
        }
    }

    public static Equipment GetEquipment(string name) {
        foreach (var equipment in Instance.equipments) {
            if (equipment.name == name) {
                return equipment;
            }
        }
        throw new UnityException("Failed to find equipment with name " + name);
    }

    public static Equipment GetEquipment(short id) {
        if (id >= Instance.equipments.Count) {
            Debug.LogError($"Failed to find equipment with id {id}, replaced it with the first available equipment.");
            return Instance.equipments[0];
        }
        return Instance.equipments[id];
    }

    public static short GetID(Equipment equipment) {
        return (short)Instance.equipments.IndexOf(equipment);
    }

    public static List<Equipment> GetEquipments() => Instance.equipments;

    public List<Equipment> equipments;
}




