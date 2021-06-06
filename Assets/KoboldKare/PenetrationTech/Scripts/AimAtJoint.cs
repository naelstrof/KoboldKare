using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAtJoint : MonoBehaviour {
    public Rigidbody body;
    public Transform connectedBody;
    public Vector3 localAnchor;
    public Vector3 localConnectedAnchor;
    public Vector3 localConnectedAimVector;
    public Transform localTransform;
    public Vector3 localAimVector;
    public Vector3 localUpVector;
    public Vector3 worldUpVector;
    //[Range(0.1f,180f)]
    //public float softness = 10f;
    public float springStrength = 10f;
    public float softness = 90f;
    public float dampingStrength = 1f;

    [Range(0f, 1f)]
    public float weight = 1f;

    private void Start() {
        body = GetComponent<Rigidbody>();
    }
    private void FixedUpdate() {
        if (body == null || connectedBody == null || weight == 0f || localTransform == null) {
            return;
        }
        //body.maxAngularVelocity = Mathf.Lerp(7f, 10f, weight);
        Vector3 selfPosition = body.transform.TransformPoint(localAnchor);
        Vector3 otherPosition = connectedBody.TransformPoint(localConnectedAnchor);
        float distanceAdjust = Mathf.Clamp01(1f - Vector3.Distance(selfPosition, otherPosition)*2f);
        otherPosition += localTransform.TransformDirection(localConnectedAimVector) * distanceAdjust;

        float fangle = 0;
        Vector3 faxis = Vector3.zero;

        Quaternion.FromToRotation(body.transform.TransformDirection(localAimVector), (otherPosition-selfPosition).normalized).ToAngleAxis(out fangle, out faxis);
        if (fangle >= 180) {
            fangle = 360 - fangle;
            faxis = -faxis;
        }

        float uangle = 0;
        Vector3 uaxis = Vector3.zero;

        Vector3 fup = Vector3.ProjectOnPlane(body.transform.TransformDirection(localAimVector), worldUpVector);
        Quaternion.FromToRotation(Vector3.ProjectOnPlane(body.transform.TransformDirection(localUpVector),fup.normalized).normalized, worldUpVector).ToAngleAxis(out uangle, out uaxis);
        if (uangle >= 180) {
            uangle = 360 - uangle;
            uaxis = -uaxis;
        }
        Vector3 newAngularVelocity = uaxis * uangle/softness;
        newAngularVelocity += faxis * fangle/softness;
        //Vector3 axis = Vector3.Lerp(faxis, uaxis, Mathf.Clamp01(uangle - fangle));
        //float angle = Mathf.Lerp(fangle, uangle, Mathf.Clamp01(uangle - fangle));
        //body.angularVelocity = Vector3.MoveTowards(body.angularVelocity, Vector3.zero, Time.fixedDeltaTime * Mathf.Max(body.angularVelocity.magnitude,1f) * 10f);
        //Vector3 dampingForce = -body.angularVelocity * dampingStrength;
        //body.angularVelocity -= (body.angularVelocity * dampingStrength * weight * Time.deltaTime);
        body.AddTorque(newAngularVelocity * weight * springStrength);
    }
}
