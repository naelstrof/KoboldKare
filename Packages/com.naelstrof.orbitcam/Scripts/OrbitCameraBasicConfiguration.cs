using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrbitCameraBasicConfiguration : OrbitCameraConfiguration {
    [SerializeField]
    private OrbitCameraPivotBase pivot;
    [SerializeField]
    private LayerMask cullingMask = ~0;
    public override OrbitCamera.OrbitCameraData GetData(Quaternion cameraRotation) {
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
