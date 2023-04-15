using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

public static class RagdollColliders {
    public class HumanoidRagdollColliders : List<RagdollCollider> {
    }

    public class RagdollCollider {
        public Transform GetParentBone(Animator animator) {
            return animator.transform.Find(parentPath);
        }
        public string parentPath;
        public string connectedBodyPath;
        public Matrix4x4 localTransform;
        public string name;
        protected virtual Collider GetTemporaryColliderForCollisionTesting(Animator animator) {
            return null;
        }
        
        private bool CheckOverlap(RagdollCollider other, Animator targetAnimator) {
            Collider a = GetTemporaryColliderForCollisionTesting(targetAnimator);
            Collider b = other.GetTemporaryColliderForCollisionTesting(targetAnimator);
            bool overlapping = Physics.ComputePenetration(a, a.transform.position, a.transform.rotation, b, b.transform.position,
                b.transform.rotation, out Vector3 dir, out float distance);
            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(b.gameObject);
            return overlapping;
        }
        

        public RagdollCollider(string name) {
            this.name = name;
        }
        
        public IEnumerable<RagdollCollider> GetOverlappingSeconds(Animator animator, HumanoidRagdollColliders allColliders) {
            foreach (var other in allColliders) {
                if (other == this) {
                    continue;
                }
                if (other.parentPath == parentPath) continue;
                if (other.connectedBodyPath != connectedBodyPath) continue;
                if (!CheckOverlap(other, animator)) continue;
                yield return other;
            }
        }

        public virtual void DrawHandles(Animator animator, HumanoidRagdollColliders allColliders) {
            Handles.color = Color.white;
            foreach (var other in GetOverlappingSeconds(animator, allColliders)) {
                Handles.color = Color.red;
                Handles.DrawLine((GetParentBone(animator).localToWorldMatrix * localTransform).GetPosition(),
                    (other.GetParentBone(animator).localToWorldMatrix * localTransform).GetPosition(), 8);
                Handles.color = Color.magenta;
            }
        }

        public virtual Collider GetOrCreate(Animator animator, float ragdollMassPerCubicMeter) {
            return null;
        }
        public virtual Collider Get(Animator animator) {
            return null;
        }
    }
    

    public class RagdollCapsuleCollider : RagdollCollider {
        public enum CapsuleDirection {
            XAxis,
            YAxis,
            ZAxis,
        }

        public float height;
        public float radius;
        public Vector3 center = Vector3.zero;
        public const CapsuleDirection capsuleDirection = CapsuleDirection.YAxis;

        protected override Collider GetTemporaryColliderForCollisionTesting(Animator animator) {
            GameObject newGameObject = new GameObject("temp", typeof(CapsuleCollider));
            CapsuleCollider caps = newGameObject.GetComponent<CapsuleCollider>();
            Matrix4x4 localToWorld = (GetParentBone(animator).localToWorldMatrix * localTransform);
            caps.transform.position = localToWorld.GetPosition();
            caps.transform.rotation = localToWorld.rotation;
            caps.transform.localScale = localToWorld.lossyScale;
            caps.center = center;
            caps.direction = (int)capsuleDirection;
            caps.radius = radius;
            caps.height = height+radius*2f;
            return caps;
        }

        public override Collider Get(Animator animator) {
            var parent = GetParentBone(animator);
            bool needsChild = localTransform != Matrix4x4.identity;
            bool hasChild = parent.Find(name + "RagdollCapsuleCollider") != null;
            if (hasChild && needsChild) {
                return parent.Find(name + "RagdollCapsuleCollider").GetComponent<CapsuleCollider>();
            }

            return parent.GetComponent<CapsuleCollider>();
        }

