using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class GenericEquipment : MonoBehaviourPun {
    public Equipment representedEquipment;
    private bool equipOnTouch = false;
    // Trying to match the Use pattern, so we can just use a GenericUsable to equip. Though technically we can call this from anything. A button that equips you with a status effect or whatever.
    public void TriggerAttachOnTouch(float duration) {
        StartCoroutine(AttachOnTouch(duration));
    }
    public void Equip(Kobold k, Vector3 position) {
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
            Equip(kobold, kobold.transform.position);
            equipOnTouch = false;
        }
    }
}
