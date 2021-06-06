using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockRotationConstraint : MonoBehaviour {
    Vector3 upDir = Vector3.forward;
    void Update() {
        transform.rotation = Quaternion.FromToRotation(transform.TransformDirection(upDir),Vector3.up) * transform.rotation;
    }
}
