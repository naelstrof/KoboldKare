using UnityEngine;
public struct OrbitCameraData {
    public Vector3 position;
    public float distance;
    public float fov;
    public Vector2 screenPoint;
    public Quaternion rotation; 
    public bool clampYaw;
    public bool clampPitch;

    public bool IsValid() {
        if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z)) {
            return false;
        }

        if (float.IsNaN(screenPoint.x) || float.IsNaN(screenPoint.y)) {
            return false;
        }

        if (rotation.normalized != rotation) {
            return false;
        }

        if (float.IsNaN(distance)) {
            return false;
        }

        return true;
    }

    public static OrbitCameraData Lerp(OrbitCameraData pivotA, OrbitCameraData pivotB, float t) {
        if (float.IsNaN(t)) {
            Debug.LogError("Tried to lerp with nan t");
        }
        if (!pivotA.IsValid()) {
            Debug.LogError("Tried to lerp with nan pivot A");
        }
        if (!pivotB.IsValid()) {
            Debug.LogError("Tried to lerp with nan pivot B");
        }

        return new OrbitCameraData {
            position = Vector3.Lerp(pivotA.position, pivotB.position, t),
            distance = Mathf.Lerp(pivotA.distance, pivotB.distance, t),
            fov = Mathf.Lerp(pivotA.fov, pivotB.fov, t),
            screenPoint = Vector2.Lerp(pivotA.screenPoint, pivotB.screenPoint, t),
            rotation = Quaternion.Lerp(pivotA.rotation, pivotB.rotation, t),
            clampPitch = t<0.5f ? pivotA.clampPitch : pivotB.clampPitch,
            clampYaw = t<0.5f ? pivotA.clampYaw : pivotB.clampYaw
        };
    }
}