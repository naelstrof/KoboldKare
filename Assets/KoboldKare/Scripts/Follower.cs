using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour {
    public Transform target;
    //public Transform targetTarget;
    public float distance = 0.12f;
    private Kobold internalKobold;
    public Kobold kobold {
        get {
            if (internalKobold == null) {
                internalKobold = GetComponentInParent<Kobold>();
            }
            return internalKobold;
        }
    }
    bool ragdoll = false;
    void Start() {
        kobold.RagdollEvent += RagdollEvent;
        transform.localPosition = transform.parent.InverseTransformPoint(target.position);
    }
    void OnDestroy() {
        if (kobold !=null) {
            kobold.RagdollEvent -= RagdollEvent;
        }
    }
    void LateUpdate() {
        transform.position -= transform.up*distance;
        Vector3 a = transform.localPosition;
        Vector3 b = transform.parent.InverseTransformPoint(target.position);
        if (ragdoll) {
            transform.localPosition = b;
        } else {
            transform.localPosition = Vector3.MoveTowards(a, b, Vector3.Distance(a,b)*Time.deltaTime*5f);
        }
        transform.position += transform.up*distance;
    }

    public void RagdollEvent(bool ragdolled) {
        ragdoll = ragdolled;
        transform.localPosition = transform.parent.InverseTransformPoint(target.position);
    }
}
