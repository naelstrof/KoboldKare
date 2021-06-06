using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GibsSpawner : MonoBehaviour
{
    public Transform _gibArmature;
    public Vector3 _randomVelocity;
    public Transform _targetArmature = null;
    private Vector3 _velocity = Vector3.zero;
    private bool _spawned = false;
    public AudioClip gibSound;

    void PrepareGib( Transform body ) {
        if (body.gameObject.GetComponent<Rigidbody>() == null && body.gameObject.GetComponent<VisualEffect>() == null) {
            return;
        }
        body.transform.parent = null;
        if (body.gameObject.GetComponent<Rigidbody>() != null) {
            Vector3 rv = new Vector3(Random.Range(-_randomVelocity.x, _randomVelocity.x),
                                     Random.Range(-_randomVelocity.y, _randomVelocity.y),
                                     Random.Range(-_randomVelocity.z, _randomVelocity.z));
            body.gameObject.GetComponent<Rigidbody>().velocity = _velocity + rv;
        }
    }

    public void FitTo(Transform to) {
        _targetArmature = to;
        Rigidbody r = _targetArmature.GetComponentInParent<Rigidbody>();
        if ( r ) {
            _velocity = r.velocity;
        }
        FitToArmature(_gibArmature, _targetArmature);
        GameManager.instance.SpawnAudioClipInWorld(gibSound, transform.position);
        _spawned = true;
    }

    void FitToArmature(Transform from, Transform to) {
        from.position = to.position;
        from.rotation = to.rotation;
        for (int i=0;i<from.childCount;i++) {
            Transform child = from.GetChild(i);
            Transform otherChild = to.Find(child.name);
            if ( otherChild == null ) {
                // Must be a gib
                PrepareGib(child);
                continue;
            }
            FitToArmature(child, otherChild);
        }
    }
    void Update() {
        Destroy(gameObject);
    }
}
