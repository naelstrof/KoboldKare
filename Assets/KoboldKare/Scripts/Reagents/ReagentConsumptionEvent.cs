using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReagentConsumptionEvent {
    public virtual void OnConsume(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
        energy += amountProcessed * scriptableReagent.GetCalories();
    }

    public virtual void OnValidate() {
    }
}
