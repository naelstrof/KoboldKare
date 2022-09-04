using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DickEquipment", menuName = "Equipment/DickEquipment", order = 1)]
public class DickEquipment : Equipment {
    public GameObject dickPrefab;
    public override GameObject[] OnEquip(Kobold k, GameObject groundPrefab) {
        base.OnEquip(k, groundPrefab);
        GenericReagentContainer container = groundPrefab == null ? null : groundPrefab.GetComponentInChildren<GenericReagentContainer>();
        GameObject[] stuff = new GameObject[1];
        GameObject dick = GameObject.Instantiate(dickPrefab, k.transform.position, k.transform.rotation);
        stuff[0] = dick;
        DickInfo info = dick.GetComponentInChildren<DickInfo>();
        if (info == null) {
            throw new UnityException("Dick equipment is missing the DickInfo monobehavior. It's needed to be able to equip!");
        }
        info.AttachTo(k);
        return stuff;
    }
}
