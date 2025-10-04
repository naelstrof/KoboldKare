using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericPhysicsReactive : MonoBehaviour {
    [SerializeField, HideInInspector]
    private UnityEvent onHardHit;
    
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> onHardHitResponses = new List<GameEventResponse>();
    
    public void OnCollisionEnter(Collision collision) {
        if (collision.relativeVelocity.magnitude > 10f && collision.impulse.magnitude > 5f) {
            foreach (var response in onHardHitResponses) {
                response?.Invoke(this);
            }
        }
    }
}
