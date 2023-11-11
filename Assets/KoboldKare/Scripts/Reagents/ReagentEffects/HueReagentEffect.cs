using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HueReagentEffect : ModifyingReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        float output = (genes.hue + usedAmount * Multiplier) % 255f;
        if (output < 0f)
        {
            output += 255;
        }
        genes = genes.With(hue: (byte)Mathf.CeilToInt(output));
    }
}
