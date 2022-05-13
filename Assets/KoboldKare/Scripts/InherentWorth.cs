using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InherentWorth : MonoBehaviour, IValuedGood {
    [SerializeField]
    private float worth;
    public float GetWorth() {
        return worth;
    }
}
