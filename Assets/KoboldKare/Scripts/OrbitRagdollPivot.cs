using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitRagdollPivot : OrbitCameraPivotBasic {
    private bool freeze;
    private Vector3 freezePosition;
    public void SetFreeze(bool newFreeze) {
        freeze = newFreeze;
        freezePosition = transform.position;
    }
    public override Vector3 GetPivotPosition(Quaternion camRotation) {
        return freeze ? freezePosition : base.GetPivotPosition(camRotation);
    }
}
