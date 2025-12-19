using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrightnessReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        k.SetBrightness((byte)Mathf.Clamp(Mathf.CeilToInt(k.brightness.Value + usedAmount * Multiplier), 0, 255));
    }
}
