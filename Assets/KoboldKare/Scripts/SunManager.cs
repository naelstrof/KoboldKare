using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SunManager : MonoBehaviour {
    public Material skyboxMaterial;
    public Volume postProcessNight;
    public Light sun;
    private float origSunIntensity;
    void Start(){
        origSunIntensity = sun.intensity;
    }
    void Update() {
        skyboxMaterial.SetFloat("_Eclipse", DayNightCycle.instance.time01);
        float zeroAdd = Mathf.Clamp01(1f - DayNightCycle.instance.time01 * 25f);
        float oneAdd = Mathf.Clamp01(1f-Mathf.Abs(DayNightCycle.instance.time01 - 1f) * 25f);
        postProcessNight.weight = zeroAdd+oneAdd;
        sun.intensity = origSunIntensity-(origSunIntensity*(zeroAdd+oneAdd));
    }
}
