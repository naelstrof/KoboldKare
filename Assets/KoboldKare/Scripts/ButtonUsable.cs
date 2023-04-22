using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonUsable : GenericUsable
{
    [SerializeField] public UnityEvent onUse;
    public override void Use()
    {
        base.Use();
        onUse.Invoke();
    }
}
