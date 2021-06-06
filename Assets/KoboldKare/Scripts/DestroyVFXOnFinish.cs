using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DestroyVFXOnFinish : MonoBehaviour {
    VisualEffect effect;
    bool initialized = false;
    void Start() {
        effect = GetComponentInChildren<VisualEffect>();
    }
    void FixedUpdate() {
        if (!initialized){
            initialized = true;
            return;
        }
        if (effect.aliveParticleCount == 0) {
            Destroy(gameObject);
        }
    }
}
