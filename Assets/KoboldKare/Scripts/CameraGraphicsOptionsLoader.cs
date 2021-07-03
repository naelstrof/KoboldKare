using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
//using UnityEngine.Rendering.HighDefinition;

public class CameraGraphicsOptionsLoader : MonoBehaviour, IGameEventOptionListener {
    //private FrameSettings frameSettings;
    //private FrameSettingsOverrideMask frameSettingsOverrideMask;
    void Start() {
        GraphicsOptions.instance.RegisterListener(this);
        //frameSettings = new FrameSettings();
        //frameSettingsOverrideMask = new FrameSettingsOverrideMask();
        foreach(GraphicsOptions.Option o in GraphicsOptions.instance.options) {
            OnEventRaised(o.type, o.value);
        }
    }
    void OnDestroy() {
        GraphicsOptions.instance.UnregisterListener(this);
    }

    public void OnEventRaised(GraphicsOptions.OptionType target, float value) {
        switch(target) {
            case GraphicsOptions.OptionType.CameraFOV:
                GetComponent<Camera>().fieldOfView = value;
                break;
            case GraphicsOptions.OptionType.AntiAliasing:
                GetComponent<Camera>().allowMSAA = (value != 0f);
                GetComponent<UniversalAdditionalCameraData>().antialiasing = value == 0 ? AntialiasingMode.None : AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                GetComponent<UniversalAdditionalCameraData>().antialiasingQuality = (AntialiasingQuality)Mathf.FloorToInt(value);
                break;
        }
        /*switch(target) {
            case GraphicsOptions.OptionType.Bloom:
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.Bloom] = true;
                frameSettings.SetEnabled(FrameSettingsField.Bloom, (value!=0f));
                break;
            case GraphicsOptions.OptionType.ScreenSpaceReflections:
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.SSR] = true;
                frameSettings.SetEnabled(FrameSettingsField.SSR, (value!=0f));
                break;
            case GraphicsOptions.OptionType.AmbientOcclusion:
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.SSAO] = true;
                frameSettings.SetEnabled(FrameSettingsField.SSAO, (value!=0f));
                break;
            case GraphicsOptions.OptionType.Shadows:
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.ShadowMaps] = true;
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.ContactShadows] = true;
                frameSettings.SetEnabled(FrameSettingsField.ShadowMaps, (value!=0f));
                frameSettings.SetEnabled(FrameSettingsField.ContactShadows, value>=2f);
                break;
            case GraphicsOptions.OptionType.MotionBlur:
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.MotionBlur] = true;
                frameSettings.SetEnabled(FrameSettingsField.MotionBlur, (value!=0f));
                break;
            case GraphicsOptions.OptionType.AntiAliasing:
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.Antialiasing] = true;
                frameSettings.SetEnabled(FrameSettingsField.Antialiasing, (value!=0f));
                // Skip temporal
                if (Mathf.FloorToInt(value) == 2) {
                    value+=1f;
                }
                GetComponent<HDAdditionalCameraData>().antialiasing = (HDAdditionalCameraData.AntialiasingMode)Mathf.FloorToInt(value);
                break;
            case GraphicsOptions.OptionType.VolumetricLighting:
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.Volumetrics] = true;
                frameSettings.SetEnabled(FrameSettingsField.Volumetrics, (value!=0f));
                break;
            case GraphicsOptions.OptionType.MaterialQuality:
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.MaterialQualityLevel] = true;
                switch(Mathf.FloorToInt(value)) {
                    case 0: frameSettings.materialQuality = Utilities.MaterialQuality.Low;break;
                    case 1: frameSettings.materialQuality = Utilities.MaterialQuality.Medium;break;
                    case 2: frameSettings.materialQuality = Utilities.MaterialQuality.High;break;
                }
                break;
            case GraphicsOptions.OptionType.SubsurfaceScattering:
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.SubsurfaceScattering] = true;
                frameSettings.SetEnabled(FrameSettingsField.SubsurfaceScattering, (value!=0f));
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.Transmission] = true;
                frameSettings.SetEnabled(FrameSettingsField.Transmission, (value!=0f));
                break;
            case GraphicsOptions.OptionType.CameraFOV:
                GetComponent<Camera>().fieldOfView = value;
                break;
            case GraphicsOptions.OptionType.PaniniProjection:
                frameSettingsOverrideMask.mask[(uint)FrameSettingsField.PaniniProjection] = true;
                frameSettings.SetEnabled(FrameSettingsField.PaniniProjection, (value!=0f));
                break;
            case GraphicsOptions.OptionType.DepthOfField:
                //frameSettingsOverrideMask.mask[(uint)FrameSettingsField.DepthOfField] = true;
                //frameSettings.SetEnabled(FrameSettingsField.DepthOfField, (value!=0f));
                break;
        }
        GetComponent<HDAdditionalCameraData>().customRenderingSettings = true;
        GetComponent<HDAdditionalCameraData>().renderingPathCustomFrameSettingsOverrideMask = frameSettingsOverrideMask;
        GetComponent<HDAdditionalCameraData>().renderingPathCustomFrameSettings = frameSettings;*/
    }
}
