using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentDatabase : MonoBehaviour {
    private static EquipmentDatabase instance;
    private Dictionary<string,Equipment> equipmentDictionary = new Dictionary<string, Equipment>();
    private static EquipmentDatabase GetInstance() {
        if (instance == null) {
            instance = Object.FindObjectOfType<EquipmentDatabase>();
        }
        return instance;
    }
    public void Awake() {
        if (instance != null) {
            Destroy(this);
        } else {
            instance = this;
        }
        foreach(var equipment in equipments) {
            equipmentDictionary.Add(equipment.name, equipment);
        }
        if (equipmentDictionary.Count >= 256) {
            throw new UnityException("Too many equipment! Only 256 unique equipment allowed...");
        }
    }
    public static Equipment GetEquipment(string name) {
        if (GetInstance().equipmentDictionary.ContainsKey(name)) {
            return GetInstance().equipmentDictionary[name];
        }
        throw new UnityException("Failed to find equipment with name " + name);
    }
    public static Equipment GetEquipment(byte id) {
        return GetInstance().equipments[id];
    }
    public static byte GetID(Equipment equipment) {
        return (byte)GetInstance().equipments.IndexOf(equipment);
    }
    public static List<Equipment> GetEquipments() => GetInstance().equipments;
    public List<Equipment> equipments;
}
