using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCameraCutscenePivot : OrbitCameraPivotBasic {
    [SerializeField] private float angleFreedom = 30f;
    public override Quaternion GetRotation(Quaternion camRotation) {
        Vector2 aim = OrbitCamera.GetPlayerIntendedScreenAim();
        float yawFreedom = Mathf.Min(angleFreedom, 360f) / 360f;
        float pitchFreedom = Mathf.Min(angleFreedom, 89f) / 89f;
        aim.x *= yawFreedom;
        aim.x = Mathf.Clamp(aim.x, -angleFreedom, angleFreedom);
        aim.y *= pitchFreedom;
        aim.y = Mathf.Clamp(aim.y, -angleFreedom, angleFreedom);
        return transform.rotation * Quaternion.Euler(-aim.y, aim.x, 0f);
    }

    public override bool GetClampYaw() => true;
}
