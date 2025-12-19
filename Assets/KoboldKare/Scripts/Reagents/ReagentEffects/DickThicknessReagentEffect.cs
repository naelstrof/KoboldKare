using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DickThicknessReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        k.SetDickThickness(Mathf.Clamp(k.dickThickness.Value + usedAmount * Multiplier, 0f, 1f));
    }
}
