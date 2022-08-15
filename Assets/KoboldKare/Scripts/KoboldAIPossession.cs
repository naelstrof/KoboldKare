using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class KoboldAIPossession : MonoBehaviourPun {
    private WaitForSeconds waitForSeconds;
    private Transform focus;
    private bool focusing = false;
    private CharacterControllerAnimator characterControllerAnimator;
    private static readonly Collider[] colliders = new Collider[32];
    [SerializeField]
    private Transform headTransform;

    private Vector3 lerpDir;

    private Rigidbody body;
    private LayerMask lookAtMask;

    void Start() {
        lerpDir = Vector3.forward;
        body = GetComponentInParent<Rigidbody>();
        waitForSeconds = new WaitForSeconds(2f);
        characterControllerAnimator = GetComponentInParent<CharacterControllerAnimator>();
        StartCoroutine(Think());
        lookAtMask = GameManager.instance.usableHitMask | LayerMask.GetMask("Player");
    }

    private void Update() {
        if (!photonView.IsMine) {
            return;
        }
        if (focus == null || headTransform == null) {
            return;
        }
        Vector3 wantedDir = focusing ? Vector3.Lerp((focus.position - headTransform.position).normalized,body.transform.forward,0.6f) : body.transform.forward;
        lerpDir = Vector3.RotateTowards(lerpDir, wantedDir, Time.deltaTime * 30f, 0f);
        characterControllerAnimator.SetEyeDir(lerpDir);
    }

    IEnumerator Think() {
        while (true) {
            yield return waitForSeconds;
            if (!photonView.IsMine) {
                continue;
            }
            if (UnityEngine.Random.Range(0f, 1f) > 0.5f) {
                continue;
            }

            if (focusing) {
                focusing = false;
                continue;
            }

            int hits = Physics.OverlapSphereNonAlloc(transform.position, 2f, colliders, lookAtMask );
            if (hits > 0) {
                for (int i = 0; i < 4; i++) {
                    Collider collider1 = colliders[UnityEngine.Random.Range(0, hits)];
                    if (collider1.transform.IsChildOf(body.transform)) {
                        continue;
                    }
                    focus = collider1.transform;
                    focusing = true;
                }
            }
        }
    }
}
