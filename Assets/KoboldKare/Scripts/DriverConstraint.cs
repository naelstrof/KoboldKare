using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriverConstraint : MonoBehaviour {
    public Transform connectedBody;
    public Rigidbody body;
    public float springStrength = 10;
    public Vector3 connectedAnchor = Vector3.zero;
    public Vector3 anchor = Vector3.zero;
    public Vector3 forwardVector = Vector3.forward;
    public Vector3 upVector = Vector3.up;
    //public Vector3 connectedBodyVelocityOverride = Vector3.zero;
    public float softness = 1f;
    public float angleSpringStrength = 0;
    public float angleSpringSoftness = 90;
    public float angleDamping = 1;
    public float dampingStrength = 0.25f;
    public bool applyForceToPoint = false;
    private Vector3 lastPosition;
    private void Start() {
        if (body == null) {
            body = GetComponent<Rigidbody>();
        }
        if (connectedBody) {
            lastPosition = connectedBody.transform.TransformPoint(connectedAnchor);
        }
    }
    public void OnDestroy() {
        if (body != null) {
            body.maxAngularVelocity = Physics.defaultMaxAngularSpeed;
        }
    }
    private void FixedUpdate() {
        if (body == null) {
            return;
        }
        Vector3 p1 = body.transform.TransformPoint(anchor);
        Vector3 p2;
        Vector3 velocityDifference;
        if (connectedBody != null) {
            p2 = connectedBody.transform.TransformPoint(connectedAnchor);
        } else {
            p2 = connectedAnchor;
        }
        Vector3 connectedVelocity = (p2-lastPosition)/Time.deltaTime;
        lastPosition = p2;
        velocityDifference = body.velocity - connectedVelocity;

        Vector3 dir = Vector3.Normalize(p2 - p1);
        float dist = Vector3.Distance(p1, p2)/softness;
        Debug.DrawLine(p1, p2, Color.red);
        Vector3 force = dir * (springStrength * dist);

        if (angleSpringStrength > 0) {
            Quaternion fq = Quaternion.FromToRotation(transform.forward, forwardVector);
            Quaternion uq = Quaternion.FromToRotation(transform.up, upVector);
            Vector3 torque = new Vector3(fq.x+uq.x, fq.y+uq.y, fq.z+uq.z);
            body.maxAngularVelocity = 16f;
            body.angularVelocity = torque * angleSpringStrength;
        }

        body.velocity -= velocityDifference * dampingStrength;
        if (applyForceToPoint) {
            body.AddForceAtPosition(force, p1);
        } else {
            body.AddForce(force);
        }
    }
}
