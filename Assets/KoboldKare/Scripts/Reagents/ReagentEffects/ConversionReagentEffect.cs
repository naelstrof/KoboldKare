using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConversionReagentEffect : ModifyingReagentEffect {
    [SerializeField, Tooltip("Reagent to convert the digested amount to.")]
    private ScriptableReagent ConvertToReagent;

    public override void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy) {
        addBack.AddMix(ConvertToReagent.GetReagent(usedAmount * Multiplier));
    }

    public override void OnValidate() {
        base.OnValidate();
        if (Multiplier <= 0f) {
            Multiplier = 0.1f;
        }
    }
}
