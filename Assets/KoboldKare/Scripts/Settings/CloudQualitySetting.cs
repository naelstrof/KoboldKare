using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityScriptableSettings {
[CreateAssetMenu(fileName = "CloudQuality", menuName = "Unity Scriptable Setting/KoboldKare/Cloud Quality Setting", order = 1)]
public class CloudQualitySetting : ScriptableSettingLocalizedDropdown {
    public UniversalRendererData forwardRenderer;
    public Material cloudMaterial;
    public Material skyboxMaterial;
    public override void SetValue(float value) {
        // Clouds don't work on OpenGL, sorry!
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore) {
            value = 0f;
        }

        foreach(var feature in forwardRenderer.rendererFeatures) {
            if (feature.name.Contains("Cloud") ) {
                feature.SetActive(value != 0);
                break;
            }
        }
        cloudMaterial.SetFloat("numStepsLight", value == 2 ? 12 : 8);
        if (value >= 2) {
            cloudMaterial.EnableKeyword("CLOUD_SHADOWS_ON");
        } else {
            cloudMaterial.DisableKeyword("CLOUD_SHADOWS_ON");
        }
        if (value == 0) {
            skyboxMaterial.EnableKeyword("_CLOUDS_ON");
        } else {
            skyboxMaterial.DisableKeyword("_CLOUDS_ON");
        }

        base.SetValue(value);
    }
}

}