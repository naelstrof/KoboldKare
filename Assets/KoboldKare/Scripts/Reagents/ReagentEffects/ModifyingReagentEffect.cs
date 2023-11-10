using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class ModifyingReagentEffect : ReagentEffect
{
    public float Multiplier = 1;

    public override void OnValidate()
    {
        base.OnValidate();
        if (Multiplier == 0f)
        {
            Multiplier = 0.1f;
        }
    }
}
