using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCameraCutscenePivot : OrbitCameraPivotBasic {
    [SerializeField] private float angleFreedom = 30f;
    public override Quaternion GetRotation(Quaternion camRotation) {
        float currentAngle = Quaternion.Angle(camRotation, transform.rotation);
        return Quaternion.RotateTowards(camRotation, transform.rotation, Mathf.Abs(currentAngle - angleFreedom));
    }
}
