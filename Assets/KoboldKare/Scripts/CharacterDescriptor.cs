using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Threading.Tasks;
using JigglePhysics;
using NetStack.Serialization;
using Photon.Pun;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Vilar.IK;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CharacterDescriptor))]
public class CharacterDescriptorEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var characterDescriptor = (CharacterDescriptor)target;
        if (characterDescriptor.InitializeIfNeeded(false)) {
            EditorUtility.SetDirty(characterDescriptor);
        }

        if (GUILayout.Button("Reset default assets")) {
            characterDescriptor.InitializeIfNeeded(true);
            EditorUtility.SetDirty(characterDescriptor);
        }

        if (characterDescriptor.GetDisplayAnimator() != null && GUILayout.Button("Create Ragdoll")) {
            ((CharacterDescriptor)target).CreateBasicRagdoll();
        }
    }
}
#endif

[RequireComponent(typeof(Ragdoller), typeof(Kobold))]
public class CharacterDescriptor : MonoBehaviour {
    private bool hideGizmos = true;
    [SerializeField,HideInInspector] private bool initialized = false;
    [Header("Main settings")]
    [SerializeField] private Animator displayAnimator;

    [SerializeField] private Vector3 colliderOffset;
    [SerializeField] private float colliderHeight = 1.2f;
    [SerializeField] private float colliderRadius = 0.2f;
    
    [SerializeField] private List<SkinnedMeshRenderer> bodyRenderers;
    
    public List<SkinnedMeshRenderer> GetBodyRenderers() => bodyRenderers;
    
    [Tooltip("How high off the ground the character collider floats.")]
    [SerializeField] private float stepHeight = 1.2f;
    
    [Header("Special Settings")]
    [SerializeField] private List<Equipment> equipOnSpawn = new List<Equipment>();

    public List<string> GetEquipOnSpawn() {
        List<string> equipNames = new List<string>();
        foreach (var equipment in equipOnSpawn) {
            if (equipment == null) {
                continue;
            }
            equipNames.Add(equipment.name);
        }

        return equipNames;
    }
    
    [SerializeField] private AnimationCurve antiPopCurveIK;
    [SerializeField] private AnimationClip tposeIK;
    
    public float GetStepHeight() => stepHeight;
    public Vector3 GetColliderOffset() => colliderOffset;
    public float GetColliderHeight() => colliderHeight;
    public float GetColliderRadius() => colliderRadius;
    
    public AnimationClip GetTPoseIK() {
        return tposeIK;
    }

    public AnimationCurve GetAntiPopCurveIK() {
        return antiPopCurveIK;
    }
    
    public Animator GetDisplayAnimator() {
        return displayAnimator;
    }

#if UNITY_EDITOR
    public bool InitializeIfNeeded(bool force) {
        if (force == false && initialized) return false;
        var serializedObject = new SerializedObject(this);
        
        var popCurve = new AnimationCurve();
        popCurve.AddKey(new Keyframe { time = 0f, value = 0f, outTangent = 1.3f });
        popCurve.AddKey(new Keyframe { time = 1.1f, value = 1f, inTangent = 0.1f });
        serializedObject.FindProperty("antiPopCurveIK").animationCurveValue = popCurve;
        var TPoseAvatar = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath("46bd2d6ffa5c8c14f850b597913018ee"));
        foreach(var asset in TPoseAvatar) {
            if (asset is not AnimationClip clip || !clip.name.Contains("T-Pose")) continue;
            serializedObject.FindProperty("tposeIK").objectReferenceValue = clip;
            break;
        }

        var genericBounceCurve = AssetDatabase.LoadAssetAtPath<InflatableCurve>(AssetDatabase.GUIDToAssetPath("e18312d1b399ef44cbae03acd0a32afb"));
        var bellyBounceCurve = AssetDatabase.LoadAssetAtPath<InflatableCurve>(AssetDatabase.GUIDToAssetPath("8bb8ec1eabdcb7043a4605858f604a8a"));
        var kobold = GetComponent<Kobold>();
        var koboldSerializedObject = new SerializedObject(kobold);
        koboldSerializedObject.FindProperty("bellyInflater").FindPropertyRelative("bounce").objectReferenceValue = bellyBounceCurve;
        koboldSerializedObject.FindProperty("fatnessInflater").FindPropertyRelative("bounce").objectReferenceValue = genericBounceCurve;
        koboldSerializedObject.FindProperty("sizeInflater").FindPropertyRelative("bounce").objectReferenceValue = genericBounceCurve;
        koboldSerializedObject.FindProperty("boobsInflater").FindPropertyRelative("bounce").objectReferenceValue = genericBounceCurve;
        var displayAnimatorProp = serializedObject.FindProperty("displayAnimator");
        if (displayAnimatorProp.objectReferenceValue == null) {
            displayAnimatorProp.objectReferenceValue = GetComponentInChildren<Animator>();
        }

