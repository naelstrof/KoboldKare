using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class ReagentEffect
{
    public abstract void Apply(NetworkedKobold k, float usedAmount, ref ReagentContents addBack, ref float energy);

    public virtual void OnValidate()
    {

    }
}
