using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class GenericEquipment : MonoBehaviourPun {
    public Equipment representedEquipment;
    // Trying to match the Use pattern, so we can just use a GenericUsable to equip. Though technically we can call this from anything. A button that equips you with a status effect or whatever.
    public void Equip(Kobold k, Vector3 position) {
        // If kobold got deleted
        if (k == null) {
            return;
        }
        if (k.photonView.IsMine) {
            k.inventory.AddEquipment(representedEquipment, gameObject, EquipmentInventory.EquipmentChangeSource.Pickup);
            SaveManager.RPC(k.photonView, "PickupEquipment", RpcTarget.OthersBuffered, new object[] { representedEquipment.GetID() });
        }
        if (photonView != null && photonView.IsMine) {
            SaveManager.Destroy(photonView.gameObject);
        }
    }
}
