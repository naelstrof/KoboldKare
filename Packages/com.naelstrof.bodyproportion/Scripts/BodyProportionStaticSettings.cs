using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naelstrof.BodyProportion {
    // TODO: This should probably interface with unity's settings, so you could configure which bones you want to be configurable. Though it's a little overkill to want more than this.
    public static class BodyProportionStaticSettings {
        [System.Flags]
        public enum BoneFlags {
            None = 0,
            Scale = 1,
            Blendshape = 2,
            IgnoreParentScale = 4,
        }

        private static readonly Dictionary<HumanBodyBones, BoneFlags> boneData = new Dictionary<HumanBodyBones, BoneFlags> {
            {HumanBodyBones.Hips, BoneFlags.Scale | BoneFlags.IgnoreParentScale | BoneFlags.Blendshape},
            {HumanBodyBones.Chest, BoneFlags.Scale | BoneFlags.Blendshape},
            {HumanBodyBones.Spine, BoneFlags.Scale | BoneFlags.Blendshape},
            {HumanBodyBones.LeftUpperLeg, BoneFlags.Scale | BoneFlags.Blendshape},
            {HumanBodyBones.RightUpperLeg, BoneFlags.Scale | BoneFlags.Blendshape},
            {HumanBodyBones.LeftShoulder, BoneFlags.Scale | BoneFlags.Blendshape},
            {HumanBodyBones.RightShoulder, BoneFlags.Scale | BoneFlags.Blendshape},
            {HumanBodyBones.LeftUpperArm, BoneFlags.Scale | BoneFlags.Blendshape},
            {HumanBodyBones.RightUpperArm, BoneFlags.Scale | BoneFlags.Blendshape},
            {HumanBodyBones.LeftHand, BoneFlags.Scale | BoneFlags.IgnoreParentScale},
            {HumanBodyBones.RightHand, BoneFlags.Scale | BoneFlags.IgnoreParentScale},
            {HumanBodyBones.LeftFoot, BoneFlags.Scale | BoneFlags.IgnoreParentScale},
            {HumanBodyBones.RightFoot, BoneFlags.Scale | BoneFlags.IgnoreParentScale},
            {HumanBodyBones.Head, BoneFlags.Scale},
        };

        public static bool HasFlag(HumanBodyBones bone, BoneFlags flag) {
            if (!boneData.ContainsKey(bone)) {
                return false;
            }
            return (boneData[bone] & flag) != 0;
        }
        
        public static Transform GetChildBone(this Animator animator, HumanBodyBones bone) {
            switch(bone) {
                case HumanBodyBones.Hips:
                    return animator.GetBoneTransform(HumanBodyBones.Spine);
                case HumanBodyBones.Spine:
                    return animator.GetBoneTransform(HumanBodyBones.Chest);
                case HumanBodyBones.Chest:
                    return animator.GetBoneTransform(HumanBodyBones.Neck);
                case HumanBodyBones.Neck:
                    return animator.GetBoneTransform(HumanBodyBones.Head);
                case HumanBodyBones.LeftShoulder:
                    return animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                case HumanBodyBones.RightShoulder:
                    return animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                case HumanBodyBones.LeftUpperArm:
                    return animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                case HumanBodyBones.RightUpperArm:
                    return animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                case HumanBodyBones.RightLowerArm:
                    return animator.GetBoneTransform(HumanBodyBones.RightHand);
                case HumanBodyBones.LeftLowerArm:
                    return animator.GetBoneTransform(HumanBodyBones.LeftHand);
                case HumanBodyBones.LeftUpperLeg:
                    return animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                case HumanBodyBones.RightUpperLeg:
                    return animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                case HumanBodyBones.LeftLowerLeg:
                    return animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                case HumanBodyBones.RightLowerLeg:
                    return animator.GetBoneTransform(HumanBodyBones.RightFoot);
                default:
                    return animator.GetBoneTransform(bone);
            }
        }
        public static Transform GetParentBone(this Animator animator, HumanBodyBones bone) {
            switch(bone) {
                case HumanBodyBones.Hips:
                    return animator.GetBoneTransform(HumanBodyBones.Hips);
                case HumanBodyBones.Spine:
                    return animator.GetBoneTransform(HumanBodyBones.Hips);
                case HumanBodyBones.Chest:
                    return animator.GetBoneTransform(HumanBodyBones.Spine);
                case HumanBodyBones.Neck:
                    return animator.GetBoneTransform(HumanBodyBones.Chest);
                case HumanBodyBones.LeftShoulder:
                    return animator.GetBoneTransform(HumanBodyBones.Chest);
                case HumanBodyBones.RightShoulder:
                    return animator.GetBoneTransform(HumanBodyBones.Chest);
                case HumanBodyBones.Head:
                    return animator.GetBoneTransform(HumanBodyBones.Neck);
                case HumanBodyBones.LeftUpperArm:
                    return animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
                case HumanBodyBones.RightUpperArm:
                    return animator.GetBoneTransform(HumanBodyBones.RightShoulder);
                case HumanBodyBones.RightLowerArm:
                    return animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                case HumanBodyBones.LeftLowerArm:
                    return animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                case HumanBodyBones.LeftUpperLeg:
                    return animator.GetBoneTransform(HumanBodyBones.Hips);
                case HumanBodyBones.RightUpperLeg:
                    return animator.GetBoneTransform(HumanBodyBones.Hips);
                case HumanBodyBones.LeftLowerLeg:
                    return animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                case HumanBodyBones.RightLowerLeg:
                    return animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                case HumanBodyBones.LeftFoot:
                    return animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                case HumanBodyBones.RightFoot:
                    return animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                case HumanBodyBones.LeftHand:
                    return animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                case HumanBodyBones.RightHand:
                    return animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                default:
                    return animator.GetBoneTransform(bone);
            }
        }
    }

}