using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleLimitJoint : MonoBehaviour {
    public Rigidbody body;
    public Rigidbody connectedBody;
    public Transform connectedTransform;
    public Vector3 localAnchor;
    public Vector3 localConnectedAnchor;
    public Vector3 localConnectedUpVector;
    public Vector3 localAimVector;
    public Vector3 localUpVector;
    public Vector3 localConnectedAimVector;
    public float springStrength = 1f;
    public float dampStrength = 1f;
    public float softness = 360f;

    private void Start() {
        body = GetComponent<Rigidbody>();
    }
    private void FixedUpdate() {
        if (body == null || connectedBody == null) {
            return;
        }
        Vector3 dir = body.transform.TransformDirection(localAimVector);
        Vector3 otherDir = connectedTransform.TransformDirection(localConnectedAimVector);

        float fangle = 0;
        Vector3 faxis = Vector3.zero;
        Quaternion.FromToRotation(dir, otherDir).ToAngleAxis(out fangle, out faxis);
        if (fangle >= 180) {
            fangle = 360 - fangle;
            faxis = -faxis;
        }

        float uangle = 0;
        Vector3 uaxis = Vector3.zero;
        Vector3 fup = Vector3.ProjectOnPlane(body.transform.TransformDirection(localAimVector), connectedTransform.TransformDirection(localConnectedUpVector));
        Quaternion.FromToRotation(Vector3.ProjectOnPlane(body.transform.TransformDirection(localUpVector),fup.normalized).normalized, connectedTransform.TransformDirection(localConnectedUpVector)).ToAngleAxis(out uangle, out uaxis);
        if (uangle >= 180) {
            uangle = 360 - uangle;
            uaxis = -uaxis;
        }

        Vector3 wantedAngVel = faxis * fangle/softness;
        wantedAngVel += uaxis * uangle/softness;
        //wantedAngVel *= springStrength;

        //if (Vector3.Dot(dir, otherDir) < 0f) {
            //body.angularVelocity = wantedAngVel;
            //connectedBody.angularVelocity = -wantedAngVel;
        //} else {
            //body.angularVelocity = Vector3.MoveTowards(body.angularVelocity, Vector3.zero, body.angularVelocity.magnitude * Time.deltaTime);
            Vector3 torque = (wantedAngVel*springStrength);
            body.AddTorque(torque * body.mass);
            //connectedBody.AddTorque(-torque * connectedBody.mass);
        //}
        //body.maxAngularVelocity = Mathf.Lerp(7f, 10f, weight);
        //body.angularVelocity = Vector3.MoveTowards(body.angularVelocity, Vector3.zero, Time.fixedDeltaTime * Mathf.Max(body.angularVelocity.magnitude,1f) * 10f);
        //body.angularVelocity += newAngularVelocity * weight * springStrength * Time.fixedDeltaTime;
    }
}
