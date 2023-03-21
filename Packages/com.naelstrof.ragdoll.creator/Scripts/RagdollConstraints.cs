using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public static class RagdollConstraints {
    private static RagdollScrunchStretchPack cachedScrunchStretchPack;
    public struct HumanoidConstraint {
        public Transform targetTransform;
        public Transform parentTransform;
        public Vector3 localForward;
        public Vector3 localUp;
        public Vector3 localRight;
        public Vector3 minAngles;
        public Vector3 maxAngles;
    }
    public class HumanoidConstraints : List<HumanoidConstraint> { }
    
    public static HumanoidConstraints GenerateConstraints(Animator animator) {
        HumanoidConstraints constraints = new HumanoidConstraints();
        var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        var rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        var rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        var leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        var leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        var chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        var neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        var spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        var rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        var rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        var leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        var leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        var rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        var leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        
        Vector3 bodyRight = (rightHand.position - leftHand.position).normalized;
        Vector3 bodyUp = (head.position - hips.position).normalized;
        Vector3 bodyForward = Vector3.Cross(bodyRight, bodyUp);
        Vector3.OrthoNormalize(ref bodyForward, ref bodyUp, ref bodyRight);
        
        // Right Upper Arm
        Vector3 rightUpperArmForward = (rightLowerArm.position - rightUpperArm.position).normalized;
        Vector3 rightUpperArmUp = (neck.position-chest.position).normalized;
        Vector3 rightUpperArmRight = Vector3.Cross(rightUpperArmUp, rightUpperArmForward).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.RightUpperArm, HumanBodyBones.RightShoulder, rightUpperArmRight, rightUpperArmUp, rightUpperArmForward));
        
        // Right Lower Arm
        Vector3 rightLowerArmForward = (rightHand.position - rightLowerArm.position).normalized;
        Vector3 rightLowerArmRight = (rightLowerArm.position-rightUpperArm.position).normalized;
        Vector3 rightLowerArmUp = Vector3.Cross(rightLowerArmForward, rightLowerArmRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, rightLowerArmRight, rightLowerArmUp, rightLowerArmForward));
        
        // Right Hand
        Vector3 rightHandForward = (rightHand.position - rightLowerArm.position).normalized;
        Vector3 rightHandRight = (rightLowerArm.position-rightUpperArm.position).normalized;
        Vector3 rightHandUp = Vector3.Cross(rightHandForward, rightHandRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm, rightHandRight, rightHandUp, rightHandForward));
        
        // Left Upper Arm
        Vector3 leftUpperArmForward = (leftLowerArm.position - leftUpperArm.position).normalized;
        Vector3 leftUpperArmUp = (neck.position-chest.position).normalized;
        Vector3 leftUpperArmRight = Vector3.Cross(leftUpperArmUp, leftUpperArmForward).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftShoulder,
            leftUpperArmRight, leftUpperArmUp, leftUpperArmForward));
        
        // Left Lower Arm
        Vector3 leftLowerArmForward = (leftHand.position - leftLowerArm.position).normalized;
        Vector3 leftLowerArmRight = (leftLowerArm.position-leftUpperArm.position).normalized;
        Vector3 leftLowerArmUp = Vector3.Cross(leftLowerArmForward, leftLowerArmRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm,
            leftLowerArmRight, leftLowerArmUp, leftLowerArmForward));
        
        // Left Hand
        Vector3 leftHandForward = (leftHand.position - leftLowerArm.position).normalized;
        Vector3 leftHandRight = (leftLowerArm.position-leftUpperArm.position).normalized;
        Vector3 leftHandUp = Vector3.Cross(leftHandForward, leftHandRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm, leftHandRight, leftHandUp, leftHandForward));
        
        // Chest
        Vector3 chestForward = (neck.position - chest.position).normalized;
        Vector3 chestRight = (rightUpperArm.position - leftUpperArm.position).normalized;
        Vector3 chestUp = Vector3.Cross(chestForward, chestRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.Chest, HumanBodyBones.Spine, chestRight, chestUp, chestForward));
        
        // Spine
        Vector3 spineForward = (chest.position - spine.position).normalized;
        Vector3 spineRight = (rightUpperArm.position - leftUpperArm.position).normalized;
        Vector3 spineUp = Vector3.Cross(spineForward, spineRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.Spine, HumanBodyBones.Hips, spineRight, spineUp, spineForward));
        
        // Right Upper Leg
        Vector3 rightUpperLegForward = (rightLowerLeg.position - rightUpperLeg.position).normalized;
        Vector3 rightUpperLegRight = (rightUpperLeg.position - leftUpperLeg.position).normalized;
        Vector3 rightUpperLegUp = Vector3.Cross(rightUpperLegForward, rightUpperLegRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.RightUpperLeg, HumanBodyBones.Hips, rightUpperLegRight, rightUpperLegUp, rightUpperLegForward));
        
        // Right Lower Leg
        Vector3 rightLowerLegForward = (rightFoot.position - rightLowerLeg.position).normalized;
        Vector3 rightLowerLegRight = (rightUpperLeg.position - rightLowerLeg.position).normalized;
        Vector3 rightLowerLegUp = Vector3.Cross(rightLowerLegForward, rightLowerLegRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightUpperLeg, rightLowerLegRight, rightLowerLegUp, rightLowerLegForward));
        
        // Right foot
        Vector3 rightFootForward = (rightLowerLeg.position - rightUpperLeg.position).normalized;
        Vector3 rightFootRight = (rightUpperLeg.position - leftUpperLeg.position).normalized;
        Vector3 rightFootUp = Vector3.Cross(rightLowerLegForward, rightLowerLegRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg, rightFootRight, rightFootUp, rightFootForward));
        
        // Left Upper Leg
        Vector3 leftUpperLegForward = (leftLowerLeg.position - leftUpperLeg.position).normalized;
        Vector3 leftUpperLegRight = (rightUpperLeg.position - leftUpperLeg.position).normalized;
        Vector3 leftUpperLegUp = Vector3.Cross(leftUpperLegForward, leftUpperLegRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.LeftUpperLeg, HumanBodyBones.Hips, leftUpperLegRight, leftUpperLegUp, leftUpperLegForward));
        
        // Left Lower Leg
        Vector3 leftLowerLegForward = (leftFoot.position - leftLowerLeg.position).normalized;
        Vector3 leftLowerLegRight = (leftUpperLeg.position - leftLowerLeg.position).normalized;
        Vector3 leftLowerLegUp = Vector3.Cross(leftLowerLegForward, leftLowerLegRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg, leftLowerLegRight, leftLowerLegUp, leftLowerLegForward));
        
        // Left foot
        Vector3 leftFootForward = (leftLowerLeg.position - leftUpperLeg.position).normalized;
        Vector3 leftFootRight = (leftUpperLeg.position - leftUpperLeg.position).normalized;
        Vector3 leftFootUp = Vector3.Cross(leftLowerLegForward, leftLowerLegRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg, leftFootRight, leftFootUp, leftFootForward));
        
        // Neck 
        Vector3 neckForward = (head.position - neck.position).normalized;
        Vector3 neckRight = bodyRight.normalized;
        Vector3 neckUp = Vector3.Cross(neckForward, neckRight).normalized;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.Neck, HumanBodyBones.Chest, neckRight, neckUp, neckForward));
        
        // Head 
        Vector3 headForward = bodyForward;
        Vector3 headRight = bodyRight;
        Vector3 headUp = bodyUp;
        constraints.Add(GenerateHumanoidConstraint(animator, HumanBodyBones.Head, HumanBodyBones.Neck, headRight, headUp, headForward));

        return constraints;
    }

    private static HumanoidConstraint GenerateHumanoidConstraint(Animator animator, HumanBodyBones target, HumanBodyBones parent, Vector3 worldJointRight, Vector3 worldJointUp, Vector3 worldJointForward) {
        var parentBone = animator.GetBoneTransform(parent);
        var targetBone = animator.GetBoneTransform(target);
        Vector3 localRight = parentBone.InverseTransformDirection(worldJointRight);
        Vector3 localUp = parentBone.InverseTransformDirection(worldJointUp);
        Vector3 localForward = parentBone.InverseTransformDirection(worldJointForward);
        Vector3.OrthoNormalize(ref localForward, ref localUp, ref localRight);
        
        GetMinMaxAngle(animator, target, parent, localRight, localUp, localForward, out Vector3 minAngles, out Vector3 maxAngles);
        
        return new HumanoidConstraint {
            localRight = localRight,
            localUp = localUp,
            localForward = localForward,
            minAngles = minAngles,
            maxAngles = maxAngles,
            targetTransform = animator.GetBoneTransform(target),
            parentTransform = animator.GetBoneTransform(parent),
        };
    }

    public static void PreviewConstraints(HumanoidConstraints constraints) {
        foreach (var constraint in constraints) {
            RenderConstraint(constraint);
        }
    }

    private static void RenderConstraint(HumanoidConstraint constraint) {
        var originalHandleColor = Handles.color;
        var targetBone = constraint.targetTransform;
        var parentBone = constraint.parentTransform;
        Quaternion rotation = parentBone.rotation;

        Vector3 right = rotation*constraint.localRight;
        Vector3 forward = rotation*constraint.localForward;
        Vector3 up = rotation*constraint.localUp;
        
        Handles.color = Color.red;
        Handles.DrawLine(targetBone.position, targetBone.position+Quaternion.AngleAxis(constraint.minAngles.x,right)*forward*0.05f, 8);
        Handles.DrawLine(targetBone.position, targetBone.position+Quaternion.AngleAxis(constraint.maxAngles.x,right)*forward*0.05f, 8);
        Handles.DrawWireArc(targetBone.position, right,
            Quaternion.AngleAxis(constraint.minAngles.x, right) * forward, constraint.maxAngles.x - constraint.minAngles.x, 0.05f, 8);
        Handles.color = Color.green;
        Handles.DrawLine(targetBone.position, targetBone.position+Quaternion.AngleAxis(constraint.minAngles.y,up)*forward*0.05f, 8);
        Handles.DrawLine(targetBone.position, targetBone.position+Quaternion.AngleAxis(constraint.maxAngles.y,up)*forward*0.05f, 8);
        Handles.DrawWireArc(targetBone.position, up, Quaternion.AngleAxis(constraint.minAngles.y,up)*forward, constraint.maxAngles.y-constraint.minAngles.y, 0.05f,8);
        Handles.color = Color.blue;
        Handles.DrawWireArc(targetBone.position, forward, Quaternion.AngleAxis(constraint.minAngles.z,forward)*right, constraint.maxAngles.z-constraint.minAngles.z, 0.025f,8);
        Handles.color = originalHandleColor;
    }
    private static void GetMinMaxAngle(Animator animator, HumanBodyBones target, HumanBodyBones parent, Vector3 localRight, Vector3 localUp, Vector3 localForward, out Vector3 minAngles, out Vector3 maxAngles) {
        if (cachedScrunchStretchPack == null) {
            cachedScrunchStretchPack = AssetDatabase.LoadAssetAtPath<RagdollScrunchStretchPack>( AssetDatabase.GUIDToAssetPath("f918570129faed5418e218f0599df41a"));
        }

        var startingPosition = animator.transform.position;
        var targetBone = animator.GetBoneTransform(target);
        var parentBone = animator.GetBoneTransform(parent);
        cachedScrunchStretchPack.GetNeutralClip().SampleAnimation(animator.gameObject, 0f);
        Quaternion localNeutralRot = Quaternion.Inverse(parentBone.rotation)*targetBone.rotation;
        var minAngleSet = GetAnglesSet(animator, cachedScrunchStretchPack.GetScrunchClips(), localRight, localUp, localForward, localNeutralRot, targetBone, parentBone);
        var maxAngleSet = GetAnglesSet(animator, cachedScrunchStretchPack.GetStretchClips(), localRight, localUp, localForward, localNeutralRot, targetBone, parentBone);

        minAngles = Vector3.zero;
        foreach (var min in minAngleSet) {
            minAngles.x = Mathf.Min(min.x, minAngles.x);
            minAngles.y = Mathf.Min(min.y, minAngles.y);
            minAngles.z = Mathf.Min(min.z, minAngles.z);
        }
        foreach (var max in maxAngleSet) {
            minAngles.x = Mathf.Min(max.x, minAngles.x);
            minAngles.y = Mathf.Min(max.y, minAngles.y);
            minAngles.z = Mathf.Min(max.z, minAngles.z);
        }
        
        maxAngles = Vector3.zero;
        foreach (var min in minAngleSet) {
            maxAngles.x = Mathf.Max(min.x, maxAngles.x);
            maxAngles.y = Mathf.Max(min.y, maxAngles.y);
            maxAngles.z = Mathf.Max(min.z, maxAngles.z);
        }
        foreach (var max in maxAngleSet) {
            maxAngles.x = Mathf.Max(max.x, maxAngles.x);
            maxAngles.y = Mathf.Max(max.y, maxAngles.y);
            maxAngles.z = Mathf.Max(max.z, maxAngles.z);
        }
        cachedScrunchStretchPack.GetNeutralClip().SampleAnimation(animator.gameObject, 0f);
    }

    private static void DecomposeSwingTwist ( Quaternion q, Vector3 twistAxis, out Quaternion swing, out Quaternion twist ) {
        Vector3 r = new Vector3(q.x, q.y, q.z);
        // singularity: rotation by 180 degree
        if (r.sqrMagnitude < Mathf.Epsilon) {
            Vector3 rotatedTwistAxis = q * twistAxis;
            Vector3 swingAxis = Vector3.Cross(twistAxis, rotatedTwistAxis);
            if (swingAxis.sqrMagnitude > Mathf.Epsilon) {
                float swingAngle = Vector3.Angle(twistAxis, rotatedTwistAxis);
                swing = Quaternion.AngleAxis(swingAngle, swingAxis);
            } else {
                // more singularity: 
                // rotation axis parallel to twist axis
                swing = Quaternion.identity; // no swing
            }
            // always twist 180 degree on singularity
            twist = Quaternion.AngleAxis(180.0f, twistAxis);
            return;
        }
        // meat of swing-twist decomposition
        Vector3 p = Vector3.Project(r, twistAxis);
        twist = new Quaternion(p.x, p.y, p.z, q.w);
        twist = Quaternion.Normalize(twist);
        swing = q * Quaternion.Inverse(twist);
    }
    private static List<Vector3> GetAnglesSet(Animator animator, ICollection<AnimationClip> clips, Vector3 localRight, Vector3 localUp, Vector3 localForward, Quaternion neutralPose, Transform targetBone, Transform parentBone) {
        List<Vector3> angleSet = new List<Vector3>();
        foreach (var clip in clips) {
            clip.SampleAnimation(animator.gameObject, 0f);
            Quaternion localClipRotation = Quaternion.Inverse(parentBone.rotation)*targetBone.rotation;
            Quaternion neutralToClip = localClipRotation * Quaternion.Inverse(neutralPose);

            DecomposeSwingTwist(neutralToClip, localRight, out Quaternion rightSwing, out Quaternion rightTwist);
            rightTwist.ToAngleAxis(out float rightAngle, out Vector3 extractRightAxis);
            rightAngle *= Vector3.Dot(localRight, extractRightAxis);
            
            DecomposeSwingTwist(neutralToClip, localUp, out Quaternion upSwing, out Quaternion upTwist);
            upTwist.ToAngleAxis(out float upAngle, out Vector3 extractUpAxis);
            upAngle *= Vector3.Dot(localUp, extractUpAxis);
            
            DecomposeSwingTwist(neutralToClip, localForward, out Quaternion forwardSwing, out Quaternion forwardTwist);
            forwardTwist.ToAngleAxis(out float forwardAngle, out Vector3 extractForwardAxis);
            forwardAngle *= Vector3.Dot(localForward, extractForwardAxis);
            
            Vector3 angles = Vector3.zero;
            angles.x = Mathf.Repeat(rightAngle + 180f, 360f)-180f;
            angles.y = Mathf.Repeat(upAngle + 180f, 360f)-180f;
            angles.z = Mathf.Repeat(forwardAngle + 180f, 360f)-180f;
            angleSet.Add(angles);
        }
        return angleSet;
    }
}

#endif