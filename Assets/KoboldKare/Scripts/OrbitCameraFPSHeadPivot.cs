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

    protected override void LateUpdate() {
        Vector3 a = transform.localPosition;
        Vector3 b = transform.parent.InverseTransformPoint(targetTransform.TransformPoint(eyeOffset));
        Vector3 correction = b - a;
        float diff = correction.magnitude;
        transform.localPosition = Vector3.MoveTowards(a, a+correction, (0.1f+diff)*Time.deltaTime*lerpTrackSpeed);
    }

    public override OrbitCameraData GetData(Camera cam) {
        var data = base.GetData(cam);
        data.distance = 0.05f;
        return data;
    }
}
