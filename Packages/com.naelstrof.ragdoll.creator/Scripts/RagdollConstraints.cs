using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;

public static class RagdollConstraints {
    public class HumanoidConstraint {
        public Transform GetTargetTransform(Animator targetAnimator) => targetAnimator.transform.Find(targetPath);
        public Transform GetParentTransform(Animator targetAnimator) => targetAnimator.transform.Find(parentPath);
        public Transform GetParentRigidbodyTransform(Animator targetAnimator) => targetAnimator.transform.Find(parentRigidbodyPath);
        protected string targetPath;
        protected string parentPath;
        protected string parentRigidbodyPath;
        protected Quaternion neutralRotation;
        protected List<Quaternion> fromNeutralToScrunch;
        protected List<Quaternion> fromNeutralToStretch;
        protected Vector3 neutralLocalForward;
        protected Vector3 neutralLocalUp;
        protected Vector3 neutralLocalRight;
        protected float massScale = 1f;

        protected HumanoidConstraint() {
        }

        public HumanoidConstraint(Animator animator, RagdollScrunchStretchPack cachedScrunchStretchPack, Transform target, Transform parent, Vector3 worldJointRight, Vector3 worldJointUp, Vector3 worldJointForward, float massScale, Transform parentRigidbody = null) {
            Vector3 localRight = parent.InverseTransformDirection(worldJointRight);
            Vector3 localUp = parent.InverseTransformDirection(worldJointUp);
            Vector3 localForward = parent.InverseTransformDirection(worldJointForward);
            Vector3.OrthoNormalize(ref localForward, ref localUp, ref localRight);
            
            List<Quaternion> stretchRotations = GetRotationSet(animator, cachedScrunchStretchPack.GetStretchClips(), target, parent);
            List<Quaternion> scrunchRotations = GetRotationSet(animator, cachedScrunchStretchPack.GetScrunchClips(), target, parent);
            
            if (parentRigidbody == null) {
                parentRigidbody = parent;
            }

            cachedScrunchStretchPack.GetNeutralClip().SampleAnimation(animator.gameObject, 0f);
            Quaternion localPosedRotation = Quaternion.Inverse(parent.rotation)*target.rotation;

            neutralLocalRight = localRight;
            neutralLocalUp = localUp;
            neutralLocalForward = localForward;
            targetPath = AnimationUtility.CalculateTransformPath(target,animator.transform);
            parentPath = AnimationUtility.CalculateTransformPath(parent, animator.transform);
            parentRigidbodyPath = AnimationUtility.CalculateTransformPath(parentRigidbody, animator.transform);
            fromNeutralToScrunch = scrunchRotations;
            fromNeutralToStretch = stretchRotations;
            neutralRotation = localPosedRotation;
            this.massScale = massScale;
        }
        public void Render(Animator animator, RagdollConfiguration configuration) {
            var originalHandleColor = Handles.color;
            var targetBone = GetTargetTransform(animator);
            
            GetWorldBasis(animator, out Vector3 right, out Vector3 up, out Vector3 forward);
            GetMinMaxAngles(animator, out Vector3 minAngles, out Vector3 maxAngles);
            minAngles.z *= configuration.twistFactor;
            maxAngles.z *= configuration.twistFactor;
            Handles.color = Color.red;
            Handles.DrawLine(targetBone.position, targetBone.position+Quaternion.AngleAxis(minAngles.x,right)*forward*0.05f, 8);
            Handles.DrawLine(targetBone.position, targetBone.position+Quaternion.AngleAxis(maxAngles.x,right)*forward*0.05f, 8);
            Handles.DrawWireArc(targetBone.position, right,
                Quaternion.AngleAxis(minAngles.x, right) * forward, maxAngles.x - minAngles.x, 0.05f, 8);
            Handles.color = Color.green;
            Handles.DrawLine(targetBone.position, targetBone.position+Quaternion.AngleAxis(minAngles.y,up)*forward*0.05f, 8);
            Handles.DrawLine(targetBone.position, targetBone.position+Quaternion.AngleAxis(maxAngles.y,up)*forward*0.05f, 8);
            Handles.DrawWireArc(targetBone.position, up, Quaternion.AngleAxis(minAngles.y,up)*forward, maxAngles.y-minAngles.y, 0.05f,8);
            Handles.color = Color.blue;
            Handles.DrawWireArc(targetBone.position, forward, Quaternion.AngleAxis(minAngles.z,forward)*right, maxAngles.z-minAngles.z, 0.025f,8);
            Handles.color = originalHandleColor;
        }

