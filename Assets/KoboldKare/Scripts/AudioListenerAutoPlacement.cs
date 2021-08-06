using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AudioListenerAutoPlacement : MonoBehaviour {
    private Camera cam;
    void LateUpdate() {
        if (cam == null || !cam.isActiveAndEnabled) {
            cam = Camera.main;
        }
        if (cam == null || !cam.isActiveAndEnabled) {
            cam = Camera.current;
        }
        if (cam == null || !cam.isActiveAndEnabled) {
            return;
        }
        transform.position = cam.transform.position;
        transform.rotation = cam.transform.rotation;
    }
}
