using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetabolizeCapacityReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        genes = genes.With(metabolizeCapacitySize: genes.metabolizeCapacitySize + usedAmount * Multiplier);
    }
}

