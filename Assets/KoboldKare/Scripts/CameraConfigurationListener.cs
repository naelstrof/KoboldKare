using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityScriptableSettings;


public class CameraConfigurationListener : MonoBehaviour {
    private Volume v;
    private VolumeProfile mainProfile;
    
    private SettingInt antiAliasing;
    private SettingInt bloomOption;
    private SettingInt blurOption;
    private SettingFloat paniniOption;
    private SettingFloat postExposure;
    private SettingFloat fovOption;
    private SettingInt dofOption;
    private SettingInt filmGrainOption;
    IEnumerator Start() {
        var handle = Addressables.LoadAssetAsync<VolumeProfile>("Assets/KoboldKare/ScriptableObjects/Main Camera Profile.asset");
        yield return handle;
        mainProfile = handle.Result;
        v = gameObject.AddComponent<Volume>();
        v.priority = -1;
        v.profile = mainProfile;
        
        var cam = GetComponent<Camera>();
        cam.nearClipPlane = 0.05f;
        
        var camData = GetComponent<UniversalAdditionalCameraData>();
        camData.dithering = true;
        camData.renderPostProcessing = true;
        
        yield return new WaitUntil(() => SettingsManager.GetSetting("AntiAliasing") != null);
        antiAliasing = SettingsManager.GetSetting("AntiAliasing") as SettingInt;
        antiAliasing.changed += OnAntiAliasingChanged;
        OnAntiAliasingChanged(antiAliasing.GetValue());
        
        yield return new WaitUntil(() => SettingsManager.GetSetting("ScreenBrightness") != null);
        postExposure = SettingsManager.GetSetting("ScreenBrightness") as SettingFloat;
        postExposure.changed += OnPostExposureChanged;
        OnPostExposureChanged(postExposure.GetValue());
        
        yield return new WaitUntil(() => SettingsManager.GetSetting("Bloom") != null);
        bloomOption = SettingsManager.GetSetting("Bloom") as SettingInt;
        bloomOption.changed += OnBloomChanged;
        OnBloomChanged(bloomOption.GetValue());
        
        yield return new WaitUntil(() => SettingsManager.GetSetting("MotionBlur") != null);
        blurOption = SettingsManager.GetSetting("MotionBlur") as SettingInt;
        blurOption.changed += OnBlurChanged;
        OnBlurChanged(blurOption.GetValue());
        
        yield return new WaitUntil(() => SettingsManager.GetSetting("PaniniProjection") != null);
        paniniOption = SettingsManager.GetSetting("PaniniProjection") as SettingFloat;
        paniniOption.changed += OnPaniniOptionChanged;
        OnPaniniOptionChanged(paniniOption.GetValue());
        
        yield return new WaitUntil(() => SettingsManager.GetSetting("DepthOfField") != null);
        dofOption = SettingsManager.GetSetting("DepthOfField") as SettingInt;
        dofOption.changed += OnDepthOfFieldChanged;
        OnDepthOfFieldChanged(dofOption.GetValue());
        
        yield return new WaitUntil(() => SettingsManager.GetSetting("FilmGrain") != null);
        filmGrainOption = SettingsManager.GetSetting("FilmGrain") as SettingInt;
        filmGrainOption.changed += OnFilmGrainChanged;
        OnFilmGrainChanged(filmGrainOption.GetValue());
        
        yield return new WaitUntil(() => SettingsManager.GetSetting("CameraFOV") != null);
        fovOption = SettingsManager.GetSetting("CameraFOV") as SettingFloat;
        fovOption.changed += OnFOVChanged;
        OnFOVChanged(fovOption.GetValue());
    }

