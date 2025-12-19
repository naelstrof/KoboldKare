using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomReagentConsumptionMetabolize : ReagentConsumptionMetabolize
{
    [Header("Effect Strength based off of metabolized amount.")]
    [Space(10f)]
    [SerializeField, SerializeReference, SerializeReferenceButton]
    private List<ModifyingReagentEffect> Effects;

    public override void OnConsume(NetworkedKobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack, ref float energy) {
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref energy);
        foreach (var effect in Effects) {
            effect.Apply(k, amountProcessed, ref addBack, ref energy);
        }
    }

    public override void OnValidate() {
        base.OnValidate();
        foreach (var effect in Effects) {
            effect.OnValidate();
        }
    }
}
