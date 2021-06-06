using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour {
    private Animator targetAnimator;
    public void Start() {
        targetAnimator = GetComponent<Animator>();
    }
    public void OnAnimatorIK(int layerIndex) {
        Transform leftfoot = targetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform rightfoot = targetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
        SetFootTarget(leftfoot, targetAnimator.GetBoneTransform(HumanBodyBones.Hips), targetAnimator, AvatarIKGoal.LeftFoot);
        SetFootTarget(rightfoot, targetAnimator.GetBoneTransform(HumanBodyBones.Hips), targetAnimator, AvatarIKGoal.RightFoot);
    }
    private void SetFootTarget(Transform foot, Transform hip, Animator a, AvatarIKGoal target) {
        RaycastHit hit;
        float dist = Vector3.Distance(hip.position, foot.position);
        if (Physics.Raycast(hip.position, (foot.position - hip.position).normalized, out hit, dist, GameManager.instance.walkableGroundMask, QueryTriggerInteraction.Ignore)) {
            a.SetIKPositionWeight(target, 1f);
            a.SetIKRotationWeight(target, 1f);
            a.SetIKPosition(target, hit.point+hit.normal*0.05f*transform.lossyScale.x);
            a.SetIKRotation(target, Quaternion.FromToRotation(Vector3.up, hit.normal)*Quaternion.AngleAxis(-90f, foot.right)*foot.rotation);
        } else {
            a.SetIKPositionWeight(target, 0f);
            a.SetIKRotationWeight(target, 0f);
        }
    }
}
