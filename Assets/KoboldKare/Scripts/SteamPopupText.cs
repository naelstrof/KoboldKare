using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SteamPopupText : MonoBehaviour {
    [SerializeField]
    private EFloatingGamepadTextInputMode mode;

    private TMP_InputField targetField;
    private void OnEnable() {
        targetField = GetComponent<TMP_InputField>();
        targetField.onSelect.AddListener(OnSelect);
        targetField.onDeselect.AddListener(OnDeselect);
    }

    private void OnDisable() {
        targetField.onSelect.RemoveListener(OnSelect);
        targetField.onDeselect.RemoveListener(OnDeselect);
    }

    void OnSelect(string str) {
        RectTransform target = targetField.GetComponent<RectTransform>();
        if (SteamManager.Initialized) {
            SteamUtils.ShowFloatingGamepadTextInput(mode, (int)target.position.x, (int)target.position.y,
                (int)target.sizeDelta.x, (int)target.sizeDelta.y);
        }
    }

    void OnDeselect(string str) {
        if (SteamManager.Initialized) {
            SteamUtils.DismissFloatingGamepadTextInput();
        }
    }
}
