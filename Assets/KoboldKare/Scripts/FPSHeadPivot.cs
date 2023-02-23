using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityScriptableSettings;

public class FPSHeadPivot : OrbitCameraPivotBase {
    public float distance = 0.12f;
    private Transform target;
    private Kobold kobold;
    [SerializeField]
    private SettingInt motionSicknessReducer;
    [SerializeField]
    private SettingFloat fov;
    
    private Vector3 startingPosition;
    private CharacterControllerAnimator characterAnimator;
    bool ragdoll;
    
    void Awake() {
        kobold = GetComponentInParent<Kobold>();
        target = kobold.GetComponent<CharacterDescriptor>().GetDisplayAnimator().GetBoneTransform(HumanBodyBones.Neck);
        startingPosition = transform.localPosition;
        transform.localPosition = transform.parent.InverseTransformPoint(target.position);
        motionSicknessReducer.changed += OnMotionSicknessReducerChanged;
        kobold.GetRagdoller().RagdollEvent += RagdollEvent;
        characterAnimator = GetComponentInParent<CharacterControllerAnimator>();
        characterAnimator.animationStateChanged += OnAnimationStateChanged;
        OnMotionSicknessReducerChanged(motionSicknessReducer.GetValue());
    }
    void OnDestroy() {
        if (kobold !=null) {
            kobold.GetRagdoller().RagdollEvent -= RagdollEvent;
        }
        motionSicknessReducer.changed -= OnMotionSicknessReducerChanged;
        characterAnimator.animationStateChanged -= OnAnimationStateChanged;
    }

    void OnAnimationStateChanged(bool animating) {
        if (animating && !enabled) {
            enabled = true;
        } else if (!animating && motionSicknessReducer.GetValue() == 1) {
            transform.localPosition = startingPosition;
            enabled = false;
        }
    }

    void OnMotionSicknessReducerChanged(int value) {
        enabled = value == 0;
        transform.localPosition = startingPosition;
    }

    void LateUpdate() {
        transform.position -= transform.up*distance;
        Vector3 a = transform.localPosition;
        Vector3 b = transform.parent.InverseTransformPoint(target.position);
        if (ragdoll || characterAnimator.IsAnimating()) {
            transform.localPosition = b;
        } else {
            transform.localPosition = Vector3.MoveTowards(a, b, Time.deltaTime*5f);
        }
        transform.position += transform.up*distance;
    }

    public void RagdollEvent(bool ragdolled) {
        ragdoll = ragdolled;
        transform.localPosition = transform.parent.InverseTransformPoint(target.position);
        if (ragdolled && !enabled) {
            enabled = true;
        } else if (!ragdolled && motionSicknessReducer.GetValue() == 1) {
            transform.localPosition = startingPosition;
            enabled = false;
        }
    }

    public override float GetFOV(Quaternion camRotation) {
        return fov.GetValue();
    }

    public override float GetDistanceFromPivot(Quaternion camRotation) {
        return 0f;
    }
}
