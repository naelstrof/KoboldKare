using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using UnityEngine;

public class KoboldPenetrableListener : PenetrableListener {
    [SerializeField] private Kobold targetKobold;
    private float oldDepth = 0f;
    
    protected override void OnPenetrationDepthChange(float newDepth) {
        base.OnPenetrationDepthChange(newDepth);
        float diff = newDepth - oldDepth;
        targetKobold.AddStimulation(Mathf.Abs(diff));
        oldDepth = newDepth;
    }

    public override void AssertValid() {
        base.AssertValid();
        if (targetKobold == null) {
            throw new PenetrableListenerValidationException($"targetKobold is null on {this}");
        }
    }

    public override void NotifyPenetration(Penetrable penetrable, Penetrator penetrator, float worldSpaceDistanceToPenisRoot, Penetrable.SetClipDistanceAction clipAction) {
        NotifyPenetrationGDO(penetrable, penetrator, worldSpaceDistanceToPenisRoot, clipAction, PenData.Depth );
    }
}
