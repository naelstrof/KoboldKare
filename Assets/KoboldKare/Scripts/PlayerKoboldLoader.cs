using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerKoboldLoader : MonoBehaviour {
    public static string[] settingNames = {"Sex", "Hue", "Brightness", "Saturation", "Contrast", "Dick", "TopBottom", "Thickness", "BoobSize", "KoboldSize", "DickSize", "BallSize", "BallSizePow", "DickType", "PermanentArousal", "SpeedPow", "Speed", "JumpStrength", "Fatness", "Fertility", "MaxCumInBelly"};
    public Kobold targetKobold;
    public UnityEvent onLoad;
    //possible dick types of the dicktype menu
    public static string[] dickTypes = { "KandiDick", "EquineDick", "TaperedDick", "KnottedDick" };

    void Start() {
        foreach(string settingName in settingNames) {
            var option = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting(settingName);
            if(option == null){
                Debug.LogWarning("tried to retrieve missing setting: " + settingName);
                continue;
            }
            option.onValueChange -= OnValueChange;
            option.onValueChange += OnValueChange;
            ProcessOption(targetKobold, option);
        }
        targetKobold.isPlayer = true;
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
            case "TopBottom": targetKobold.topBottom = setting.value; break;
            case "Thickness": targetKobold.thickness = setting.value; break;
            case "BoobSize": {
                targetKobold.baseBoobSize = setting.value*30f;
                foreach (var boob in targetKobold.boobs) {
                    boob.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("Milk"), setting.value * 0.3f);
                }
                break;
            }
            case "KoboldSize": targetKobold.sizeInflatable.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("GrowthSerum"), setting.value * targetKobold.sizeInflatable.reagentVolumeDivisor); break;
            case "DickSize": targetKobold.baseDickSize = setting.value; break;
            case "BallSize": targetKobold.baseBallSize = Mathf.Pow(setting.value, UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("BallSizePow").value); break;
            case "BallSizePow": targetKobold.baseBallSize = Mathf.Pow(UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("BallSize").value, setting.value); break;
            case "DickType":{
                if(UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("Dick").value != 0){
                    KoboldInventory inventory = targetKobold.GetComponent<KoboldInventory>();
                    while(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch) != null) {
                        inventory.RemoveEquipment(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch),false);
                    }
                    inventory.PickupEquipment(EquipmentDatabase.GetEquipment(dickTypes[System.Convert.ToInt32(setting.value)]), null);
                }
                break;
            }
            case "PermanentArousal": targetKobold.permanentArousal = setting.value; break;
            case "SpeedPow":{
                KoboldCharacterController kkc = targetKobold.GetComponent<KoboldCharacterController>();
                float speed = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("Speed").value;
                kkc.speed = Mathf.Pow(speed, setting.value);
                float jumpStrength = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("JumpStrength").value;
                kkc.jumpStrength = Mathf.Pow(jumpStrength, setting.value);
                break;
            }
            case "Speed":{
                KoboldCharacterController kkc = targetKobold.GetComponent<KoboldCharacterController>();
                float exp = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("SpeedPow").value;
                kkc.speed = Mathf.Pow(setting.value, exp);
                break;
            }
            case "JumpStrength":{
                KoboldCharacterController kkc = targetKobold.GetComponent<KoboldCharacterController>();
                float exp = UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("SpeedPow").value;
                kkc.jumpStrength = Mathf.Pow(setting.value, exp);
                break;
            }
            case "Fatness":{
                var fat = ReagentDatabase.GetReagent("Fat");
                foreach (var ss in targetKobold.subcutaneousStorage) {
                    ss.GetContainer().OverrideReagent(fat, setting.value);
                }
                break;
            }
            case "Fertility": targetKobold.fertility = setting.value; break;
            case "MaxCumInBelly": targetKobold.maximumCum = setting.value; break;
        }
    }
    public void OnValueChange(UnityScriptableSettings.ScriptableSetting setting) {
        ProcessOption(targetKobold, setting);
    }
}
