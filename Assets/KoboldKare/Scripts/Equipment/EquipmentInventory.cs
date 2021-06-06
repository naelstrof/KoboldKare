using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentInventory {
    public enum EquipmentChangeSource {
        Misc,
        Drop,
        Pickup,
        Network
    }
    public EquipmentInventory(Kobold k) {
        kobold = k;
    }
    public Kobold kobold;
    [System.Serializable]
    public class EquipmentGameObjectsPair {
        public EquipmentGameObjectsPair(Equipment e, GameObject[] objects) {
            equipment = e;
            gameObjects = objects;
        }
        public Equipment equipment;
        public GameObject[] gameObjects;
    }
    public List<EquipmentGameObjectsPair> equipment = new List<EquipmentGameObjectsPair>();
    public delegate void EquipmentChangedHandler(EquipmentInventory inventory, EquipmentChangeSource source);
    public event EquipmentChangedHandler EquipmentChangedEvent;
    public void AddEquipment(Equipment e, EquipmentChangeSource source) {
        AddEquipment(e, null, source);
        EquipmentChangedEvent?.Invoke(this, source);
    }
    public void AddEquipment(Equipment e, GameObject groundPrefab, EquipmentChangeSource source) {
        if (e.slot != Equipment.EquipmentSlot.Misc) {
            for (int i=0;i<equipment.Count;i++) {
                if (equipment[i].equipment.slot == e.slot) {
                    RemoveEquipment(equipment[i].equipment, EquipmentChangeSource.Drop);
                }
            }
        }
        equipment.Add(new EquipmentGameObjectsPair(e, e.OnEquip(kobold, groundPrefab)));
        EquipmentChangedEvent?.Invoke(this, source);
    }
    public void RemoveEquipment(int id, EquipmentChangeSource source, bool dropOnGround = true) {
        if (id < 0 || id >= equipment.Count) {
            return;
        }
        Equipment e = equipment[id].equipment;
        e.OnUnequip(kobold, dropOnGround);
        if (equipment[id].gameObjects != null) {
            foreach (GameObject g in equipment[id].gameObjects) {
                GameObject.Destroy(g);
            }
        }
        equipment.RemoveAt(id);
        EquipmentChangedEvent?.Invoke(this, source);
    }
    public void Clear(EquipmentChangeSource source, bool dropOnGround = true) {
        for (int i = 0; i < equipment.Count;) {
            RemoveEquipment(equipment[i], source, dropOnGround);
        }
        EquipmentChangedEvent?.Invoke(this, source);
    }
    public void RemoveEquipment(Equipment e, EquipmentChangeSource source, bool dropOnGround = true, bool removeAllInstances = false) {
        for (int i=0;i<equipment.Count;i++) {
            if (equipment[i].equipment == e) {
                e.OnUnequip(kobold, dropOnGround);
                if (equipment[i].gameObjects != null) {
                    foreach (GameObject g in equipment[i].gameObjects) {
                        GameObject.Destroy(g);
                    }
                }
                equipment.RemoveAt(i);
                if (!removeAllInstances) {
                    EquipmentChangedEvent?.Invoke(this, source);
                    return;
                }
            }
        }
        EquipmentChangedEvent?.Invoke(this, source);
    }
    public void RemoveEquipment(EquipmentGameObjectsPair e, EquipmentChangeSource source, bool dropOnGround = true) {
        e.equipment.OnUnequip(kobold, dropOnGround);
        if (e.gameObjects != null) {
            foreach (GameObject g in e.gameObjects) {
                GameObject.Destroy(g);
            }
        }
        equipment.Remove(e);
        EquipmentChangedEvent?.Invoke(this, source);
    }
}
