using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityScriptableSettings;

[CreateAssetMenu(fileName = "LookYAxisInverterSetting", menuName = "Unity Scriptable Setting/KoboldKare/Y-Axis Look Inverter Setting", order = 1)]
public class MouseInverterSetting : SettingLocalizedDropdown {
    [SerializeField]
    private List<InputActionReference> inputActionReferences;

    private void TurnOffInvert(InputActionReference inputActionReference) {
        for (int i = 0; i < inputActionReference.action.bindings.Count; i++) {
            var binding = inputActionReference.action.bindings[i];
            if (binding.overrideProcessors != null && binding.overrideProcessors.Contains("invertVector2(invertX=false,invertY=true")) {
                inputActionReference.action.RemoveBindingOverride(i);
            }
        }
    }

    private void TurnOnInvert(InputActionReference inputActionReference) {
        for (int i = 0; i < inputActionReference.action.bindings.Count; i++) {
            var binding = inputActionReference.action.bindings[i];
            binding.overrideProcessors = string.IsNullOrEmpty(binding.processors) ? "invertVector2(invertX=false,invertY=true)" : $"{binding.processors},invertVector2(invertX=false,invertY=true)";
            inputActionReference.action.ApplyBindingOverride(i,binding);
        }
    }

    public override void SetValue(int value) {
        base.SetValue(value);
        if (value == 0) {
            foreach (var action in inputActionReferences) {
                TurnOffInvert(action);
            }
        } else {
            foreach (var action in inputActionReferences) {
                TurnOnInvert(action);
            }
        }
    }
}
