using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityScriptableSettings;

public class SaveChangesButton : MonoBehaviour {
    private bool changed = false;

    private void Awake() {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    private void OnClicked() {
        UnityScriptableSettings.SettingsManager.Save();
        SetChanged(false);
    }

    void OnEnable() {
        var settings = UnityScriptableSettings.SettingsManager.GetSettings();
        foreach (var setting in settings) {
            if (setting is SettingInt sint) {
                sint.changed += OnChangedInt;
            } else if (setting is SettingFloat sfloat) {
                sfloat.changed += OnChangedFloat;
            } else if (setting is SettingString sstring) {
                sstring.changed += OnChangedString;
            }
        }
        GetComponent<Button>().interactable = changed;
    }

    private void OnDisable() {
        var settings = UnityScriptableSettings.SettingsManager.GetSettings();
        foreach (var setting in settings) {
            if (setting is SettingInt sint) {
                sint.changed -= OnChangedInt;
            } else if (setting is SettingFloat sfloat) {
                sfloat.changed -= OnChangedFloat;
            } else if (setting is SettingString sstring) {
                sstring.changed -= OnChangedString;
            }
        }
    }

    private void SetChanged(bool newChanged) {
        changed = newChanged;
        GetComponent<Button>().interactable = changed;
    }

    private void OnChangedFloat(float newValue) {
        SetChanged(true);
    }

    private void OnChangedString(string newValue) {
        SetChanged(true);
    }

    private void OnChangedInt(int newValue) {
        SetChanged(true);
    }
}
