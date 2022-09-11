using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[System.Serializable]
public class ConsumptionDiscreteTrigger : ReagentConsumptionEvent {
    [SerializeField]
    protected float requiredCumulativeReagent = 5;
    [SerializeField]
    protected int maximumTriggerCount = 1;
    public override void OnConsume(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
        float volume = reagentMemory.GetVolumeOf(scriptableReagent);
        reagentMemory.AddMix(scriptableReagent.GetReagent(amountProcessed));
        float newVolume = reagentMemory.GetVolumeOf(scriptableReagent);
        float diff = newVolume - volume;
        float start = Mathf.Repeat(volume, requiredCumulativeReagent);
        int triggerCount = Mathf.FloorToInt((start + diff) / requiredCumulativeReagent);
        if (maximumTriggerCount != -1 && !(newVolume < (maximumTriggerCount + 1) * requiredCumulativeReagent)) {
            return;
        }
        for (int i = 0; i < triggerCount; i++) {
            OnTrigger(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
        }
    }
    protected virtual void OnTrigger(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
    }
}
