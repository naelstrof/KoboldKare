using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxEnergyReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        genes = genes.With(maxEnergy: (byte)Mathf.Clamp(genes.maxEnergy + usedAmount * Multiplier, 5f, 255f));
    }
}
