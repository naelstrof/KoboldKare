using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisJoint : MonoBehaviour {
    public Rigidbody body;
    public Rigidbody connectedBody;
    public Vector3 localForwardAxis;
    public Vector3 localAnchor;
    public Vector3 localConnectedAnchor;
    public Vector3 localConnectedAxis;
    //[Range(0.1f,180f)]
    //public float softness = 10f;
    public float springStrength = 70f;
    public float overPenetrateStrength = 10f;
    public float springDamp = 0f;
    public float angleDamp = 0f;
    public float deflectionSpringStrength = 60f;
    public float massRatio = 0.9f;
    public float deflectionForgivenessDegrees = 30f;
    public float axisAlignmentForgivenessMeters = 0.12f;

    [Range(0f, 1f)]
    public float weight = 1f;

    private void Start() {
        body = GetComponent<Rigidbody>();
    }
    private void FixedUpdate() {
        if (body == null || connectedBody == null || Mathf.Approximately(weight, 0f)) {
            return;
        }
        Vector3 holePosition = body.transform.TransformPoint(localAnchor);
        Vector3 dickPosition = connectedBody.transform.TransformPoint(localConnectedAnchor);
        Vector3 dickDir = connectedBody.transform.TransformDirection(localConnectedAxis);
        Vector3 holeDir = body.transform.TransformDirection(localForwardAxis);
        Vector3 desiredHolePosition = Vector3.Project(holePosition - dickPosition, dickDir) + dickPosition;
        float dist = Vector3.Distance(holePosition, desiredHolePosition);
        float distAdjust = Mathf.Clamp01(1f - dist);
        Vector3 dickToHoleDir = ((holePosition - holeDir*distAdjust) - (dickPosition-dickDir*distAdjust)).normalized;

        // Damping forces
        Vector3 holeVel = body.GetPointVelocity(holePosition);
        Vector3 dickVel = connectedBody.GetPointVelocity(desiredHolePosition);
        body.AddForceAtPosition((dickVel - holeVel) * springDamp, holePosition, ForceMode.VelocityChange);
        connectedBody.AddForceAtPosition((holeVel - dickVel) * springDamp, desiredHolePosition, ForceMode.VelocityChange);
        //connectedBody.velocity += (holeVel - dickVel) * springDamp;
        body.angularVelocity -= body.angularVelocity * angleDamp;
        //connectedBody.angularVelocity -= connectedBody.angularVelocity * angleDamp;

        // Face each-other:
        Vector3 bcross = Vector3.Cross(holeDir, -dickToHoleDir);
        float bangleDiff = Mathf.Max(Vector3.Angle(holeDir, -dickToHoleDir) - deflectionForgivenessDegrees*dist, 0f);
        body.AddTorque(bcross * bangleDiff * deflectionSpringStrength * weight * 0.25f, ForceMode.Acceleration);

        Vector3 dcross = Vector3.Cross(dickDir, dickToHoleDir);
        float dangleDiff = Mathf.Max(Vector3.Angle(dickDir, dickToHoleDir) - deflectionForgivenessDegrees*dist, 0f);
        connectedBody.AddTorque(dcross * dangleDiff * deflectionSpringStrength * weight, ForceMode.Acceleration);

        // Prevent over-penetration:
        Vector3 dickToHoleReal = holePosition - dickPosition;
        // If we've over-penetrated
        float dot = Vector3.Dot(dickToHoleReal, dickDir);
        if (dot < 0) {
            Vector3 overPenetrationForce = dickDir * dot;
            body.AddForceAtPosition(-overPenetrationForce * overPenetrateStrength * weight*0.5f, holePosition, ForceMode.VelocityChange);
            connectedBody.AddForceAtPosition(overPenetrationForce * overPenetrateStrength * weight*2f, dickPosition, ForceMode.VelocityChange);

            desiredHolePosition = dickPosition;
            Vector3 axisDirection = (desiredHolePosition - holePosition).normalized;
            float axisDistance = (desiredHolePosition - holePosition).magnitude;
            Vector3 axisForce = axisDirection * axisDistance;

            body.AddForceAtPosition(axisForce * springStrength * weight, holePosition, ForceMode.Acceleration);
            connectedBody.AddForceAtPosition(-axisForce * springStrength * weight, desiredHolePosition, ForceMode.Acceleration);
        } else {
            // Try to keep the hole along the dick axis, but along an orbit
            Vector3 diff = desiredHolePosition - holePosition;
            Vector3 axisForce = Vector3.ProjectOnPlane(diff, dickToHoleReal).normalized * Mathf.Max(diff.magnitude-axisAlignmentForgivenessMeters*dist,0f);
            body.AddForceAtPosition(axisForce * springStrength * weight, holePosition, ForceMode.Acceleration);
            connectedBody.AddForceAtPosition(-axisForce * springStrength * weight, desiredHolePosition, ForceMode.Acceleration);
        }
    }
}
