using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentDatabase : MonoBehaviour {
    private static EquipmentDatabase instance;
    public void Awake() {
        if (instance != null) {
            Destroy(this);
        } else {
            instance = this;
        }
        
        equipmentSorter = new EquipmentSorter();
        
        if (equipments.Count >= 256) {
            throw new UnityException("Too many equipment! Only 256 unique equipment allowed...");
        }
    }
    private class EquipmentSorter : IComparer<Equipment> {
        public int Compare(Equipment x, Equipment y) {
            return String.Compare(x.name, y.name, StringComparison.InvariantCulture);
        }
    }
    private EquipmentSorter equipmentSorter;
    public static void AddEquipment(Equipment newEquipment) {
        for (int i = 0; i < instance.equipments.Count; i++) {
            var reagent = instance.equipments[i];
            // Replace strategy
            if (reagent.name == newEquipment.name) {
                instance.equipments[i] = newEquipment;
                instance.equipments.Sort(instance.equipmentSorter);
                return;
            }
        }

        instance.equipments.Add(newEquipment);
        instance.equipments.Sort(instance.equipmentSorter);
    }
    
    public static void RemoveEquipment(Equipment equipment) {
        if (instance.equipments.Contains(equipment)) {
            instance.equipments.Remove(equipment);
        }
    }
    
    public static Equipment GetEquipment(string name) {
        foreach (var equipment in instance.equipments) {
            if (equipment.name == name) {
                return equipment;
            }
        }
        throw new UnityException("Failed to find equipment with name " + name);
    }
    public static Equipment GetEquipment(byte id) {
        if (id >= instance.equipments.Count) {
            Debug.LogError($"Failed to find equipment with id {id}, replaced it with first available equipment.");
            return instance.equipments[0];
        }
        return instance.equipments[id];
    }
    public static byte GetID(Equipment equipment) {
        return (byte)instance.equipments.IndexOf(equipment);
    }
    public static List<Equipment> GetEquipments() => instance.equipments;
    public List<Equipment> equipments;
}
