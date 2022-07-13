using System;
using System.Collections;
using System.Collections.Generic;
using Naelstrof.Inflatable;
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
        OnContentsChanged(GenericReagentContainer.InjectType.Inject);
    }

    private void OnContentsChanged(GenericReagentContainer.InjectType injectType) {
        float eggPlant = container.GetVolumeOf(ReagentDatabase.GetReagent("EggplantJuice"));
        float growth = container.GetVolumeOf(ReagentDatabase.GetReagent("GrowthSerum"));
        dildoSizeInflatable.SetSize(Mathf.Log(2f + (eggPlant + growth)/20f, 2f), this);
    }

    public float GetWorth() {
        return 15f;
    }
}
