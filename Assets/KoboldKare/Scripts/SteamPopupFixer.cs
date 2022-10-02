using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityScriptableSettings;

[RequireComponent(typeof(ScriptableSettingSpawner))]
public class SteamPopupFixer : MonoBehaviour {
    private ScriptableSettingSpawner spawner;
    void OnEnable() {
        spawner = GetComponent<ScriptableSettingSpawner>();
        spawner.doneSpawning += OnDoneSpawning;
    }

    void OnDoneSpawning() {
        foreach (var inputField in GetComponentsInChildren<TMP_InputField>()) {
            inputField.gameObject.AddComponent<SteamPopupText>();
        }
    }
}
