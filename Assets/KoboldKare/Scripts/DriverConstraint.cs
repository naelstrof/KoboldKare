using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriverConstraint : MonoBehaviour {
    public Rigidbody body;
    public float springStrength = 10;
    public Vector3 anchor = Vector3.zero;
    public Vector3 forwardVector = Vector3.forward;
    public Vector3 upVector = Vector3.up;
    private float softness = 1f;
    public float angleSpringStrength;
    public float dampingStrength = 0.25f;
    private bool applyForceToPoint = false;
    private Frame lastFrame;
    private Frame currentFrame;
    private bool init = false;
    private class Frame {
        public double time;
        public Vector3 position;
    }
    
    public void SetWorldAnchor(Vector3 newWorldAnchor) {
        lastFrame = currentFrame;
        currentFrame = new Frame { time = Time.timeAsDouble, position = newWorldAnchor };
    }

    private void Start() {
        if (body == null) {
            body = GetComponent<Rigidbody>();
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
        double sample = Time.timeAsDouble - lastFrame.time;
        double diff = currentFrame.time - lastFrame.time;
        if (diff != 0f) {
            sample /= (float)diff;
        }

        // Have to predict the position due to update/fixed update discrepancies.
        Vector3 p2 = Vector3.LerpUnclamped(lastFrame.position, currentFrame.position, Mathf.Clamp((float)sample, -0.5f, 1.5f));
        Vector3 velocityDifference;
        Vector3 connectedVelocity = (currentFrame.position-lastFrame.position);
        if (diff != 0f) {
            connectedVelocity /= (float)diff;
        } else {
            connectedVelocity = Vector3.zero;
        }
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

        //body.velocity -= velocityDifference * dampingStrength;
        if (applyForceToPoint) {
            body.AddForceAtPosition(force-velocityDifference*dampingStrength, p1, ForceMode.Acceleration);
        } else {
            body.AddForce(force-velocityDifference*dampingStrength, ForceMode.Acceleration);
        }
    }
}
