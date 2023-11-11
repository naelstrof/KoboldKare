using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatSizeReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        genes = genes.With(fatSize: Mathf.Max(genes.fatSize + usedAmount * Multiplier, -2f));
    }
}
