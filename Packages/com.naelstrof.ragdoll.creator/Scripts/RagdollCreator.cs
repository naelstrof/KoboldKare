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

    public static RagdollCreator CreateRagdollWizard(Animator animator) {
        var creator = DisplayWizard<RagdollCreator>("Create ragdoll", "Create");
        creator.cachedScrunchStretchPack = AssetDatabase.LoadAssetAtPath<RagdollScrunchStretchPack>( AssetDatabase.GUIDToAssetPath("f918570129faed5418e218f0599df41a"));
        creator.targetAnimator = animator;
        return creator;
    }

    protected override bool DrawWizardGUI() {
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
        Undo.IncrementCurrentGroup();
        foreach (var collider in targetColliders) {
            collider.GetOrCreate(targetAnimator);
        }
        foreach (var constraint in targetConstraints) {
            constraint.GetOrCreate(targetAnimator, configuration);
        }
        Undo.SetCurrentGroupName("Created ragdoll");
        Selection.activeGameObject = targetAnimator.gameObject;
        created = true;
    }

    private void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }
 
    private void OnDisable() {
        exited?.Invoke(created, targetAnimator, targetConstraints, targetColliders);
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView view) {
        if (targetAnimator == null) {
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