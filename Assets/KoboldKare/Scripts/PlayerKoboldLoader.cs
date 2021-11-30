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
        switch(setting.name) {
            case "Sex": targetKobold.sex = setting.value; break;
            case "Hue": targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation.With(r:setting.value); break;
            case "Brightness": targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation.With(g:setting.value); break;
            case "Saturation": targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation.With(b:setting.value); break;
            case "Contrast": targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation = targetKobold.HueBrightnessContrastSaturation.With(a:setting.value); break;
            case "Dick":  {
                KoboldInventory inventory = targetKobold.GetComponent<KoboldInventory>();
                if (inventory.Contains(EquipmentDatabase.GetEquipment("EquineDick"))) {
                    break;
                }
                inventory.PickupEquipment(EquipmentDatabase.GetEquipment("EquineDick"), null);
                break;
            }
            case "TopBottom": targetKobold.topBottom = setting.value; break;
            case "Thickness": targetKobold.thickness = setting.value; break;
            case "BoobSize": {
                foreach (var boob in targetKobold.boobs) {
                    boob.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("Fat"), setting.value * boob.reagentVolumeDivisor * 0.7f);
                    boob.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("Milk"), setting.value * boob.reagentVolumeDivisor * 0.3f);
                }
                break;
            }
            case "KoboldSize": targetKobold.sizeInflatable.GetContainer().OverrideReagent(ReagentDatabase.GetReagent("GrowthSerum"), setting.value * targetKobold.sizeInflatable.reagentVolumeDivisor); break;
        }
    }
    public void OnValueChange(UnityScriptableSettings.ScriptableSetting setting) {
        ProcessOption(targetKobold, setting);
    }
}
