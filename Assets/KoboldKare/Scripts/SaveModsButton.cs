using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveModsButton : MonoBehaviour {
    private Button button;
    private void Awake() {
        button = GetComponent<UnityEngine.UI.Button>();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked() {
        ModManager.SaveConfig();
    }

    private void Update() {
        button.interactable = ModManager.GetChanged();
    }
}
