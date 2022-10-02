using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class DS4 : MonoBehaviour {
    [SerializeField]
    private TextAsset customLayout;
    private void Start() {
        InputSystem.RegisterLayoutOverride(customLayout.text, "DualShock4GamepadHID");
    }
    public static Quaternion GetRotation(float scale = 1) {
        Gamepad pad = Gamepad.current;
        if (pad == null) {
            return Quaternion.identity;
        }
        if (pad.name != "DualShock4GamepadHID") {
            return Quaternion.identity;
        }
        float x = ProcessRawData(pad.GetChildControl<ButtonControl>("gyro X 14").ReadValue()) * scale;
        float y = ProcessRawData(pad.GetChildControl<ButtonControl>("gyro Y 16").ReadValue()) * scale;
        float z = -ProcessRawData(pad.GetChildControl<ButtonControl>("gyro Z 18").ReadValue()) * scale;
        return Quaternion.Euler(x, y, z);
    }
    private static float ProcessRawData(float data) {
        return data > 0.5f ? 1f - data : -data;
    }
}