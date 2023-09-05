using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityScriptableSettings;

[CreateAssetMenu(fileName = "New Prefab Select Single", menuName = "Unity Scriptable Setting/KoboldKare/Prefab Select Single", order = 1)]
public class PrefabSelectSingleSetting : SettingDropdown {
    [SerializeField]
    private PrefabDatabase database;

    private string selectedPrefab;
    
    private void OnEnable() {
        selectedPrefab = null;
        database.AddPrefabReferencesChangedListener(LoadDatabase);
    }

    private void OnDestroy() {
        database.RemovePrefabReferencesChangedListener(LoadDatabase);
    }

    public override void Save() {
        if (string.IsNullOrEmpty(selectedPrefab)) {
            return;
        }
        PlayerPrefs.SetString(name, selectedPrefab);
    }

    public override void Load() {
        selectedPrefab = PlayerPrefs.GetString(name, null);
        var prefabs = database.GetPrefabReferenceInfos();
        if (string.IsNullOrEmpty(selectedPrefab)) {
            selectedValue = 0;
            return;
        }

        bool found = false;
        for (int i = 0; i < prefabs.Count; i++) {
            var prefab = prefabs[i];
            if (selectedPrefab != prefab.GetKey()) continue;
            selectedValue = i;
            found = true;
        }

        if (found) {
            return;
        }

        selectedValue = 0;
    }

    public override void SetValue(int value) {
        var prefabs = database.GetPrefabReferenceInfos();
        List<string> newOptions = new List<string>();
        foreach (var prefab in prefabs) {
            if (!prefab.IsValid()) {
                continue;
            }
            newOptions.Add(prefab.GetKey());
        }

        if (newOptions.Count == 0) {
            base.SetValue(value);
            return;
        }

        selectedPrefab = newOptions[Mathf.Clamp(value, 0, newOptions.Count-1)];
        base.SetValue(value);
    }

    private void LoadDatabase(ReadOnlyCollection<PrefabDatabase.PrefabReferenceInfo> prefabs) {
        List<string> newOptions = new List<string>();
        foreach (var prefab in prefabs) {
            if (!prefab.IsValid()) {
                continue;
            }
            newOptions.Add(prefab.GetKey());
        }
        dropdownOptions = newOptions.ToArray();
        //Debug.Log("Options changed to count " + dropdownOptions.Length);
        int newSelectedValue = Mathf.Clamp(selectedValue, 0, newOptions.Count);
        if (string.IsNullOrEmpty(selectedPrefab)) {
            for (int i = 0; i < prefabs.Count; i++) {
                var prefab = prefabs[i];
                if (!prefab.IsValid()) {
                    continue;
                }
                if (newSelectedValue == i) {
                    selectedPrefab = prefab.GetKey();
                }
            }
        }
        SetValue(newSelectedValue);
        // There's a chance an update wasn't fired, due to the value not changing-- but dropdowns need to be refreshed.
        NotifyChange();
    }

    public string GetPrefab() {
        var prefabs = database.GetValidPrefabReferenceInfos();
        foreach (var prefab in prefabs) {
            if (prefab.GetKey() == selectedPrefab) {
                return prefab.GetKey();
            }
        }
        // try to return SOME valid prefab
        foreach (var prefab in prefabs) {
            return prefab.GetKey();
        }
        return selectedPrefab;
    }
}
