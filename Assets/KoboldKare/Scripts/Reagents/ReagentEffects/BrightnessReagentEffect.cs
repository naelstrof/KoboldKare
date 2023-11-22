using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrightnessReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        genes = genes.With(brightness: (byte)Mathf.Clamp(Mathf.CeilToInt(genes.brightness + usedAmount * Multiplier), 0, 255));
    }
}
