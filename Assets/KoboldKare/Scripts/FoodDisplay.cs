using System;
using System.Collections;
using System.Collections.Generic;
using Naelstrof.Inflatable;
using UnityEngine;

public class FoodDisplay : MonoBehaviour {
    [SerializeField]
    private Inflatable sizeInflater;

    private NetworkedEntity networkedEntity;

    private void OnEnable() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        sizeInflater.OnEnable();
        
        if (networkedEntity != null) {
            networkedEntity.reagentContents.OnChange += OnReagentsChanged;
            OnReagentsChanged(networkedEntity.reagentContents.Value, networkedEntity.reagentContents.Value, true);
        }

    }

    private void OnDisable() {
        if (networkedEntity != null) {
            networkedEntity.reagentContents.OnChange -= OnReagentsChanged;
        }
    }

    void OnReagentsChanged(ReagentContents contents, ReagentContents next, bool asServer) {
        sizeInflater.SetSize(0.5f+Mathf.Log(1f + next.volume / 20f, 2f), this);
    }
}
