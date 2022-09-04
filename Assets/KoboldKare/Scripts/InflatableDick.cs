using System.Collections;
using System.Collections.Generic;
using Naelstrof.Inflatable;
using PenetrationTech;
using UnityEngine;

[System.Serializable]
public class InflatableDick : InflatableListener {
    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private Transform targetJiggleRoot;
    [SerializeField]
    private Penetrator targetPenetrator;
    private Vector3 startScale;
    private Vector3 startJiggleScale;
    private float dickThicknessRatio = 0.5f;

    public void SetDickThickness(float dickThickness) {
        dickThicknessRatio = dickThickness;
    }

    public void SetTransform(Transform newTargetTransform) {
        targetTransform = newTargetTransform;
    }

    public override void OnEnable() {
        startScale = targetTransform.localScale;
        if (targetJiggleRoot != null) {
            startJiggleScale = targetJiggleRoot.localScale;
        }
    }

    public override void OnSizeChanged(float newSize) {
        Vector3 worldForward = targetPenetrator.GetWorldForward();
        Vector3 localForward = targetPenetrator.GetRootBone().InverseTransformDirection(worldForward);
        Vector3 localOthers = Vector3.one - localForward;
        Vector3 squishedStartScale =
            startScale + Vector3.Lerp(-localForward * 0.5f, localForward * 0.5f, dickThicknessRatio)
                       + Vector3.Lerp(localOthers * 0.25f, -localOthers * 0.25f, dickThicknessRatio);
        
        targetTransform.localScale = squishedStartScale*Mathf.Max(newSize,0.05f);
        if (targetJiggleRoot != null) {
            // No need to shrink the jiggle
            targetJiggleRoot.localScale =
                startJiggleScale * Mathf.Max(newSize, 1f);
        }

        targetPenetrator.SetPenetrationMarginOfError(Mathf.Min(0.1f+1f/newSize, 1f));
    }
}
