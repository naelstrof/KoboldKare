using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CumInstantlyConsumption : ConsumptionDiscreteTrigger {
    protected override void OnTrigger(NetworkedKobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack, ref float energy) {
        k.SetBallSize(k.ballSize.Value + requiredCumulativeReagent);
        k.Cum();
        base.OnTrigger(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref energy);
    }

}
