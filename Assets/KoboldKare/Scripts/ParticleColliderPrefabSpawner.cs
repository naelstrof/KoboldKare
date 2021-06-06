using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleColliderPrefabSpawner : MonoBehaviour
{
    public GameObject prefab;
    private ParticleSystem ps;
    private void Start() {
        ps = GetComponent<ParticleSystem>();
    }
    private void OnParticleCollision(GameObject other) {
        List<ParticleCollisionEvent> events = new List<ParticleCollisionEvent>();
        ParticlePhysicsExtensions.GetCollisionEvents(ps, other, events);
        foreach( ParticleCollisionEvent e in events ) {
            if (e.velocity.magnitude > 0.5f && Mathf.Abs(e.velocity.y) > 0.1f ) {
                GameObject.Instantiate(prefab, e.intersection-(e.normal*1.5f), Quaternion.LookRotation(Vector3.forward,e.normal));
            }
        }
    }
}
