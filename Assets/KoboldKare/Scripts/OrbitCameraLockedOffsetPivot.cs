using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCameraLockedOffsetPivot : OrbitCameraPivotBasic {
    private Vector3 offset;
    private Quaternion rotOffset = Quaternion.identity;
    
    public void Lock(Vector3 position, Quaternion rotation) {
        offset = Quaternion.Inverse(OrbitCamera.GetPlayerIntendedRotation())*(position-transform.position);
        rotOffset = Quaternion.Inverse(rotation*OrbitCamera.GetPlayerIntendedRotation());
    }

    public override Vector3 GetPivotPosition(Quaternion camRotation) {
        float actualDistance = offset.magnitude;
        Vector3 dir = (camRotation * offset).normalized;
        int hits = Physics.RaycastNonAlloc(new Ray(transform.position, dir), hitResults, actualDistance, obstacleMask);
        float cameraNearClipPlane = 0.15f;
        if (hits <= 0) return transform.position + dir * actualDistance;
        Array.Sort(hitResults, 0, hits, hitSorter);
        actualDistance = Mathf.Max(hitResults[0].distance-cameraNearClipPlane,0f);
        return transform.position + dir * actualDistance;
    }

    public override Quaternion GetPostRotationOffset(Quaternion camRotation) {
        return rotOffset;
    }

    public override float GetFOV(Quaternion camRotation) {
        return baseFOV.GetValue();
    }

    public override float GetDistanceFromPivot(Quaternion camRotation) {
        return 0f;
    }
}
