using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.HighDefinition;
//using Aura2API;
using KoboldKare;

public class FogDayAdjuster : MonoBehaviour {
    public Volume v;
    //public AuraVolume volume;
    public AnimationCurve attenuationDistance;
    //public AnimationCurve ambientCurve;
    //public AnimationCurve colorStrengthCurve;

    public Gradient colorGradient;

    //private Fog f;
    public void Start() {
        //v.profile.TryGet<Fog>(out f);
    }
    public void FixedUpdate() {
        //float d = attenuationDistance.Evaluate(DayNightCycle.instance.time01);
        //float a = ambientCurve.Evaluate(time.Get01FullCycle());
        //f.meanFreePath.Override(d);
        //volume.lightInjection.injectionParameters.strength = colorStrengthCurve.Evaluate(time.Get01FullCycle());
        //volume.lightInjection.color = colorGradient.Evaluate(time.Get01FullCycle());
        //volume.densityInjection.strength = d;
        //volume.ambientInjection.strength = a;
    }
}
