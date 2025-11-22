using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OrbitCameraPivotBase : MonoBehaviour {
    private static Vector3[] frustumCorners = new Vector3[4];
    private static RaycastHit[] raycastHits = new RaycastHit[32];
    private static LayerMask obstacleMask;
    public abstract OrbitCameraData GetData(Camera cam);
    protected virtual void Awake() {
        obstacleMask = LayerMask.GetMask("World");
    }
    protected bool CastNearPlane(Camera cam, Quaternion camRotation, Vector2 screenOffset, Vector3 from, Vector3 to, out float distance) {
        const float minDistance = 0.1f;
        var camTransform = cam.transform;
        var rot = camTransform.rotation;
        camTransform.rotation = camRotation;
        
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        Vector3 extents = new Vector3(Vector3.Distance(frustumCorners[0], frustumCorners[2]), Vector3.Distance(frustumCorners[0], frustumCorners[1]), cam.nearClipPlane);
        Vector3 diff = to - from;
        Vector3 dir = -OrbitCamera.GetScreenRay(cam, screenOffset).direction;
        Vector3 cameraStart = from;// - dir * (cam.nearClipPlane*0.5f);
        for(int i=0;i<4;i++) {
            Debug.DrawLine(cameraStart + camRotation * frustumCorners[i], cameraStart + camRotation * frustumCorners[i]+dir*diff.magnitude);
        }
        int hitCount = Physics.BoxCastNonAlloc(cameraStart, extents * 0.5f, dir, raycastHits, camRotation, diff.magnitude, obstacleMask);
        distance = Vector3.Distance(to, from);
        if (hitCount == 0) {
            cam.transform.rotation = rot;
            return false;
        }

        bool insideProp = false;
        for (int i = 0; i < hitCount; i++) {
            if (raycastHits[i].point == Vector3.zero) {
                insideProp = true;
                break;
            }
            distance = Mathf.Min(distance, Mathf.Max(raycastHits[i].distance-cam.nearClipPlane-minDistance, 0f));
        }

        // If we're inside something, then we panic and try to use a regular raycasts to figure out what to do.
        if (insideProp) {
            // Center-camera
            hitCount = Physics.RaycastNonAlloc(cameraStart, dir, raycastHits, distance, obstacleMask);
            for (int j = 0; j < hitCount; j++) {
                distance = Mathf.Min(distance, Mathf.Max(raycastHits[j].distance-cam.nearClipPlane-minDistance, 0f));
            }
            // All four corners
            for (int i = 0; i < 4; i++) {
                hitCount = Physics.RaycastNonAlloc(cameraStart + camRotation*frustumCorners[i], dir, raycastHits, distance, obstacleMask);
                for (int j = 0; j < hitCount; j++) {
                    distance = Mathf.Min(distance, Mathf.Max(raycastHits[j].distance-cam.nearClipPlane-minDistance,0f));
                }
            }
        }

        // Bring the camera in a bit so that the occlusion culling doesn't flicker...
        distance -= cam.nearClipPlane;
        cam.transform.rotation = rot;
        return true;
    }
}
