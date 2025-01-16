using System.Collections;
using System.Collections.Generic;
using Naelstrof.Easing;
using UnityEngine;

public class OrbitCameraConfigurationSlide : OrbitCameraBasicConfiguration {
    [SerializeField] private OrbitCameraPivotBase pivotB;
    public override OrbitCameraData GetData(Camera cam) {
        if (pivot == null || pivotB == null) {
            lastData ??= OrbitCamera.GetCurrentCameraData();
            return lastData.Value;
        }
        
        var pivotAData = pivot.GetData(cam);
        var pivotBData = pivotB.GetData(cam);

        Vector3 axis = (pivotAData.position - pivotBData.position).normalized;
        float dot = Vector3.Dot(cam.transform.forward, axis);
        float remap = dot.Remap(-1f, 1f, 0f, 1f);
        lastData = OrbitCameraData.Lerp(pivotAData, pivotBData, Easing.Sinusoidal.InOut(remap));
        return lastData.Value;
    }

    public void SetOtherPivot(OrbitCameraPivotBase pivot) {
        pivotB = pivot;
    }
}