        public override Collider GetOrCreate(Animator animator, float ragdollMassPerCubicMeter) {
            GameObject childObject = null;
            var parent = GetParentBone(animator);
            bool needsChild = localTransform != Matrix4x4.identity;
            bool hasChild = parent.Find(name + "RagdollCapsuleCollider") != null;
            
            if (!hasChild && needsChild) {
                childObject = new GameObject(name+"RagdollCapsuleCollider", typeof(CapsuleCollider));
                childObject.transform.SetParent(parent);
                Undo.RegisterCreatedObjectUndo(childObject, "Created a capsule.");
                hasChild = true;
            }
            if (hasChild && !needsChild) {
                Undo.DestroyObjectImmediate(parent.Find(name + "RagdollCapsuleCollider").gameObject);
            }
            if (needsChild && childObject == null) {
                childObject = parent.Find(name + "RagdollCapsuleCollider").gameObject;
                Undo.RegisterCompleteObjectUndo(childObject, "Changing the capsule.");
            }

            if (needsChild && hasChild) {
                Undo.RegisterFullObjectHierarchyUndo(childObject.transform, "Moved child object");
                childObject.transform.localPosition = localTransform.GetPosition();
                childObject.transform.localRotation = localTransform.rotation;
            }

            var parentRigidbody = parent.GetComponent<Rigidbody>();
            if (parentRigidbody == null) {
                parentRigidbody = parent.gameObject.AddComponent<Rigidbody>();
                Undo.RegisterCreatedObjectUndo(parentRigidbody, "Created rigidbody");
            }
            Undo.RecordObject(parentRigidbody, "Adjusted rigidbody");
            Matrix4x4 localToWorld = (parent.localToWorldMatrix * localTransform);
            float r = radius * localToWorld.lossyScale.x;
            float a = height * localToWorld.lossyScale.x;
            float volume = Mathf.PI * r * r * (4f / 3f * r + a);
            parentRigidbody.mass = volume * ragdollMassPerCubicMeter;

            CapsuleCollider collider;
            if (childObject == null) {
                collider = parent.GetComponent<CapsuleCollider>();
                if (collider == null) {
                    collider = parent.gameObject.AddComponent<CapsuleCollider>();
                }
            } else {
                collider = childObject.GetComponent<CapsuleCollider>();
                if (collider == null) {
                    collider = childObject.gameObject.AddComponent<CapsuleCollider>();
                }
            }

            Undo.RecordObject(collider, "Capsule changes");
            collider.center = center;
            collider.direction = (int)capsuleDirection;
            collider.radius = radius;
            collider.height = height+radius*2f;
            
            return collider;
        }

        public RagdollCapsuleCollider(Animator animator, string name, Transform parent, Vector3 pointA, Vector3 pointB, float radius, Transform connectedBody) : base(name) {
            Vector3 forward = parent.InverseTransformPoint(pointB) - parent.InverseTransformPoint(pointA);
            Vector3 up = Vector3.up;
            Vector3.OrthoNormalize(ref forward, ref up);
            localTransform = Matrix4x4.Rotate(Quaternion.LookRotation(up, forward));
            
            Matrix4x4 localToWorld = parent.localToWorldMatrix * localTransform;
            Matrix4x4 worldToLocal = Matrix4x4.Inverse(localToWorld);
            Vector3 localPointA = worldToLocal.MultiplyPoint(pointA);
            Vector3 localPointB = worldToLocal.MultiplyPoint(pointB);
            center = (localPointA + localPointB) * 0.5f;
            this.radius = radius;
            height = Vector3.Distance(localPointA, localPointB);
            this.parentPath = AnimationUtility.CalculateTransformPath(parent, animator.transform);
            Vector3 localDiff = localPointB - localPointA;
            /*capsuleDirection = CapsuleDirection.YAxis;
            if (Mathf.Abs(localDiff.x) > Mathf.Abs(localDiff.y) && Mathf.Abs(localDiff.x) > Mathf.Abs(localDiff.z)) {
                capsuleDirection = CapsuleDirection.XAxis;
            } else if (Mathf.Abs(localDiff.z) > Mathf.Abs(localDiff.x) &&
                       Mathf.Abs(localDiff.z) > Mathf.Abs(localDiff.y)) {
                capsuleDirection = CapsuleDirection.ZAxis;
            }*/
            this.connectedBodyPath = AnimationUtility.CalculateTransformPath(connectedBody, animator.transform);
        }

