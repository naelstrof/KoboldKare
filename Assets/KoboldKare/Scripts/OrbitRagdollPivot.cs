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
    public override OrbitCameraData GetData(Camera cam) {
        var data = base.GetData(cam);
        data.position = freeze ? freezePosition : data.position;
        return data;
    }
}
