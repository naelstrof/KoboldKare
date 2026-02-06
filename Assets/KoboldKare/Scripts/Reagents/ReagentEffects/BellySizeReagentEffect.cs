using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellySizeReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        if (!k.TryGetKobold(out var kobold)) {
            return;
        }
        float currentUsedAmount = Mathf.Max(k.reagentContents.Value.volume, 20f);
        k.SetBellySize(Mathf.Max(k.bellySize.Value + usedAmount * Multiplier, currentUsedAmount));
    }
}
