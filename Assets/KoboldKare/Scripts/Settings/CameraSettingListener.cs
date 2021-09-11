using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace UnityScriptableSettings {

public class CameraSettingListener : MonoBehaviour {
    public ScriptableSetting antiAliasing;
    public ScriptableSetting fov;
    private UniversalAdditionalCameraData camData;
    private Camera cam;
    // Start is called before the first frame update
    void Start() {
        cam = GetComponent<Camera>();
        camData = GetComponent<UniversalAdditionalCameraData>();
        antiAliasing.onValueChange += OnValueChanged;
        fov.onValueChange += OnValueChanged;
        OnValueChanged(antiAliasing);
        OnValueChanged(fov);
    }
    public void OnValueChanged(ScriptableSetting option) {
        if (cam == null) {
            return;
        }
        if (option == antiAliasing) {
            cam.allowMSAA = (option.value != 0f);
            camData.antialiasing = option.value == 0 ? AntialiasingMode.None : AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            switch(Mathf.FloorToInt(option.value)-1) {
                case 0: camData.antialiasingQuality = AntialiasingQuality.Low; break;
                case 1: camData.antialiasingQuality = AntialiasingQuality.Medium; break;
                case 2: camData.antialiasingQuality = AntialiasingQuality.High; break;
            }
        }
        if (option == fov) {
            cam.fieldOfView = option.value;
        }
    }
}

}