using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throbber : MonoBehaviour {
    private float angle = 0f;
    void Update() {
        angle += Time.unscaledDeltaTime * Mathf.Sin(Time.unscaledTime*4f).Remap(-1f, 1f, 0f, 1f) * 180f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
