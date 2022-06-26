using System;
using System.Collections;
using System.Collections.Generic;
using Naelstrof.Inflatable;
using UnityEngine;

public class Dildo : GenericEquipment {
    private Inflatable dildoSizeInflatable;
    private GenericReagentContainer container;
    [SerializeField]
    private AnimationCurve bounceCurve;

    private void Awake() {
        container = gameObject.AddComponent<GenericReagentContainer>();
        container.type = GenericReagentContainer.ContainerType.Mouth;
        dildoSizeInflatable = new Inflatable(bounceCurve, 0.8f);
        dildoSizeInflatable.AddListener(new InflatableTransform(transform));
        container.OnChange.AddListener(OnContentsChanged);
    }

    private void Start() {
        OnContentsChanged(GenericReagentContainer.InjectType.Inject);
    }

    private void OnContentsChanged(GenericReagentContainer.InjectType injectType) {
        float eggPlant = container.GetVolumeOf(ReagentDatabase.GetReagent("EggplantJuice"));
        float growth = container.GetVolumeOf(ReagentDatabase.GetReagent("GrowthSerum"));
        if (dildoSizeInflatable.SetSize(Mathf.Log(2f + (eggPlant + growth)/20f, 2f), out IEnumerator tween)) {
            StartCoroutine(tween);
        }
    }
}