        public override void DrawHandles(Animator animator, HumanoidRagdollColliders allColliders) {
            base.DrawHandles(animator, allColliders);
            var parent = GetParentBone(animator);
            Matrix4x4 localToWorld = parent.localToWorldMatrix * localTransform;
            Vector3 direction = Vector3.up;
            Matrix4x4 rotation = Matrix4x4.identity;
            Vector3 localPointA = center + direction * this.height * 0.5f;
            Vector3 localPointB = center - direction * this.height * 0.5f;
            Vector3 localDiff = localPointB - localPointA;
            Vector3 lengthWise = Vector3.up;
            var height = localDiff.magnitude;
            center = Matrix4x4.Inverse(rotation) * ((localPointA + localPointB) * 0.5f);
            using var scope = new Handles.DrawingScope(localToWorld * rotation);
            DrawWireCapsule(center + lengthWise * height * 0.5f, center - lengthWise * height * 0.5f, radius);
        }

        private static void DrawWireCapsule(Vector3 upper, Vector3 lower, float radius) {
            var offsetX = new Vector3(radius, 0f, 0f);
            var offsetZ = new Vector3(0f, 0f, radius);
            Handles.DrawWireArc(upper, Vector3.back, Vector3.left, 180, radius);
            Handles.DrawLine(lower + offsetX, upper + offsetX);
            Handles.DrawLine(lower - offsetX, upper - offsetX);
            Handles.DrawWireArc(lower, Vector3.back, Vector3.left, -180, radius);
            Handles.DrawWireArc(upper, Vector3.left, Vector3.back, -180, radius);
            Handles.DrawLine(lower + offsetZ, upper + offsetZ);
            Handles.DrawLine(lower - offsetZ, upper - offsetZ);
            Handles.DrawWireArc(lower, Vector3.left, Vector3.back, 180, radius);
            Handles.DrawWireDisc(upper, Vector3.up, radius);
            Handles.DrawWireDisc(lower, Vector3.up, radius);
        }
    }

    public class RagdollBoxCollider : RagdollCollider {
        public RagdollBoxCollider(Animator animator, string name, Matrix4x4 localTransform, Transform parent, Vector3 pointA, Vector3 pointB,
            Vector3 connectA, Vector3 connectB, float depth, float depthOffset, Transform connectedBody) : base(name) {
            Matrix4x4 localToWorld = parent.localToWorldMatrix * localTransform;
            var worldToLocal = Matrix4x4.Inverse(localToWorld);
            Vector3 localPointA = worldToLocal.MultiplyPoint(pointA);
            Vector3 localPointB = worldToLocal.MultiplyPoint(pointB);
            Vector3 localConnectPointA = worldToLocal.MultiplyPoint(connectA);
            Vector3 localConnectPointB = worldToLocal.MultiplyPoint(connectB);

            Vector3 localDiffA = localPointB - localPointA;
            Vector3 localDiffB = localConnectPointB - localConnectPointA;

            Vector3 depthAdjust = Vector3.Cross(localDiffA.normalized, localDiffB.normalized) * depth;

            center = (localPointA + localPointB) * 0.5f;
            size = Vector3.zero;
            size += new Vector3(Mathf.Abs(localDiffA.x), Mathf.Abs(localDiffA.y), Mathf.Abs(localDiffA.z));
            size += new Vector3(Mathf.Abs(localDiffB.x), Mathf.Abs(localDiffB.y), Mathf.Abs(localDiffB.z));
            size += new Vector3(Mathf.Abs(depthAdjust.x), Mathf.Abs(depthAdjust.y), Mathf.Abs(depthAdjust.z));
            center -= depthOffset * depthAdjust;
            this.parentPath = AnimationUtility.CalculateTransformPath(parent, animator.transform);
            this.localTransform = localTransform;
            this.connectedBodyPath = AnimationUtility.CalculateTransformPath(connectedBody, animator.transform);
        }
        
        protected override Collider GetTemporaryColliderForCollisionTesting(Animator animator) {
            GameObject newGameObject = new GameObject("temp", typeof(BoxCollider));
            BoxCollider box = newGameObject.GetComponent<BoxCollider>();
            Matrix4x4 localToWorld = (GetParentBone(animator).localToWorldMatrix * localTransform);
            box.transform.position = localToWorld.GetPosition();
            box.transform.rotation = localToWorld.rotation;
            box.transform.localScale = localToWorld.lossyScale;
            box.center = center;
            box.size = size;
            
            return box;
        }

