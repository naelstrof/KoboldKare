using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityScriptableSettings;

public class PlayerKoboldLoader : MonoBehaviour {
    private static readonly string[] settingNames = {"ClothingHue", "Hue", "Brightness", "Saturation", "BoobSize", "KoboldSize", "DickSize", "DickThickness", "BallSize"};
    // FIXME: FISHNET
    private NetworkedKobold targetKobold;
    void OnEnable() {
        //targetKobold = GetComponent<Kobold>();
        targetKobold = GetComponentInParent<NetworkedKobold>();
        foreach(string settingName in settingNames) {
            var option = SettingsManager.GetSetting(settingName);
            if (option is SettingFloat optionFloat) {
                optionFloat.changed -= OnValueChange;
                optionFloat.changed += OnValueChange;
            } else {
                throw new UnityException($"Setting {settingName} is not a SettingFloat");
            }
        }
        var dickOption = SettingsManager.GetSetting("Dick");
        if (dickOption is SettingInt optionInt) {
            optionInt.changed -= OnValueChange;
            optionInt.changed += OnValueChange;
        } else {
            throw new UnityException($"Setting Dick is not a SettingInt");
        }
        ApplyPlayerGenes(targetKobold);
    }
    
    void OnDisable() {
        foreach(string settingName in settingNames) {
            var option = SettingsManager.GetSetting(settingName);
            if (option is SettingFloat optionFloat) {
                optionFloat.changed -= OnValueChange;
            }
        }
        var dickOption = SettingsManager.GetSetting("Dick");
        if (dickOption is SettingInt optionInt) {
            optionInt.changed -= OnValueChange;
        }
    }
    private static void ProcessOption(GeneHolder holder, SettingInt setting) {
        switch (setting.name) {
            case "Dick":
                holder.SetDickEquip((setting.GetValue() == 0f) ? "None" : "HumanoidDick");
                break;
        }
    }
    private static void ProcessOption(GeneHolder holder, SettingFloat setting) {
        switch(setting.name) {
            case "Hue": holder.SetHue((byte)Mathf.RoundToInt(setting.GetValue()*255f)); break;
            case "ClothingHue": holder.SetClothingHue((byte)Mathf.RoundToInt(setting.GetValue()*255f)); break;
            case "Brightness": holder.SetBrightness((byte)Mathf.RoundToInt(setting.GetValue()*255f)); break;
            case "Saturation": holder.SetSaturation((byte)Mathf.RoundToInt(setting.GetValue()*255f)); break;
            case "DickSize": holder.SetDickSize(Mathf.Lerp(0f, 10f, setting.GetValue())); break;
            case "BallSize": holder.SetBallSize(Mathf.Lerp(5f, 10f, setting.GetValue())); break;
            case "DickThickness": holder.SetDickThickness(Mathf.Lerp(0.3f, 0.7f, setting.GetValue())); break;
            case "BoobSize": holder.SetBreastSize(setting.GetValue() * 30f); break;
            case "KoboldSize": holder.SetBaseSize(setting.GetValue() * 20f); break;
        }
    }

    public static void ApplyPlayerGenes(GeneHolder target) {
        target.SetFatSize(0f);
        foreach (string setting in settingNames) {
            ProcessOption(target, SettingsManager.GetSetting(setting) as SettingFloat);
        }
        ProcessOption(target, SettingsManager.GetSetting("Dick") as SettingInt);
        var prefabSelect = SettingsManager.GetSetting("PlayablePrefabSelect") as PrefabSelectSingleSetting;
        if (prefabSelect != null && prefabSelect.TryGetPrefab(out string prefabName)) {
            target.SetSpecies(prefabName);
        } else {
            target.SetSpecies("Kobold");
        }
    }

    void OnValueChange(int newValue) {
        ApplyPlayerGenes(targetKobold);
    }

    void OnValueChange(float newValue) {
        ApplyPlayerGenes(targetKobold);
    }
}
