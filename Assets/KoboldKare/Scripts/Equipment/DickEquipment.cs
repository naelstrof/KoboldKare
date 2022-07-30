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
        foreach(DickInfo.DickSet set in info.dicks) {
            if (container != null) {
                k.SetGenes(k.GetGenes().With(dickSize: container.volume));
            }
            set.dickIdentifier = GetInstanceID();
        }
        info.AttachTo(k);
        return stuff;
    }
    public override GameObject OnUnequip(Kobold k, bool dropOnGround = true) {
        GameObject groundPrefab = base.OnUnequip(k, dropOnGround);
        ReagentContents contents = new ReagentContents();
        // Search for a dick that matches our equipment, then use the info to remove all the dicks associated with it.
        foreach(DickInfo.DickSet set in k.activeDicks) {
            if (set.dickIdentifier == GetInstanceID()) {
                contents.AddMix(ReagentDatabase.GetID(ReagentDatabase.GetReagent("GrowthSerum")), k.GetGenes().dickSize);
            }
        }
        foreach(DickInfo.DickSet set in k.activeDicks) {
            if (set.dickIdentifier == GetInstanceID()) {
                set.info.RemoveFrom(k);
                break;
            }
        }
        if (groundPrefab != null) {
            GenericReagentContainer container = groundPrefab.GetComponentInChildren<GenericReagentContainer>();
            if (container != null) {
                container.AddMix(contents, GenericReagentContainer.InjectType.Inject);
            }
        }
        return groundPrefab;
    }
}
