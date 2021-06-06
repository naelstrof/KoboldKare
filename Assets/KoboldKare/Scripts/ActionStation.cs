using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionStation : MonoBehaviour {
    public bool completedAction = false;
    private bool use;
    public bool inUse {
        get {
            return use;
        }
        set {
            use = value;
            if (use) {
                foreach (Collider c in GetComponents<Collider>()) {
                    c.enabled = false;
                }
            } else {
                foreach (Collider c in GetComponents<Collider>()) {
                    c.enabled = true;
                }
            }
        }
    }
    public AnimationClip clip;
    public float duration = -1;
    public void Start() {
        if (duration == -1) {
            duration = clip.length;
        }
    }
}
