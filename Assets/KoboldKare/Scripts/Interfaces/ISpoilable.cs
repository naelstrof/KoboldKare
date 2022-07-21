using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Events;

public interface ISpoilable {
    void OnSpoil();
    Transform transform { get; }
}
