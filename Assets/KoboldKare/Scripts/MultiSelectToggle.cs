using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiSelectToggle : MonoBehaviour {
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    private TMP_Text label;
    private MultiSelectPanel.MultiSelectOption option;
    public void SetOption(MultiSelectPanel.MultiSelectOption newOption) {
        option = newOption;
        label.text = option.label;
        toggle.SetIsOnWithoutNotify(option.enabled);
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(bool value) {
        option.enabled = value;
        option.onValueChanged?.Invoke(value);
    }
}
