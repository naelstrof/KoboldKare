using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

[System.Serializable]
public class OrbitCameraCharacterConfiguration : OrbitCameraConfiguration {
    [SerializeField]
    private OrbitCameraPivotBase shoulderPivot;
    [SerializeField]
    private OrbitCameraPivotBase buttPivot;

    private OrbitCamera.OrbitCameraData? lastData;
    
    public void SetPivots(OrbitCameraPivotBase shoulder, OrbitCameraPivotBase butt) {
        shoulderPivot = shoulder;
        buttPivot = butt;
    }
    
    public override OrbitCamera.OrbitCameraData GetData(Quaternion cameraRotation) {
        if (shoulderPivot == null) {
            lastData ??= OrbitCamera.GetCurrentCameraData();
            return lastData.Value;
        }

        OrbitCamera.OrbitCameraData topData = new OrbitCamera.OrbitCameraData(shoulderPivot, cameraRotation);
        OrbitCamera.OrbitCameraData bottomData = new OrbitCamera.OrbitCameraData(buttPivot, cameraRotation);
        Vector3 forward = cameraRotation * Vector3.forward;
        float t = Easing.OutCubic(Mathf.Clamp01(-forward.y + 1f));
        return OrbitCamera.OrbitCameraData.Lerp(bottomData, topData, t);
    }
}