        if (displayAnimatorProp.objectReferenceValue != null) {
            displayAnimator = displayAnimatorProp.objectReferenceValue as Animator;
            var attachPointArray = koboldSerializedObject.FindProperty("attachPoints");
            CreateOrSetAttachPoint(Equipment.AttachPoint.Chest, displayAnimator.GetBoneTransform(HumanBodyBones.Chest),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.Head, displayAnimator.GetBoneTransform(HumanBodyBones.Head),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.Neck, displayAnimator.GetBoneTransform(HumanBodyBones.Neck),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.LeftCalf,
                displayAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.RightCalf,
                displayAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.LeftForearm,
                displayAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.RightForearm,
                displayAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.LeftHand,
                displayAnimator.GetBoneTransform(HumanBodyBones.LeftHand),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.RightHand,
                displayAnimator.GetBoneTransform(HumanBodyBones.RightHand),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.LeftArm,
                displayAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.RightArm,
                displayAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.LeftLeg,
                displayAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.RightLeg,
                displayAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.LeftFeet,
                displayAnimator.GetBoneTransform(HumanBodyBones.LeftFoot),
                attachPointArray);
            CreateOrSetAttachPoint(Equipment.AttachPoint.RightFeet,
                displayAnimator.GetBoneTransform(HumanBodyBones.RightFoot),
                attachPointArray);
            koboldSerializedObject.FindProperty("hip").objectReferenceValue = displayAnimator.GetBoneTransform(HumanBodyBones.Hips);
        }

        koboldSerializedObject.FindProperty("heartPrefab").FindPropertyRelative("gameObject").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("b47e824ef9dd0654bae5ca33a2d5dd4b"));
        koboldSerializedObject.FindProperty("heartHitMask").intValue = 1 << LayerMask.NameToLayer("UsablePickups");
        koboldSerializedObject.FindProperty("tummyGrumbles").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioPack>(AssetDatabase.GUIDToAssetPath("67a1644657f256b47ab2a61a75c069d6")); 
        koboldSerializedObject.FindProperty("garglePack").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioPack>(AssetDatabase.GUIDToAssetPath("2098de8eac6d5e0419986616fa2a8f15")); 
        koboldSerializedObject.FindProperty("milkLactator").FindPropertyRelative("milkSplatMaterial").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath("3821f9133468bfa449f3dbee8d5a1aff"));
        
        if (displayAnimator != null && displayAnimator.runtimeAnimatorController == null) {
            var defaultAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(UnityEditor.AssetDatabase.GUIDToAssetPath("01936098084665e4bb7c834e8c46c5cc"));
            displayAnimator.runtimeAnimatorController = defaultAnimatorController;
            displayAnimator.applyRootMotion = false;
        }

        gameObject.layer = LayerMask.NameToLayer("Player");
        serializedObject.FindProperty("initialized").boolValue = true;
        serializedObject.ApplyModifiedProperties();
        koboldSerializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(gameObject);
        return true;
    }

    private void CreateOrSetAttachPoint(Equipment.AttachPoint pointReference, Transform targetTransform, SerializedProperty prop) {
        if (targetTransform == null) {
            return;
        }

        for (int i = 0; i < prop.arraySize; i++) {
            var targetProp = prop.GetArrayElementAtIndex(i);
            if (targetProp.FindPropertyRelative("attachPoint").intValue != (int)pointReference) continue;
            targetProp.FindPropertyRelative("targetTransform").objectReferenceValue = targetTransform;
            return;
        }

        prop.InsertArrayElementAtIndex(0);
        var newProp = prop.GetArrayElementAtIndex(0);
        newProp.FindPropertyRelative("attachPoint").intValue = (int)pointReference;
        newProp.FindPropertyRelative("targetTransform").objectReferenceValue = targetTransform;
    }

    private void OnDrawGizmosSelected() {
        if (hideGizmos) {
            return;
        }
        DrawWireCapsule(transform.localToWorldMatrix, colliderOffset+Vector3.up * (colliderHeight-colliderRadius*2f) * 0.5f,
            colliderOffset+Vector3.down * (colliderHeight-colliderRadius*2f) * 0.5f, colliderRadius);
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

    public void CreateBasicRagdoll() {
        hideGizmos = true;
        RagdollCreator.CreateRagdollWizard(displayAnimator).exited += OnCreateBasicRagdoll;
    }
    
    private void OnCreateBasicRagdoll(bool created, Animator animator, RagdollConstraints.HumanoidConstraints constraints, RagdollColliders.HumanoidRagdollColliders colliders) {
        hideGizmos = false;
        if (!created) return;
        var serializedRagdoller = new SerializedObject(GetComponent<Ragdoller>());
        var ragdollBodiesProp = serializedRagdoller.FindProperty("ragdollBodies");
        ragdollBodiesProp.ClearArray();
        foreach (var coll in colliders) {
            var realCollider = coll.Get(animator);
            realCollider.material = UnityEditor.AssetDatabase.LoadAssetAtPath<PhysicMaterial>(UnityEditor.AssetDatabase.GUIDToAssetPath("aed15ac3b782c8c4a8403ba6c6039f0e"));
            var ragdollRigidbody = realCollider.GetComponentInParent<Rigidbody>();
            bool find = false;
            for (int i = 0; i < ragdollBodiesProp.arraySize; i++) {
                if (ragdollBodiesProp.GetArrayElementAtIndex(i).objectReferenceValue != ragdollRigidbody) continue;
                find = true;
                break;
            }

            realCollider.gameObject.layer = LayerMask.NameToLayer("Hitbox");
            if (find) continue;
            ragdollBodiesProp.InsertArrayElementAtIndex(0);
            ragdollBodiesProp.GetArrayElementAtIndex(0).objectReferenceValue = ragdollRigidbody;
        }

        var hipBody = animator.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();
        serializedRagdoller.FindProperty("hipBody").objectReferenceValue = hipBody;
        serializedRagdoller.ApplyModifiedProperties();
    }

#endif
}
