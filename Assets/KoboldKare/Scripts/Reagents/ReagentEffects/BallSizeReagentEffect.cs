using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSizeReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        genes = genes.With(ballSize: Mathf.Max(genes.ballSize + usedAmount * Multiplier, 0f));
    }
}

