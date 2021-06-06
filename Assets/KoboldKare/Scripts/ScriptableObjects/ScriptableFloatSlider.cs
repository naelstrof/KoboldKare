using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableFloatSlider : UnityEngine.UI.Slider {
    public ScriptableFloat val;
    public new void Start() {
        maxValue = val.max;
        minValue = val.min;
    }
    public override float value {
        get {
            if (val != null) {
                return val.value;
            } else {
                return base.value;
            }
        }
        set {
            if (val != null ) {
                val.set(value);
            }
            base.value = value;
        }
    }
}
