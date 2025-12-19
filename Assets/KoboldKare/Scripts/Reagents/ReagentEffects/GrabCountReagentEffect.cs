using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabCountReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        k.SetGrabCount((byte)Mathf.Clamp(Mathf.CeilToInt(k.grabCount.Value + usedAmount * Multiplier), 1, 255));
    }
}
