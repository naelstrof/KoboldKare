using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class MilkedInstantlyConsumption : ConsumptionDiscreteTrigger {
    protected override void OnTrigger(NetworkedKobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref float energy) {
        k.SetBreastSize(k.breastSize.Value + requiredCumulativeReagent);
        k.Lactate();
        base.OnTrigger(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref energy);
    }
}
