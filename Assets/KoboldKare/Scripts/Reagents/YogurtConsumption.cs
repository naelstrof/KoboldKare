using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class YogurtConsumption : ReagentConsumptionEvent {
    [SerializeField] private float maxPerGeneration = 30f;
    public override void OnConsume(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
        float allowedMix = maxPerGeneration - reagentMemory.GetVolumeOf(scriptableReagent);
        float mixAmount = Mathf.Min(amountProcessed, allowedMix);
        reagentMemory.AddMix(scriptableReagent.GetReagent(mixAmount));
        genes.metabolizeCapacitySize += mixAmount;
        amountProcessed = mixAmount;
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
    }
}
