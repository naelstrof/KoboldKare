using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaturationReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        k.SetSaturation((byte)Mathf.Clamp(Mathf.CeilToInt(k.saturation.Value + usedAmount * Multiplier), 0, 255));
    }
}
