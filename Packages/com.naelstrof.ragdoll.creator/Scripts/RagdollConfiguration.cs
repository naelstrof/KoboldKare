using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class RagdollConfiguration {
    [Header("Head")] [Range(0.01f, 0.6f)]
    public float headRadius = 0.1f;

    [SerializeField, Range(0.01f, 0.6f)] public float muzzleLength = 0.1f;
    [SerializeField, Range(0f, 1f)] public float muzzleOffset = 0.1f;
    
    [Header("Arms")] [SerializeField, Range(0.01f, 0.3f)]
    public float upperArmRadius = 0.07f;

    [SerializeField, Range(0.01f, 0.3f)] public float lowerArmRadius = 0.07f;
    [SerializeField, Range(0.05f, 0.5f)] public float handLength = 0.25f;

    [Header("Spine")] [SerializeField, Range(0.01f, 0.6f)]
    public float chestDepth = 0.07f;

    [SerializeField, Range(0f, 1f)] public float chestOffset = 0.3f;

    [SerializeField, Range(0.01f, 0.6f)] public float hipDepth = 0.07f;
    [SerializeField, Range(0f, 1f)] public float hipOffset = 0.3f;


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

    [HideInInspector]
    public string tailPath;

    [SerializeField] public AnimationCurve tailRadiusCurve = new(new Keyframe(0f, 1f), new Keyframe(1f, 0.5f));
    [SerializeField, Range(0.01f, 0.5f)] public float tailRadiusMultiplier = 0.1f;
    [SerializeField, Range(2.5f, 60f)] public float tailFlexibility = 25f;

    [Header("Constraints")] [Range(0f, 1f)]
    public float twistFactor = 0.25f;

#if UNITY_EDITOR
    public void Save(Animator animator) {
        if (tailRoot == null) {
            tailPath = "";
            return;
        }
        tailPath = AnimationUtility.CalculateTransformPath(tailRoot, animator.transform);
    }

    public void Load(Animator animator) {
        if (string.IsNullOrEmpty(tailPath)) {
            return;
        }
        tailRoot = animator.transform.Find(tailPath);
    }
#endif
}
