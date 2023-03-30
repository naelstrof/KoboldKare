using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class RagdollConfiguration {
    [Range(1f, 1000f)]
    [Tooltip("The amount of mass assigned to each rigidbody based on its volume, you should adjust this if your find your ragdolls are too heavy/light.")]
    public float ragdollMassPerCubicMeter = 400f;
    [Header("Head")] [Range(0.01f, 0.6f)]
    [Tooltip("How big around the head is in meters.")]
    public float headRadius = 0.1f;

    [Tooltip("How long the head's muzzle is, adjusts the length of the head box collider.")]
    [SerializeField, Range(0.01f, 0.6f)] public float muzzleLength = 0.1f;
    [Tooltip("How forward/back the head is.")]
    [SerializeField, Range(0f, 1f)] public float muzzleOffset = 0.1f;
    
    [Header("Arms")] [SerializeField, Range(0.01f, 0.3f)]
    [Tooltip("Controls the neck and upper arms radius, in meters.")]
    public float upperArmRadius = 0.07f;

    [Tooltip("Controls the lower arm radius, and hand width, in meters.")]
    [SerializeField, Range(0.01f, 0.3f)] public float lowerArmRadius = 0.07f;
    [Tooltip("How long the hands are.")]
    [SerializeField, Range(0.05f, 0.5f)] public float handLength = 0.25f;

    [Header("Spine")] [SerializeField, Range(0.01f, 0.6f)]
    [Tooltip("How thick the chest is.")]
    public float chestDepth = 0.07f;

    [Tooltip("Controls how much the chest sticks out from the spine.")]
    [SerializeField, Range(0f, 1f)] public float chestOffset = 0.3f;

    [Tooltip("How thick the hip is.")]
    [SerializeField, Range(0.01f, 0.6f)] public float hipDepth = 0.07f;
    [Tooltip("Controls how far the hip sticks out from the spine.")]
    [SerializeField, Range(0f, 1f)] public float hipOffset = 0.3f;


    [Header("Legs")] [SerializeField, Range(0.01f, 0.3f)]
    [Tooltip("How thick the upper legs are, in meters as a radius.")]
    public float upperLegRadius = 0.1f;

    [Tooltip("How thick the lower legs are, and how wide the feet are. In meters.")]
    [SerializeField, Range(0.01f, 0.3f)] public float lowerLegRadius = 0.07f;
    [Tooltip("How long the feet are. In meters.")]
    [SerializeField, Range(0.05f, 0.5f)] public float footLength = 0.2f;
    [Tooltip("How far forward/back the feet are from the ankle.")]
    [SerializeField, Range(0f, 1f)] public float footOffset = 0.2f;

    [Header("Digitigrade Legs (Optional)")] [SerializeField]
    [Tooltip("Controls if the lower leg should be built out of two capsule colliders rather than one.")]
    public bool digitigradeLegs = false;

    [Tooltip("How far back the digitigrade joint is, measured in leg lengths.")]
    [SerializeField, Range(0f, 1f)] public float digitigradePushBack = 0.25f;
    [Tooltip("How far up the digitigrade joint is, measured in leg lengths.")]
    [SerializeField, Range(0f, 1f)] public float digitigradePushUp = 0.25f;

    [Header("Tail (Optional)")] [SerializeField]
    [Tooltip("If specified, it will attempt to generate tail colliders using this bone as the root.")]
    public Transform tailRoot;

    [HideInInspector]
    public string tailPath;

    [Tooltip("The curve of the tail from which the collider radius is sampled.")]
    [SerializeField] public AnimationCurve tailRadiusCurve = new(new Keyframe(0f, 1f), new Keyframe(1f, 0.5f));
    [Tooltip("A multiplier for the tail curve to adjust the ultimate radius.")]
    [SerializeField, Range(0.01f, 0.5f)] public float tailRadiusMultiplier = 0.1f;
    [Tooltip("How flexible each tail joint is, in degrees. Be weary that this is per joint, so tails with many joints will become very flexible.")]
    [SerializeField, Range(2.5f, 60f)] public float tailFlexibility = 25f;

    [Header("Constraints")] [Range(0f, 1f)]
    [Tooltip("Sampled humanoid animation data often is exaggerated, this multiplier helps correct for limbs being too twisty.")]
    public float twistFactor = 0.25f;
    [Range(0f, 1f)]
    [Tooltip("Sampled humanoid animation data often is exaggerated, this multiplier helps correct spines being too bendy.")]
    public float spineBendFactor = 0.5f;

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
