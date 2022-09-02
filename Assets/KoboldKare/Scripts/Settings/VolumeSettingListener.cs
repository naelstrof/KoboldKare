using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityScriptableSettings {
public class VolumeSettingListener : MonoBehaviour {
    public UnityEngine.Rendering.Volume v;
    public SettingInt bloomOption;
    public SettingInt blurOption;
    public SettingFloat paniniOption;
    public SettingFloat postExposure;
    public SettingInt dofOption;
    public SettingInt filmGrainOption;

    private void OnEnable() {
        blurOption.changed += OnBlurChanged;
        OnBlurChanged(blurOption.GetValue());
        bloomOption.changed += OnBloomChanged;
        OnBloomChanged(bloomOption.GetValue());
        paniniOption.changed += OnPaniniOptionChanged;
        OnPaniniOptionChanged(paniniOption.GetValue());
        dofOption.changed += OnDepthOfFieldChanged;
        OnDepthOfFieldChanged(dofOption.GetValue());
        filmGrainOption.changed += OnFilmGrainChanged;
        OnFilmGrainChanged(filmGrainOption.GetValue());
        postExposure.changed += OnPostExposureChanged;
        OnPostExposureChanged(postExposure.GetValue());
    }
    private void OnDisable() {
        blurOption.changed -= OnBlurChanged;
        bloomOption.changed -= OnBloomChanged;
        paniniOption.changed -= OnPaniniOptionChanged;
        dofOption.changed -= OnDepthOfFieldChanged;
        filmGrainOption.changed -= OnFilmGrainChanged;
        postExposure.changed -= OnPostExposureChanged;
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

}