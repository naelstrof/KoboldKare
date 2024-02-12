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
        
        if (equipments.Count >= 256) {
            throw new UnityException("Too many equipment! Only 256 unique equipment allowed...");
        }
    }
    
    public static void AddEquipment(Equipment newEquipment) {
        if (instance.equipments.ContainsKey(newEquipment.name)) {
            instance.equipments[newEquipment.name] = newEquipment;
        } else {
            instance.equipments.Add(newEquipment.name, newEquipment);
        }
    }
    
    public static void RemoveEquipment(Equipment equipment) {
        if (instance.equipments.ContainsKey(equipment.name)) {
            instance.equipments.Remove(equipment.name);
        }
    }
    
    public static Equipment GetEquipment(string name) {
        if (instance.equipments.ContainsKey(name)) {
            return instance.equipments[name];
        }
        throw new UnityException("Failed to find equipment with name " + name);
    }
    
    public static List<Equipment> GetEquipments() => new List<Equipment>(instance.equipments.Values);
    public Dictionary<string, Equipment> equipments = new Dictionary<string, Equipment>();
}