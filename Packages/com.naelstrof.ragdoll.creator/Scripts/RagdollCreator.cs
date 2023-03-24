using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class RagdollCreator : ScriptableWizard {
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private RagdollColliderConfiguration colliderConfiguration;
    
    private bool generatedConstraints;
    private RagdollConstraints.HumanoidConstraints targetConstraints;
    private RagdollColliders.HumanoidRagdollColliders targetColliders;
    private RagdollScrunchStretchPack cachedScrunchStretchPack;

    public delegate void ExitAction();

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
            var path = EditorUtility.SaveFilePanel("Save ragdoll collider configuration.", "", "RagdollColliderConfiguration","asset");
            if (path.Length != 0) {
                Uri uriPath = new Uri(path);
                Uri relativeUri = new Uri(Application.dataPath);
                string relativePath = relativeUri.MakeRelativeUri(uriPath).ToString();
                var colliderConfig = ScriptableObject.CreateInstance<RagdollColliderConfigurationObject>();
                colliderConfiguration.Save(targetAnimator);
                colliderConfig.configuration = colliderConfiguration;
                AssetDatabase.CreateAsset(colliderConfig, relativePath);
            }
        }

        if (GUILayout.Button("Load Configuration...")) {
            var path = EditorUtility.OpenFilePanel("Load ragdoll collider configuration.", "", "asset");
            if (path.Length != 0) {
                Uri uriPath = new Uri(path);
                Uri relativeUri = new Uri(Application.dataPath);
                string relativePath = relativeUri.MakeRelativeUri(uriPath).ToString();
                var colliderConfig = AssetDatabase.LoadAssetAtPath<RagdollColliderConfigurationObject>(relativePath);
                colliderConfiguration = colliderConfig.configuration;
                colliderConfiguration.Load(targetAnimator);
                changed = true;
            }
        }
        GUILayout.EndHorizontal();

        if (changed) {
            colliderConfiguration.Save(targetAnimator);
            targetColliders = RagdollColliders.GenerateColliders(targetAnimator, colliderConfiguration, cachedScrunchStretchPack);
        }

        return changed;
    }

    private void OnWizardCreate() {
        Undo.IncrementCurrentGroup();
        foreach (var collider in targetColliders) {
            collider.GetOrCreate(targetAnimator);
        }
        Undo.SetCurrentGroupName("Created ragdoll");
        Selection.activeGameObject = targetAnimator.gameObject;
    }

    private void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }
 
    private void OnDisable() {
        exited?.Invoke();
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView view) {
        if (targetAnimator == null) {
            return;
        }

        if (!generatedConstraints) {
            targetConstraints = RagdollConstraints.GenerateConstraints(targetAnimator, cachedScrunchStretchPack);
            targetColliders = RagdollColliders.GenerateColliders(targetAnimator, colliderConfiguration, cachedScrunchStretchPack);
            generatedConstraints = true;
        }
        RagdollConstraints.PreviewConstraints(targetAnimator, targetConstraints);
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