using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomConsumptionDiscreteTrigger : ConsumptionDiscreteTrigger {
    [Header("Effect Strength based off of \"Required Cumulative Reagent\" value")]
    [Space(10f)]
    [SerializeField, SerializeReference, SerializeReferenceButton]
    private List<ReagentEffect> Effects;

    protected override void OnTrigger(NetworkedKobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack,  ref float energy) {
        foreach (var Effect in Effects) {
            Effect.Apply(k, requiredCumulativeReagent, ref addBack, ref energy);
        }
        base.OnTrigger(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack,  ref energy);
    }

    public override void OnValidate() {
        base.OnValidate();
        foreach (var Effect in Effects)
        {
            Effect.OnValidate();
        }
    }
}
