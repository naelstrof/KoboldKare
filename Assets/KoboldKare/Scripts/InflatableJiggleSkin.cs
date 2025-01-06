using System.Collections;
using System.Collections.Generic;
using JigglePhysics;
using Naelstrof.Inflatable;
using UnityEngine;

[System.Serializable]
public class InflatableJiggleSkin : InflatableListener {
    [SerializeField]
    private JiggleSkin targetJiggleSkin;
    [SerializeField]
    private Transform targetTransform;

    private JiggleSkin.JiggleZone targetZone;
    private float defaultRadius;
    public override void OnEnable() {
        base.OnEnable();
        foreach (var jiggleRig in targetJiggleSkin.jiggleZones) {
            if (jiggleRig.GetRootTransform() == targetTransform) {
                targetZone = jiggleRig;
            }
        }
        defaultRadius = targetZone.radius;
    }

    public override void OnSizeChanged(float newSize) {
        base.OnSizeChanged(newSize);
        targetZone.radius = defaultRadius + newSize*0.75f;
    }
}
