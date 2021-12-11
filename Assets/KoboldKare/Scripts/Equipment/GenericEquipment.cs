using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class GenericEquipment : GenericUsable {
    public Equipment representedEquipment;
    [SerializeField]
    private Sprite displaySprite;
    private bool equipOnTouch = false;
    // Trying to match the Use pattern, so we can just use a GenericUsable to equip. Though technically we can call this from anything. A button that equips you with a status effect or whatever.
    public void TriggerAttachOnTouch(float duration) {
        StartCoroutine(AttachOnTouch(duration));
    }
    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }
    public override void Use(Kobold k) {
        base.Use(k);
        Equip(k);
    }
    private void Equip(Kobold k) {
        // If kobold got deleted
        if (k == null) {
            return;
        }
        if (k.photonView.IsMine) {
            KoboldInventory inventory = k.GetComponent<KoboldInventory>();
            inventory.PickupEquipment(representedEquipment, gameObject);
        }
        if (photonView != null && photonView.IsMine) {
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
}
