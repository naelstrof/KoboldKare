using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RNGDick : MonoBehaviourPun
{
    public Equipment[] genitalia;
    // Start is called before the first frame update
    void Start() {
        // We need to wait a frame or two to make sure that we are fully instantiated.
        if (!SaveManager.isLoading) {
            StartCoroutine(WaitASecAndSpawnDick());
        }
    }
    private IEnumerator WaitASecAndSpawnDick() {
        yield return null;
        yield return null;
        if(photonView.IsMine && NetworkManager.instance.localPlayerInstance!=photonView) {
            Kobold k = GetComponent<Kobold>();
            // If they already have a dick
            if (k.inventory.equipment.Count > 0) {
                // Clear their old dick
                bool needsDick = false;
                for (int i=0;i<k.inventory.equipment.Count;i++) {
                    if (k.inventory.equipment[i].equipment is DickEquipment) {
                        needsDick = true;
                        k.inventory.RemoveEquipment(i, EquipmentInventory.EquipmentChangeSource.Misc, false);
                    }
                }
                //k.inventory.Clear(EquipmentInventory.EquipmentChangeSource.Misc, false);

                // And give em a new one!
                if (needsDick) {
                    k.inventory.AddEquipment(genitalia[Random.Range(0,genitalia.Length)], EquipmentInventory.EquipmentChangeSource.Misc);
                }
            }
        }
    }
}
