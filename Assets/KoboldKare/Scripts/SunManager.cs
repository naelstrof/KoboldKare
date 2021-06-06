using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SunManager : MonoBehaviour
{
    public Light sun;
    private Quaternion _startRotation;
    private Vector3 _startUp;
    public Gradient sunColorGradient;
    public Gradient fogColorGradient;
    //public Gradient fogColorGradient;
    // Update is called once per frame
    private void Start() {
        Quaternion startRot = Quaternion.Euler(new Vector3(-90, 180, 0));
        transform.rotation = startRot;
        _startRotation = startRot;
        _startUp = Vector3.Normalize(transform.up + transform.forward*5f);
    }
    void Update() {
        //float daylight = Mathf.Clamp01(DayNightCycle.instance.daylight);
        //GetComponent<HDAdditionalLightData>().SetIntensity(1f + daylight * 20f);
        //GetComponent<HDAdditionalLightData>().SetColor(_dayColorGradient.Evaluate(DayNightCycle.instance.day01));
        transform.rotation = _startRotation * Quaternion.AngleAxis(DayNightCycle.instance.time01 * 360, _startUp);
    }
    // Update is called once per frame
    void FixedUpdate() {
        sun.color = sunColorGradient.Evaluate(DayNightCycle.instance.time01);
        RenderSettings.fogColor = fogColorGradient.Evaluate(DayNightCycle.instance.time01);
        RenderSettings.fogDensity = 0.0015f - Mathf.Max(DayNightCycle.instance.daylight*0.0005f, 0f);
        Shader.SetGlobalColor("ambientColorMultiplier", sun.color);
        sun.intensity = Mathf.Max(DayNightCycle.instance.daylight*2f, 0.25f);
    }
}
