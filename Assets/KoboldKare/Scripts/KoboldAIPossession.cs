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
    private Transform headTransform;

    private Ragdoller ragdoller;

    private Vector3 lerpDir;

    private Rigidbody body;
    private LayerMask lookAtMask;

    private void Awake() {
        ragdoller = GetComponentInParent<Ragdoller>();
        lerpDir = Vector3.forward;
        waitForSeconds = new WaitForSeconds(2f);
        characterControllerAnimator = GetComponentInParent<CharacterControllerAnimator>();
        body = GetComponentInParent<Rigidbody>();
    }

    void OnEnable() {
        lookAtMask = GameManager.instance.usableHitMask | LayerMask.GetMask("Player");
        StartCoroutine(Think());
    }

    private void LateUpdate() {
        if (!photonView.IsMine) {
            return;
        }
        
        if (ragdoller != null && ragdoller.ragdolled) {
            Quaternion rot = Quaternion.LookRotation(headTransform.forward, Vector3.up);
            var rotEuler = rot.eulerAngles;
            characterControllerAnimator.SetEyeRot(new Vector2(rotEuler.y, -rotEuler.x));
            return;
        }
        
        if (focus == null || headTransform == null) {
            return;
        }


        Vector3 wantedDir = focusing ? Vector3.Lerp((focus.position - headTransform.position).normalized,body.transform.forward,0.6f) : body.transform.forward;
        lerpDir = Vector3.RotateTowards(lerpDir, wantedDir, Time.deltaTime * 30f, 0f);
        Quaternion rotB = Quaternion.LookRotation(lerpDir, Vector3.up);
        var rotEulerB = rotB.eulerAngles;
        characterControllerAnimator.SetEyeRot(new Vector2(rotEulerB.y, -rotEulerB.x));
    }

    IEnumerator Think() {
        yield return new WaitUntil(()=>GetComponentInParent<CharacterDescriptor>().GetDisplayAnimator().GetBoneTransform(HumanBodyBones.Head) != null);
        headTransform = GetComponentInParent<CharacterDescriptor>().GetDisplayAnimator().GetBoneTransform(HumanBodyBones.Head);
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
