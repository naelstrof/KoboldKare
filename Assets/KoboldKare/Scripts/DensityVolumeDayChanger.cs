using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering.HighDefinition;

//[RequireComponent(typeof(DensityVolume))]
public class DensityVolumeDayChanger : MonoBehaviour {
    public AnimationCurve FogDistanceCurve;
    public Gradient ColorCurve;
    //private DensityVolume volume;
    private void Start() {
        //volume = GetComponent<DensityVolume>();
    }
    void FixedUpdate() {
        //float fogDistanceSample = FogDistanceCurve.Evaluate(DayNightCycle.instance.time01);
        //Color colorSample = ColorCurve.Evaluate(DayNightCycle.instance.time01);
        //volume.parameters.albedo = colorSample;
        //volume.parameters.meanFreePath = fogDistanceSample;
    }
}
