using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityScriptableSettings;

[CreateAssetMenu(fileName = "LookYAxisInverterSetting", menuName = "Unity Scriptable Setting/KoboldKare/Y-Axis Look Inverter Setting", order = 1)]
public class MouseInverterSetting : SettingLocalizedDropdown {
    private void TurnOffInvert(InputAction action) {
        for (int i = 0; i < action.bindings.Count; i++) {
            var binding = action.bindings[i];
            if (binding.overrideProcessors != null && binding.overrideProcessors.Contains("invertVector2(invertX=false,invertY=true")) {
                action.RemoveBindingOverride(i);
            }
        }
    }

    private void TurnOnInvert(InputAction action) {
        for (int i = 0; i < action.bindings.Count; i++) {
            var binding = action.bindings[i];
            binding.overrideProcessors = string.IsNullOrEmpty(binding.processors) ? "invertVector2(invertX=false,invertY=true)" : $"{binding.processors},invertVector2(invertX=false,invertY=true)";
            action.ApplyBindingOverride(i,binding);
        }
    }

    public override void SetValue(int value) {
        base.SetValue(value);
        var controls = GameManager.GetPlayerControls();
        if (value == 0) {
            TurnOffInvert(controls.Player.Look);
            TurnOffInvert(controls.Player.LookJoystick);
        } else {
            TurnOnInvert(controls.Player.Look);
            TurnOnInvert(controls.Player.LookJoystick);
        }
    }
}
