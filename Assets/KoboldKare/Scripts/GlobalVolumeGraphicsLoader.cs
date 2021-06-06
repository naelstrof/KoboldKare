using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeGraphicsLoader : MonoBehaviour, IGameEventOptionListener {
    public Volume v;
    public GraphicsOptions options;
    private float distance;
    protected Camera internalCamera;
    private Camera mainCamera {
        get {
            if (internalCamera == null || !internalCamera.isActiveAndEnabled) {
                internalCamera = Camera.main;
            }
            if (internalCamera == null || !internalCamera.isActiveAndEnabled) {
                internalCamera = Camera.current;
            }
            return internalCamera;
        }
    }
    void Start() {
        options.RegisterListener(this);
        foreach(GraphicsOptions.Option o in options.options) {
            OnEventRaised(o.type, o.value);
        }
    }
    public void FixedUpdate() {
        DepthOfField dof;
        v.profile.TryGet<DepthOfField>(out dof);
        RaycastHit hit;
        if (mainCamera == null) {
            return;
        }
        if (!Physics.SphereCast(mainCamera.transform.position-mainCamera.transform.forward, 0.5f, mainCamera.transform.forward, out hit, 300f, GameManager.instance.waterSprayHitMask, QueryTriggerInteraction.Ignore )) {
            hit.distance = 300f;
        }
        distance = Mathf.MoveTowards(distance, hit.distance, Time.deltaTime * Mathf.Abs(distance-hit.distance)*2f);
        float focusedRadius = distance/2f+10f;
        dof.focusDistance.Override(distance);
        //dof.nearFocusStart.Override(0f);
        //dof.nearFocusEnd.Override(Mathf.Max(0.1f, distance-focusedRadius));
        //dof.farFocusStart.Override(distance+focusedRadius);
        //dof.farFocusEnd.Override(distance+distance + focusedRadius);
    }
    void OnDestroy() {
        options.UnregisterListener(this);
    }
    public void OnEventRaised(GraphicsOptions.OptionType target, float value) {
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
    }
}
