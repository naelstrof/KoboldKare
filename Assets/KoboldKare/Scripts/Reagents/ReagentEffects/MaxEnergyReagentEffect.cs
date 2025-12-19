using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxEnergyReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        k.SetMaxEnergy((byte)Mathf.Clamp(k.maxEnergy.Value + usedAmount * Multiplier, 5f, 255f));
    }
}
