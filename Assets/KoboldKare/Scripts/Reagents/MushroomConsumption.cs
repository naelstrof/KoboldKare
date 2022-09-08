using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class MushroomConsumption : ReagentConsumptionMetabolize {
    public override void OnConsume(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
        genes.baseSize = Mathf.Max(genes.baseSize-amountProcessed * 0.2f, 0f);
        genes.ballSize = Mathf.Max(genes.ballSize-amountProcessed * 0.2f,0f);
        genes.dickSize = Mathf.Max(genes.dickSize-amountProcessed * 0.2f, 0.2f);
        genes.fatSize = Mathf.Max(genes.fatSize-amountProcessed * 0.2f,-2f);
        genes.breastSize = Mathf.Max(genes.breastSize-amountProcessed * 0.2f,0f);
        genes.saturation = (byte)Mathf.Clamp(genes.saturation-(byte)(Mathf.CeilToInt(amountProcessed*6f)), 0, 255);
    }
}