        public void GetWorldBasis(Animator animator, out Vector3 right, out Vector3 up, out Vector3 forward) {
            Quaternion currentRotation = Quaternion.Inverse(GetParentTransform(animator).rotation) * GetTargetTransform(animator).rotation;
            Quaternion rotationCorrection =  currentRotation * Quaternion.Inverse(neutralRotation);
            Quaternion rotation = GetParentTransform(animator).rotation*rotationCorrection;
            right = rotation * neutralLocalRight;
            up = rotation * neutralLocalUp;
            forward = rotation * neutralLocalForward;
        }

        public virtual void GetMinMaxAngles(Animator animator, out Vector3 minAngles, out Vector3 maxAngles) {
            Quaternion currentRotation = Quaternion.Inverse(GetParentTransform(animator).rotation) * GetTargetTransform(animator).rotation;
            Quaternion rotationCorrection =  currentRotation * Quaternion.Inverse(neutralRotation);
            Quaternion correction = rotationCorrection;
            var minAngleSet = GetAnglesSet(fromNeutralToScrunch, correction*neutralRotation, correction*neutralLocalRight, correction*neutralLocalUp, correction*neutralLocalForward);
            var maxAngleSet = GetAnglesSet(fromNeutralToStretch, correction*neutralRotation, correction*neutralLocalRight, correction*neutralLocalUp, correction*neutralLocalForward);
            minAngles = Vector3.one*360f;
            maxAngles = Vector3.one*-360f;
            foreach (var minAngle in minAngleSet) {
               minAngles.x = Mathf.Min(minAngle.x, minAngles.x);
               minAngles.y = Mathf.Min(minAngle.y, minAngles.y);
               minAngles.z = Mathf.Min(minAngle.z, minAngles.z);
               maxAngles.x = Mathf.Max(minAngle.x, maxAngles.x);
               maxAngles.y = Mathf.Max(minAngle.y, maxAngles.y);
               maxAngles.z = Mathf.Max(minAngle.z, maxAngles.z);
            }
            foreach (var maxAngle in maxAngleSet) {
               minAngles.x = Mathf.Min(maxAngle.x, minAngles.x);
               minAngles.y = Mathf.Min(maxAngle.y, minAngles.y);
               minAngles.z = Mathf.Min(maxAngle.z, minAngles.z);
               maxAngles.x = Mathf.Max(maxAngle.x, maxAngles.x);
               maxAngles.y = Mathf.Max(maxAngle.y, maxAngles.y);
               maxAngles.z = Mathf.Max(maxAngle.z, maxAngles.z);
            }
        }

