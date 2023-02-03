using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class PrefabDatabaseMultiSelect : MultiSelectPanel {
    [SerializeField]
    private PrefabDatabase database;
    protected override void Start() {
        Refresh(database.GetPrefabReferenceInfos());
    }

    private void OnEnable() {
        database.AddPrefabReferencesChangedListener(Refresh);
        Refresh(database.GetPrefabReferenceInfos());
    }

    private void OnDisable() {
        database.RemovePrefabReferencesChangedListener(Refresh);
    }

    private void Refresh(ReadOnlyCollection<PrefabDatabase.PrefabReferenceInfo> prefabs) {
        List<MultiSelectOption> options = new List<MultiSelectOption>();
        foreach (var prefab in prefabs) {
            var option = new MultiSelectOption {
                label = prefab.GetKey(),
                enabled = prefab.IsValid()
            };
            option.onValueChanged += (newValue) => {
                prefab.SetEnabled(newValue);
            };
            options.Add(option);
        }
        SetOptions(options);
    }
}