        public Vector3 size;
        public Vector3 center;
        public Vector3 extents => size * 0.5f;
        public override Collider Get(Animator animator) {
            var parent = GetParentBone(animator);
            bool needsChild = localTransform != Matrix4x4.identity;
            bool hasChild = parent.Find(name + "RagdollBoxCollider") != null;
            if (hasChild && needsChild) {
                return parent.Find(name + "RagdollBoxCollider").GetComponent<BoxCollider>();
            }
            return parent.GetComponent<BoxCollider>();
        }
        public override Collider GetOrCreate(Animator animator, float ragdollMassPerCubicMeter) {
            var parent = GetParentBone(animator);
            GameObject childObject = null;
            bool needsChild = localTransform != Matrix4x4.identity;
            bool hasChild = parent.Find(name + "RagdollBoxCollider") != null;
            if (!hasChild && needsChild) {
                childObject = new GameObject(name+"RagdollBoxCollider", typeof(BoxCollider));
                childObject.transform.SetParent(parent);
                Undo.RegisterCreatedObjectUndo(childObject, "Created box collider");
                hasChild = true;
            }

            if (hasChild && !needsChild) {
                Undo.DestroyObjectImmediate(parent.Find(name + "RagdollBoxCollider").gameObject);
            }

            if (needsChild && childObject == null) {
                childObject = parent.Find(name + "RagdollBoxCollider").gameObject;
                Undo.RegisterCompleteObjectUndo(childObject, "Changed box collider");
            }

            if (needsChild && hasChild) {
                Undo.RegisterFullObjectHierarchyUndo(childObject, "Moved box collider");
                childObject.transform.localPosition = localTransform.GetPosition();
                childObject.transform.localRotation = localTransform.rotation;
            }

            var parentRigidbody = parent.GetComponent<Rigidbody>();
            if (parentRigidbody == null) {
                parentRigidbody = parent.gameObject.AddComponent<Rigidbody>();
                Undo.RegisterCreatedObjectUndo(parentRigidbody, "Created rigidbody");
            }
            Undo.RecordObject(parentRigidbody, "Adjusted rigidbody");
            Matrix4x4 localToWorld = parent.transform.localToWorldMatrix * localTransform;
            Vector3 worldSize = localToWorld.MultiplyVector(size);
            float volume = Mathf.Abs(worldSize.x) * Mathf.Abs(worldSize.y) * Mathf.Abs(worldSize.z);
            parentRigidbody.mass = volume * ragdollMassPerCubicMeter;

            BoxCollider collider;
            if (needsChild && hasChild) {
                collider = childObject.GetComponent<BoxCollider>();
            } else {
                collider = parent.GetComponent<BoxCollider>();
                if (collider == null) {
                    collider = parent.gameObject.AddComponent<BoxCollider>();
                }
            }

            Undo.RecordObject(collider, "Changed box collider's info");
            collider.center = center;
            collider.size = size;
            return collider;
        }

