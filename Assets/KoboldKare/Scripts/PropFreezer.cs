using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class PropFreezer : MonoBehaviourPun, IAdvancedInteractable {
    public List<GameObject> deleteOnUnfreeze = new List<GameObject>();
    public bool frozen {
        get {
            return GetComponent<Rigidbody>().constraints == RigidbodyConstraints.FreezeAll;
        }
        set {
            if (!value) {
                foreach (GameObject o in deleteOnUnfreeze) {
                    Destroy(o);
                }
                deleteOnUnfreeze.Clear();
            }
            if (value && GetComponent<Rigidbody>().constraints != RigidbodyConstraints.FreezeAll) {
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
            if (!value && GetComponent<Rigidbody>().constraints != RigidbodyConstraints.None) {
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
        }
    }
    public void Start() {
        frozen = true;
    }
    public void OnCollisionEnter(Collision collision) {
        if (collision.rigidbody != null && !collision.rigidbody.isKinematic) {
            frozen = false;
        }
    }

    public void InteractTo(Vector3 worldPosition, Quaternion worldRotation) {
    }

    public void OnInteract(Kobold k) {
        frozen = false;
    }

    public void OnEndInteract(Kobold k) {
    }

    public bool PhysicsGrabbable() {
        return true;
    }
}
