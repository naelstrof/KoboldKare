using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class MushroomConsumption : ReagentConsumptionMetabolize {
    public override void OnConsume(NetworkedKobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack, ref float energy) {
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref energy);
        k.SetBaseSize(Mathf.Max(k.baseSize.Value-amountProcessed * 0.2f, 0f));
        k.SetBallSize(Mathf.Max(k.ballSize.Value-amountProcessed * 0.2f,0f));
        k.SetDickSize(Mathf.Max(k.dickSize.Value-amountProcessed * 0.2f, 0.2f));
        k.SetFatSize(Mathf.Max(k.fatSize.Value-amountProcessed * 0.2f,-2f));
        k.SetBreastSize(Mathf.Max(k.breastSize.Value-amountProcessed * 0.2f,0f));
        k.SetSaturation((byte)Mathf.Clamp(k.saturation.Value-(byte)(Mathf.CeilToInt(amountProcessed*6f)), 0, 255));
    }
}
