using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[System.Serializable]
public class LactateReagentEffect : ReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        k.photonView.RPC(nameof(Kobold.MilkRoutine), RpcTarget.All);
    }
}
