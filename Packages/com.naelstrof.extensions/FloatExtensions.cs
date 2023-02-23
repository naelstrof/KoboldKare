using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloatExtensions {
    public static float Remap(this float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static float CircularLerp(float a, float b, float t, float circumference = 1f) {
        // Push a and b closer than arcLength together
        if (b > a+circumference*0.5f) {
            b -= circumference;
        }
        if (b < a-circumference*0.5f) {
            b += circumference;
        }
        return Mathf.Repeat(Mathf.Lerp(a, b, t), circumference);
    }
}
