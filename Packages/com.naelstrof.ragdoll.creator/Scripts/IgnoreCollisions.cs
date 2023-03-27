using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollisions : MonoBehaviour {
    public List<Collider> groupA = new List<Collider>();
    public List<Collider> groupB = new List<Collider>();
    void Start() {
        foreach(Collider a in groupA) {
            foreach(Collider b in groupB) {
                Physics.IgnoreCollision(a,b,true);
            }
        }
    }
}
