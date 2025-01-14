using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCameraLockedLerpTrackPivot : OrbitCameraLerpTrackBasicPivot {
    public override OrbitCameraData GetData(Camera cam) {
        var data = base.GetData(cam);
        data.rotation = Quaternion.identity;
        return data;
    }
}
