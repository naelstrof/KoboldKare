using System;
using System.Collections;
using System.Collections.Generic;
using Naelstrof.Inflatable;
using Photon.Pun;
using UnityEngine;

public class Dildo : GenericEquipment, IValuedGood {
    [SerializeField]
    private Inflatable dildoSizeInflatable;
    private GenericReagentContainer container;

    private void Awake() {
        dildoSizeInflatable.OnEnable();
        container = gameObject.AddComponent<GenericReagentContainer>();
        container.type = GenericReagentContainer.ContainerType.Mouth;
        container.OnChange.AddListener(OnContentsChanged);
        photonView.ObservedComponents.Add(container);
    }

    private void Start() {
        OnContentsChanged(container.GetContents(), GenericReagentContainer.InjectType.Inject);
    }

    private void OnContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        float eggPlant = contents.GetVolumeOf(ReagentDatabase.GetReagent("EggplantJuice"));
        float growth = contents.GetVolumeOf(ReagentDatabase.GetReagent("GrowthSerum"));
        dildoSizeInflatable.SetSize(Mathf.Log(2f + (eggPlant + growth)/20f, 2f), this);
    }

    protected override void Equip(Kobold k) {
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
            float eggPlant = container.GetVolumeOf(ReagentDatabase.GetReagent("EggplantJuice"));
            float growth = container.GetVolumeOf(ReagentDatabase.GetReagent("GrowthSerum"));
            k.SetGenes(k.GetGenes().With(dickEquip: (byte)EquipmentDatabase.GetID(representedEquipment),
                dickSize: eggPlant + growth));
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }

    public float GetWorth() {
        return 15f;
    }
}
