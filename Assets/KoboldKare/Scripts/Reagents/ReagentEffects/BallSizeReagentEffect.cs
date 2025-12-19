using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSizeReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        k.SetBallSize(Mathf.Max(k.ballSize.Value + usedAmount * Multiplier, 0f));
    }
}

