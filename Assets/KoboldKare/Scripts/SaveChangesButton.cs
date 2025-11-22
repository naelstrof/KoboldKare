using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Samples.RebindUI;
using UnityEngine.UI;
using UnityScriptableSettings;

public class SaveChangesButton : MonoBehaviour {
    private static bool changed = false;

    private void Awake() {
        GetComponent<Button>().onClick.AddListener(OnClicked);
        InputSystem.onActionChange += OnActionChanged;
    }

    private void OnActionChanged(object arg1, InputActionChange arg2) {
        if (arg2 == InputActionChange.BoundControlsChanged) {
            SetChanged(true);
        }
    }

    private void OnClicked() {
        SettingsManager.Save();
        InputOptions.SaveControls();
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
