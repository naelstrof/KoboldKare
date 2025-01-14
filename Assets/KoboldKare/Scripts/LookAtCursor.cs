using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LookAtCursor : MonoBehaviour {
    [Range(0f,20f)]
    public float distanceFromCamera = 1f;
    private CharacterControllerAnimator characterAnimator;
    private Animator animator;
    void Start() {
        characterAnimator = GetComponentInParent<CharacterControllerAnimator>();
        animator = GetComponentInChildren<Animator>();
    }
    private void Update() {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 headPos = animator.GetBoneTransform(HumanBodyBones.Head).position;
        Vector3 lookPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distanceFromCamera),
            Camera.MonoOrStereoscopicEye.Mono);
        Quaternion rot = Quaternion.LookRotation((lookPoint - headPos).normalized, Vector3.up);
        var rotEuler = rot.eulerAngles;
        characterAnimator.SetEyeRot(new Vector2(rotEuler.y, -rotEuler.x));
        characterAnimator.SetFacingDirection((lookPoint - transform.position).normalized);
    }
}
