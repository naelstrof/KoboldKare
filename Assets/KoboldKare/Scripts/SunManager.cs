using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SunManager : MonoBehaviour
{
    [Range(0f,1f)]
    public float moon;
    public Light sun;
    public float intensity = 2f;
    private Quaternion _startRotation;
    private Vector3 _startUp;
    public Gradient sunColorGradient;
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
        transform.rotation = _startRotation * Quaternion.AngleAxis(DayNightCycle.instance.time01 * 360 + moon*180f, _startUp);
        if (moon <= 0f) {
            Shader.SetGlobalVector("_SunDirection", -transform.forward);
            Shader.SetGlobalVector("_SunRight", transform.right);
        }
        sun.color = sunColorGradient.Evaluate(DayNightCycle.instance.time01);
        if (moon == 0f) {
            sun.intensity = DayNightCycle.instance.daylight * intensity;
        } else {
            sun.intensity = (1f-DayNightCycle.instance.daylight)*intensity;
        }
    }
}
