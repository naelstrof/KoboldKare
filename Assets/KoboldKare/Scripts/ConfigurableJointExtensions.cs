using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IJointData {
    public Joint Apply(Rigidbody connectedBodyOverride);
}

public class ConfigurableJointData : IJointData {
    public GameObject gameObject;
    public ConfigurableJointMotion angularXMotion, angularYMotion, angularZMotion;
    public JointDrive angularXDrive, angularYZDrive;
    public ConfigurableJointMotion xMotion, yMotion, zMotion;
    public JointDrive xDrive, yDrive, zDrive;
    public Rigidbody connectedBody;
    public bool autoConfigureConnectedAnchor;
    public Vector3 anchor, connectedAnchor;
    public bool configuredInWorldSpace;
    public Vector3 axis, secondaryAxis;
    public SoftJointLimitSpring angularXLimitSpring, angularYZLimitSpring, linearLimitSpring;
    public float massScale, connectedMassScale;
    public float breakForce, breakTorque;
    public SoftJointLimit lowAngularXLimit, highAngularXLimit, linearLimit;
    public SoftJointLimit angularYLimit, angularZLimit;
    public bool swapBodies, enableCollision, enablePreprocessing;
    public JointDrive slerpDrive;
    public Vector3 targetPosition, targetVelocity, targetAngularVelocity;
    public Quaternion targetRotation;
    public JointProjectionMode projectionMode;
    public float projectionAngle, projectionDistance;
    public RotationDriveMode rotationDriveMode;
    public Joint Apply( Rigidbody connectedBodyOverride = null ) {
        return (Joint)Apply(gameObject, connectedBodyOverride);
    }
    public ConfigurableJoint Apply(GameObject g, Rigidbody connectedBodyOverride = null) {
        ConfigurableJoint j = g.AddComponent<ConfigurableJoint>();
        j.rotationDriveMode = rotationDriveMode;
        j.projectionMode = projectionMode;
        j.projectionAngle = projectionAngle;
        j.projectionDistance = projectionDistance;
        j.angularXMotion = angularXMotion;
        j.angularYMotion = angularYMotion;
        j.angularZMotion = angularZMotion;
        j.angularXDrive = angularXDrive;
        j.angularYZDrive = angularYZDrive;
        j.xMotion = xMotion;
        j.yMotion = yMotion;
        j.zMotion = zMotion;
        j.xDrive = xDrive;
        j.yDrive = yDrive;
        j.zDrive = zDrive;
        j.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
        j.anchor = anchor;
        j.connectedAnchor = connectedAnchor;
        j.configuredInWorldSpace = configuredInWorldSpace;
        j.axis = axis;
        j.secondaryAxis = secondaryAxis;
        j.angularXLimitSpring = angularXLimitSpring;
        j.angularYZLimitSpring = angularYZLimitSpring;
        j.linearLimitSpring = linearLimitSpring;
        j.massScale = massScale;
        j.connectedMassScale = connectedMassScale;
        j.breakForce = breakForce;
        j.breakTorque = breakTorque;
        j.lowAngularXLimit = lowAngularXLimit;
        j.highAngularXLimit = highAngularXLimit;
        j.linearLimit = linearLimit;
        j.angularYLimit = angularYLimit;
        j.angularZLimit = angularZLimit;
        j.swapBodies = swapBodies;
        j.enableCollision = enableCollision;
        j.enablePreprocessing = enablePreprocessing;
        j.slerpDrive = slerpDrive;
        //j.targetPosition = targetPosition;
        //j.targetVelocity = targetVelocity;
        //j.targetAngularVelocity = targetAngularVelocity;
        j.targetRotation = targetRotation;
        j.connectedBody = connectedBodyOverride == null ? connectedBody : connectedBodyOverride;
        return j;
    }
    public ConfigurableJointData(ConfigurableJoint j) {
        gameObject = j.gameObject;
        angularXMotion = j.angularXMotion;
        angularYMotion = j.angularYMotion;
        angularZMotion = j.angularZMotion;
        angularXDrive = j.angularXDrive;
        angularYZDrive = j.angularYZDrive;
        xMotion = j.xMotion;
        yMotion = j.yMotion;
        zMotion = j.zMotion;
        xDrive = j.xDrive;
        yDrive = j.yDrive;
        zDrive = j.zDrive;
        autoConfigureConnectedAnchor = j.autoConfigureConnectedAnchor;
        anchor = j.anchor;
        connectedAnchor = j.connectedAnchor;
        configuredInWorldSpace = j.configuredInWorldSpace;
        axis = j.axis;
        secondaryAxis = j.secondaryAxis;
        angularXLimitSpring = j.angularXLimitSpring;
        angularYZLimitSpring = j.angularYZLimitSpring;
        linearLimitSpring = j.linearLimitSpring;
        massScale = j.massScale;
        connectedMassScale = j.connectedMassScale;
        breakForce = j.breakForce;
        breakTorque = j.breakTorque;
        lowAngularXLimit = j.lowAngularXLimit;
        highAngularXLimit = j.highAngularXLimit;
        linearLimit = j.linearLimit;
        angularYLimit = j.angularYLimit;
        angularZLimit = j.angularZLimit;
        swapBodies = j.swapBodies;
        enableCollision = j.enableCollision;
        enablePreprocessing = j.enablePreprocessing;
        slerpDrive = j.slerpDrive;
        targetPosition = j.targetPosition;
        targetVelocity = j.targetVelocity;
        targetAngularVelocity = j.targetAngularVelocity;
        targetRotation = j.targetRotation;
        projectionMode = j.projectionMode;
        projectionAngle = j.projectionAngle;
        projectionDistance = j.projectionDistance;
        connectedBody = j.connectedBody;
        rotationDriveMode = j.rotationDriveMode;
    }
}
public class CharacterJointData : IJointData {
    public GameObject gameObject;
    public ConfigurableJointMotion angularXMotion, angularYMotion, angularZMotion;
    public JointDrive angularXDrive, angularYZDrive;
    public ConfigurableJointMotion xMotion, yMotion, zMotion;
    public JointDrive xDrive, yDrive, zDrive;
    public Rigidbody connectedBody;
    public bool autoConfigureConnectedAnchor;
    public Vector3 anchor, connectedAnchor;
    public bool configuredInWorldSpace;
    public Vector3 axis, swingAxis;
    public SoftJointLimitSpring swingLimitSpring;
    public SoftJointLimitSpring twistLimitSpring;
    public float massScale, connectedMassScale;
    public float breakForce, breakTorque;
    public SoftJointLimit swing1Limit, swing2Limit;
    public SoftJointLimit highTwistLimit, lowTwistLimit;
    public bool swapBodies, enableCollision, enablePreprocessing;
    public JointDrive slerpDrive;
    public Vector3 targetPosition, targetVelocity, targetAngularVelocity;
    public Quaternion targetRotation;
    public JointProjectionMode projectionMode;
    public float projectionAngle, projectionDistance;
    public RotationDriveMode rotationDriveMode;
    public bool enableProjection;
    public Joint Apply( Rigidbody connectedBodyOverride = null ) {
        return (Joint)Apply(gameObject, connectedBodyOverride);
    }
    public CharacterJoint Apply(GameObject g, Rigidbody connectedBodyOverride = null) {
        CharacterJoint j = g.AddComponent<CharacterJoint>();
        j.connectedBody = connectedBodyOverride == null ? connectedBody : connectedBodyOverride;
        j.projectionAngle = projectionAngle;
        j.projectionDistance = projectionDistance;
        j.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
        j.anchor = anchor;
        j.connectedAnchor = connectedAnchor;
        j.axis = axis;
        j.swingAxis = swingAxis;
        j.twistLimitSpring = twistLimitSpring;
        j.massScale = massScale;
        j.connectedMassScale = connectedMassScale;
        j.breakForce = breakForce;
        j.breakTorque = breakTorque;
        j.enableCollision = enableCollision;
        j.enablePreprocessing = enablePreprocessing;
        j.swing1Limit = swing1Limit;
        j.swing2Limit = swing2Limit;
        j.swingLimitSpring = swingLimitSpring;
        j.highTwistLimit = highTwistLimit;
        j.lowTwistLimit = lowTwistLimit;
        j.enableProjection = enableProjection;
        //j.targetPosition = targetPosition;
        //j.targetVelocity = targetVelocity;
        //j.targetAngularVelocity = targetAngularVelocity;
        return j;
    }
    public CharacterJointData(GameObject target) {
        gameObject = target;
        autoConfigureConnectedAnchor = true;
        anchor = Vector3.zero;
        //connectedAnchor =
        axis = Vector3.right;
        swingAxis = Vector3.back;
        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = 33f;
        limit.bounciness = 0.1f;
        limit.contactDistance = 0f;
        swing1Limit = limit;
        SoftJointLimit otherLimit = new SoftJointLimit();
        otherLimit.limit = 9f;
        otherLimit.bounciness = 0.1f;
        otherLimit.contactDistance = 0f;
        swing2Limit = otherLimit;
        SoftJointLimitSpring limitSpring = new SoftJointLimitSpring();
        limitSpring.spring = 1f;
        limitSpring.damper = 0.1f;
        swingLimitSpring = limitSpring;

        twistLimitSpring = limitSpring;

        SoftJointLimit twistLimit = new SoftJointLimit();
        twistLimit.limit = 30f;
        twistLimit.bounciness = 0.1f;
        highTwistLimit = twistLimit;

        SoftJointLimit otherTwistLimit = new SoftJointLimit();
        otherTwistLimit.limit = -40f;
        otherTwistLimit.bounciness = 0.1f;
        lowTwistLimit = otherTwistLimit;

        massScale = 10f;
        connectedMassScale = 1f;
        breakForce = Mathf.Infinity;
        breakTorque = Mathf.Infinity;

        enableCollision = false;
        enablePreprocessing = true;
        projectionAngle = 5f;
        projectionDistance = 0.1f;
        connectedBody = null;
        enableProjection = true;
    }
    public CharacterJointData(CharacterJoint j) {
        gameObject = j.gameObject;
        autoConfigureConnectedAnchor = j.autoConfigureConnectedAnchor;
        anchor = j.anchor;
        connectedAnchor = j.connectedAnchor;
        axis = j.axis;
        swingAxis = j.swingAxis;
        swing1Limit = j.swing1Limit;
        swing2Limit = j.swing2Limit;
        swingLimitSpring = j.swingLimitSpring;
        twistLimitSpring = j.twistLimitSpring;
        highTwistLimit = j.highTwistLimit;
        lowTwistLimit = j.lowTwistLimit;
        massScale = j.massScale;
        connectedMassScale = j.connectedMassScale;
        breakForce = j.breakForce;
        breakTorque = j.breakTorque;
        enableCollision = j.enableCollision;
        enablePreprocessing = j.enablePreprocessing;
        projectionAngle = j.projectionAngle;
        projectionDistance = j.projectionDistance;
        connectedBody = j.connectedBody;
        enableProjection = j.enableProjection;
    }
}
