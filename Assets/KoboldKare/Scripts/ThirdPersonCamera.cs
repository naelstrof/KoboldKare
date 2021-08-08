using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {
    public Transform firstperson;
    private Vector3 offset;
    public LayerMask hitmask;
    private RaycastHit[] hits = new RaycastHit[3];
    void Start() {
        offset = transform.localPosition;
    }
    void Update() {
        Vector3 dir = (transform.parent.TransformPoint(offset) - firstperson.position).normalized;
        float dist = Vector3.Distance(transform.parent.TransformPoint(offset), firstperson.position);
        Vector3 targetPoint = offset;
        RaycastHit hit;
        if (Physics.Raycast(firstperson.position-dir*0.1f, dir, out hit, dist+0.1f, hitmask, QueryTriggerInteraction.Ignore)) {
            targetPoint = transform.parent.InverseTransformPoint(hit.point)+dir*0.1f;
        }
        float positionLerpTime = 2f;
        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPoint, positionLerpPct);
    }
}
