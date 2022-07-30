using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerKoboldLoader : MonoBehaviour {
    public static string[] settingNames = {"Sex", "Hue", "Brightness", "Saturation", "Contrast", "Dick", "TopBottom", "Thickness", "BoobSize", "KoboldSize"};
    public Kobold targetKobold;
    public UnityEvent onLoad;

    void Start() {
        foreach(string settingName in settingNames) {
            var option = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting(settingName);
            option.onValueChange -= OnValueChange;
            option.onValueChange += OnValueChange;
            ProcessOption(targetKobold, option);
        }
    }
    void OnDestroy() {
        foreach(string settingName in settingNames) {
            var option = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting(settingName);
            option.onValueChange -= OnValueChange;
        }
    }
    public static void ProcessOption(Kobold targetKobold, UnityScriptableSettings.ScriptableSetting setting) {
        KoboldGenes genes = targetKobold.GetGenes();
        switch(setting.name) {
            case "Sex": targetKobold.sex = setting.value; break;
            case "Hue": genes.hue = (byte)Mathf.RoundToInt(setting.value*255f); break;
            case "Brightness": genes.brightness = (byte)Mathf.RoundToInt(setting.value*255f); break;
            case "Saturation": genes.saturation = (byte)Mathf.RoundToInt(setting.value*255f); break;
            case "Contrast": genes.contrast = (byte)Mathf.RoundToInt(setting.value*255f); break;
            case "Dick": genes.dickEquip = (byte)Mathf.RoundToInt(setting.value); break;
            case "TopBottom": targetKobold.bodyProportion.SetTopBottom(setting.value); break;
            case "Thickness": targetKobold.bodyProportion.SetThickness(setting.value); break;
            case "BoobSize": genes.breastSize = setting.value * 30f; break;
            case "KoboldSize": genes.baseSize = setting.value * 20f; break;
        }
        targetKobold.SetGenes(genes);
    }
    public void OnValueChange(UnityScriptableSettings.ScriptableSetting setting) {
        ProcessOption(targetKobold, setting);
    }
}
