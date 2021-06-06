using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class EnvironmentalSound : MonoBehaviour, IGameEventGenericListener<float> {
    public AnimationCurve volumeCurve;
    public GameEventFloat metabolizeEvent;
    private AudioSource source;
    public void OnEventRaised(GameEventGeneric<float> e, float f) {
        float v = volumeCurve.Evaluate(DayNightCycle.instance.time01);
        source.volume = v;
    }
    void Start() {
        source = GetComponent<AudioSource>();
        metabolizeEvent.RegisterListener(this);
    }
    void OnDestroy() {
        metabolizeEvent.UnregisterListener(this);
    }
}
