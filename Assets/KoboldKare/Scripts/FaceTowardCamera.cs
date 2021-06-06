using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTowardCamera : MonoBehaviour {
    public Vector3 forward = Vector3.forward;
    public Vector3 worldUp = Vector3.up;
    public Camera main;
    void Start() {
        main = Camera.main;
    }
    void Update() {
        if (main == null) {
            main = Camera.main;
            return;
        }
        Vector3 dir = Vector3.Normalize(main.transform.position - transform.position);
        transform.rotation = Quaternion.FromToRotation(transform.TransformDirection(forward), dir) * transform.rotation;
        transform.rotation = Quaternion.FromToRotation(transform.up, worldUp) * transform.rotation;
    }
}
