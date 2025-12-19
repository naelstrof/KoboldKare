using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatSizeReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        k.SetFatSize(Mathf.Max(k.fatSize.Value + usedAmount * Multiplier, -2f));
    }
}
