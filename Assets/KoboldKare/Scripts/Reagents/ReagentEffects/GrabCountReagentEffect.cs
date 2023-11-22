using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabCountReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        genes = genes.With(grabCount: (byte)Mathf.Clamp(Mathf.CeilToInt(genes.grabCount + usedAmount * Multiplier), 1, 255));
    }
}
