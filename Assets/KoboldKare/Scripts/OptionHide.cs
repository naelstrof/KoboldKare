using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityScriptableSettings;

public class OptionHide : MonoBehaviour {
    [SerializeField] private SettingInt setting;
    [SerializeField] private List<GameObject> gameObjectsToHide;
    private void OnEnable() {
        setting.changed += OnChanged;
        OnChanged(setting.GetValue());
    }

    private void OnDisable() {
        setting.changed -= OnChanged;
    }

    void OnChanged(int value) {
        foreach (var obj in gameObjectsToHide) {
            obj.SetActive(value == 0);
        }
    }
}
