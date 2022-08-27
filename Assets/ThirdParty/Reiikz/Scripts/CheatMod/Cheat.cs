using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Reiikz.KoboldKare.Klamp;

namespace Reiikz.KoboldKare.Cheat {

    public class Cheat
    {

        public static void ProcessOption(Kobold targetKobold, UnityScriptableSettings.ScriptableSetting setting) {

            switch(setting.name){
                // case "KoboldSize": {
                //     setting.value * 20f; break;
                // }
                // case "BallSize": targetKobold.SetBaseBallsSize(Mathf.Pow(setting.value, UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("BallSizePow").value)); break;
                // case "BallSizePow": targetKobold.SetBaseBallsSize(Mathf.Pow(UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("BallSize").value, setting.value)); break;
                case "DickType":{
                    // if(UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("Dick").value != 0){
                    //     KoboldInventory inventory = targetKobold.GetComponent<KoboldInventory>();
                    //     while(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch) != null) {
                    //         inventory.RemoveEquipment(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch),false);
                    //     }
                    //     inventory.PickupEquipment(EquipmentDatabase.GetEquipment(ModAPI.getItemDatabases.DickDB[(int)setting.value]), null);
                    // }
                    break;
                }
                // case "DickSize": {
                //     targetKobold.SetBaseDickSize(Mathf.Pow(10, setting.value));
                //     break;
                // }
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
                // case "Fatness":{
                //     targetKobold.SetBaseFatness(setting.value);
                //     break;
                // }
                case "Fertility": targetKobold.fertility = setting.value; break;
                // case "MaxCumInBelly": targetKobold.maximumCum = setting.value; break;
                // case "Rainbow": {
                //     targetKobold.gay = setting.value == 1f;
                //     if(setting.value == 0f){
                //         targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation.With(r:UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("Hue").value);
                //         targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation.With(g:UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("Brightness").value);
                //     }
                //     break;
                // }
                case "RainbowStep":{
                    try{
                        LoadCustomParts.instance.rainbowStep = setting.value;
                    }catch(System.Exception e){}
                    break;
                }
                // case "MetabolizationRate": {
                //     targetKobold.metabolizationRate = setting.value;
                //     break;
                // }
                case "FIX_LAG": {
                    LoadCustomParts.CleanUpServer();
                    break;
                }
                case "WhatToSpawn": {
                    int howMuch = (int) UnityScriptableSettings.ScriptableSettingsManager.instance.GetSetting("HowMuchToSpawn").value;
                    int thing = (int) setting.value;
                    LoadCustomParts.instance.spawnShit(thing, howMuch);
                    break;
                }
            }

        }

    }


}