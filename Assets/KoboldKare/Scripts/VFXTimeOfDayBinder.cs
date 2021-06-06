using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXTimeOfDayBinder : MonoBehaviour {
    public VisualEffect system;
    void FixedUpdate() {
        //Color c = color.Evaluate(time.Get01FullCycle());
        //ParticleSystem.MainModule main = system.main;
        //main.startColor = c;
        system.SetFloat("TimeOfDay", DayNightCycle.instance.time01);
    }
}
