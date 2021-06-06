using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeJointConstraint : MonoBehaviour {
    public Rigidbody connectedBody;
    public Vector3 connectedAnchor;
    //public Rigidbody body;
    public Vector3 anchor;
    public float springStrength;
    public float dampStrength;
    public float maxDistance;
    void FixedUpdate() {
        Vector3 bodyVel, cbodyVel;
        Vector3 connectedBodyPos;
        if (connectedBody != null) {
            connectedBodyPos = connectedBody.transform.TransformPoint(connectedAnchor);
            cbodyVel = connectedBody.velocity;
        } else {
            connectedBodyPos = connectedAnchor;
            cbodyVel = Vector3.zero;
        }
        Vector3 pos;
        pos = anchor;
        bodyVel = Vector3.zero;
        Vector3 normDir = Vector3.Normalize(connectedBodyPos - pos);
        float dist = Vector3.Distance(pos, connectedBodyPos);
        float power = Mathf.Max(dist - maxDistance, 0f);
        //Vector3 connectedBodyDamp = -connectedBody.GetPointVelocity(connectedBodyPos) * dampStrength;
        Vector3 dampingForce = (cbodyVel-bodyVel) * dampStrength;
        //if (power >1f) {
            //power *= power;
        //}
        //if (connectedBody != null) {
            //connectedBody.velocity -= dampingForce;
            connectedBody.AddForceAtPosition(-normDir * springStrength * power, connectedBodyPos);
        //}
    }
}
