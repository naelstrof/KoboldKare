using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CustomReagentConsumptionEvent : ReagentConsumptionEvent
{
    [Header("Effect Strength based off of digested amount.")]
    [Space(10f)]
    [SerializeField, SerializeReference, SerializeReferenceButton]
    private List<ModifyingReagentEffect> Effects;

    public override void OnConsume(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy)
    {
        foreach (var Effect in Effects)
        {
            Effect.Apply(k, amountProcessed, ref genes, ref addBack, ref energy);
        }
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
    }

    public override void OnValidate()
    {
        base.OnValidate();
        foreach (var Effect in Effects)
        {
            Effect.OnValidate();
        }
    }
}