        public ConfigurableJoint GetOrCreate(Animator animator, RagdollConfiguration configuration) {
            var parentTransform = GetParentTransform(animator);
            var targetTransform = GetTargetTransform(animator);
            var parentRigidbodyTransform = GetParentRigidbodyTransform(animator);
            var parentRigidbody = parentRigidbodyTransform.GetComponent<Rigidbody>();
            if (parentRigidbody == null) {
                parentRigidbody = parentTransform.gameObject.AddComponent<Rigidbody>();
            }
            var targetRigidbody = targetTransform.GetComponent<Rigidbody>();
            if (targetRigidbody == null) {
                targetRigidbody = targetTransform.gameObject.AddComponent<Rigidbody>();
            }

            float massRatio = parentRigidbody.mass / targetRigidbody.mass;
            // Try to fix ridiculous mass ratios
            while (massRatio > 4f) {
                targetRigidbody.mass *= 2f;
                massRatio = parentRigidbody.mass / targetRigidbody.mass;
            }
            while (massRatio < 1f/4f) {
                parentRigidbody.mass *= 2f;
                massRatio = parentRigidbody.mass / targetRigidbody.mass;
            }

            var configurableJoint = targetTransform.GetComponent<ConfigurableJoint>();
            if (configurableJoint == null) {
                configurableJoint = targetTransform.gameObject.AddComponent<ConfigurableJoint>();
            }
            
            GetWorldBasis(animator, out Vector3 worldRightAxis, out Vector3 worldUpAxis, out Vector3 worldForwardAxis);
            GetMinMaxAngles(animator, out Vector3 minAngles, out Vector3 maxAngles);
            
            configurableJoint.connectedBody = parentRigidbody;
            configurableJoint.autoConfigureConnectedAnchor = false;
            configurableJoint.anchor = Vector3.zero;
            configurableJoint.connectedAnchor = parentRigidbodyTransform.InverseTransformPoint(targetTransform.position);
            configurableJoint.axis = -targetTransform.InverseTransformDirection(worldUpAxis);
            configurableJoint.secondaryAxis = targetTransform.InverseTransformDirection(worldRightAxis);
            configurableJoint.xMotion = ConfigurableJointMotion.Locked;
            configurableJoint.yMotion = ConfigurableJointMotion.Locked;
            configurableJoint.zMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularXMotion = ConfigurableJointMotion.Limited;
            configurableJoint.angularYMotion = ConfigurableJointMotion.Limited;
            configurableJoint.angularZMotion = ConfigurableJointMotion.Limited;
            configurableJoint.rotationDriveMode = RotationDriveMode.Slerp;
            var angularLimitSpring = configurableJoint.angularXLimitSpring;
            angularLimitSpring.spring = 1000f*targetRigidbody.mass;
            angularLimitSpring.damper = angularLimitSpring.spring*0.1f;
            configurableJoint.angularXLimitSpring = angularLimitSpring;
            configurableJoint.angularYZLimitSpring = angularLimitSpring;
            configurableJoint.enablePreprocessing = false;

            if (Mathf.Abs(minAngles.x - maxAngles.x) < 10f) {
                configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
            }
            
            if (Mathf.Abs(minAngles.y - maxAngles.y) < 10f) {
                configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
            }
            
            if (Mathf.Abs(minAngles.z - maxAngles.z) < 10f) {
                configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
            }

            var lowXLimit = configurableJoint.lowAngularXLimit;
            lowXLimit.limit = minAngles.y;
            lowXLimit.contactDistance = 15f;
            configurableJoint.lowAngularXLimit = lowXLimit;
            var highXLimit = configurableJoint.highAngularXLimit;
            highXLimit.limit = maxAngles.y;
            highXLimit.contactDistance = 15f;
            configurableJoint.highAngularXLimit = highXLimit;
            var lowYLimit = configurableJoint.angularYLimit;
            lowYLimit.limit = Mathf.Min(Mathf.Abs(minAngles.x), Mathf.Abs(maxAngles.x));
            lowYLimit.contactDistance = 15f;
            configurableJoint.angularYLimit = lowYLimit;
            var lowZLimit = configurableJoint.angularZLimit;
            lowZLimit.limit = Mathf.Min(Mathf.Abs(minAngles.z), Mathf.Abs(maxAngles.z))*configuration.twistFactor;
            lowZLimit.contactDistance = 15f;
            configurableJoint.angularZLimit = lowZLimit;
            var slerp = configurableJoint.slerpDrive;
            slerp.positionSpring = 60f*targetRigidbody.mass;
            slerp.positionDamper = slerp.positionSpring*0.1f;
            configurableJoint.SetTargetRotationLocal(neutralRotation, targetTransform.localRotation);
            configurableJoint.slerpDrive = slerp;
            configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
            configurableJoint.projectionAngle = 10f;
            configurableJoint.projectionDistance = 0.2f;
            configurableJoint.connectedMassScale = massScale;
            configurableJoint.massScale = 1f;
            configurableJoint.enablePreprocessing = false;
            return configurableJoint;
        }
    }
    
