using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class KoboldEvent : UnityEvent<Kobold> {}
public class GenericGrabbable : MonoBehaviour, IGrabbable {
    public KoboldEvent onGrab;
    public KoboldEvent onRelease;
    public KoboldEvent onThrow;
    public Rigidbody[] bodies;
    public Renderer[] renderers;
    public Transform center;
    public GrabbableType grabbableType;
    public bool OnGrab(Kobold kobold) {
        onGrab.Invoke(kobold);
        return true;
    }

    public void OnRelease(Kobold kobold) {
        onRelease.Invoke(kobold);
    }
    public void OnThrow(Kobold kobold) {
        onThrow.Invoke(kobold);
    }
    public Vector3 GrabOffset() {
        return Vector3.zero;
    }

    public Rigidbody[] GetRigidBodies()
    {
        return bodies;
    }

    public Renderer[] GetRenderers()
    {
        return renderers;
    }

    public Transform GrabTransform(Rigidbody r)
    {
        return center;
    }

    public GrabbableType GetGrabbableType() {
        return grabbableType;
    }
}
