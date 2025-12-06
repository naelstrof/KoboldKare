using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using NetStack.Serialization;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;

[RequireComponent(typeof(Kobold))]
public class KoboldInventory : MonoBehaviour, ISavable {
    private Dictionary<Equipment, List<GameObject[]>> equipmentDisplays = new Dictionary<Equipment, List<GameObject[]>>();
    private Dictionary<Equipment, List<AssetGroup.AssetLocation.AssetHandle<Equipment>>> equipmentHandles = new();
    private static List<string> staticIncomingEquipment = new ();
    public int Count => equipment.Count;
    private List<Equipment> equipment = new List<Equipment>();
    public delegate void EquipmentChangedEvent(List<Equipment> newEquipment);
    public EquipmentChangedEvent equipmentChanged;
    private Kobold kobold;
    void Awake() {
        kobold = GetComponent<Kobold>();
    }
    public List<Equipment> GetAllEquipment() {
        return new List<Equipment>(equipment);
    }
    public int GetEquipmentInstanceCount(Equipment thing) {
        int count = 0;
        for(int i=0;i<equipment.Count;i++) {
            if (equipment[i] == thing) {
                count++;
            }
        }
        return count;
    }
    public Equipment GetEquipmentInSlot(Equipment.EquipmentSlot slot) {
        foreach( Equipment e in equipment) {
            if (e.slot == slot) {
                return e;
            }
        }
        return null;
    }

    //[PunRPC]
    public void PickupEquipmentRPC(short equipmentID, int groundPrefabID) {
        // FIXME FISHNET
        /*PhotonView view = PhotonNetwork.GetPhotonView(groundPrefabID);
        if (EquipmentDatabase.TryGetAsset(equipmentID, out var equip)) {
            PickupEquipment(equip, view == null ? null : view.gameObject);
            PhotonProfiler.LogReceive(sizeof(short) + sizeof(int));
        } else {
            Debug.LogError("Tried to pick up equipment with invalid ID: " + equipmentID);
        }*/
    }

    public async Task PickupEquipment(string equipmentName, GameObject groundPrefab) {
        var handle = await KoboldKareObjectPostProcessor.GetAssetAsync("Equipment", equipmentName, GameManager.GetErrorEquipment());
        
        GameObject[] displays = handle.asset.OnEquip(kobold, groundPrefab);

        // Remember the created objects
        if (!equipmentDisplays.ContainsKey(handle.asset)) {
            equipmentDisplays[handle.asset] = new List<GameObject[]>();
        }

        if (!equipmentHandles.ContainsKey(handle.asset)) {
            equipmentHandles[handle.asset] = new List<AssetGroup.AssetLocation.AssetHandle<Equipment>>();
        }
        
        equipmentHandles[handle.asset].Add(handle);
        equipmentDisplays[handle.asset].Add(displays);
        equipment.Add(handle.asset);
        equipmentChanged?.Invoke(equipment);
        
    }
    public bool Contains(Equipment thing) => equipment.Contains(thing);
    public void RemoveEquipment(Equipment.EquipmentSlot slot, bool dropOnGround) {
        Equipment e = GetEquipmentInSlot(slot);
        if (e!= null) {
            RemoveEquipment(e, dropOnGround);
        }
    }
    public void RemoveEquipment(Equipment thing, bool dropOnGround) {
        if (!equipment.Contains(thing)) {
            return;
        }
        equipment.Remove(thing);
        thing.OnUnequip(kobold, dropOnGround);

        // Destroy the created objects
        foreach(GameObject obj in equipmentDisplays[thing][0]) {
            Destroy(obj);
        }
        equipmentDisplays[thing].RemoveAt(0);
        
        equipmentHandles[thing][0].Release();
        equipmentHandles[thing].RemoveAt(0);

        equipmentChanged?.Invoke(equipment);
    }
    async Task ReplaceEquipmentWith(List<string> newEquipmentNames) {
        bool same = newEquipmentNames.Count == equipment.Count;
        for(int i=0;i<equipment.Count&&same;i++) {
            if (equipment[i].name != newEquipmentNames[i]) {
                same = false;
            }
        }
        if (same) {
            return;
        }
        while(equipment.Count != 0) {
            RemoveEquipment(equipment[0], false);
        }
        foreach(var e in newEquipmentNames) {
            await PickupEquipment(e, null);
        }
    }
    // FIXME FISHNET
    /*
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            BitBuffer bitBuffer = new BitBuffer(4);
            bitBuffer.AddShort((short)equipment.Count);
            foreach(Equipment e in equipment) {
                bitBuffer.AddShort(EquipmentDatabase.GetID(e));
            }
            stream.SendNext(bitBuffer);
        } else {
            BitBuffer data = (BitBuffer)stream.ReceiveNext();
            short equipmentCount = data.ReadShort();
            staticIncomingEquipment.Clear();
            for(int i=0;i<equipmentCount;i++) {
                if (EquipmentDatabase.TryGetAsset(data.ReadShort(), out var equipped)) {
                    staticIncomingEquipment.Add(equipped);
                }
            }
            ReplaceEquipmentWith(staticIncomingEquipment);
            PhotonProfiler.LogReceive(data.Length);
        }
    }*/

    public void Save(JSONNode node) {
        JSONArray equipments = new JSONArray();
        foreach(Equipment e in equipment) {
            equipments.Add(e.name);
        }
        node["equipments"] = equipments;
    }

    public async Task Load(JSONNode node) {
        JSONArray equipments = node["equipments"].AsArray;
        staticIncomingEquipment.Clear();
        for(int i=0;i<equipments.Count;i++) {
            staticIncomingEquipment.Add(equipments[i]);
        }
        await ReplaceEquipmentWith(staticIncomingEquipment);
    }
}

