using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mixer : MonoBehaviour {
    public GenericReagentContainer target;
    [System.Serializable]
    public class InspectorReagent {
        public ScriptableReagent reagent;
        public float volume;
    }
    public InspectorReagent[] reagents;
    public void Mix() {
        foreach(var reagent in reagents) {
            target.AddMix(reagent.reagent, reagent.volume, GenericReagentContainer.InjectType.Inject);
        }
    }
}
