using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CumConsumption : ReagentConsumptionEvent {
    [SerializeField]
    ScriptableReagent eggReagent;
    public override void OnConsume(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
        addBack.AddMix(eggReagent.GetReagent(amountProcessed * 4f));
    }
}
