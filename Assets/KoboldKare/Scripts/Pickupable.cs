using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Pickupable : MonoBehaviour {
    private Rigidbody body;
    
    [SerializeField, HideInInspector]
    private UnityEvent OnPickup;
    [SerializeField, HideInInspector]
    private UnityEvent OnDrop;
    
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> onPickupResponses = new List<GameEventResponse>();
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> onDropResponses = new List<GameEventResponse>();

    private void Awake() {
        GameEventSanitizer.SanitizeRuntime(OnPickup, onPickupResponses, this);
        GameEventSanitizer.SanitizeRuntime(OnDrop, onDropResponses, this);
    }

    private void OnValidate() {
        GameEventSanitizer.SanitizeEditor(nameof(OnPickup), nameof(onPickupResponses), this);
        GameEventSanitizer.SanitizeEditor(nameof(OnDrop), nameof(onDropResponses), this);
    }

    private void Start() {
        body = GetComponent<Rigidbody>();
    }
    public void OnGrab() {
        foreach(var response in onPickupResponses) {
            response?.Invoke(this);
        }
    }
    public void OnRelease() {
        foreach(var response in onDropResponses) {
            response?.Invoke(this);
        }
    }
    void FixedUpdate() {
        if (body.velocity.magnitude > 5f) {
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        } else {
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
    }
}
