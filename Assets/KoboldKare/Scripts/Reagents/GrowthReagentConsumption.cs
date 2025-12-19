using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class GrowthReagentConsumption : ReagentConsumptionMetabolize {
    public override void OnConsume(NetworkedKobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack, ref float energy) {
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref energy);
        k.SetBaseSize(k.baseSize.Value + amountProcessed);
    }
}
