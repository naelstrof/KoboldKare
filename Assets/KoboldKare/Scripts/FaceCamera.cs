using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {
    private Camera cachedCamera;
    private Camera cam {
        get {
            if (cachedCamera == null || !cachedCamera.isActiveAndEnabled) {
                cachedCamera = Camera.current;
            }
            if (cachedCamera == null || !cachedCamera.isActiveAndEnabled) {
                cachedCamera = Camera.main;
            }
            return cachedCamera;
        }
    }
    void LateUpdate() {
        if (cam == null) {
            return;
        }
        transform.rotation = Quaternion.LookRotation((transform.position-cam.transform.position).normalized);
    }
}
