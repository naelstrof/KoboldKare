using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityScriptableSettings;

public class PlayerKoboldLoader : MonoBehaviour {
    private static readonly string[] settingNames = {"ClothingHue", "Hue", "Brightness", "Saturation", "BoobSize", "KoboldSize", "DickSize", "DickThickness", "BallSize"};
    private Kobold targetKobold;
    void OnEnable() {
        targetKobold = GetComponent<Kobold>();
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
        targetKobold.SetGenes(GetPlayerGenes());
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
    private static KoboldGenes ProcessOption(KoboldGenes genes, SettingInt setting) {
        switch (setting.name) {
            case "Dick":
                var database = GameManager.GetPenisDatabase();
                var validInfos = database.GetValidPrefabReferenceInfos();
                var info = database.GetInfoByName("HumanoidDick");
                if (info == null) {
                    genes.dickEquip = (setting.GetValue() == 0f) ? CommandDick.unEquipID : (short)1;
                } else {
                    genes.dickEquip = (setting.GetValue() == 0f) ? CommandDick.unEquipID : (short)(validInfos.IndexOf(info) + 1);
                }

                break;
        }
        return genes;
    }
    private static KoboldGenes ProcessOption(KoboldGenes genes, SettingFloat setting) {
        switch(setting.name) {
            case "Hue": genes.hue = (byte)Mathf.RoundToInt(setting.GetValue()*255f); break;
            case "ClothingHue": genes.clothingHue = (byte)Mathf.RoundToInt(setting.GetValue()*255f); break;
            case "Brightness": genes.brightness = (byte)Mathf.RoundToInt(setting.GetValue()*255f); break;
            case "Saturation": genes.saturation = (byte)Mathf.RoundToInt(setting.GetValue()*255f); break;
            case "DickSize": genes.dickSize = Mathf.Lerp(0f, 10f, setting.GetValue()); break;
            case "BallSize": genes.ballSize = Mathf.Lerp(5f, 10f, setting.GetValue()); break;
            case "DickThickness": genes.dickThickness = Mathf.Lerp(0.3f, 0.7f, setting.GetValue()); break;
            case "BoobSize": genes.breastSize = setting.GetValue() * 30f; break;
            case "KoboldSize": genes.baseSize = setting.GetValue() * 20f; break;
        }
        return genes;
    }

    public static KoboldGenes GetPlayerGenes() {
        KoboldGenes genes = new KoboldGenes();
        foreach (string setting in settingNames) {
            genes = ProcessOption(genes, SettingsManager.GetSetting(setting) as SettingFloat);
        }
        genes = ProcessOption(genes, SettingsManager.GetSetting("Dick") as SettingInt);
        //genes = genes.With(species: 
        var prefabSelect = SettingsManager.GetSetting("PlayablePrefabSelect") as PrefabSelectSingleSetting;
        var database = GameManager.GetPlayerDatabase().GetValidPrefabReferenceInfos();
        for(int i=0;i<database.Count;i++) {
            if (prefabSelect.GetPrefab() == database[i].GetKey()) {
                genes = genes.With(species: (byte)i);
                break;
            }
        }

        return genes;
    }

    void OnValueChange(int newValue) {
        targetKobold.SetGenes(GetPlayerGenes());
    }

    void OnValueChange(float newValue) {
        targetKobold.SetGenes(GetPlayerGenes());
    }
}
