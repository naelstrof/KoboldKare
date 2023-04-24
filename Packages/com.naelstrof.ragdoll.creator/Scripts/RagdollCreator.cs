using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;

public class RagdollCreator : ScriptableWizard {
    [SerializeField] private Animator targetAnimator;
    [FormerlySerializedAs("colliderConfiguration")] [SerializeField] private RagdollConfiguration configuration;
    
    private bool generatedConstraints;
    private RagdollConstraints.HumanoidConstraints targetConstraints;
    private RagdollColliders.HumanoidRagdollColliders targetColliders;
    private RagdollScrunchStretchPack cachedScrunchStretchPack;
    private bool created = false;

    public delegate void ExitAction(bool created, Animator animator, RagdollConstraints.HumanoidConstraints constraints, RagdollColliders.HumanoidRagdollColliders colliders);

    public event ExitAction exited;

    [MenuItem("Tools/Naelstrof/Ragdoll Creator")]
    public static void CreateRagdollStandalone() {
        if (Selection.activeGameObject == null) {
            throw new UnityException("Select an animator before running this tool.");
        }
        var animator = Selection.activeGameObject.GetComponentInChildren<Animator>();
        if (animator == null) {
            throw new UnityException("No animator found on active selected object.");
        }
        CreateRagdollWizard(animator);
    }

    public static RagdollCreator CreateRagdollWizard(Animator animator) {
        var creator = DisplayWizard<RagdollCreator>("Create ragdoll", "Create");
        creator.cachedScrunchStretchPack = AssetDatabase.LoadAssetAtPath<RagdollScrunchStretchPack>( AssetDatabase.GUIDToAssetPath("f918570129faed5418e218f0599df41a"));
        creator.targetAnimator = animator;
        return creator;
    }

    protected override bool DrawWizardGUI() {
        if (Physics.defaultSolverIterations < 15) {
            EditorGUILayout.HelpBox( $"Physics default solver iterations is recommended to be between 15 and 20. It's currently set to {Physics.defaultSolverIterations}.", MessageType.Warning);
        }

        if (Physics.defaultSolverVelocityIterations <= 1) {
            EditorGUILayout.HelpBox( $"Physics default solver velocity iterations is recommended to be above 1. It's currently set to {Physics.defaultSolverVelocityIterations}.", MessageType.Warning);
        }

        if (!IsValid(out string errorMessage)) {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            return true;
        }

        bool changed = base.DrawWizardGUI();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Configuration...")) {
            var path = EditorUtility.SaveFilePanel("Save ragdoll configuration.", "", "RagdollConfiguration","asset");
            if (path.Length != 0) {
                Uri uriPath = new Uri(path);
                Uri relativeUri = new Uri(Application.dataPath);
                string relativePath = relativeUri.MakeRelativeUri(uriPath).ToString();
                var colliderConfig = CreateInstance<RagdollConfigurationObject>();
                configuration.Save(targetAnimator);
                colliderConfig.configuration = configuration;
                AssetDatabase.CreateAsset(colliderConfig, relativePath);
            }
        }

        if (GUILayout.Button("Load Configuration...")) {
            var path = EditorUtility.OpenFilePanel("Load ragdoll configuration.", "", "asset");
            if (path.Length != 0) {
                Uri uriPath = new Uri(path);
                Uri relativeUri = new Uri(Application.dataPath);
                string relativePath = relativeUri.MakeRelativeUri(uriPath).ToString();
                var colliderConfig = AssetDatabase.LoadAssetAtPath<RagdollConfigurationObject>(relativePath);
                configuration = colliderConfig.configuration;
                configuration.Load(targetAnimator);
                changed = true;
            }
        }
        GUILayout.EndHorizontal();

        if (changed) {
            configuration.Save(targetAnimator);
            targetColliders = RagdollColliders.GenerateColliders(targetAnimator, configuration, cachedScrunchStretchPack);
            targetConstraints = RagdollConstraints.GenerateConstraints(targetAnimator, configuration, cachedScrunchStretchPack);
        }

        return changed;
    }

    private void OnWizardCreate() {
        if (!IsValid(out string ignoreMessage)) {
            return;
        }

        Undo.IncrementCurrentGroup();
        RagdollColliders.MakeCollidersReal(targetAnimator, configuration, targetColliders);
        RagdollConstraints.MakeRagdollConstraintsReal(targetAnimator, configuration, targetConstraints);
        Selection.activeGameObject = targetAnimator.gameObject;
        Undo.SetCurrentGroupName("Created ragdoll");
        created = true;
    }

    private void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private bool IsValid(out string errorMessage) {
        if (targetAnimator == null) {
            errorMessage = "animator is null! This should never happen if the wizard is spawned correctly...";
            return false;
        }

        if (!targetAnimator.isHuman) {
            errorMessage = "Target animator is not humanoid! It must be a humanoid animator specified within the rig tab of the import settings.";
            return false;
        }

        List<HumanBodyBones> bonesToCheck = new List<HumanBodyBones>(new [] {
            HumanBodyBones.Hips,
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            HumanBodyBones.Neck,
            HumanBodyBones.Head,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.RightHand,
            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.RightFoot,
        });
        foreach (var bone in bonesToCheck) {
            if (targetAnimator.GetBoneTransform(bone) != null) continue;
            errorMessage = $"{bone} doesn't exist on the avatar, please specify it in the rig configuration.";
            return false;
        }
        errorMessage = "";
        return true;
    }

    private void OnDisable() {
        exited?.Invoke(created, targetAnimator, targetConstraints, targetColliders);
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView view) {
        if (!IsValid(out string errorMessage)) {
            return;
        }

        if (!generatedConstraints) {
            targetColliders = RagdollColliders.GenerateColliders(targetAnimator, configuration, cachedScrunchStretchPack);
            targetConstraints = RagdollConstraints.GenerateConstraints(targetAnimator, configuration, cachedScrunchStretchPack);
            generatedConstraints = true;
        }
        RagdollConstraints.PreviewConstraints(targetAnimator, configuration, targetConstraints);
        RagdollColliders.PreviewColliders(targetAnimator, targetColliders);
    }

    
    private void ForceTPose(Animator animator, Vector3 originalAnimatorPosition) {
        foreach (var skeletonBone in targetAnimator.avatar.humanDescription.skeleton) {
            foreach (HumanBodyBones humanBodyBone in Enum.GetValues(typeof(HumanBodyBones))) {
                if (humanBodyBone == HumanBodyBones.LastBone) {
                    break;
                }
                var bodyBone = targetAnimator.GetBoneTransform(humanBodyBone);
                if (bodyBone == null) {
                    continue;
                }

                if (skeletonBone.name != bodyBone.name) {
                    continue;
                }
                if (humanBodyBone == HumanBodyBones.Hips) {
                    bodyBone.localPosition = skeletonBone.position;
                }
                bodyBone.localRotation = skeletonBone.rotation;
            }
        }
        targetAnimator.transform.position = originalAnimatorPosition;
    }

}
#endif