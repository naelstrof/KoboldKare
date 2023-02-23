using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrbitCameraConfiguration {
    public virtual OrbitCamera.OrbitCameraData GetData(Quaternion cameraRotation) {
        return new OrbitCamera.OrbitCameraData();
    }
    public virtual LayerMask GetCullingMask() => ~0;
}