        public override void DrawHandles(Animator animator, HumanoidRagdollColliders allColliders) {
            base.DrawHandles(animator, allColliders);
            var parent = GetParentBone(animator);
            Matrix4x4 localToWorld = parent.localToWorldMatrix * localTransform;
            Matrix4x4 worldToLocal = Matrix4x4.Inverse(localToWorld);
            using var scope = new Handles.DrawingScope(localToWorld);
            Handles.DrawWireCube(center, size);
        }
    }
    public static HumanoidRagdollColliders GenerateColliders(Animator animator,
        RagdollConfiguration configuration, RagdollScrunchStretchPack cachedScrunchStretchPack) {
        var newGameObject = Object.Instantiate(animator.gameObject);
        animator = newGameObject.GetComponent<Animator>();
        cachedScrunchStretchPack.GetNeutralClip().SampleAnimation(animator.gameObject, 0f);
        HumanoidRagdollColliders colliders = new HumanoidRagdollColliders();
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        var hip = animator.GetBoneTransform(HumanBodyBones.Hips);
        var neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        var chest = animator.GetBoneTransform(HumanBodyBones.Chest);

        // Left arm
        var leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        var leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

        colliders.Add(new RagdollCapsuleCollider(animator, "leftUpperArm", leftUpperArm, leftUpperArm.position,
            leftLowerArm.position, configuration.upperArmRadius, chest));
        colliders.Add(new RagdollCapsuleCollider(animator, "leftLowerArm", leftLowerArm, leftLowerArm.position,
            leftHand.position, configuration.lowerArmRadius, leftUpperArm));
        // Left hand
        Vector3 leftHandForward = (leftHand.position - leftLowerArm.position).normalized;
        Vector3 leftHandRight = (leftLowerArm.position - leftUpperArm.position).normalized;
        Vector3 leftHandUp = Vector3.Cross(leftHandForward, leftHandRight);
        Vector3.OrthoNormalize(ref leftHandForward, ref leftHandUp, ref leftHandRight);
        Matrix4x4 leftHandChild = Matrix4x4.Rotate(Quaternion.LookRotation(
            leftHand.transform.InverseTransformDirection(leftHandForward),
            leftHand.transform.InverseTransformDirection(leftHandUp)));
        colliders.Add(new RagdollBoxCollider(animator, "leftHand", leftHandChild, leftHand, leftHand.position,
            leftHand.position + leftHandForward * configuration.handLength,
            leftHand.position + leftHandRight * configuration.lowerArmRadius * 2f,
            leftHand.position - leftHandRight * configuration.lowerArmRadius * 2f, configuration.lowerArmRadius * 2f,
            0, leftLowerArm));

        // Right arm
        var rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        var rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        colliders.Add(new RagdollCapsuleCollider(animator, "rightUpperArm", rightUpperArm, rightUpperArm.position,
            rightLowerArm.position, configuration.upperArmRadius, chest));
        colliders.Add(new RagdollCapsuleCollider(animator, "rightLowerArm", rightLowerArm, rightLowerArm.position,
            rightHand.position, configuration.lowerArmRadius, rightUpperArm));

        // Right hand
        Vector3 rightHandForward = (rightHand.position - rightLowerArm.position).normalized;
        Vector3 rightHandRight = (rightLowerArm.position - rightUpperArm.position).normalized;
        Vector3 rightHandUp = Vector3.Cross(rightHandForward, rightHandRight);
        Vector3.OrthoNormalize(ref rightHandForward, ref rightHandUp, ref rightHandRight);
        Matrix4x4 rightHandChild = Matrix4x4.Rotate(Quaternion.LookRotation(
            rightHand.transform.InverseTransformDirection(rightHandForward),
            rightHand.transform.InverseTransformDirection(rightHandUp)));
        colliders.Add(new RagdollBoxCollider(animator, "rightHand", rightHandChild, rightHand, rightHand.position,
            rightHand.position + rightHandForward * configuration.handLength,
            rightHand.position + rightHandRight * configuration.lowerArmRadius * 2f,
            rightHand.position - rightHandRight * configuration.lowerArmRadius * 2f, configuration.lowerArmRadius * 2f,
            0, rightLowerArm));

        var leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        var leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        var leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        var rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

        // Left foot
        Vector3 leftFootRight = (rightFoot.position - leftFoot.position).normalized;
        Vector3 leftFootUp = (leftLowerLeg.position - leftFoot.position).normalized;
        Vector3 leftFootForward = Vector3.Cross(leftFootRight, leftFootUp);
        Vector3.OrthoNormalize(ref leftFootForward, ref leftFootUp, ref leftFootRight);
        Matrix4x4 leftFootChild = Matrix4x4.Rotate(Quaternion.Inverse(leftFoot.rotation) *
                                                   Quaternion.LookRotation(leftFootForward, leftFootUp));
        colliders.Add(new RagdollBoxCollider(animator, "leftFoot", leftFootChild, leftFoot,
            leftFoot.position + leftFootUp * configuration.lowerLegRadius * 0.75f,
            leftFoot.position - leftFootUp * configuration.lowerLegRadius * 0.75f,
            leftFoot.position + leftFootRight * configuration.lowerLegRadius,
            leftFoot.position - leftFootRight * configuration.lowerLegRadius, configuration.footLength,
            configuration.footOffset, leftLowerLeg));

        // Left leg
        colliders.Add(new RagdollCapsuleCollider(animator, "leftUpperLeg", leftUpperLeg, leftUpperLeg.position,
            leftLowerLeg.position, configuration.upperLegRadius, hip));
        if (!configuration.digitigradeLegs) {
            colliders.Add(new RagdollCapsuleCollider(animator, "leftLowerLeg", leftLowerLeg, leftLowerLeg.position,
                leftFoot.position, configuration.lowerLegRadius, leftUpperLeg));
        } else {
            float lowerLegLength = Vector3.Distance(leftLowerLeg.position, leftFoot.position);
            Vector3 leftDigitigradeTarget = leftFoot.position -
                                            leftFootForward * configuration.digitigradePushBack * lowerLegLength +
                                            leftFootUp * configuration.digitigradePushUp * lowerLegLength;
            Vector3 leftUpperDigitgradeRight = (rightFoot.position - leftFoot.position).normalized;
            Vector3 leftUpperDigitgradeUp = (leftLowerLeg.position - leftDigitigradeTarget).normalized;
            Vector3 leftUpperDigitgradeForward = Vector3.Cross(leftUpperDigitgradeRight, leftUpperDigitgradeUp);
            Vector3.OrthoNormalize(ref leftUpperDigitgradeForward, ref leftUpperDigitgradeUp,
                ref leftUpperDigitgradeRight);
            colliders.Add(new RagdollCapsuleCollider(animator, "leftLowerLeg", leftLowerLeg,
                leftLowerLeg.position, leftDigitigradeTarget, configuration.lowerLegRadius, leftUpperLeg));
            Vector3 leftLowerDigitgradeRight = (rightFoot.position - leftFoot.position).normalized;
            Vector3 leftLowerDigitgradeUp = (leftDigitigradeTarget - leftFoot.position).normalized;
            Vector3 leftLowerDigitgradeForward = Vector3.Cross(leftLowerDigitgradeRight, leftLowerDigitgradeUp);
            Vector3.OrthoNormalize(ref leftLowerDigitgradeForward, ref leftLowerDigitgradeUp,
                ref leftLowerDigitgradeRight);
            colliders.Add(new RagdollCapsuleCollider(animator, "leftLowerLegDigitigrade", leftLowerLeg,
                leftDigitigradeTarget, leftFoot.position, configuration.lowerLegRadius, leftUpperLeg));
        }

        var rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        var rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);

