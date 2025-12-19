using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReagentConsumptionMetabolize : ReagentConsumptionEvent {
    public override void OnConsume(NetworkedKobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack,ref float energy) {
        if (!k.TryGetKobold(out var kobold)) {
            return;
        }
        float spaceAvailable = kobold.metabolizedContents.GetMaxVolume()-kobold.metabolizedContents.volume;
        float mixAmount = Mathf.Min(spaceAvailable, amountProcessed);
        kobold.metabolizedContents.AddMix(scriptableReagent.GetReagent(mixAmount));
        //addBack.AddMix(scriptableReagent.GetReagent(amountProcessed - mixAmount));
        amountProcessed = mixAmount;
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref energy);
    }
}
