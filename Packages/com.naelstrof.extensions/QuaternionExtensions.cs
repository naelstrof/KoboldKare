using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionExtensions {
    public static Quaternion LookRotationUpPriority(Vector3 forward, Vector3 up) {
        return Quaternion.LookRotation(up, forward)*Quaternion.Euler(-90,180,0);
    }
}
