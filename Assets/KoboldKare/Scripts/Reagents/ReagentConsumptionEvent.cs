using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReagentConsumptionEvent {
    public virtual void OnConsume(NetworkedKobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack, ref float energy) {
        energy += amountProcessed * scriptableReagent.GetCalories();
    }

    public virtual void OnValidate() {
    }
}
