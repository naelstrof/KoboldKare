using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPumper : MonoBehaviour {
    public GameObject output;
    private IFluidOutput foutput;
    private GenericReagentContainer target;
    void Start() {
        foutput = output.GetComponent<IFluidOutput>();
        target = GetComponentInParent<GenericReagentContainer>();
        target.OnChange.AddListener(OnChanged);
        foutput.Fire(target);
    }
    void OnDestroy() {
        if (target != null) {
            target.OnChange.RemoveListener(OnChanged);
        }
    }
    void OnChanged(GenericReagentContainer.InjectType type) {
        if (!foutput.isFiring && target.volume > 1f) {
            foutput.Fire(target);
        }
    }
}
