using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[System.Serializable]
public class ConsumptionDiscreteTrigger : ReagentConsumptionEvent {
    [SerializeField]
    private float requiredCumulativeReagent = 9;
    public override void OnConsume(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
        float volume = reagentMemory.GetVolumeOf(scriptableReagent);
        reagentMemory.AddMix(scriptableReagent.GetReagent(amountProcessed));
        if (volume < requiredCumulativeReagent && reagentMemory.GetVolumeOf(scriptableReagent) >= requiredCumulativeReagent) {
            OnTrigger(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
        }
    }
    protected virtual void OnTrigger(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
    }
}
