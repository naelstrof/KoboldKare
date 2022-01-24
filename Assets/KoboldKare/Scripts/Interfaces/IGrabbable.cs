using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IGrabbable {
    bool OnGrab(Kobold kobold);
    void OnRelease(Kobold kobold);
    void OnThrow(Kobold kobold);
    Rigidbody[] GetRigidBodies();
    Renderer[] GetRenderers();
    Transform GrabTransform(Rigidbody body);
    Transform transform { get; }
    //GrabbableType GetGrabbableType();
    //void OnGrabStay();
}