    private void OnEnable() {
        if (antiAliasing != null) {
            antiAliasing.changed += OnAntiAliasingChanged;
            OnAntiAliasingChanged(antiAliasing.GetValue());
        }
        if (postExposure != null) {
            postExposure.changed += OnPostExposureChanged;
            OnPostExposureChanged(postExposure.GetValue());
        }
        if (bloomOption != null) {
            bloomOption.changed += OnBloomChanged;
            OnBloomChanged(bloomOption.GetValue());
        }
        if (blurOption != null) {
            blurOption.changed += OnBlurChanged;
            OnBlurChanged(blurOption.GetValue());
        }
        if (paniniOption != null) {
            paniniOption.changed += OnPaniniOptionChanged;
            OnPaniniOptionChanged(paniniOption.GetValue());
        }
        if (dofOption != null) {
            dofOption.changed += OnDepthOfFieldChanged;
            OnDepthOfFieldChanged(dofOption.GetValue());
        }
        if (filmGrainOption != null) {
            filmGrainOption.changed += OnFilmGrainChanged;
            OnFilmGrainChanged(filmGrainOption.GetValue());
        }
        if (fovOption != null) {
            fovOption.changed += OnFOVChanged;
            OnFOVChanged(fovOption.GetValue());
        }
    }

    private void OnDisable() {
        if (antiAliasing != null) {
            antiAliasing.changed -= OnAntiAliasingChanged;
        }
        if (postExposure != null) {
            postExposure.changed -= OnPostExposureChanged;
        }
        if (bloomOption != null) {
            bloomOption.changed -= OnBloomChanged;
        }
        if (blurOption != null) {
            blurOption.changed -= OnBlurChanged;
        }
        if (paniniOption != null) {
            paniniOption.changed -= OnPaniniOptionChanged;
        }
        if (dofOption != null) {
            dofOption.changed -= OnDepthOfFieldChanged;
        }
        if (filmGrainOption != null) {
            filmGrainOption.changed -= OnFilmGrainChanged;
        }
        if (fovOption != null) {
            fovOption.changed -= OnFOVChanged;
        }
    }
    void OnAntiAliasingChanged(int value) {
        GetComponent<Camera>().allowMSAA = (value != 0f);
        var camData = GetComponent<UniversalAdditionalCameraData>();
        camData.antialiasing = value == 0 ? AntialiasingMode.None : AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        switch(value-1) {
            case 0: camData.antialiasingQuality = AntialiasingQuality.Low; break;
            case 1: camData.antialiasingQuality = AntialiasingQuality.Medium; break;
            case 2: camData.antialiasingQuality = AntialiasingQuality.High; break;
        }
    }
    
    void OnFOVChanged(float value) {
        GetComponent<Camera>().fieldOfView = value;
    }

    void OnBlurChanged(int value) {
        if (!v.profile.TryGet(out MotionBlur blur)) { return; }
        blur.active = (value!=0);
        switch(value-1) {
            case 0: blur.quality.Override(MotionBlurQuality.Low); break;
            case 1: blur.quality.Override(MotionBlurQuality.Medium); break;
            case 2: blur.quality.Override(MotionBlurQuality.High); break;
        }
    }

    void OnBloomChanged(int value) {
        if (!v.profile.TryGet(out Bloom bloom)) { return; }
        bloom.active = (value!=0);
        bloom.highQualityFiltering.Override(value>1);
    }

    void OnPaniniOptionChanged(float value) {
        if (!v.profile.TryGet(out PaniniProjection proj)) { return; }
        //proj.cropToFit = new UnityEngine.Rendering.ClampedFloatParameter(1f-value, 0f, 1f, true);
        proj.active = (value!=0);
        proj.distance.Override(value);
        proj.cropToFit.Override(1f);
    }

    void OnDepthOfFieldChanged(int value) {
        if (!v.profile.TryGet(out DepthOfField depth)) { return; }
        depth.active = (value!=0);
        depth.mode.Override(DepthOfFieldMode.Bokeh);
    }

    void OnFilmGrainChanged(int value) {
        if (!v.profile.TryGet(out FilmGrain grain)) { return; }
        grain.active = value!=0;
    }

    void OnPostExposureChanged(float value) {
        if (!v.profile.TryGet(out ColorAdjustments adjustments)) { return; }
        adjustments.postExposure.Override(value);
    }
}
