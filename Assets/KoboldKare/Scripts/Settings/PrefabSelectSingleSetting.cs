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
        ModManager.AddFinishedLoadingListener(LoadDatabase);
    }

    private void OnDestroy() {
        ModManager.RemoveFinishedLoadingListener(LoadDatabase);
    }

    public override void Save() {
        if (string.IsNullOrEmpty(selectedPrefab)) {
            return;
        }
        PlayerPrefs.SetString(name, selectedPrefab);
    }

    public override void Load() {
        selectedPrefab = PlayerPrefs.GetString(name, null);
        if (!database.TryGetGroupName(out var groupName)) {
            return;
        }

        List<string> prefabNames = new List<string>();
        KoboldKareObjectPostProcessor.GetAllAssetNamesInGroup(groupName, prefabNames);
        if (string.IsNullOrEmpty(selectedPrefab)) {
            selectedValue = 0;
            return;
        }

        bool found = false;
        for (int i = 0; i < prefabNames.Count; i++) {
            var prefab = prefabNames[i];
            if (selectedPrefab != prefab) continue;
            selectedValue = i;
            found = true;
        }

        if (found) {
            return;
        }

        selectedValue = 0;
    }

    public override void SetValue(int value) {
        if (!database.TryGetGroupName(out var groupName)) {
            return;
        }

        List<string> prefabNames = new List<string>();
        KoboldKareObjectPostProcessor.GetAllAssetNamesInGroup(groupName, prefabNames);
        List<string> newOptions = new List<string>();
        foreach (var prefab in prefabNames) {
            newOptions.Add(prefab);
        }

        if (newOptions.Count == 0) {
            base.SetValue(value);
            return;
        }

        selectedPrefab = newOptions[Mathf.Clamp(value, 0, newOptions.Count-1)];
        base.SetValue(value);
    }

    private void LoadDatabase() {
        List<string> newOptions = new List<string>();
        if (!database.TryGetGroupName(out var groupName)) {
            return;
        }

        List<string> prefabNames = new List<string>();
        KoboldKareObjectPostProcessor.GetAllAssetNamesInGroup(groupName, prefabNames);
        foreach (var prefab in prefabNames) {
            newOptions.Add(prefab);
        }
        dropdownOptions = newOptions.ToArray();
        //Debug.Log("Options changed to count " + dropdownOptions.Length);
        int newSelectedValue = Mathf.Clamp(selectedValue, 0, newOptions.Count);
        if (string.IsNullOrEmpty(selectedPrefab)) {
            for (int i = 0; i < prefabNames.Count; i++) {
                var prefab = prefabNames[i];
                if (newSelectedValue == i) {
                    selectedPrefab = prefab;
                }
            }
        }
        SetValue(newSelectedValue);
        // There's a chance an update wasn't fired, due to the value not changing-- but dropdowns need to be refreshed.
        NotifyChange();
    }

    public bool TryGetPrefab(out string prefabName) {
        if (!database.TryGetGroupName(out var groupName)) {
            prefabName = selectedPrefab;
            return false;
        }
        if (KoboldKareObjectPostProcessor.HasAssetInGroup(groupName, selectedPrefab)) {
            prefabName = selectedPrefab;
            return true;
        }
        if (KoboldKareObjectPostProcessor.TryGetRandomAssetKey(groupName, out string randomPrefab)) {
            prefabName = randomPrefab;
            return true;
        }
        prefabName = selectedPrefab;
        return false;
    }
}
