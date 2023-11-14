using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DickThicknessReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        genes = genes.With(dickThickness: Mathf.Clamp(genes.dickThickness + usedAmount * Multiplier, 0f, 1f));
    }
}
