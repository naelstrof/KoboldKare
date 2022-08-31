using System;
using System.Collections;
using System.Collections.Generic;
using Naelstrof.Inflatable;
using UnityEngine;

public class FoodDisplay : MonoBehaviour {
    [SerializeField]
    private Inflatable sizeInflater;

    private GenericReagentContainer container;

    private void OnEnable() {
        container = GetComponentInParent<GenericReagentContainer>();
        sizeInflater.OnEnable();
        container.OnChange.AddListener(OnReagentsChanged);
        OnReagentsChanged(container.GetContents(), GenericReagentContainer.InjectType.Inject);
    }

    private void OnDisable() {
        container.OnChange.RemoveListener(OnReagentsChanged);
    }

    void OnReagentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        sizeInflater.SetSize(0.5f+Mathf.Log(1f + container.volume / 20f, 2f), this);
    }
}
