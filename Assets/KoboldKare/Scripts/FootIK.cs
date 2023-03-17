using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour {
    private Animator targetAnimator;
    private Transform leftFoot;
    private Transform rightFoot;
    private Transform leftKnee;
    private Transform rightKnee;
    private Transform hips;
    void Awake() {
        targetAnimator = GetComponent<Animator>();
        leftFoot = targetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFoot = targetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
        
        leftKnee = targetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        rightKnee = targetAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        hips = targetAnimator.GetBoneTransform(HumanBodyBones.Hips);
    }
    void OnAnimatorIK(int layerIndex) {
        if (!isActiveAndEnabled) {
            return;
        }
        SetFootTarget(leftFoot, leftKnee, hips, targetAnimator, AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee);
        SetFootTarget(rightFoot, rightKnee, hips, targetAnimator, AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee);
    }
    void SetFootTarget(Transform foot, Transform knee, Transform hip, Animator a, AvatarIKGoal target, AvatarIKHint hint) {
        var footPosition = foot.position;
        Vector3 localFoot = hip.position - footPosition;
        Vector3 rayOrigin = Vector3.Project(localFoot, Vector3.up) + footPosition;
        float rayDist = Vector3.Project(localFoot, Vector3.up).magnitude;
        if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, rayDist, GameManager.instance.walkableGroundMask, QueryTriggerInteraction.Ignore)) {
            a.SetIKPositionWeight(target, 1f);
            a.SetIKRotationWeight(target, 1f);
            a.SetIKHintPositionWeight(hint,0.5f);
            a.SetIKPosition(target, hit.point+hit.normal*0.05f*transform.lossyScale.x);
            var hipPosition = hip.position;
            Vector3 hipToFoot = (foot.position - hipPosition).normalized;
            var kneePosition = knee.position;
            Vector3 kneeToHipToFoot = Vector3.Project(kneePosition - hipPosition, hipToFoot) + hipPosition;
            Vector3 kneeForward = kneePosition - kneeToHipToFoot;
            Vector3 kneeGuess = hipPosition + kneeForward * 3f;
            a.SetIKHintPosition(hint, kneeGuess);
            a.SetIKRotation(target, QuaternionExtensions.LookRotationUpPriority(kneeForward, hit.normal));
        } else {
            a.SetIKHintPositionWeight(hint,0f);
            a.SetIKPositionWeight(target, 0f);
            a.SetIKRotationWeight(target, 0f);
        }
    }
}
