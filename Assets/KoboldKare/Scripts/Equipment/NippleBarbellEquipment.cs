using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Nipple Barbell Equipment", menuName = "Equipment/Nipple Barbell", order = 1)]
public class NippleBarbellEquipment : Equipment {
    public override GameObject[] OnEquip(Kobold k, GameObject groundPrefab) {
        k.nippleBarbells.gameObject.SetActive(true);
        return base.OnEquip(k, groundPrefab);
    }

    public override GameObject OnUnequip(Kobold k, bool dropOnGround = true) {
        k.nippleBarbells.gameObject.SetActive(false);
        return base.OnUnequip(k, dropOnGround);
    }
}
