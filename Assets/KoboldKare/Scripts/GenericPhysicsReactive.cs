using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericPhysicsReactive : MonoBehaviour {
    public UnityEvent onHardHit;
    public void OnCollisionEnter(Collision collision) {
        if (collision.relativeVelocity.magnitude > 10f && collision.impulse.magnitude > 5f) {
            onHardHit.Invoke();
        }
    }
}
