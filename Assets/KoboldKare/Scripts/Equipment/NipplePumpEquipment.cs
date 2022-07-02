using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Nipple Pump Equipment", menuName = "Equipment/Nipple Pump", order = 1)]
public class NipplePumpEquipment : Equipment {
    public override GameObject[] OnEquip(Kobold k, GameObject groundPrefab) {
        k.nipplePumps.gameObject.SetActive(true);
        return base.OnEquip(k, groundPrefab);
    }

    public override GameObject OnUnequip(Kobold k, bool dropOnGround = true) {
        k.nipplePumps.gameObject.SetActive(false);
        return base.OnUnequip(k, dropOnGround);
    }
}
