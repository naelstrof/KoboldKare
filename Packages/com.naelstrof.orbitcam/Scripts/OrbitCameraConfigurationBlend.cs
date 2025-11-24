using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrbitCameraConfigurationBlend : OrbitCameraConfiguration {
    [SerializeField]
    private OrbitCameraPivotBase pivotA;
    [SerializeField]
    private OrbitCameraPivotBase pivotB;
    [SerializeField]
    private float blend = 0.5f;
    public override OrbitCameraData GetData(Camera cam) {
        return OrbitCameraData.Lerp(pivotA.GetData(cam), pivotB.GetData(cam), blend);
    }

    public void SetPivots(OrbitCameraPivotBase a, OrbitCameraPivotBase b, float blend) {
        this.blend = blend;
        pivotA = a;
        pivotB = b;
    }
}
