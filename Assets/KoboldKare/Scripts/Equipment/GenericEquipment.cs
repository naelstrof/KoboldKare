using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GenericEquipment : GenericUsable, IPunObservable, IOnPhotonViewOwnerChange {
    public Equipment representedEquipment;
    [SerializeField]
    private Sprite displaySprite;
    private Kobold tryingToEquip;
    private bool equipOnTouch = false;
    // Trying to match the Use pattern, so we can just use a GenericUsable to equip. Though technically we can call this from anything. A button that equips you with a status effect or whatever.
    public void TriggerAttachOnTouch(float duration) {
        StartCoroutine(AttachOnTouch(duration));
    }
    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }
    public override void LocalUse(Kobold k) {
        //base.LocalUse(k);
        Equip(k);
    }
    private void Equip(Kobold k) {
        // If kobold got deleted
        if (k == null) {
            return;
        }
        // Try to take control of the equipment, if we don't have permission.
        if (k.photonView.IsMine && !photonView.IsMine && tryingToEquip == null) {
            tryingToEquip = k;
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
        // Only successfully equip if we own both the equipment, and the kobold. Otherwise, wait for ownership to successfully transfer
        if (k.photonView.IsMine && photonView.IsMine) {
            KoboldInventory inventory = k.GetComponent<KoboldInventory>();
            inventory.PickupEquipment(representedEquipment, gameObject);
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }
    private IEnumerator AttachOnTouch(float duration) {
        equipOnTouch = true;
        yield return new WaitForSeconds(duration);
        equipOnTouch = false;
    }
    void OnCollisionEnter(Collision collision) {
        if (!equipOnTouch) {
            return;
        }
        if (collision == null || collision.rigidbody == null) {
            return;
        }
        Kobold kobold = collision.rigidbody.GetComponentInParent<Kobold>();
        if (kobold != null) {
            Equip(kobold);
            equipOnTouch = false;
        }
    }
    public void OnOwnerChange(Player newOwner, Player previousOwner) {
        if (newOwner == PhotonNetwork.LocalPlayer && tryingToEquip != null) {
            KoboldInventory inventory = tryingToEquip.GetComponent<KoboldInventory>();
            inventory.PickupEquipment(representedEquipment, gameObject);
            PhotonNetwork.Destroy(photonView.gameObject);
        }
        // Someone else must've won the handshake, so we clear our attempt to equip.
        tryingToEquip = null;
    }
}