    public class SimpleHumanoidConstraint : HumanoidConstraint {
        private float angleLimit;
        public SimpleHumanoidConstraint(Animator animator, Transform target, Transform parent, Vector3 worldRight, Vector3 worldUp, Vector3 worldForward, float angleLimit, float massScale) {
            Vector3 localRight = parent.InverseTransformDirection(worldRight);
            Vector3 localUp = parent.InverseTransformDirection(worldUp);
            Vector3 localForward = parent.InverseTransformDirection(worldForward);
            Vector3.OrthoNormalize(ref localForward, ref localUp, ref localRight);
            Quaternion localPosedRotation = Quaternion.Inverse(parent.rotation)*target.rotation;

            neutralLocalRight = localRight;
            neutralLocalUp = localUp;
            neutralLocalForward = localForward;
            targetPath = AnimationUtility.CalculateTransformPath(target,animator.transform);
            parentPath = AnimationUtility.CalculateTransformPath(parent, animator.transform);
            parentRigidbodyPath = AnimationUtility.CalculateTransformPath(parent, animator.transform);
            neutralRotation = localPosedRotation;
            this.massScale = massScale;
            this.angleLimit = angleLimit;
        }

        public override void GetMinMaxAngles(Animator animator, out Vector3 minAngles, out Vector3 maxAngles) {
            minAngles = new Vector3(-angleLimit, -angleLimit, -angleLimit);
            maxAngles = new Vector3(angleLimit, angleLimit, angleLimit);
        }
    }
    public class HumanoidConstraints : List<HumanoidConstraint> { }
    
