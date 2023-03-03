using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using UnityEngine;

public class KoboldJawOpenListener : SimpleBlendshapeListener {
    [SerializeField] private Animator koboldAnimator;
    private CharacterControllerAnimator controllerAnimator;

    private static readonly int LookUp = Animator.StringToHash("LookUp");
    private bool lastWork;
    private float jawVelocity;

    protected override void OnPenetrationDepthChange(float newDepth) {
        base.OnPenetrationDepthChange(newDepth);
        bool work = newDepth > 0f;
        if (work == lastWork) return;
        koboldAnimator.SetBool(LookUp, work);
        controllerAnimator.SetLookEnabled(!work);
        lastWork = work;
    }

    public override void OnEnable(Penetrable p) {
        base.OnEnable(p);
        controllerAnimator = koboldAnimator.GetComponentInParent<CharacterControllerAnimator>();
    }

    public override void AssertValid() {
        base.AssertValid();

        if (koboldAnimator == null) {
            throw new PenetrableListenerValidationException($"KoboldAnimator is null on {this}");
        }
    }

    public override void OnDrawGizmosSelected(Penetrable p) {
        base.OnDrawGizmosSelected(p);
        #if UNITY_EDITOR
            CatmullSpline path = p.GetSplinePath();
            Vector3 position = path.GetPositionFromT(t);
            Vector3 tangent = path.GetVelocityFromT(t).normalized;
            Vector3 normal = path.GetBinormalFromT(t).normalized;
            
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(position, tangent, 0.025f);
        #endif
    }

    public override void NotifyPenetration(Penetrable penetrable, Penetrator penetrator, float worldSpaceDistanceToPenisRoot, Penetrable.SetClipDistanceAction clipAction) {
        NotifyPenetrationGDO(penetrable, penetrator, worldSpaceDistanceToPenisRoot, clipAction, PenData.Depth | PenData.Girth);
    }
}
