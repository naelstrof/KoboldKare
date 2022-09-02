using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace UnityScriptableSettings {

public class CameraSettingListener : MonoBehaviour {
    public SettingInt antiAliasing;
    public SettingFloat fov;
    private UniversalAdditionalCameraData camData;
    private Camera cam;
    // Start is called before the first frame update

    private void OnEnable() {
        cam = GetComponent<Camera>();
        camData = GetComponent<UniversalAdditionalCameraData>();
        antiAliasing.changed += OnAntiAliasingChanged;
        fov.changed += OnFOVChanged;
        
        OnAntiAliasingChanged(antiAliasing.GetValue());
        OnFOVChanged(fov.GetValue());
    }

    private void OnDisable() {
        antiAliasing.changed -= OnAntiAliasingChanged;
        fov.changed -= OnFOVChanged;
    }

    void OnAntiAliasingChanged(int value) {
        cam.allowMSAA = (value != 0f);
        camData.antialiasing = value == 0 ? AntialiasingMode.None : AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        switch(value-1) {
            case 0: camData.antialiasingQuality = AntialiasingQuality.Low; break;
            case 1: camData.antialiasingQuality = AntialiasingQuality.Medium; break;
            case 2: camData.antialiasingQuality = AntialiasingQuality.High; break;
        }
    }
    void OnFOVChanged(float value) {
        cam.fieldOfView = value;
    }
}

}