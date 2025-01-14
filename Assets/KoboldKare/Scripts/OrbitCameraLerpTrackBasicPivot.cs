using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class OrbitCameraLerpTrackBasicPivot : OrbitCameraLerpTrackPivot {
    [SerializeField] private Vector2 screenOffset = Vector2.one * 0.5f;
    [SerializeField] float desiredDistanceFromPivot = 1f;

    public void SetInfo(Vector2 screenOffset, float desiredDistanceFromPivot) {
        this.screenOffset = screenOffset;
        this.desiredDistanceFromPivot = desiredDistanceFromPivot;
    }
    
    public override OrbitCameraData GetData(Camera cam) {
        var data = base.GetData(cam);
        data.screenPoint = screenOffset;
        if (CastNearPlane(cam, data.rotation, screenOffset, data.position, data.position + data.rotation * Vector3.back*desiredDistanceFromPivot, out var newDistance)) {
            data.distance = newDistance;
        } else {
            data.distance = desiredDistanceFromPivot;
        }
        Vector3 forward = data.rotation * Vector3.back;
        data.fov = Mathf.Lerp(Mathf.Min(fov.GetValue() * 1.5f,100f), fov.GetValue(), Easing.OutCubic(Mathf.Clamp01(forward.y + 1f)));
        return data;
    }
}
