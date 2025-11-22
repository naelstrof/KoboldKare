using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrbitCameraBasicConfiguration : OrbitCameraConfiguration {
    [SerializeField]
    protected OrbitCameraPivotBase pivot;
    [SerializeField]
    protected LayerMask cullingMask = ~0;

    protected OrbitCameraData? lastData;
    public override OrbitCameraData GetData(Camera cam) {
        if (pivot == null) {
            lastData ??= OrbitCamera.GetCurrentCameraData();
            return lastData.Value;
        }
        lastData = pivot.GetData(cam);
        return lastData.Value;
    }

    public void SetPivot(OrbitCameraPivotBase pivot) {
        this.pivot = pivot;
    }

    public void SetCullingMask(LayerMask mask) {
        cullingMask = mask;
    }
    public override LayerMask GetCullingMask() => cullingMask;
}
