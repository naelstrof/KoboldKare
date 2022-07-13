using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledItem : MonoBehaviour {
    public delegate void OnResetAction();
    public event OnResetAction resetTrigger;
    public virtual void Reset() {
        resetTrigger?.Invoke();
    }
}
