using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateByTime : MonoBehaviour {
    public Vector3 axis = new Vector3(0, 0, 1);
    public float offset = 0f;
    public float multiplier = 1f;
    void FixedUpdate() {
        transform.rotation = Quaternion.AngleAxis(offset + DayNightCycle.instance.time01 * 360f * multiplier, axis);
    }
}
