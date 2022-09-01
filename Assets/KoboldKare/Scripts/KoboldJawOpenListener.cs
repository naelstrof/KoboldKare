using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using UnityEngine;

public class KoboldJawOpenListener : PenetrableListener {
    [SerializeField] private Transform jawTransform;
    [SerializeField] private Vector3 jawOpenDirection;
    [SerializeField] private Animator koboldAnimator;
    [SerializeField] private CharacterControllerAnimator controllerAnimator;

    private Penetrable currentPenetrable;
    private static readonly int LookUp = Animator.StringToHash("LookUp");
    private Vector3 startingLocalPosition;

    protected override void OnPenetrationDepthChange(float newDepth) {
        base.OnPenetrationDepthChange(newDepth);
        bool work = newDepth > currentPenetrable.GetSplinePath().GetDistanceFromTime(GetT(currentPenetrable));
        koboldAnimator.SetBool(LookUp, work);
        controllerAnimator.SetLookEnabled(!work);
    }

    protected override void OnPenetrationGirthRadiusChange(float newGirthRadius) {
        base.OnPenetrationGirthRadiusChange(newGirthRadius);
        jawTransform.localPosition = startingLocalPosition + jawOpenDirection*newGirthRadius;
    }

    public override void OnEnable(Penetrable p) {
        base.OnEnable(p);
        startingLocalPosition = jawTransform.localPosition;
        currentPenetrable = p;
    }

    public override void AssertValid() {
        base.AssertValid();
        if (jawTransform == null) {
            throw new PenetrableListenerValidationException($"jawTransform is null on {this}");
        }

        if (koboldAnimator == null) {
            throw new PenetrableListenerValidationException($"KoboldAnimator is null on {this}");
        }
        if (controllerAnimator == null) {
            throw new PenetrableListenerValidationException($"controllerAnimator is null on {this}");
        }
    }

    public override void NotifyPenetration(Penetrable penetrable, Penetrator penetrator, float worldSpaceDistanceToPenisRoot, Penetrable.SetClipDistanceAction clipAction) {
        NotifyPenetrationGDO(penetrable, penetrator, worldSpaceDistanceToPenisRoot, clipAction, PenData.Depth | PenData.Girth);
    }
}
