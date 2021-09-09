using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityScriptableSettings {
[CreateAssetMenu(fileName = "GoopQuality", menuName = "Unity Scriptable Setting/KoboldKare/Goop Quality Setting", order = 1)]
public class GoopQualitySettings : ScriptableSettingSlider {
    public ForwardRendererData forwardRenderer;
    public RaymarchRenderFeature goopFeature;
    public override void SetValue(float value) {
        foreach(var feature in forwardRenderer.rendererFeatures) {
            if (feature.name.Contains("Raymarch") ) {
                feature.SetActive(value != 0);
                break;
            }
        }

        goopFeature.m_Settings.renderQuality = Mathf.Clamp01(value);
        if ( goopFeature.m_RaymarchRenderPass != null) {
            goopFeature.m_RaymarchRenderPass.m_Settings.renderQuality = Mathf.Clamp01(value);
        }

        base.SetValue(value);
    }
}

}