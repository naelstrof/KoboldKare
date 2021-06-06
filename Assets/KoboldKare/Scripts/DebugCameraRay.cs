using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugCameraRay : MonoBehaviour {
    void Update() {
        for(float x=0f;x<1f;x+=0.1f) {
            for(float y=0f;y<1f;y+=0.1f) {
                Vector2 uv = new Vector2(x,y);
                Vector4 viewVector = GetComponent<Camera>().projectionMatrix.inverse * new Vector4(uv.x * 2f - 1f, uv.y * 2f -1f, 0f, 1f);
                viewVector = GetComponent<Camera>().cameraToWorldMatrix * new Vector4(viewVector.x, viewVector.y, viewVector.z,0f);
                Debug.DrawLine(transform.position, transform.position + new Vector3(viewVector.x, viewVector.y, viewVector.z), Color.red);
            }
        }
    }
}
