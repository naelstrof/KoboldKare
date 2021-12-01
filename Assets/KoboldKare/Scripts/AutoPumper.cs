using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPumper : MonoBehaviour {
    public FluidOutput output;
    private GenericReagentContainer target;
    void Start() {
        target = GetComponentInParent<GenericReagentContainer>();
        target.OnChange.AddListener(OnChanged);
        output.Fire(target);
    }
    void OnDestroy() {
        if (target != null) {
            target.OnChange.RemoveListener(OnChanged);
        }
    }
    void OnChanged(GenericReagentContainer.InjectType type) {
        if (!output.isFiring && target.volume > output.GetVPS()) {
            output.Fire(target);
        }
    }
}
