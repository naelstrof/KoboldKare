using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderSetScriptableFloat : MonoBehaviour {
    public Slider target;
    public ScriptableFloat targetFloat;
    public void Start() {
        target.maxValue = targetFloat.max;
        target.minValue = targetFloat.min;
        target.value = targetFloat.value;
    }
    public void OnChange() {
        targetFloat.set(target.value);
    }
}
