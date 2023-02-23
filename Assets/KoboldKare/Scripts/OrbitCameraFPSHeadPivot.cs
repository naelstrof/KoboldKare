using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements.Experimental;
using UnityEngine;

public class OrbitCameraFPSHeadPivot : OrbitCameraLerpTrackPivot {
    private Vector3 eyeOffset;
    public override void Initialize(Animator targetAnimator, HumanBodyBones bone, float lerpTrackSpeed) {
        base.Initialize(targetAnimator, bone, lerpTrackSpeed);
        Vector3 eyeCenter = (targetAnimator.GetBoneTransform(HumanBodyBones.LeftEye).position + targetAnimator.GetBoneTransform(HumanBodyBones.RightEye).position) * 0.5f;
        eyeOffset = targetTransform.InverseTransformPoint(eyeCenter);
    }

    public override Vector3 GetPivotPosition(Quaternion camRotation) {
        return base.GetPivotPosition(camRotation) + targetTransform.TransformVector(eyeOffset);
    }

    public override float GetDistanceFromPivot(Quaternion camRotation) {
        if (ragdoller.ragdolled) {
            return 0f;
        }
        Vector3 forward = camRotation * Vector3.forward;
        float t = Mathf.Clamp01(forward.y + 1f);
        return Mathf.Lerp(0f, -0.1f, Easing.OutCubic(t));
    }
}
