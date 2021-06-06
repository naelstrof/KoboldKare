using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using KoboldKare;

[CreateAssetMenu(fileName = "DefaultFloat", menuName = "Data/ScriptableFloat", order = 1)]
public class ScriptableFloat : ScriptableObject {
    public float startingValue = 0;
    public float startingMax = 100;
    public float startingMin = 0;
    [System.Serializable]
    public class UnityFloatEvent : UnityEvent<float> {}

    public UnityFloatEvent OnExhaust;
    public UnityFloatEvent OnFull;
    public UnityFloatEvent OnChanged;
    public float value { get; private set; }
    [NonSerialized]
    private float lastValue = 0;

    [NonSerialized]
    public float max = 100;

    [NonSerialized]
    public float min = 0;

    void OnEnable() {
        value = startingValue;
        lastValue = startingValue;
        max = startingMax;
        min = startingMin;
    }
    public void setWithoutNotify(float amount) {
        value = amount;
        lastValue = amount;
    }

    public void set(float amount) {
        value = Mathf.Min(Mathf.Max(min, amount), max);
        Check();
    }

    public void deplete() {
        value = min;
        Check();
    }

    public void fill() {
        if (value != max) {
            value = max;
        }
        Check();
    }

    public bool has(float amount) {
        return value >= amount;
    }

    public bool charge(float amount) {
        if (value >= amount) {
            take(amount);
            return true;
        }
        return false;
    }

    public void take(float amount) {
        if ( value <= min) {
            return;
        }
        value = Mathf.Max(value - amount, min);
        Check();
    }

    public void give(float amount) {
        if ( value >= max) {
            return;
        }
        value = Mathf.Min(value + amount, max);
        Check();
    }

    public void setMin(float amount) {
        min = amount;
        value = Mathf.Max(value, min);
        Check();
    }

    public void setMax(float amount) {
        max = amount;
        value = Mathf.Min(value, max);
        Check();
    }

    private void Check() {
        if ( lastValue != value ) {
            lastValue = value;
            OnChanged.Invoke(value);
        }
        if ( value <= min ) {
            OnExhaust.Invoke(value);
        }
        if ( value >= max ) {
            OnFull.Invoke(value);
        }
    }
}
