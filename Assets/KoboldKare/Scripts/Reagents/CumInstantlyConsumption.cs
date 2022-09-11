using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CumInstantlyConsumption : ConsumptionDiscreteTrigger {
    protected override void OnTrigger(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
        genes = genes.With(ballSize: genes.ballSize + requiredCumulativeReagent);
        k.photonView.RPC(nameof(Kobold.Cum), RpcTarget.All);
        base.OnTrigger(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
    }

}
