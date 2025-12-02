using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[System.Serializable]
public class CumReagentEffect : ReagentEffect
{
    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy) {
        // FIXME FISHNET
        //k.photonView.RPC(nameof(Kobold.Cum), RpcTarget.All);
    }
}
