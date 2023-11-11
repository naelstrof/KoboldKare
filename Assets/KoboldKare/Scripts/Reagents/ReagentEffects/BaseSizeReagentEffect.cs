using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSizeReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        genes = genes.With(baseSize: Mathf.Max(genes.baseSize + usedAmount * Multiplier, 0f));
    }
}
