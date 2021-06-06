using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCursor : MonoBehaviour {
    [Range(0f,20f)]
    public float distanceFromCamera = 5f;
    private Animator animator;
    private Vector3 currentPosition = Vector3.zero;
    void Start() {
        animator = GetComponent<Animator>();
    }
    private void OnAnimatorIK(int layerIndex) {
        currentPosition = Vector3.Lerp(currentPosition, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceFromCamera), Camera.MonoOrStereoscopicEye.Mono), Time.deltaTime * 15f);
        animator.SetLookAtWeight(1f, 0.4f, 1f, 1f, 0f);
        animator.SetLookAtPosition(currentPosition);
    }
}
