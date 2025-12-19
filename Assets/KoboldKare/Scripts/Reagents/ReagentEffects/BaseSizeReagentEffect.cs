using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSizeReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        k.SetBaseSize(Mathf.Max(k.baseSize.Value + usedAmount * Multiplier, 0f));
    }
}
