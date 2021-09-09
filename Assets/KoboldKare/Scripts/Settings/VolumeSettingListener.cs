using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityScriptableSettings {
public class VolumeSettingListener : MonoBehaviour {
    public UnityEngine.Rendering.Volume v;
    public ScriptableSetting bloomOption;
    public ScriptableSetting blurOption;
    public ScriptableSetting paniniOption;
    public ScriptableSetting dofOption;
    public ScriptableSetting filmGrainOption;
    void Start() {
        bloomOption.onValueChange += OnValueChange;
        blurOption.onValueChange += OnValueChange;
        paniniOption.onValueChange += OnValueChange;
        dofOption.onValueChange += OnValueChange;
        filmGrainOption.onValueChange += OnValueChange;
        OnValueChange(blurOption);
        OnValueChange(bloomOption);
        OnValueChange(paniniOption);
        OnValueChange(dofOption);
        OnValueChange(filmGrainOption);
    }
    void OnValueChange(ScriptableSetting option) {
        if (option == blurOption) {
            MotionBlur blur;
            if (!v.profile.TryGet<MotionBlur>(out blur)) { return; }
            blur.active = (option.value!=0);
            switch(Mathf.FloorToInt(option.value)-1) {
                case 0: blur.quality.Override(MotionBlurQuality.Low); break;
                case 1: blur.quality.Override(MotionBlurQuality.Medium); break;
                case 2: blur.quality.Override(MotionBlurQuality.High); break;
            }
        }
        if (option == bloomOption) {
            Bloom bloom;
            if (!v.profile.TryGet<Bloom>(out bloom)) { return; }
            bloom.active = (option.value!=0);
            bloom.highQualityFiltering.Override(option.value>1);
        }
        if (option == paniniOption) {
            PaniniProjection proj;
            if (!v.profile.TryGet<PaniniProjection>(out proj)) { return; }
            //proj.cropToFit = new UnityEngine.Rendering.ClampedFloatParameter(1f-value, 0f, 1f, true);
            proj.active = (option.value!=0);
            proj.distance.Override(option.value);
            proj.cropToFit.Override(1f);
        }
        if (option == dofOption) {
            DepthOfField depth;
            if (!v.profile.TryGet<DepthOfField>(out depth)) { return; }
            depth.active = (option.value!=0);
            depth.mode.Override(DepthOfFieldMode.Bokeh);
        }
        if (option == filmGrainOption) {
            FilmGrain grain;
            if (!v.profile.TryGet<FilmGrain>(out grain)) { return; }
            grain.active = (option.value!=0);
        }
    }
/*public void OnEventRaised(GraphicsOptions.OptionType target, float value) {
        switch(target) {
            case GraphicsOptions.OptionType.DepthOfField:
                DepthOfField depth;
                if (!v.profile.TryGet<DepthOfField>(out depth)) { break; }
                depth.active = (value!=0);
                depth.mode.Override(DepthOfFieldMode.Bokeh);
            break;
            case GraphicsOptions.OptionType.PaniniProjection:
                PaniniProjection proj;
                if (!v.profile.TryGet<PaniniProjection>(out proj)) { break; }
                //proj.cropToFit = new UnityEngine.Rendering.ClampedFloatParameter(1f-value, 0f, 1f, true);
                proj.active = (value!=0);
                proj.distance.Override(value);
                proj.cropToFit.Override(1f);
            break;
            case GraphicsOptions.OptionType.Bloom:
                Bloom bloom;
                if (!v.profile.TryGet<Bloom>(out bloom)) { break; }
                bloom.active = (value!=0);
                bloom.highQualityFiltering.Override(value>1);
                //bloom.quality.Override((int)(value-1));
            break;
            case GraphicsOptions.OptionType.FilmGrain:
                FilmGrain grain;
                if (!v.profile.TryGet<FilmGrain>(out grain)) { break; }
                grain.active = (value!=0);
            break;
            case GraphicsOptions.OptionType.MotionBlur:
                MotionBlur motion;
                if (!v.profile.TryGet<MotionBlur>(out motion)) { break; }
                motion.active = (value!=0);
                motion.quality.Override((MotionBlurQuality)(value-1));
            break;
        }
        return;
    }*/
}

}