        // Right foot
        Vector3 rightFootRight = (rightFoot.position - leftFoot.position).normalized;
        Vector3 rightFootUp = (rightLowerLeg.position - rightFoot.position).normalized;
        Vector3 rightFootForward = Vector3.Cross(rightFootRight, rightFootUp);
        Vector3.OrthoNormalize(ref rightFootForward, ref rightFootUp, ref rightFootRight);
        Matrix4x4 rightFootChild = Matrix4x4.Rotate(Quaternion.Inverse(rightFoot.rotation) *
                                                    Quaternion.LookRotation(rightFootForward, rightFootUp));
        colliders.Add(new RagdollBoxCollider(animator, "rightFoot", rightFootChild, rightFoot,
            rightFoot.position + rightFootUp * configuration.lowerLegRadius * 0.75f,
            rightFoot.position - rightFootUp * configuration.lowerLegRadius * 0.75f,
            rightFoot.position + rightFootRight * configuration.lowerLegRadius,
            rightFoot.position - rightFootRight * configuration.lowerLegRadius, configuration.footLength,
            configuration.footOffset, rightLowerLeg));

        // Right leg
        colliders.Add(new RagdollCapsuleCollider(animator, "rightUpperLeg", rightUpperLeg, rightUpperLeg.position,
            rightLowerLeg.position, configuration.upperLegRadius, hip));
        if (!configuration.digitigradeLegs) {
            colliders.Add(new RagdollCapsuleCollider(animator, "rightLowerLeg", rightLowerLeg,
                rightLowerLeg.position, rightFoot.position, configuration.lowerLegRadius, rightUpperLeg));
        } else {
            float lowerLegLength = Vector3.Distance(rightLowerLeg.position, rightFoot.position);
            Vector3 rightDigitigradeTarget = rightFoot.position -
                                             rightFootForward * configuration.digitigradePushBack * lowerLegLength +
                                             rightFootUp * configuration.digitigradePushUp * lowerLegLength;
            Vector3 rightUpperDigitgradeRight = (rightFoot.position - leftFoot.position).normalized;
            Vector3 rightUpperDigitgradeUp = (rightLowerLeg.position - rightDigitigradeTarget).normalized;
            Vector3 rightUpperDigitgradeForward = Vector3.Cross(rightUpperDigitgradeRight, rightUpperDigitgradeUp);
            Vector3.OrthoNormalize(ref rightUpperDigitgradeForward, ref rightUpperDigitgradeUp,
                ref rightUpperDigitgradeRight);
            colliders.Add(new RagdollCapsuleCollider(animator, "rightLowerLeg", rightLowerLeg,
                rightLowerLeg.position, rightDigitigradeTarget, configuration.lowerLegRadius, rightUpperLeg));
            Vector3 rightLowerDigitgradeRight = (rightFoot.position - leftFoot.position).normalized;
            Vector3 rightLowerDigitgradeUp = (rightDigitigradeTarget - rightFoot.position).normalized;
            Vector3 rightLowerDigitgradeForward = Vector3.Cross(rightLowerDigitgradeRight, rightLowerDigitgradeUp);
            Vector3.OrthoNormalize(ref rightLowerDigitgradeForward, ref rightLowerDigitgradeUp,
                ref rightLowerDigitgradeRight);
            colliders.Add(new RagdollCapsuleCollider(animator, "rightLowerLegDigitigrade", rightLowerLeg,
                rightDigitigradeTarget, rightFoot.position, configuration.lowerLegRadius, rightUpperLeg));
        }

