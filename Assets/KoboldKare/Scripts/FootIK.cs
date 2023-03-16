using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour {
    private Animator targetAnimator;
    private Transform leftFoot;
    private Transform rightFoot;
    private Transform hips;
    void Awake() {
        targetAnimator = GetComponent<Animator>();
        leftFoot = targetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFoot = targetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
        hips = targetAnimator.GetBoneTransform(HumanBodyBones.Hips);
    }
    void OnAnimatorIK(int layerIndex) {
        if (!isActiveAndEnabled) {
            return;
        }
        SetFootTarget(leftFoot, hips, targetAnimator, AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee);
        SetFootTarget(rightFoot, hips, targetAnimator, AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee);
    }
    void SetFootTarget(Transform foot, Transform hip, Animator a, AvatarIKGoal target, AvatarIKHint hint) {
        var footPosition = foot.position;
        Vector3 localFoot = hip.position - footPosition;
        Vector3 rayOrigin = Vector3.Project(localFoot, Vector3.up) + footPosition;
        float rayDist = Vector3.Project(localFoot, Vector3.up).magnitude;
        if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, rayDist, GameManager.instance.walkableGroundMask, QueryTriggerInteraction.Ignore)) {
            a.SetIKPositionWeight(target, 1f);
            a.SetIKRotationWeight(target, 1f);
            a.SetIKHintPositionWeight(hint,0.5f);
            a.SetIKPosition(target, hit.point+hit.normal*0.05f*transform.lossyScale.x);
            Vector3 localHitPoint = (foot.position+a.transform.forward*0.25f) - hip.position;
            Vector3 planeForward = localHitPoint;
            Vector3 planeUp = a.transform.up;
            Vector3.OrthoNormalize(ref planeForward, ref planeUp);
            Vector3 kneePlane = Vector3.Cross(planeForward, planeUp);
            Vector3 kneeGuess = Vector3.ProjectOnPlane(localHitPoint + (a.transform.up+a.transform.forward)*a.transform.lossyScale.x*2f, kneePlane)+hip.position;
            a.SetIKHintPosition(hint, kneeGuess);
            Vector3 hintDir = (kneeGuess - hit.point).normalized;
            a.SetIKRotation(target, QuaternionExtensions.LookRotationUpPriority(hintDir, hit.normal));
        } else {
            a.SetIKHintPositionWeight(hint,0f);
            a.SetIKPositionWeight(target, 0f);
            a.SetIKRotationWeight(target, 0f);
        }
    }
}
