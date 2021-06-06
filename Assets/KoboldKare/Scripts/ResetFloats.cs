using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetFloats : MonoBehaviour {
    public ScriptableFloat grabCount;
    public ScriptableFloat money;
    // Start is called before the first frame update
    void Start() {
        //DayNightCycle.instance.time01 = 0.35f;
        money.deplete();
        money.give(30f);
        grabCount.deplete();
        grabCount.give(1);
    }
    private void OnEnable() {
        //DayNightCycle.instance.time01 = 0.35f;
        money.deplete();
        money.give(30f);
        grabCount.deplete();
        grabCount.give(1);
    }
}
