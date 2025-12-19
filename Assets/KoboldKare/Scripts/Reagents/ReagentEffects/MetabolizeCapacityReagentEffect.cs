using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetabolizeCapacityReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        if (!k.TryGetKobold(out var kobold)) {
            return;
        }
        float currentUsedAmount = Mathf.Max(kobold.metabolizedContents.volume, 20f);
        k.SetMetabolizeCapacitySize(Mathf.Max(k.metabolizeCapacitySize.Value + usedAmount * Multiplier, currentUsedAmount));
    }
}

