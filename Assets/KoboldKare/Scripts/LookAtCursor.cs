using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LookAtCursor : MonoBehaviour {
    [Range(0f,20f)]
    public float distanceFromCamera = 5f;
    private Animator animator;
    private Vector3 currentPosition = Vector3.zero;
    void Start() {
        animator = GetComponent<Animator>();
    }
    private void OnAnimatorIK(int layerIndex) {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        currentPosition = Vector3.Lerp(currentPosition, Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distanceFromCamera), Camera.MonoOrStereoscopicEye.Mono), Time.deltaTime * 15f);
        animator.SetLookAtWeight(1f, 0.4f, 1f, 1f, 0f);
        animator.SetLookAtPosition(currentPosition);
    }
}
