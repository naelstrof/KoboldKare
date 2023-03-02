using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrbitCameraBasicConfiguration : OrbitCameraConfiguration {
    [SerializeField]
    private OrbitCameraPivotBase pivot;
    [SerializeField]
    private LayerMask cullingMask = ~0;

    private OrbitCamera.OrbitCameraData? lastData;
    public override OrbitCamera.OrbitCameraData GetData(Quaternion cameraRotation) {
        if (pivot == null) {
            lastData ??= OrbitCamera.GetCurrentCameraData();
            return lastData.Value;
        }
        return new OrbitCamera.OrbitCameraData(pivot, cameraRotation);
    }

    public void SetPivot(OrbitCameraPivotBase pivot) {
        this.pivot = pivot;
    }

    public void SetCullingMask(LayerMask mask) {
        cullingMask = mask;
    }
    public override LayerMask GetCullingMask() => cullingMask;
}
