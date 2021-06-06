using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GrabbableType : uint {
    None = 0,
    Kobold = 1,
    Fruit = 2,
    Sprayer = 4,
    Scanner = 8,
    Dildo = 16,
    Flask = 32,
    Grenade = 64,
    Any = uint.MaxValue,
}

public interface IGrabbable {
    bool OnGrab(Kobold kobold);
    void OnRelease(Kobold kobold);
    Rigidbody[] GetRigidBodies();
    Renderer[] GetRenderers();
    Transform GrabTransform(Rigidbody body);
    Transform transform { get; }
    GrabbableType GetGrabbableType();
    //void OnGrabStay();
}
