using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class ReagentEffect
{
    public abstract void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy);

    public virtual void OnValidate()
    {

    }
}
