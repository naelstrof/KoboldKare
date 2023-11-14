using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetabolizeCapacityReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        float CurrentUsedAmount = Mathf.Max(k.metabolizedContents.volume, 20f);
        genes = genes.With(metabolizeCapacitySize: Mathf.Max(genes.metabolizeCapacitySize + usedAmount * Multiplier, CurrentUsedAmount));
    }
}