        // Chest
        var spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        Vector3 chestRight = (rightHand.position - leftHand.position).normalized;
        Vector3 chestUp = (neck.position - chest.position).normalized;
        Vector3.OrthoNormalize(ref chestRight, ref chestUp);
        Vector3 chestForward = Vector3.Cross(chestRight, chestUp);
        Matrix4x4 chestChild =
            Matrix4x4.Rotate(Quaternion.Inverse(chest.rotation) * Quaternion.LookRotation(chestForward, chestUp));
        colliders.Add(new RagdollBoxCollider(animator, "chest", chestChild, chest, chest.position,
            (leftUpperArm.position + rightUpperArm.position) * 0.5f + chestUp * configuration.upperArmRadius * 0.5f,
            leftUpperArm.position, rightUpperArm.position, configuration.chestDepth, configuration.chestOffset, spine));

        Vector3 hipVector = (rightUpperLeg.position - leftUpperLeg.position).normalized;
        Vector3 leftUpperLegAdjust = leftUpperLeg.position - hipVector * configuration.upperLegRadius * 0.25f;
        Vector3 rightUpperLegAdjust = rightUpperLeg.position + hipVector * configuration.upperLegRadius * 0.25f;

        // Spine
        Vector3 spineRight = (rightHand.position - leftHand.position).normalized;
        Vector3 spineUp = (chest.position - spine.position).normalized;
        Vector3.OrthoNormalize(ref spineRight, ref spineUp);
        Vector3 spineForward = Vector3.Cross(spineRight, spineUp);
        Matrix4x4 spineChild =
            Matrix4x4.Rotate(Quaternion.Inverse(spine.rotation) * Quaternion.LookRotation(spineForward, spineUp));
        colliders.Add(new RagdollBoxCollider(animator, "spine", spineChild, spine, spine.position, chest.position,
            (leftUpperLegAdjust + leftUpperArm.position) * 0.5f, (rightUpperLegAdjust + rightUpperArm.position) * 0.5f,
            (configuration.hipDepth + configuration.chestDepth) * 0.5f,
            (configuration.hipOffset + configuration.chestOffset) * 0.5f, hip));

        // Hip
        Vector3 legCenter = (leftUpperLeg.position + rightUpperLeg.position) * 0.5f;
        Vector3 fakeHipPosition = (hip.position + legCenter) * 0.5f;

        Vector3 hipRight = (rightHand.position - leftHand.position).normalized;
        Vector3 hipUp = (spine.position - fakeHipPosition).normalized;
        Vector3.OrthoNormalize(ref hipRight, ref hipUp);
        Vector3 hipForward = Vector3.Cross(hipRight, hipUp);
        Matrix4x4 hipChild =
            Matrix4x4.Rotate(Quaternion.Inverse(hip.rotation) * Quaternion.LookRotation(hipForward, hipUp));
        colliders.Add(new RagdollBoxCollider(animator, "hip", hipChild, hip, fakeHipPosition, spine.position,
            leftUpperLegAdjust, rightUpperLegAdjust, configuration.hipDepth, configuration.hipOffset, animator.transform));

