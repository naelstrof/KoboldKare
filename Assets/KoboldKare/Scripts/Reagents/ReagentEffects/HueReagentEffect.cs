using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HueReagentEffect : ModifyingReagentEffect {
    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        float output = (k.hue.Value + usedAmount * Multiplier) % 255f;
        if (output < 0f) {
            output += 255;
        }
        float clothingHue = (k.clothingHue.Value + usedAmount * Multiplier) % 255f;
        if (clothingHue < 0f) {
            output += 255;
        }
        k.SetHue((byte)Mathf.CeilToInt(output));
        k.SetClothingHue((byte)Mathf.CeilToInt(clothingHue));
    }
}
