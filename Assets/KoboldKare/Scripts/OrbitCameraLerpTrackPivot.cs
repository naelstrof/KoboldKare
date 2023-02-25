using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using UnityScriptableSettings;

public class OrbitCameraLerpTrackPivot : OrbitCameraPivotBase {
    protected Transform targetTransform;
    protected Ragdoller ragdoller;
    private CharacterControllerAnimator characterControllerAnimator;
    protected SettingFloat fov;
    private float lerpTrackSpeed = 0.1f;
    public virtual void Initialize(Animator targetAnimator, HumanBodyBones bone, float lerpTrackSpeed) {
        targetTransform = targetAnimator.GetBoneTransform(bone);
        transform.SetParent(targetAnimator.transform, false);
        ragdoller = GetComponentInParent<Ragdoller>();
        characterControllerAnimator = GetComponentInParent<CharacterControllerAnimator>();
        transform.position = targetTransform.position;
        fov = SettingsManager.GetSetting("CameraFOV") as SettingFloat;
        this.lerpTrackSpeed = lerpTrackSpeed;
    }

    public override Vector3 GetPivotPosition(Quaternion camRotation) {
        return ragdoller.ragdolled || characterControllerAnimator.IsAnimating() ? targetTransform.position : transform.position;
    }

    private void LateUpdate() {
        Vector3 a = transform.localPosition;
        Vector3 b = transform.parent.InverseTransformPoint(targetTransform.position);
        Vector3 correction = Vector3.ProjectOnPlane(b - a, transform.parent.InverseTransformDirection(OrbitCamera.GetPlayerIntendedRotation()*Vector3.right));
        float diff = Vector3.Distance(a, a+correction);
        Vector3 correctionDiff = (b - a) - correction;
        // Just snap left/right movement, to prevent motion sickness
        if (correctionDiff.magnitude > 0.25f) {
            a += correctionDiff;
        }
        transform.localPosition = Vector3.MoveTowards(a, a+correction, (0.1f+diff)*Time.deltaTime*lerpTrackSpeed);
    }

    public override float GetFOV(Quaternion camRotation) {
        return fov.GetValue();
    }
    public void SnapInstant() {
        Vector3 b = transform.parent.InverseTransformPoint(targetTransform.position);
        transform.localPosition = b;
    }
}
