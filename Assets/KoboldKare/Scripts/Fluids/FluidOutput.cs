using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidOutput : MonoBehaviour {
    public virtual bool isFiring { get; }
    public virtual float GetVPS() {
        return 0f;
    }
    public virtual void Fire(GenericReagentContainer b) {
    }
    public virtual void StopFiring() {
    }
}
