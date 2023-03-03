using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCameraLockedLerpTrackPivot : OrbitCameraLerpTrackBasicPivot {
    public override Quaternion GetRotation(Quaternion camRotation) {
        return Quaternion.identity;
    }
}
