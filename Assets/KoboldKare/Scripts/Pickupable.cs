using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Pickupable : MonoBehaviour
{
    private Rigidbody body;
    public UnityEvent OnPickup;
    public UnityEvent OnDrop;

    private void Start() {
        body = GetComponent<Rigidbody>();
    }
    public void OnGrab() {
        OnPickup.Invoke();
    }
    public void OnRelease() {
        OnDrop.Invoke();
    }
    void FixedUpdate()
    {
        if (body.velocity.magnitude > 5f) {
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        } else {
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
    }
}
