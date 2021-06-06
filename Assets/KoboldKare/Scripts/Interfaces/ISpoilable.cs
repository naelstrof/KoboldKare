using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Events;

public interface ISpoilable {
    float spoilIntensity { get; set; }
    UnityEvent onSpoilEvent { get; }
    Transform transform { get; }
}
