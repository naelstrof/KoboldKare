using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DickSizeReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        k.SetDickSize(Mathf.Max(k.dickSize.Value + usedAmount * Multiplier, 0.2f));
    }
}