    public static HumanoidConstraints GenerateConstraints(Animator animator, RagdollConfiguration configuration, RagdollScrunchStretchPack cachedScrunchStretchPack) {
        var newGameObject = Object.Instantiate(animator.gameObject);
        animator = newGameObject.GetComponent<Animator>();
        
        cachedScrunchStretchPack.GetNeutralClip().SampleAnimation(animator.gameObject,0f);
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
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, rightUpperArm, animator.GetBoneTransform(HumanBodyBones.RightShoulder), rightUpperArmRight, rightUpperArmUp, rightUpperArmForward, 2f, chest));
        
        // Right Lower Arm
        Vector3 rightLowerArmForward = (rightHand.position - rightLowerArm.position).normalized;
        Vector3 rightLowerArmRight = (rightLowerArm.position-rightUpperArm.position).normalized;
        Vector3 rightLowerArmUp = Vector3.Cross(rightLowerArmForward, rightLowerArmRight).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, rightLowerArm, rightUpperArm, rightLowerArmRight, rightLowerArmUp, rightLowerArmForward, 2f));
        
        // Right Hand
        Vector3 rightHandForward = (rightHand.position - rightLowerArm.position).normalized;
        Vector3 rightHandRight = (rightLowerArm.position-rightUpperArm.position).normalized;
        Vector3 rightHandUp = Vector3.Cross(rightHandForward, rightHandRight).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, rightHand, rightLowerArm, rightHandRight, rightHandUp, rightHandForward, 2f));
        
        // Left Upper Arm
        Vector3 leftUpperArmForward = (leftLowerArm.position - leftUpperArm.position).normalized;
        Vector3 leftUpperArmUp = (neck.position-chest.position).normalized;
        Vector3 leftUpperArmRight = Vector3.Cross(leftUpperArmUp, leftUpperArmForward).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, leftUpperArm, animator.GetBoneTransform(HumanBodyBones.LeftShoulder), leftUpperArmRight, leftUpperArmUp, leftUpperArmForward, 2f, chest));
        
        // Left Lower Arm
        Vector3 leftLowerArmForward = (leftHand.position - leftLowerArm.position).normalized;
        Vector3 leftLowerArmRight = (leftUpperArm.position-leftLowerArm.position).normalized;
        Vector3 leftLowerArmUp = Vector3.Cross(leftLowerArmForward, leftLowerArmRight).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, leftLowerArm, leftUpperArm, leftLowerArmRight, leftLowerArmUp, leftLowerArmForward, 2f));
        
        // Left Hand
        Vector3 leftHandForward = (leftHand.position - leftLowerArm.position).normalized;
        Vector3 leftHandRight = (leftUpperArm.position-leftLowerArm.position).normalized;
        Vector3 leftHandUp = Vector3.Cross(leftHandForward, leftHandRight).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, leftHand, leftLowerArm, leftHandRight, leftHandUp, leftHandForward, 2f));
        
        // Chest
        Vector3 chestForward = (neck.position - chest.position).normalized;
        Vector3 chestRight = (rightUpperArm.position - leftUpperArm.position).normalized;
        Vector3 chestUp = Vector3.Cross(chestForward, chestRight).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, chest, spine, chestRight, chestUp, chestForward, 1f));
        
        // Spine
        Vector3 spineForward = (chest.position - spine.position).normalized;
        Vector3 spineRight = (rightUpperArm.position - leftUpperArm.position).normalized;
        Vector3 spineUp = Vector3.Cross(spineForward, spineRight).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, spine, hips, spineRight, spineUp, spineForward, 2f));
        
        // Right Upper Leg
        Vector3 rightUpperLegForward = (rightLowerLeg.position - rightUpperLeg.position).normalized;
        Vector3 rightUpperLegUp = (rightUpperLeg.position - leftUpperLeg.position).normalized;
        Vector3 rightUpperLegRight = Vector3.Cross(rightUpperLegUp, rightUpperLegForward).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, rightUpperLeg, hips, rightUpperLegRight, rightUpperLegUp, rightUpperLegForward, 1f));
        
        // Right Lower Leg
        Vector3 rightLowerLegForward = (rightFoot.position - rightLowerLeg.position).normalized;
        Vector3 rightLowerLegRight = (rightUpperLeg.position - rightLowerLeg.position).normalized;
        Vector3 rightLowerLegUp = Vector3.Cross(rightLowerLegForward, rightLowerLegRight).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, rightLowerLeg, rightUpperLeg, rightLowerLegRight, rightLowerLegUp, rightLowerLegForward, 1f));
        
        // Right foot
        Vector3 rightFootForward = (rightLowerLeg.position - rightUpperLeg.position).normalized;
        Vector3 rightFootRight = (rightUpperLeg.position - leftUpperLeg.position).normalized;
        Vector3 rightFootUp = Vector3.Cross(rightLowerLegForward, rightLowerLegRight).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, rightFoot, rightLowerLeg, rightFootRight, rightFootUp, rightFootForward, 2f));
        
        // Left Upper Leg
        Vector3 leftUpperLegForward = (leftLowerLeg.position - leftUpperLeg.position).normalized;
        Vector3 leftUpperLegUp = (leftUpperLeg.position - rightUpperLeg.position).normalized;
        Vector3 leftUpperLegRight = Vector3.Cross(leftUpperLegUp, leftUpperLegForward).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, leftUpperLeg, hips, leftUpperLegRight, leftUpperLegUp, leftUpperLegForward, 2f));
        
        // Left Lower Leg
        Vector3 leftLowerLegForward = (leftFoot.position - leftLowerLeg.position).normalized;
        Vector3 leftLowerLegRight = (leftUpperLeg.position - leftLowerLeg.position).normalized;
        Vector3 leftLowerLegUp = Vector3.Cross(leftLowerLegRight, leftLowerLegForward).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, leftLowerLeg, leftUpperLeg, -leftLowerLegRight, leftLowerLegUp, leftLowerLegForward,2f));
        
        // Left foot
        Vector3 leftFootForward = (leftLowerLeg.position - leftUpperLeg.position).normalized;
        Vector3 leftFootRight = (leftUpperLeg.position - leftUpperLeg.position).normalized;
        Vector3 leftFootUp = Vector3.Cross(leftLowerLegForward, leftLowerLegRight).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, leftFoot, leftLowerLeg, leftFootRight, -leftFootUp, leftFootForward,2f));
        
        // Neck 
        Vector3 neckForward = (head.position - neck.position).normalized;
        Vector3 neckRight = bodyRight.normalized;
        Vector3 neckUp = Vector3.Cross(neckForward, neckRight).normalized;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, neck, chest, neckRight, neckUp, neckForward,2f));
        
        // Head 
        Vector3 headForward = bodyForward;
        Vector3 headRight = bodyRight;
        Vector3 headUp = bodyUp;
        constraints.Add(new HumanoidConstraint(animator, cachedScrunchStretchPack, head, neck, headRight, headUp, headForward, 2f));
        
        
        // Tail
        if (!string.IsNullOrEmpty(configuration.tailPath)) {
            const int maxDepth = 16;
            Transform start = animator.transform.Find(configuration.tailPath);
            int depth = 0;
            Transform end = start.GetChild(0);
            while (depth < maxDepth) {
                depth++;
                if (end.childCount == 0) {
                    break;
                }

                end = end.GetChild(0);
            }

            Vector3 startForward = (end.position - start.position).normalized;
            Vector3 startRight = -bodyRight;
            Vector3 startUp = Vector3.Cross(startForward, startRight);
            constraints.Add(new SimpleHumanoidConstraint(animator, start, hips, startRight, startUp, startForward, configuration.tailFlexibility, 3f));
            int currentDepth = 0;
            end = start.GetChild(0);
            while (currentDepth <= depth) {
                Vector3 tailForward = (end.position - start.position).normalized;
                Vector3 tailRight = -bodyRight;
                Vector3 tailUp = Vector3.Cross(tailForward, tailRight);
                constraints.Add(new SimpleHumanoidConstraint(animator, end, start, tailRight, tailUp, tailForward, configuration.tailFlexibility, 2f));
                currentDepth++;
                if (end.childCount == 0) {
                    break;
                }

                start = end;
                end = end.GetChild(0);
            }

            Vector3 tailEndForward = (end.position - start.position).normalized;
            Vector3 tailEndRight = -bodyRight;
            Vector3 tailEndUp = Vector3.Cross(tailEndForward, tailEndRight);
            constraints.Add(new SimpleHumanoidConstraint(animator, end, start, tailEndRight, tailEndUp, tailEndForward, configuration.tailFlexibility, 2f));
        }

        Object.DestroyImmediate(newGameObject);

        return constraints;
    }

    public static void PreviewConstraints(Animator animator, RagdollConfiguration configuration, HumanoidConstraints constraints) {
        foreach (var constraint in constraints) {
            constraint.Render(animator, configuration);
        }
    }

    private static Vector3 RotateAngles(Vector3 angleLimits, Quaternion rotation) {
        Quaternion newRotation = Quaternion.Euler(angleLimits.x, angleLimits.y,angleLimits.z);
        newRotation = rotation * newRotation;
        var euler = newRotation.eulerAngles;
        euler.x = Mathf.Repeat(euler.x + 180f, 360f) - 180f;
        euler.y = Mathf.Repeat(euler.y + 180f, 360f) - 180f;
        euler.z = Mathf.Repeat(euler.z + 180f, 360f) - 180f;
        return new Vector3(euler.x, euler.y, euler.z);
    }

    private static List<Quaternion> GetRotationSet(Animator animator, ICollection<AnimationClip> clips, Transform target, Transform parent) {
        List<Quaternion> rotSet = new List<Quaternion>();
        foreach (var clip in clips) {
            clip.SampleAnimation(animator.gameObject, 0f);
            Quaternion localClipRotation = Quaternion.Inverse(parent.rotation)*target.rotation;
            rotSet.Add(localClipRotation);
        }
        return rotSet;
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
    private static List<Vector3> GetAnglesSet(IList<Quaternion> localRotations, Quaternion localNeutralPose, Vector3 localRight, Vector3 localUp, Vector3 localForward) {
        List<Vector3> angleSet = new List<Vector3>();
        foreach (var localClipRotation in localRotations) {
            Quaternion neutralToClip = localClipRotation * Quaternion.Inverse(localNeutralPose);

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