using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class OrbitCameraLerpTrackBasicPivot : OrbitCameraLerpTrackPivot {
    [SerializeField] private Vector2 screenOffset = Vector2.one * 0.5f;
    [SerializeField] float desiredDistanceFromPivot = 1f;
    
    private LayerMask obstacleMask;
    private RaycastHit[] hitResults;
    private HitResultSorter hitSorter;

    public void SetInfo(Vector2 screenOffset, float desiredDistanceFromPivot) {
        this.screenOffset = screenOffset;
        this.desiredDistanceFromPivot = desiredDistanceFromPivot;
    }

    private void Awake() {
        obstacleMask = LayerMask.GetMask("World");
        hitResults = new RaycastHit[32];
        hitSorter = new HitResultSorter();
    }

    private class HitResultSorter : IComparer<RaycastHit> {
        public int Compare(RaycastHit x, RaycastHit y) {
            return x.distance.CompareTo(y.distance);
        }
    }

    public override Vector2 GetScreenOffset(Quaternion camRotation) => screenOffset;

    public override float GetDistanceFromPivot(Quaternion camRotation) {
        Vector3 forward = camRotation * Vector3.back;
        
        float realDesiredDistance = Mathf.Lerp(desiredDistanceFromPivot*0.5f, desiredDistanceFromPivot*2f, (forward.y + 1f) / 2f);
        float actualDistance = realDesiredDistance;
        int hits = Physics.RaycastNonAlloc(new Ray(GetPivotPosition(camRotation), forward), hitResults, actualDistance, obstacleMask);
        if (hits > 0) {
            Array.Sort(hitResults, 0, hits, hitSorter);
            actualDistance = hitResults[0].distance;
        }
        return actualDistance;
    }

    // Worms-eye view gets more FOV, birds-eye view gets less
    public override float GetFOV(Quaternion camRotation) {
        Vector3 forward = camRotation * Vector3.back;
        return Mathf.Lerp(Mathf.Min(fov.GetValue() * 1.5f,100f), fov.GetValue(), Easing.OutCubic(Mathf.Clamp01(forward.y + 1f)));
    }
}
