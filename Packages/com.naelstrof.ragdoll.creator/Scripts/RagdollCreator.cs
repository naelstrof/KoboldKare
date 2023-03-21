using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class RagdollCreator : ScriptableWizard {
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private RagdollConfiguration configuration;
    
    [Serializable]
    public class RagdollConfiguration {
        [Header("Head")] [Range(0.01f, 0.6f)]
        public float headRadius = 0.1f;

        [SerializeField, Range(0.01f, 0.6f)] public float muzzleLength = 0.1f;
        [SerializeField, Range(0f, 1f)] public float muzzleOffset = 0.1f;

        [Header("Spine")] [SerializeField, Range(0.01f, 0.6f)]
        public float chestDepth = 0.07f;

        [SerializeField, Range(0f, 1f)] public float chestOffset = 0.3f;

        [SerializeField, Range(0.01f, 0.6f)] public float hipDepth = 0.07f;
        [SerializeField, Range(0f, 1f)] public float hipOffset = 0.3f;

        [Header("Arms")] [SerializeField, Range(0.01f, 0.3f)]
        public float upperArmRadius = 0.07f;

        [SerializeField, Range(0.01f, 0.3f)] public float lowerArmRadius = 0.07f;
        [SerializeField, Range(0.05f, 0.5f)] public float handLength = 0.25f;

        [Header("Legs")] [SerializeField, Range(0.01f, 0.3f)]
        public float upperLegRadius = 0.1f;

        [SerializeField, Range(0.01f, 0.3f)] public float lowerLegRadius = 0.07f;
        [SerializeField, Range(0.05f, 0.5f)] public float footLength = 0.2f;
        [SerializeField, Range(0f, 1f)] public float footOffset = 0.2f;

        [Header("Digitigrade Legs (Optional)")] [SerializeField]
        public bool digitigradeLegs = false;

        [SerializeField, Range(0f, 1f)] public float digitigradePushBack = 0.25f;
        [SerializeField, Range(0f, 1f)] public float digitigradePushUp = 0.25f;

        [Header("Tail (Optional)")] [SerializeField]
        public Transform tailRoot;

        [SerializeField] public AnimationCurve tailRadiusCurve = new(new Keyframe(0f, 1f), new Keyframe(1f, 0.5f));
        [SerializeField, Range(0.01f, 0.5f)] public float tailRadiusMultiplier = 0.1f;
    }
    private bool generatedConstraints;
    private RagdollConstraints.HumanoidConstraints targetConstraints;
    private RagdollColliders.HumanoidRagdollColliders targetColliders;

    public delegate void ExitAction();

    public event ExitAction exited;

    public static RagdollCreator CreateRagdollWizard(Animator animator) {
        var creator = DisplayWizard<RagdollCreator>("Create ragdoll", "Finish");
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
        if (targetAnimator == null) {
            return;
        }

        if (!generatedConstraints) {
            targetConstraints = RagdollConstraints.GenerateConstraints(targetAnimator);
            targetColliders = RagdollColliders.GenerateColliders(targetAnimator, configuration);
            generatedConstraints = true;
        }
        RagdollConstraints.PreviewConstraints(targetConstraints);
        RagdollColliders.PreviewColliders(targetColliders);
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