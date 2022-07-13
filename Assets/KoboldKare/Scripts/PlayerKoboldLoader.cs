using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerKoboldLoader : MonoBehaviour {
    public static string[] settingNames = {"Sex", "Hue", "Brightness", "Saturation", "Contrast", "Dick", "TopBottom", "Thickness", "BoobSize", "KoboldSize", "DickSize", "BallSize", "BallSizePow", "DickType", "PermanentArousal", "SpeedPow", "Speed", "JumpStrength", "Fatness", "Fertility", "MaxCumInBelly", "Rainbow", "RainbowStep", "MetabolizationRate", "FIX_LAG", "WhatToSpawn", "HowMuchToSpawn"};
    public Kobold targetKobold;
    public UnityEvent onLoad;
    //possible dick types of the dicktype menu
    public static string[] dickTypes = { "KandiDick", "EquineDick", "TaperedDick", "KnottedDick" };
    public LoadCustomParts customScript = null;

    void Start() {
        targetKobold.isPlayer = true;
        foreach(string settingName in settingNames) {
            var option = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting(settingName);
            if(option == null){
                Debug.LogWarning("tried to retrieve missing setting: " + settingName);
                continue;
            }
            option.onValueChange -= OnValueChange;
            option.onValueChange += OnValueChange;
            if(!settingName.Equals("FIX_LAG")) ProcessOption(targetKobold, option);
            if(!settingName.Equals("WhatToSpawn")) ProcessOption(targetKobold, option);
        }
    }
    void OnDestroy() {
        foreach(string settingName in settingNames) {
            var option = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting(settingName);
            option.onValueChange -= OnValueChange;
        }
    }
    public static void ProcessOption(Kobold targetKobold, UnityScriptableSettings.ScriptableSetting setting) {
        switch(setting.name) {
            case "Sex": targetKobold.sex = setting.value; break;
            case "Hue": targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation.With(r:setting.value); break;
            case "Brightness": targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation.With(g:setting.value); break;
            case "Saturation": targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation.With(b:setting.value); break;
            case "Contrast": targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation.With(a:setting.value); break;
            case "Dick":  {
                KoboldInventory inventory = targetKobold.GetComponent<KoboldInventory>();

                //Add Dicks
                if(setting.value !=0){
                    if (inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch) != null) {
                        break; //Don't add dicks if we already have one
                    }
                    try{
                        inventory.PickupEquipment(EquipmentDatabase.GetEquipment(dickTypes[System.Convert.ToInt32(UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("DickType").value)]), null);
                    }catch(System.Exception e){
                        inventory.PickupEquipment(EquipmentDatabase.GetEquipment("EquineDick"), null);
                    }
                } else { //Remove Dicks
                    while(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch) != null) {
                        inventory.RemoveEquipment(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch),false);
                    }
                }
                
                break;
            }
            case "TopBottom": targetKobold.bodyProportion.SetTopBottom(setting.value); break;
            case "Thickness": targetKobold.bodyProportion.SetThickness(setting.value); break;
            case "BoobSize": {
                targetKobold.SetBaseBoobSize(setting.value*30f);
                break;
            }
            case "KoboldSize": targetKobold.SetBaseSize(setting.value*20f); break;
        }
    }
    public void OnValueChange(UnityScriptableSettings.ScriptableSetting setting) {
        ProcessOption(targetKobold, setting);
    }
}
