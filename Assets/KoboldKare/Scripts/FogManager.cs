using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogManager : MonoBehaviour {
    public Gradient fogColorGradient;
    void Update() {
        RenderSettings.fogColor = fogColorGradient.Evaluate(DayNightCycle.instance.time01);
        RenderSettings.fogDensity = 0.0015f - Mathf.Max(DayNightCycle.instance.daylight*0.0005f, 0f);
    }
}
