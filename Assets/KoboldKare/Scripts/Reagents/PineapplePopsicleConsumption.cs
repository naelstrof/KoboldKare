using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class PineapplePopsicleConsumption : ReagentConsumptionEvent {
    [SerializeField]
    private float reactionRequirement = 9f;
    public override void OnConsume(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
        base.OnConsume(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
        reagentMemory.AddMix(scriptableReagent.GetReagent(amountProcessed));
        float reagentVolume = reagentMemory.GetVolumeOf(scriptableReagent);
        if (k.photonView.IsMine && reagentVolume > reactionRequirement) {
            genes = genes.With(ballSize: genes.ballSize + reactionRequirement);
            k.photonView.RPC(nameof(Kobold.Cum), RpcTarget.All);
            reagentMemory.OverrideReagent(ReagentDatabase.GetID(scriptableReagent), reagentVolume-reactionRequirement);
        }
    }

}
