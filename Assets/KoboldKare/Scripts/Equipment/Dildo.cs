using System;
using System.Collections;
using System.Collections.Generic;
using Naelstrof.Inflatable;
using PenetrationTech;
using Photon.Pun;
using UnityEngine;

public class Dildo : GenericEquipment, IValuedGood {
    [SerializeField]
    private Inflatable dildoSizeInflatable;
    private GenericReagentContainer container;
    [SerializeField] private Penetrator listenPenetrator;

    public delegate void PenetrateAction(Penetrator penetrator, Penetrable penetrable);

    public static event PenetrateAction dildoPenetrateStart;
    public static event PenetrateAction dildoPenetrateEnd;

    private void Awake() {
        dildoSizeInflatable.OnEnable();
        container = gameObject.AddComponent<GenericReagentContainer>();
        container.type = GenericReagentContainer.ContainerType.Mouth;
        container.OnChange.AddListener(OnContentsChanged);
        photonView.ObservedComponents.Add(container);
    }

    private void OnEnable() {
        listenPenetrator.penetrationStart += OnPenetrationStart;
        listenPenetrator.penetrationEnd += OnPenetrationEnd;
    }

    private void OnDisable() {
        listenPenetrator.penetrationStart -= OnPenetrationStart;
        listenPenetrator.penetrationEnd -= OnPenetrationEnd;
    }

    private void OnPenetrationStart(Penetrable penetrable) {
        dildoPenetrateStart?.Invoke(listenPenetrator, penetrable);
    }
    private void OnPenetrationEnd(Penetrable penetrable) {
        dildoPenetrateEnd?.Invoke(listenPenetrator, penetrable);
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
