using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReagentConsumptionMetabolize : ReagentConsumptionEvent {
    public override void OnConsume(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
        float spaceAvailable = k.metabolizedContents.GetMaxVolume()-k.metabolizedContents.volume;
        float mixAmount = Mathf.Min(spaceAvailable, amountProcessed);
        k.metabolizedContents.AddMix(scriptableReagent.GetReagent(mixAmount));
        //addBack.AddMix(scriptableReagent.GetReagent(amountProcessed - mixAmount));
        amountProcessed = mixAmount;
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
    }
}
