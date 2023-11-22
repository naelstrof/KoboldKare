using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellySizeReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        float CurrentUsedAmount = Mathf.Max(k.bellyContainer.volume, 20f);
        genes = genes.With(bellySize: Mathf.Max(genes.bellySize + usedAmount * Multiplier, CurrentUsedAmount));
    }
}
