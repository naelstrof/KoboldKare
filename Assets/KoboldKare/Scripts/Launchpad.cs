using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Launchpad))]
public class LaunchpadEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
    }
    public void OnSceneGUI(){
        Launchpad t = (Launchpad)target;
        Vector3 globalPosition = Handles.PositionHandle(t.transform.TransformPoint(t.localTarget), t.transform.rotation);
        if (Vector3.Distance(t.transform.InverseTransformPoint(globalPosition), t.localTarget) > 0.01f) {
            t.localTarget = t.transform.InverseTransformPoint(globalPosition);
            EditorUtility.SetDirty(target);
        }
    }
}
#endif

public class Launchpad : UsableMachine {
    public Vector3 localTarget;
    public float flightTime = 5f;
    public float playerFlightTime = 4.5f;
    private WaitForFixedUpdate waitForFixedUpdate;

    public override bool CanUse(Kobold k) {
        return false;
    }

    public UnityEvent OnFire;
    [HideInInspector]
    public Vector3 playerGravityMod = new Vector3(0f, -4f, 0f);
    private float fireDelay = 1f;
    private float lastFireTime;

    private void Awake() {
        waitForFixedUpdate = new WaitForFixedUpdate();
    }

    private IEnumerator HighQualityCollision(Rigidbody body) {
        // Already high quality, don't want to mess with it.
        if (body.collisionDetectionMode != CollisionDetectionMode.Discrete) {
            yield break;
        }

        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        while (body.velocity.magnitude > 1f) {
            yield return waitForFixedUpdate;
        }
        body.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    private IEnumerator DisableControllerForTime(KoboldCharacterController controller) {
        controller.enabled = false;
        yield return new WaitForSeconds(0.5f);
        controller.enabled = true;
    }

    public override void SetConstructed(bool isConstructed) {
        base.SetConstructed(isConstructed);
        foreach (Collider coll in GetComponents<Collider>()) {
            coll.enabled = isConstructed;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!constructed) {
            return;
        }

        var rigidbodies = other.GetAllComponents<Rigidbody>();
        if (rigidbodies.Length == 0) {
            return;
        }


        KoboldCharacterController controller = other.GetComponentInParent<KoboldCharacterController>();
        float gravity = Physics.gravity.y;
        float alteredFlightTime = flightTime;
        if (controller != null && !controller.body.isKinematic) {
            gravity = Physics.gravity.y + controller.gravityMod.y*controller.transform.lossyScale.x;
            alteredFlightTime = playerFlightTime;
            StartCoroutine(DisableControllerForTime(controller));
        } else {
            foreach (Rigidbody body in rigidbodies) {
                StartCoroutine(HighQualityCollision(body));
            }
        }

        float initialYVelocity = (transform.TransformVector(localTarget).y - .5f * gravity * (alteredFlightTime * alteredFlightTime)) / alteredFlightTime;
        float xDistance = transform.TransformVector(localTarget).With(y: 0).magnitude;
        float initialXVelocity = xDistance/alteredFlightTime;

        Vector3 xForceDir = transform.TransformVector(localTarget).With(y: 0).normalized;
        Vector3 yForceDir = Vector3.up;
        Vector3 initialVelocity = xForceDir * initialXVelocity + yForceDir * initialYVelocity;
        foreach (Rigidbody r in rigidbodies){
            r.velocity = initialVelocity;
        }
        if (lastFireTime + fireDelay < Time.time) {
            OnFire.Invoke();
            lastFireTime = Time.time;
        }
    }

    private void OnDrawGizmos(){
        float xDistance = transform.TransformVector(localTarget).With(y: 0).magnitude;
        float initialXVelocity = xDistance/flightTime;
        float initialYVelocity = (transform.TransformVector(localTarget).y - .5f * Physics.gravity.y * (flightTime * flightTime))/flightTime;

        // Players have extra g-forces on them
        float initialpXVelocity = xDistance/playerFlightTime;
        float initialpYVelocity = (transform.TransformVector(localTarget).y - .5f * (Physics.gravity.y+playerGravityMod.y) * (playerFlightTime * playerFlightTime))/playerFlightTime;
        Vector3 lastPos = transform.position;
        Vector3 lastpPos = transform.position;
        Vector3 xForceDir = transform.TransformVector(localTarget).With(y: 0).normalized;
        Vector3 yForceDir = Vector3.up;
        Gizmos.color = Color.white;
        for (float t = 0; t < flightTime; t+=0.5f) {
            Vector3 sample = transform.position + xForceDir*initialXVelocity*t + yForceDir * (initialYVelocity*t + .5f * Physics.gravity.y * (t*t));
            Gizmos.DrawLine(lastPos, sample);
            lastPos = sample;
        }
        Gizmos.DrawLine(lastPos, transform.TransformPoint(localTarget));
        Gizmos.color = Color.green;
        for (float t = 0; t < playerFlightTime; t+=0.5f) {
            Vector3 psample = transform.position + xForceDir*initialpXVelocity*t + yForceDir * (initialpYVelocity*t + .5f * (Physics.gravity.y+playerGravityMod.y) * (t*t));
            Gizmos.DrawLine(lastpPos, psample);
            lastpPos = psample;
        }
        Gizmos.DrawLine(lastpPos, transform.TransformPoint(localTarget));
    }
}
