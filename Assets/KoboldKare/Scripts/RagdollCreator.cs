using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class RagdollCreator : ScriptableWizard {
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private Ragdoller targetRagdoller;

    [Header("Head")]
    [SerializeField,Range(0.01f,0.6f)]
    private float headRadius = 0.1f;
    [SerializeField,Range(0.01f,0.6f)]
    private float muzzleLength = 0.1f;
    [SerializeField,Range(0f,1f)]
    private float muzzleOffset = 0.1f;
    
    [Header("Spine")]
    [SerializeField,Range(0.01f,0.6f)]
    private float chestDepth = 0.07f;
    [SerializeField,Range(0f,1f)]
    private float chestOffset = 0.3f;
    
    [SerializeField,Range(0.01f,0.6f)]
    private float hipDepth = 0.07f;
    [SerializeField,Range(0f,1f)]
    private float hipOffset = 0.3f;
    
    [Header("Arms")]
    [SerializeField,Range(0.01f,0.3f)]
    private float upperArmRadius = 0.07f;
    [SerializeField,Range(0.01f,0.3f)]
    private float lowerArmRadius = 0.07f;
    [SerializeField,Range(0.05f,0.5f)]
    private float handLength = 0.25f;
    
    [Header("Legs")]
    [SerializeField,Range(0.01f,0.3f)]
    private float upperLegRadius = 0.1f;
    [SerializeField,Range(0.01f,0.3f)]
    private float lowerLegRadius = 0.07f;
    [SerializeField,Range(0.05f,0.5f)]
    private float footLength = 0.2f;
    [SerializeField,Range(0f,1f)]
    private float footOffset = 0.2f;

    [Header("Digitigrade Legs (Optional)")]
    [SerializeField]
    private bool digitigradeLegs = false;
    [SerializeField,Range(0f,1f)]
    private float digitigradePushBack = 0.25f;
    [SerializeField,Range(0f,1f)]
    private float digitigradePushUp = 0.25f;
    
    
    [Header("Tail (Optional)")]
    [SerializeField] private Transform tailRoot;
    [SerializeField] private AnimationCurve tailRadiusCurve = new(new Keyframe(0f,1f), new Keyframe(1f,0.5f));
    [SerializeField, Range(0.01f,0.5f)] private float tailRadiusMultiplier = 0.1f;
    
    public delegate void ExitAction();

    public event ExitAction exited;

    public static RagdollCreator CreateRagdollWizard(Ragdoller ragdoller, Animator animator) {
        var creator = DisplayWizard<RagdollCreator>("Create ragdoll", "Finish");
        creator.targetRagdoller = ragdoller;
        creator.targetAnimator = animator;
        return creator;
    }
    private void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }
 
    private void OnDisable() {
        exited?.Invoke();
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView view) {
        if (targetRagdoller == null || targetAnimator == null) {
            return;
        }
        //PreviewConstraints(targetAnimator);
        PreviewBasicRagdollColliders(targetAnimator);
    }

    //private void PreviewConstraints(Animator animator) {
    //}

    private void PreviewBasicRagdollColliders(Animator animator) {
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        var hip = animator.GetBoneTransform(HumanBodyBones.Hips);
        var neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        var chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        
        // Left arm
        var leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        var leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        
        DrawRagdollCapsule(Matrix4x4.identity, leftUpperArm.transform.localToWorldMatrix, leftUpperArm.position, leftLowerArm.position, upperArmRadius);
        DrawRagdollCapsule(Matrix4x4.identity, leftLowerArm.transform.localToWorldMatrix, leftLowerArm.position, leftHand.position, lowerArmRadius);
        // Left hand
        Vector3 leftHandForward = (leftHand.position - leftLowerArm.position).normalized;
        Vector3 leftHandUp = (head.position - hip.position).normalized;
        Vector3 leftHandRight = Vector3.Cross(leftHandForward, leftHandUp);
        Vector3.OrthoNormalize(ref leftHandForward, ref leftHandUp, ref leftHandRight);
        Matrix4x4 leftHandChild = Matrix4x4.Rotate(Quaternion.LookRotation(leftHand.transform.InverseTransformDirection(leftHandForward), leftHand.transform.InverseTransformDirection(leftHandUp)));
        DrawRagdollBox(leftHandChild, leftHand.transform.localToWorldMatrix, leftHand.position, leftHand.position+leftHandForward*handLength, leftHand.position+leftHandRight*lowerArmRadius*2f, leftHand.position-leftHandRight*lowerArmRadius*2f, lowerArmRadius*2f, 0);
        
        // Right arm
        var rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        var rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        DrawRagdollCapsule(Matrix4x4.identity, rightUpperArm.transform.localToWorldMatrix, rightUpperArm.position, rightLowerArm.position, upperArmRadius);
        DrawRagdollCapsule(Matrix4x4.identity, rightLowerArm.transform.localToWorldMatrix, rightLowerArm.position, rightHand.position, lowerArmRadius);
        
        // Right hand
        Vector3 rightHandForward = (rightHand.position - rightLowerArm.position).normalized;
        Vector3 rightHandUp = (head.position - hip.position).normalized;
        Vector3 rightHandRight = Vector3.Cross(rightHandForward, rightHandUp);
        Vector3.OrthoNormalize(ref rightHandForward, ref rightHandUp, ref rightHandRight);
        Matrix4x4 rightHandChild = Matrix4x4.Rotate(Quaternion.LookRotation(rightHand.transform.InverseTransformDirection(rightHandForward), rightHand.transform.InverseTransformDirection(rightHandUp)));
        DrawRagdollBox(rightHandChild, rightHand.transform.localToWorldMatrix, rightHand.position, rightHand.position+rightHandForward*handLength, rightHand.position+rightHandRight*lowerArmRadius*2f, rightHand.position-rightHandRight*lowerArmRadius*2f, lowerArmRadius*2f, 0);
        
        var leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        var leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        var leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        var rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

        // Left foot
        Vector3 leftFootRight = (rightFoot.position - leftFoot.position).normalized;
        Vector3 leftFootUp = (leftLowerLeg.position - leftFoot.position).normalized;
        Vector3 leftFootForward = Vector3.Cross(leftFootRight, leftFootUp);
        Vector3.OrthoNormalize(ref leftFootForward, ref leftFootUp, ref leftFootRight);
        Matrix4x4 leftFootChild = Matrix4x4.Rotate(Quaternion.Inverse(leftFoot.rotation)*Quaternion.LookRotation(leftFootForward, leftFootUp));
        DrawRagdollBox(leftFootChild, leftFoot.transform.localToWorldMatrix, leftFoot.position+leftFootUp*lowerLegRadius*0.75f, leftFoot.position-leftFootUp*lowerLegRadius*0.75f, leftFoot.position+leftFootRight*lowerLegRadius, leftFoot.position-leftFootRight*lowerLegRadius, footLength, footOffset);
        
        // Left leg
        DrawRagdollCapsule(Matrix4x4.identity, leftUpperLeg.transform.localToWorldMatrix, leftUpperLeg.position, leftLowerLeg.position, upperLegRadius);
        if (!digitigradeLegs) {
            DrawRagdollCapsule(Matrix4x4.identity, leftLowerLeg.transform.localToWorldMatrix, leftLowerLeg.position,
                leftFoot.position, lowerLegRadius);
        } else {
            float lowerLegLength = Vector3.Distance(leftLowerLeg.position, leftFoot.position);
            Vector3 leftDigitigradeTarget = leftFoot.position - leftFootForward * digitigradePushBack * lowerLegLength + leftFootUp * digitigradePushUp * lowerLegLength;
            Vector3 leftUpperDigitgradeRight = (rightFoot.position - leftFoot.position).normalized;
            Vector3 leftUpperDigitgradeUp = (leftLowerLeg.position - leftDigitigradeTarget).normalized;
            Vector3 leftUpperDigitgradeForward = Vector3.Cross(leftUpperDigitgradeRight, leftUpperDigitgradeUp);
            Vector3.OrthoNormalize(ref leftUpperDigitgradeForward, ref leftUpperDigitgradeUp, ref leftUpperDigitgradeRight);
            Matrix4x4 leftUpperDigitgradeChild = Matrix4x4.Rotate(Quaternion.Inverse(leftLowerLeg.rotation)*Quaternion.LookRotation(leftUpperDigitgradeForward, leftUpperDigitgradeUp));
            DrawRagdollCapsule(leftUpperDigitgradeChild, leftLowerLeg.transform.localToWorldMatrix, leftLowerLeg.position, leftDigitigradeTarget, lowerLegRadius);
            Vector3 leftLowerDigitgradeRight = (rightFoot.position - leftFoot.position).normalized;
            Vector3 leftLowerDigitgradeUp = (leftDigitigradeTarget - leftFoot.position).normalized;
            Vector3 leftLowerDigitgradeForward = Vector3.Cross(leftLowerDigitgradeRight, leftLowerDigitgradeUp);
            Vector3.OrthoNormalize(ref leftLowerDigitgradeForward, ref leftLowerDigitgradeUp, ref leftLowerDigitgradeRight);
            Matrix4x4 leftLowerDigitgradeChild = Matrix4x4.Rotate(Quaternion.Inverse(leftLowerLeg.rotation)*Quaternion.LookRotation(leftLowerDigitgradeForward, leftLowerDigitgradeUp));
            DrawRagdollCapsule(leftLowerDigitgradeChild, leftLowerLeg.transform.localToWorldMatrix, leftDigitigradeTarget, leftFoot.position, lowerLegRadius);
        }
        
        var rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        var rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);

        // Right foot
        Vector3 rightFootRight = (rightFoot.position - leftFoot.position).normalized;
        Vector3 rightFootUp = (rightLowerLeg.position - rightFoot.position).normalized;
        Vector3 rightFootForward = Vector3.Cross(rightFootRight, rightFootUp);
        Vector3.OrthoNormalize(ref rightFootForward, ref rightFootUp, ref rightFootRight);
        Matrix4x4 rightFootChild = Matrix4x4.Rotate(Quaternion.Inverse(rightFoot.rotation)*Quaternion.LookRotation(rightFootForward, rightFootUp));
        DrawRagdollBox(rightFootChild, rightFoot.transform.localToWorldMatrix, rightFoot.position+rightFootUp*lowerLegRadius*0.75f, rightFoot.position-rightFootUp*lowerLegRadius*0.75f, rightFoot.position+rightFootRight*lowerLegRadius, rightFoot.position-rightFootRight*lowerLegRadius, footLength, footOffset);
        
        // Right leg
        DrawRagdollCapsule(Matrix4x4.identity, rightUpperLeg.transform.localToWorldMatrix, rightUpperLeg.position, rightLowerLeg.position, upperLegRadius);
        if (!digitigradeLegs) {
            DrawRagdollCapsule(Matrix4x4.identity, rightLowerLeg.transform.localToWorldMatrix, rightLowerLeg.position,
                rightFoot.position, lowerLegRadius);
        } else {
            float lowerLegLength = Vector3.Distance(rightLowerLeg.position, rightFoot.position);
            Vector3 rightDigitigradeTarget = rightFoot.position - rightFootForward * digitigradePushBack * lowerLegLength + rightFootUp * digitigradePushUp * lowerLegLength;
            Vector3 rightUpperDigitgradeRight = (rightFoot.position - leftFoot.position).normalized;
            Vector3 rightUpperDigitgradeUp = (rightLowerLeg.position - rightDigitigradeTarget).normalized;
            Vector3 rightUpperDigitgradeForward = Vector3.Cross(rightUpperDigitgradeRight, rightUpperDigitgradeUp);
            Vector3.OrthoNormalize(ref rightUpperDigitgradeForward, ref rightUpperDigitgradeUp, ref rightUpperDigitgradeRight);
            Matrix4x4 rightUpperDigitgradeChild = Matrix4x4.Rotate(Quaternion.Inverse(rightLowerLeg.rotation)*Quaternion.LookRotation(rightUpperDigitgradeForward, rightUpperDigitgradeUp));
            DrawRagdollCapsule(rightUpperDigitgradeChild, rightLowerLeg.transform.localToWorldMatrix, rightLowerLeg.position, rightDigitigradeTarget, lowerLegRadius);
            Vector3 rightLowerDigitgradeRight = (rightFoot.position - leftFoot.position).normalized;
            Vector3 rightLowerDigitgradeUp = (rightDigitigradeTarget - rightFoot.position).normalized;
            Vector3 rightLowerDigitgradeForward = Vector3.Cross(rightLowerDigitgradeRight, rightLowerDigitgradeUp);
            Vector3.OrthoNormalize(ref rightLowerDigitgradeForward, ref rightLowerDigitgradeUp, ref rightLowerDigitgradeRight);
            Matrix4x4 rightLowerDigitgradeChild = Matrix4x4.Rotate(Quaternion.Inverse(rightLowerLeg.rotation)*Quaternion.LookRotation(rightLowerDigitgradeForward, rightLowerDigitgradeUp));
            DrawRagdollCapsule(rightLowerDigitgradeChild, rightLowerLeg.transform.localToWorldMatrix, rightDigitigradeTarget, rightFoot.position, lowerLegRadius);
        }
        
        // Chest
        Vector3 chestRight = (rightHand.position - leftHand.position).normalized;
        Vector3 chestUp = (neck.position - chest.position).normalized;
        Vector3.OrthoNormalize(ref chestRight, ref chestUp);
        Vector3 chestForward = Vector3.Cross(chestRight, chestUp);
        Matrix4x4 chestChild = Matrix4x4.Rotate(Quaternion.Inverse(chest.rotation)*Quaternion.LookRotation(chestForward, chestUp));
        DrawRagdollBox(chestChild, chest.transform.localToWorldMatrix, chest.position, (leftUpperArm.position + rightUpperArm.position) *0.5f + chestUp*upperArmRadius*0.5f, leftUpperArm.position, rightUpperArm.position, chestDepth, chestOffset);
        
        Vector3 hipVector = (rightUpperLeg.position - leftUpperLeg.position).normalized;
        Vector3 leftUpperLegAdjust = leftUpperLeg.position - hipVector * upperLegRadius * 0.25f;
        Vector3 rightUpperLegAdjust = rightUpperLeg.position + hipVector * upperLegRadius * 0.25f;
        
        // Spine
        var spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        Vector3 spineRight = (rightHand.position - leftHand.position).normalized;
        Vector3 spineUp = (chest.position - spine.position).normalized;
        Vector3.OrthoNormalize(ref spineRight, ref spineUp);
        Vector3 spineForward = Vector3.Cross(spineRight, spineUp);
        Matrix4x4 spineChild = Matrix4x4.Rotate(Quaternion.Inverse(spine.rotation)*Quaternion.LookRotation(spineForward, spineUp));
        DrawRagdollBox(spineChild, spine.transform.localToWorldMatrix, spine.position, chest.position, (leftUpperLegAdjust+leftUpperArm.position)*0.5f, (rightUpperLegAdjust+rightUpperArm.position)*0.5f, (hipDepth+chestDepth)*0.5f, (hipOffset+chestOffset)*0.5f);
        
        // Hip
        Vector3 legCenter = (leftUpperLeg.position + rightUpperLeg.position)*0.5f;
        Vector3 fakeHipPosition = (hip.position + legCenter) * 0.5f;
        
        Vector3 hipRight = (rightHand.position - leftHand.position).normalized;
        Vector3 hipUp = (spine.position - fakeHipPosition).normalized;
        Vector3.OrthoNormalize(ref hipRight, ref hipUp);
        Vector3 hipForward = Vector3.Cross(hipRight, hipUp);
        Matrix4x4 hipChild = Matrix4x4.Rotate(Quaternion.Inverse(hip.rotation)*Quaternion.LookRotation(hipForward, hipUp));
        DrawRagdollBox(hipChild, hip.transform.localToWorldMatrix,fakeHipPosition, spine.position, leftUpperLegAdjust, rightUpperLegAdjust, hipDepth, hipOffset);
        
        // Neck
        DrawRagdollCapsule(Matrix4x4.identity, neck.transform.localToWorldMatrix, neck.position, head.position, upperArmRadius);
        
        // Head
        Vector3 headRight = (rightHand.position - leftHand.position).normalized;
        Vector3 headUp = (head.position - neck.position).normalized;
        Vector3.OrthoNormalize(ref headRight, ref headUp);
        Vector3 headForward = Vector3.Cross(headRight, headUp);

        Matrix4x4 headChild = Matrix4x4.Rotate(Quaternion.Inverse(head.rotation)*Quaternion.LookRotation(headForward, headUp));
        
        DrawRagdollBox(headChild, head.transform.localToWorldMatrix, head.position, head.position + headUp * headRadius * 2f, head.position - headRight * headRadius, head.position + headRight * headRadius, muzzleLength*2f, muzzleOffset);
        
        // Tail
        if (tailRoot != null) {
            const int maxDepth = 24;
            int depth = 0;
            Transform end = tailRoot.GetChild(0);
            while (depth < maxDepth) {
                depth++;
                if (end.childCount == 0) {
                    break;
                }
                end = end.GetChild(0);
            }
            int currentDepth = 0;
            Transform start = tailRoot;
            end = tailRoot.GetChild(0);
            while (currentDepth <= depth) {
                float tailRadiusSample = tailRadiusCurve.Evaluate((float)currentDepth / (float)depth)*tailRadiusMultiplier;
                DrawRagdollCapsule(Matrix4x4.identity, start.transform.localToWorldMatrix, start.position, end.position, tailRadiusSample);
                currentDepth++;
                if (end.childCount == 0) {
                    break;
                }
                start = end;
                end = end.GetChild(0);
            }
            float lastSample = tailRadiusCurve.Evaluate(1f)*tailRadiusMultiplier;
            Vector3 forward = end.position - start.position;
            DrawRagdollCapsule(Matrix4x4.identity, end.transform.localToWorldMatrix, end.position, end.position+forward, lastSample);
        }
    }

    private void CreateBasicRagdollFor(Ragdoller ragdoller, Animator animator) {
        // Left arm
        var leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        var leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        CreateRagdollCapsule(leftUpperArm.gameObject, leftUpperArm.position, leftLowerArm.position, upperArmRadius);
        CreateRagdollCapsule(leftLowerArm.gameObject, leftLowerArm.position, leftHand.position, upperArmRadius);
    }

    private static CapsuleCollider CreateRagdollCapsule(GameObject target, Vector3 pointA, Vector3 pointB, float radius) {
        if (!target.TryGetComponent(out CapsuleCollider capsuleCollider)) {
            capsuleCollider = target.AddComponent<CapsuleCollider>();
        }
        Vector3 localPointA = target.transform.InverseTransformPoint(pointA);
        Vector3 localPointB = target.transform.InverseTransformPoint(pointB);
        Vector3 localDiff = localPointB - localPointA;
        if (localDiff.x > localDiff.y && localDiff.x > localDiff.z) {
            capsuleCollider.direction = 0;
        } else if (localDiff.y > localDiff.x && localDiff.y > localDiff.z) {
            capsuleCollider.direction = 1;
        } else {
            capsuleCollider.direction = 2;
        }
        capsuleCollider.height = localDiff.magnitude;
        capsuleCollider.center = (localPointA + localPointB) * 0.5f;
        capsuleCollider.radius = radius;
        return capsuleCollider;
    }

    private static void DrawRagdollBox(Matrix4x4 local, Matrix4x4 parent, Vector3 pointA, Vector3 pointB, Vector3 connectA, Vector3 connectB, float depth, float depthOffset) {
        Matrix4x4 inverse = Matrix4x4.Inverse(parent*local);
        Vector3 localPointA = inverse.MultiplyPoint(pointA);
        Vector3 localPointB = inverse.MultiplyPoint(pointB);
        Vector3 localConnectPointA = inverse.MultiplyPoint(connectA);
        Vector3 localConnectPointB = inverse.MultiplyPoint(connectB);
        
        Vector3 localDiffA = localPointB - localPointA;
        Vector3 localDiffB = localConnectPointB - localConnectPointA;

        Vector3 depthAdjust = Vector3.Cross(localDiffA.normalized,localDiffB.normalized)*depth;
        
        Matrix4x4 boxSpace = parent*local;
        var center = (localPointA + localPointB) * 0.5f;
        using var scope = new Handles.DrawingScope(boxSpace);
        Vector3 size = Vector3.zero;
        size += new Vector3(Mathf.Abs(localDiffA.x), Mathf.Abs(localDiffA.y), Mathf.Abs(localDiffA.z));
        size += new Vector3(Mathf.Abs(localDiffB.x), Mathf.Abs(localDiffB.y), Mathf.Abs(localDiffB.z));
        size += new Vector3(Mathf.Abs(depthAdjust.x), Mathf.Abs(depthAdjust.y), Mathf.Abs(depthAdjust.z));
        Handles.DrawWireCube(center-depthAdjust*depthOffset, size);
    }

    private static void DrawRagdollCapsule(Matrix4x4 local, Matrix4x4 parent, Vector3 pointA, Vector3 pointB, float radius) {
        Matrix4x4 inverseTarget = Matrix4x4.Inverse(parent*local);
        Vector3 localPointA = inverseTarget.MultiplyPoint(pointA);
        Vector3 localPointB = inverseTarget.MultiplyPoint(pointB);
        Vector3 localDiff = localPointB - localPointA;
        Vector3 lengthWise = Vector3.up;
        Matrix4x4 rotation = Matrix4x4.identity;
        Matrix4x4 capsuleSpace = parent*local;
        if (Mathf.Abs(localDiff.x) > Mathf.Abs(localDiff.y) && Mathf.Abs(localDiff.x) > Mathf.Abs(localDiff.z)) {
            rotation = Matrix4x4.Rotate(Quaternion.AngleAxis(90f, (local*parent*Vector3.forward).normalized));
        } else if (Mathf.Abs(localDiff.z) > Mathf.Abs(localDiff.x) && Mathf.Abs(localDiff.z) > Mathf.Abs(localDiff.y)) {
            rotation = Matrix4x4.Rotate(Quaternion.AngleAxis(-90f, (local*parent*Vector3.right).normalized));
        }
        
        var height = localDiff.magnitude;
        Vector3 center = Matrix4x4.Inverse(rotation)*((localPointA + localPointB) * 0.5f);
        DrawWireCapsule(capsuleSpace*rotation, center + lengthWise * height * 0.5f, center - lengthWise * height * 0.5f, radius);
    }

    private static void DrawWireCapsule(Matrix4x4 space, Vector3 upper, Vector3 lower, float radius) {
        using var scope = new Handles.DrawingScope(space);
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
#endif