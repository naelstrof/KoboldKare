using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCameraCutscenePivot : OrbitCameraPivotBasic {
    [SerializeField] private float angleFreedom = 30f;
    private Quaternion GetRotation(Quaternion camRotation) {
        Vector2 aim = OrbitCamera.GetPlayerIntendedScreenAim();
        float yawFreedom = Mathf.Min(angleFreedom, 360f) / 360f;
        float pitchFreedom = Mathf.Min(angleFreedom, 89f) / 89f;
        aim.x *= yawFreedom;
        aim.x = Mathf.Clamp(aim.x, -angleFreedom, angleFreedom);
        aim.y *= pitchFreedom;
        aim.y = Mathf.Clamp(aim.y, -angleFreedom, angleFreedom);
        return transform.rotation * Quaternion.Euler(-aim.y, aim.x, 0f);
    }
    public override OrbitCameraData GetData(Camera cam) {
        var rotation = GetRotation(cam.transform.rotation);
        return new OrbitCameraData {
            screenPoint = screenOffset,
            position = transform.position,
            distance = GetDistanceFromPivot(cam, rotation, screenOffset),
            clampPitch = true,
            clampYaw = true,
            fov = GetFOV(rotation),
            rotation = rotation,
        };
    }
}
