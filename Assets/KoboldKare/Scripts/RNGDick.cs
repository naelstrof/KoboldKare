using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RNGDick : MonoBehaviourPun
{
    public Equipment[] genitalia;
    // Start is called before the first frame update
    void Start()
    {
        if(photonView.IsMine && NetworkManager.instance.localPlayerInstance!=photonView) {
            Kobold k = GetComponent<Kobold>();
            float randomInt = Random.Range(0f,2f);
            if(randomInt <= 0.50f) {
                k.inventory.AddEquipment(genitalia[Random.Range(0,genitalia.Length)], EquipmentInventory.EquipmentChangeSource.Misc);
            }
        }
    }
}
