using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class EnvironmentalSound : MonoBehaviour {
    public AnimationCurve volumeCurve;
    public GameEventFloat metabolizeEvent;
    private AudioSource source;
    public void OnEventRaised(float f) {
        float v = volumeCurve.Evaluate(DayNightCycle.instance.time01);
        source.volume = v;
    }
    void Start() {
        source = GetComponent<AudioSource>();
        metabolizeEvent.AddListener(OnEventRaised);
    }
    void OnDestroy() {
        metabolizeEvent.RemoveListener(OnEventRaised);
    }
}
