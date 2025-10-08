using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonModsSetAllActive : MonoBehaviour {
    [SerializeField]
    private bool active = false;
    private Button button;
    void OnEnable() {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }
    void OnDisable() {
        button.onClick.AddListener(OnClick);
    }
    
    void OnClick() {
        ModManager.AllModsSetActive(active);
    }
}
