using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreastSizeReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        k.SetBreastSize(Mathf.Max(k.breastSize.Value + usedAmount * Multiplier, 0f));
    }
}
