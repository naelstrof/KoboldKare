using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreastSizeReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        genes = genes.With(breastSize: Mathf.Max(genes.breastSize + usedAmount * Multiplier, 0f));
    }
}
