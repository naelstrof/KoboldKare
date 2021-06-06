using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(RigPoser))]
public class RigPoserEditor : Editor {
    SerializedProperty OriginalAnimations;
    SerializedProperty SaveOutFolder;
    SerializedProperty LeftKneeTarget;
    SerializedProperty LeftFootTarget;
    SerializedProperty RightKneeTarget;
    SerializedProperty RightFootTarget;
    SerializedProperty RightElbowTarget;
    SerializedProperty RightHandTarget;
    SerializedProperty LeftElbowTarget;
    SerializedProperty LeftHandTarget;
    SerializedProperty HeadTarget;
    SerializedProperty HipTarget;
    SerializedProperty FootOffset;
    SerializedProperty BodyOffset;
    void OnEnable() {
        OriginalAnimations = serializedObject.FindProperty("OriginalAnimations");
        SaveOutFolder = serializedObject.FindProperty("SaveOutFolder");
        LeftKneeTarget = serializedObject.FindProperty("LeftKneeTarget");
        LeftFootTarget = serializedObject.FindProperty("LeftFootTarget");
        RightKneeTarget = serializedObject.FindProperty("RightKneeTarget");
        RightFootTarget = serializedObject.FindProperty("RightFootTarget");
        RightElbowTarget = serializedObject.FindProperty("RightElbowTarget");
        RightHandTarget = serializedObject.FindProperty("RightHandTarget");
        LeftElbowTarget = serializedObject.FindProperty("LeftElbowTarget");
        LeftHandTarget = serializedObject.FindProperty("LeftHandTarget");
        HeadTarget = serializedObject.FindProperty("HeadTarget");
        HipTarget = serializedObject.FindProperty("HipTarget");
        FootOffset = serializedObject.FindProperty("FootOffset");
        BodyOffset = serializedObject.FindProperty("BodyOffset");
    }
    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(OriginalAnimations,true);
        EditorGUILayout.PropertyField(SaveOutFolder);
        EditorGUILayout.PropertyField(LeftKneeTarget);
        EditorGUILayout.PropertyField(LeftFootTarget);
        EditorGUILayout.PropertyField(RightKneeTarget);
        EditorGUILayout.PropertyField(RightFootTarget);
        EditorGUILayout.PropertyField(RightElbowTarget);
        EditorGUILayout.PropertyField(RightHandTarget);
        EditorGUILayout.PropertyField(LeftElbowTarget);
        EditorGUILayout.PropertyField(LeftHandTarget);
        EditorGUILayout.PropertyField(HeadTarget);
        EditorGUILayout.PropertyField(HipTarget);
        EditorGUILayout.PropertyField(FootOffset);
        EditorGUILayout.PropertyField(BodyOffset);
        if (GUILayout.Button("Copy Pose")) {
            ((RigPoser)serializedObject.targetObject).Copy();
        }
        if (GUILayout.Button("Convert Animations")) {
            ((RigPoser)serializedObject.targetObject).CopyAnimation();
        }
        if (GUILayout.Button("Auto Find Targets")) {
            ((RigPoser)serializedObject.targetObject).AutoFindTargets();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
public class RigPoser : MonoBehaviour {
    public List<AnimationClip> OriginalAnimations = new List<AnimationClip>();
    public string SaveOutFolder = "Assets/";
    public Transform LeftKneeTarget;
    public Transform LeftFootTarget;
    public Transform RightKneeTarget;
    public Transform RightFootTarget;
    public Transform RightElbowTarget;
    public Transform RightHandTarget;
    public Transform LeftElbowTarget;
    public Transform LeftHandTarget;
    public Transform HeadTarget;
    public Transform HipTarget;
    [Range(-1, 1)]
    public float BodyOffset = 0.9f;
    [Range(0, 1)]
    public float FootOffset = 0.16f;
    private enum IKTarget {
        Hip = 0,
        Head,
        LeftHand,
        LeftElbow,
        RightHand,
        RightElbow,
        LeftFoot,
        LeftKnee,
        RightKnee,
        RightFoot
    }
    private List<List<AnimationCurve>> AnimationCurvePositions = new List<List<AnimationCurve>>();
    private List<List<AnimationCurve>> AnimationCurveRotations = new List<List<AnimationCurve>>();
    private List<Transform> OrganizedTargets = new List<Transform>();

    // All the human properties that get overridden by IK targets
    private string[] PropertyBlacklist = { "LeftHandQ", "RightHandQ", "LeftHandT", "RightHandT",
                                           "Spine", "Chest",
                                           "UpperChest", "Neck",
                                           "Head", "Left Upper Leg",
                                           "Left Lower Leg", "Left Foot",
                                           "Right Upper Leg", "Right Lower Leg",
                                           "Right Foot", "Left Arm",
                                           "Right Arm", "Left Forearm",
                                           "Right Forearm", "Left Hand",
                                           "Right Hand" };
#if UNITY_EDITOR
    private void CopyTransform(Transform a, Transform b, Vector3? offset = null) {
        if ( offset == null ) {
            offset = Vector3.zero;
        }
        a.position = b.position + (Vector3)offset;
        a.rotation = b.rotation;
    }
    private void CopyJoint(Transform a, Transform upper, Transform lower, Transform hand, Vector3? offset = null) {
        if ( offset == null ) {
            offset = Vector3.zero;
        }
        Vector3 elbowDir;
        if ((-lower.up + upper.up).magnitude < 0.1) {
            elbowDir = hand.forward;
        } else {
            elbowDir = Vector3.Normalize(-lower.up + upper.up);
        }
        a.position = lower.position + elbowDir * 0.5f + (Vector3)offset;
    }
    private void InitializeCurves() {
        AnimationCurvePositions.Clear();
        AnimationCurveRotations.Clear();
        foreach (IKTarget t in (IKTarget[])Enum.GetValues(typeof(IKTarget))) {
            AnimationCurvePositions.Add(new List<AnimationCurve>());
            AnimationCurveRotations.Add(new List<AnimationCurve>());
            AnimationCurvePositions[(int)t].Add(new AnimationCurve());
            AnimationCurvePositions[(int)t].Add(new AnimationCurve());
            AnimationCurvePositions[(int)t].Add(new AnimationCurve());
            AnimationCurveRotations[(int)t].Add(new AnimationCurve());
            AnimationCurveRotations[(int)t].Add(new AnimationCurve());
            AnimationCurveRotations[(int)t].Add(new AnimationCurve());
            AnimationCurveRotations[(int)t].Add(new AnimationCurve());
            OrganizedTargets.Add(null);
        }
        OrganizedTargets[(int)IKTarget.Hip] = HipTarget;
        OrganizedTargets[(int)IKTarget.Head] = HeadTarget;
        OrganizedTargets[(int)IKTarget.LeftHand] = LeftHandTarget;
        OrganizedTargets[(int)IKTarget.RightHand] = RightHandTarget;
        OrganizedTargets[(int)IKTarget.LeftElbow] = LeftElbowTarget;
        OrganizedTargets[(int)IKTarget.RightElbow] = RightElbowTarget;
        OrganizedTargets[(int)IKTarget.LeftFoot] = LeftFootTarget;
        OrganizedTargets[(int)IKTarget.RightFoot] = RightFootTarget;
        OrganizedTargets[(int)IKTarget.LeftKnee] = LeftKneeTarget;
        OrganizedTargets[(int)IKTarget.RightKnee] = RightKneeTarget;
    }
    public void AutoFindTargets() {
        LeftKneeTarget = transform.Find("Rig/LeftKneeTarget");
        LeftFootTarget = transform.Find("Rig/LeftFootTarget");
        RightKneeTarget = transform.Find("Rig/RightKneeTarget");
        RightFootTarget = transform.Find("Rig/RightFootTarget");
        RightElbowTarget = transform.Find("Rig/RightElbowTarget");
        RightHandTarget = transform.Find("Rig/RightHandTarget");
        LeftElbowTarget = transform.Find("Rig/LeftElbowTarget");
        LeftHandTarget = transform.Find("Rig/LeftHandTarget");
        HeadTarget = transform.Find("Rig/HeadTarget");
        HipTarget = transform.Find("Rig/HipTarget");
    }
    public void CopyAnimation() {
        foreach (AnimationClip CopyFrom in OriginalAnimations) {
            // First off, we must copy the whole animation, but ignore the root motion.
            AnimationClip copy = new AnimationClip();
            copy.frameRate = CopyFrom.frameRate;
            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(CopyFrom)) {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(CopyFrom, binding);
                // Skip all root motion
                //if (binding.propertyName.StartsWith("RootT") || binding.propertyName.StartsWith("RootQ")) {
                    //continue;
                //}
                copy.SetCurve(binding.path, binding.type, binding.propertyName, curve);
            }
            AnimationClip newClip = new AnimationClip();
            // Initialize some lookup table stuff to simplify code
            InitializeCurves();
            // Copy some curves from the original animation, things like finger grasping and facial animation.
            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(CopyFrom)) {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(CopyFrom, binding);
                // Skip all humanoid animation that gets overridden by IK
                foreach (string s in PropertyBlacklist) {
                    if (binding.propertyName.StartsWith(s)) {
                        continue;
                    }
                }
                // Skip IK target animations, if they exist (they shouldn't, but just in case)
                foreach (IKTarget target in (IKTarget[])Enum.GetValues(typeof(IKTarget))) {
                    if (binding.path == "Rig/" + OrganizedTargets[(int)target].name) {
                        continue;
                    }
                }
                // Copy curves
                newClip.SetCurve(binding.path, binding.type, binding.propertyName, curve);
            }
            // Start sampling the original animation to convert the humanoid animations to normal positions and rotations.
            for (float t = 0; t <= copy.length; t += copy.length / copy.frameRate) {
                // Play the animation back.
                // We sample from copy, which is a copy of the original animation but without the root motion.
                copy.SampleAnimation(gameObject, t);
                // Copy the positions and rotation of the limbs with the IK targets
                Copy();
                // Save the positions and rotations as animation curves
                foreach (IKTarget target in (IKTarget[])Enum.GetValues(typeof(IKTarget))) {
                    Vector3 pos = OrganizedTargets[(int)target].localPosition;
                    Quaternion rot = OrganizedTargets[(int)target].localRotation;
                    AnimationCurvePositions[(int)target][0].AddKey(t, pos.x);
                    AnimationCurvePositions[(int)target][1].AddKey(t, pos.y);
                    AnimationCurvePositions[(int)target][2].AddKey(t, pos.z);
                    AnimationCurveRotations[(int)target][0].AddKey(t, rot.x);
                    AnimationCurveRotations[(int)target][1].AddKey(t, rot.y);
                    AnimationCurveRotations[(int)target][2].AddKey(t, rot.z);
                    AnimationCurveRotations[(int)target][3].AddKey(t, rot.w);
                }
            }
            // Copy the curves into our new clip
            foreach (IKTarget target in (IKTarget[])Enum.GetValues(typeof(IKTarget))) {
                newClip.SetCurve("Rig/" + OrganizedTargets[(int)target].name, typeof(Transform), "m_LocalPosition.x", AnimationCurvePositions[(int)target][0]);
                newClip.SetCurve("Rig/" + OrganizedTargets[(int)target].name, typeof(Transform), "m_LocalPosition.y", AnimationCurvePositions[(int)target][1]);
                newClip.SetCurve("Rig/" + OrganizedTargets[(int)target].name, typeof(Transform), "m_LocalPosition.z", AnimationCurvePositions[(int)target][2]);
                newClip.SetCurve("Rig/" + OrganizedTargets[(int)target].name, typeof(Transform), "m_LocalRotation.x", AnimationCurveRotations[(int)target][0]);
                newClip.SetCurve("Rig/" + OrganizedTargets[(int)target].name, typeof(Transform), "m_LocalRotation.y", AnimationCurveRotations[(int)target][1]);
                newClip.SetCurve("Rig/" + OrganizedTargets[(int)target].name, typeof(Transform), "m_LocalRotation.z", AnimationCurveRotations[(int)target][2]);
                newClip.SetCurve("Rig/" + OrganizedTargets[(int)target].name, typeof(Transform), "m_LocalRotation.w", AnimationCurveRotations[(int)target][3]);
            }
            // Save the clip out
            newClip.frameRate = CopyFrom.frameRate;
            AssetDatabase.CreateAsset(newClip, SaveOutFolder+"/"+CopyFrom.name+"IK.anim");
            AssetDatabase.SaveAssets();
            //Rig.localPosition = Vector3.zero;
            //Rig.localRotation = Quaternion.identity;
            Debug.Log("Saved to "+SaveOutFolder+"/"+CopyFrom.name+"IK.anim :thumbsup:");
        }
    }
    public void Copy() {
        Animator a = GetComponent<Animator>();
        CopyTransform(HeadTarget, a.GetBoneTransform(HumanBodyBones.Head), Vector3.up * BodyOffset);
        CopyTransform(LeftHandTarget, a.GetBoneTransform(HumanBodyBones.LeftHand), Vector3.up * BodyOffset);
        CopyTransform(RightHandTarget, a.GetBoneTransform(HumanBodyBones.RightHand),Vector3.up * BodyOffset);
        CopyTransform(RightFootTarget, a.GetBoneTransform(HumanBodyBones.RightFoot), Vector3.down * FootOffset + Vector3.up * BodyOffset);
        CopyTransform(LeftFootTarget, a.GetBoneTransform(HumanBodyBones.LeftFoot), Vector3.down * FootOffset + Vector3.up * BodyOffset);
        CopyTransform(HipTarget, a.GetBoneTransform(HumanBodyBones.Hips), Vector3.up * BodyOffset);
        CopyJoint(LeftKneeTarget, a.GetBoneTransform(HumanBodyBones.LeftUpperLeg), a.GetBoneTransform(HumanBodyBones.LeftLowerLeg), a.GetBoneTransform(HumanBodyBones.LeftFoot), Vector3.up * BodyOffset);
        CopyJoint(RightKneeTarget, a.GetBoneTransform(HumanBodyBones.RightUpperLeg), a.GetBoneTransform(HumanBodyBones.RightLowerLeg), a.GetBoneTransform(HumanBodyBones.RightFoot), Vector3.up * BodyOffset);
        CopyJoint(LeftElbowTarget, a.GetBoneTransform(HumanBodyBones.LeftUpperArm), a.GetBoneTransform(HumanBodyBones.LeftLowerArm), a.GetBoneTransform(HumanBodyBones.LeftHand), Vector3.up * BodyOffset);
        CopyJoint(RightElbowTarget, a.GetBoneTransform(HumanBodyBones.RightUpperArm), a.GetBoneTransform(HumanBodyBones.RightLowerArm), a.GetBoneTransform(HumanBodyBones.RightHand), Vector3.up * BodyOffset);
    }
#endif
}
