using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityScriptableSettings;

[CreateAssetMenu(fileName = "LookYAxisInverterSetting", menuName = "Unity Scriptable Setting/KoboldKare/Y-Axis Look Inverter Setting", order = 1)]
public class MouseInverterSetting : SettingLocalizedDropdown {
    [SerializeField]
    private InputActionReference inputActionReference;

    public override void SetValue(int value) {
        base.SetValue(value);
        if (value == 0) {
            for (int i = 0; i < inputActionReference.action.bindings.Count; i++) {
                var binding = inputActionReference.action.bindings[i];
                if (binding.overrideProcessors != null && binding.overrideProcessors.Contains("invertVector2(invertX=false,invertY=true")) {
                    inputActionReference.action.RemoveBindingOverride(i);
                }
            }
        } else {
            for (int i = 0; i < inputActionReference.action.bindings.Count; i++) {
                var binding = inputActionReference.action.bindings[i];
                binding.overrideProcessors = "invertVector2(invertX=false,invertY=true)";
                inputActionReference.action.ApplyBindingOverride(i,binding);
            }
        }
    }
}
