using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JointSpawner {

    /*public static CharacterJoint addCharacterJoint(Rigidbody body, Rigidbody connectedRigidBody, Vector3 position) {
        CharacterJoint joint = body.gameObject.AddComponent<CharacterJoint>();
        joint.connectedBody = connectedRigidBody;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = body.transform.InverseTransformPoint(position);
        joint.connectedAnchor = connectedRigidBody.transform.InverseTransformPoint(position);
        //configureJoint(joint, lx, hx, y, z, axis);
        return joint;
    }

    private static void configureJoint(ConfigurableJoint joint, float lx, float hx, float y, float z, Vector3 axis) {
        joint.axis = axis;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
        SoftJointLimitSpring limspring = new SoftJointLimitSpring();
        SoftJointLimit xhlim = new SoftJointLimit();
        SoftJointLimit xllim = new SoftJointLimit();
        SoftJointLimit ylim = new SoftJointLimit();
        SoftJointLimit zlim = new SoftJointLimit();
        //JointDrive drive = new JointDrive();
        //drive.positionSpring = 10f;
        //drive.positionDamper = 10f;
        //drive.mode = JointDriveMode.PositionAndVelocity;
        xllim.limit = lx;
        xllim.bounciness = 0.1f;
        xhlim.limit = hx;
        xhlim.bounciness = 0.1f;
        ylim.limit = y;
        ylim.bounciness = 0.1f;
        zlim.limit = z;
        zlim.bounciness = 0.1f;
        limspring.spring = 6f;
        limspring.damper = 0.2f;
        joint.lowAngularXLimit = xllim;
        joint.angularXLimitSpring = limspring;
        joint.highAngularXLimit = xhlim;
        joint.angularYLimit = ylim;
        joint.angularZLimit = zlim;
        joint.angularYZLimitSpring = limspring;
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = 0.1f;
        joint.projectionAngle = 1f;
        joint.enableCollision = false;
        joint.autoConfigureConnectedAnchor = true;
        //joint.angularXDrive=drive;
        //joint.angularYZDrive=drive;
        //joint.xDrive=drive;
        //joint.yDrive=drive;
        //joint.zDrive=drive;
    }*/
}
