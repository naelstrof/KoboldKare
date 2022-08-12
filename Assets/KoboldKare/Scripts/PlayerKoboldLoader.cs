using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityScriptableSettings;

public class PlayerKoboldLoader : MonoBehaviour {
    private static readonly string[] settingNames = {"Sex", "Hue", "Brightness", "Saturation", "Dick", "TopBottom", "Thickness", "BoobSize", "KoboldSize"};
    public Kobold targetKobold;
    void Start() {
        KoboldGenes genes = new KoboldGenes();
        foreach(string settingName in settingNames) {
            var option = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting(settingName);
            option.onValueChange -= OnValueChange;
            option.onValueChange += OnValueChange;
            genes = ProcessOption(genes, option);
        }
        targetKobold.SetGenes(genes);
    }
    void OnDestroy() {
        foreach(string settingName in settingNames) {
            var option = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting(settingName);
            option.onValueChange -= OnValueChange;
        }
    }
    private static KoboldGenes ProcessOption(KoboldGenes genes, UnityScriptableSettings.ScriptableSetting setting) {
        switch(setting.name) {
            //case "Sex": targetKobold.sex = setting.value; break;
            case "Hue": genes.hue = (byte)Mathf.RoundToInt(setting.value*255f); break;
            case "Brightness": genes.brightness = (byte)Mathf.RoundToInt(setting.value*255f); break;
            case "Saturation": genes.saturation = (byte)Mathf.RoundToInt(setting.value*255f); break;
            case "Dick": genes.dickEquip = (setting.value == 0f) ? byte.MaxValue : (byte)0; break;
            //case "TopBottom": targetKobold.bodyProportion.SetTopBottom(setting.value); break;
            //case "Thickness": targetKobold.bodyProportion.SetThickness(setting.value); break;
            case "BoobSize": genes.breastSize = setting.value * 30f; break;
            case "KoboldSize": genes.baseSize = setting.value * 20f; break;
        }
        return genes;
    }

    public static KoboldGenes GetPlayerGenes() {
        KoboldGenes genes = new KoboldGenes();
        foreach (string setting in settingNames) {
            genes = ProcessOption(genes, ScriptableSettingsManager.instance.GetSetting(setting));
        }
        return genes;
    }

    public void OnValueChange(UnityScriptableSettings.ScriptableSetting setting) {
        targetKobold.SetGenes(ProcessOption(targetKobold.GetGenes(), setting));
    }
}
