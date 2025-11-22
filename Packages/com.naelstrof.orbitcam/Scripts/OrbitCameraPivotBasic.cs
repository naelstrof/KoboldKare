using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class OrbitCameraPivotBasic : OrbitCameraPivotBase {
    [SerializeField] protected Vector2 screenOffset = Vector2.one * 0.5f;
    [SerializeField] protected float desiredDistanceFromPivot = 1f;
    [SerializeField] protected float baseFOV = 65f;
    private float distanceMemory;
    private float distanceVel;
    public void SetBaseFOV(float fov) {
        baseFOV = fov;
    }
    public float GetBaseFOV() => baseFOV;
    public void SetDesiredDistanceFromPivot(float desiredDistanceFromPivot) {
        this.desiredDistanceFromPivot = desiredDistanceFromPivot;
    }
    public float GetDesiredDistanceFromPivot() => desiredDistanceFromPivot;

    public void SetScreenOffset(Vector2 screenOffset) {
        this.screenOffset = screenOffset;
    }
    public Vector2 GetScreenOffset() => screenOffset;
    public override OrbitCameraData GetData(Camera cam) {
        var rotation = cam.transform.rotation;
        float desiredDistance = GetDistanceFromPivot(cam, rotation, screenOffset);
        if (Time.deltaTime != 0f) {
            if (distanceMemory < desiredDistance) {
                distanceMemory = Mathf.SmoothDamp(distanceMemory, desiredDistance, ref distanceVel, 0.1f);
            } else { // Clipping into walls leads to bad flashing with unity's occlusion. Can cause some bad flickering with columns but it's worth it to prevent flickering the world.
                distanceMemory = desiredDistance;
                distanceVel = 0f;
            }
        }
        return new OrbitCameraData {
            screenPoint = screenOffset,
            position = transform.position,
            distance = distanceMemory,
            clampPitch = true,
            clampYaw = false,
            fov = GetFOV(rotation),
            rotation = rotation,
        };
    }
    
    protected float GetDistanceFromPivot(Camera cam, Quaternion rotation, Vector2 screenOffset) {
        var position = transform.position;
        CastNearPlane(cam, rotation, screenOffset, position, position - cam.transform.forward * desiredDistanceFromPivot, out float newDistance);
        return newDistance;
    }

    // Worms-eye view gets more FOV, birds-eye view gets less
    protected float GetFOV(Quaternion camRotation) {
        Vector3 forward = camRotation * Vector3.back;
        float fov = Mathf.Lerp(Mathf.Min(baseFOV * 1.5f,100f), baseFOV, Easing.OutCubic(Mathf.Clamp01(forward.y + 1f)));
        return fov;
    }
}