        // Neck
        colliders.Add(new RagdollCapsuleCollider(animator, "neck", neck, neck.position, head.position,
            configuration.upperArmRadius, chest));

        // Head
        Vector3 headRight = (rightHand.position - leftHand.position).normalized;
        Vector3 headUp = (head.position - neck.position).normalized;
        Vector3 headForward = Vector3.Cross(headRight, headUp);
        Vector3.OrthoNormalize(ref headForward, ref headUp, ref headRight);

        Matrix4x4 headChild = Matrix4x4.Rotate(Quaternion.Inverse(head.rotation) * Quaternion.LookRotation(headForward, headUp));

        Vector3 headCenter = head.position + headUp * configuration.headRadius;
        colliders.Add(new RagdollCapsuleCollider(animator, "head", neck, headCenter-headForward*configuration.muzzleLength*(1f-configuration.muzzleOffset), headCenter+headForward*configuration.muzzleLength*configuration.muzzleOffset, configuration.headRadius, chest));

        // Tail
        if (!string.IsNullOrEmpty(configuration.tailPath)) {
            const int maxDepth = 16;
            Transform start = animator.transform.Find(configuration.tailPath);
            int depth = 0;
            Transform end = start.GetChild(0);
            while (depth < maxDepth) {
                depth++;
                if (end.childCount == 0 || end.GetChild(0).name.Contains("Collider")) {
                    break;
                }
                end = end.GetChild(0);
            }

            int currentDepth = 0;
            end = start.GetChild(0);
            while (currentDepth <= depth) {
                float tailRadiusSample = configuration.tailRadiusCurve.Evaluate((float)currentDepth / (float)depth) *
                                         configuration.tailRadiusMultiplier;
                colliders.Add(new RagdollCapsuleCollider(animator, $"tail{currentDepth}", start.transform, start.position,
                    end.position, tailRadiusSample, start.parent));
                currentDepth++;
                if (end.childCount == 0 || end.GetChild(0).name.Contains("Collider")) {
                    break;
                }

                start = end;
                end = end.GetChild(0);
            }

            float lastSample = configuration.tailRadiusCurve.Evaluate(1f) * configuration.tailRadiusMultiplier;
            Vector3 forward = end.position - start.position;
            colliders.Add(new RagdollCapsuleCollider(animator, "tailEnd", end.transform, end.position,
                end.position + forward, lastSample, start));
        }

        Object.DestroyImmediate(newGameObject);
        return colliders;
    }

    public static void PreviewColliders(Animator animator, HumanoidRagdollColliders colliders) {
        foreach (var collider in colliders) {
            collider.DrawHandles(animator, colliders);
        }
    }

    public static void MakeCollidersReal(Animator animator, RagdollConfiguration configuration, HumanoidRagdollColliders colliders) {
        foreach (var collider in colliders) {
            var ignoreColliders = collider.GetParentBone(animator).GetComponent<IgnoreCollisions>();
            if (ignoreColliders != null) {
                Undo.DestroyObjectImmediate(ignoreColliders);
            }
            collider.GetOrCreate(animator, configuration.ragdollMassPerCubicMeter);
        }

        foreach (var collider in colliders) {
            List<RagdollCollider> otherColliders = new List<RagdollCollider>(collider.GetOverlappingSeconds(animator, colliders));
            if (otherColliders.Count <= 0) continue;
            var ignoreColliders = collider.GetParentBone(animator).GetComponent<IgnoreCollisions>();
            if (ignoreColliders == null) {
                ignoreColliders = collider.GetParentBone(animator).gameObject.AddComponent<IgnoreCollisions>();
            }

            var currentCollider = collider.GetOrCreate(animator, configuration.ragdollMassPerCubicMeter);
            if (!ignoreColliders.groupA.Contains(currentCollider)) {
                ignoreColliders.groupA.Add(currentCollider);
            }

            foreach (var other in otherColliders) {
                var otherCollider = other.GetOrCreate(animator, configuration.ragdollMassPerCubicMeter);
                if (!ignoreColliders.groupB.Contains(otherCollider)) {
                    ignoreColliders.groupB.Add(otherCollider);
                }
            }
        }
    }
}
#endif