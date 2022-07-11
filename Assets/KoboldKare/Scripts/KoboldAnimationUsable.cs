using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

[RequireComponent(typeof(CharacterControllerAnimator))]
public class KoboldAnimationUsable : GenericUsable {
    private Kobold selfKobold;
    private CharacterControllerAnimator animator;
    private static Collider[] colliders = new Collider[32];
    private LayerMask mask;
    [SerializeField] private Sprite sprite;
    public override Sprite GetSprite(Kobold k) {
        return sprite;
    }

    void Start() {
        mask = LayerMask.GetMask("AnimationSet");
        selfKobold = GetComponent<Kobold>();
        animator = GetComponent<CharacterControllerAnimator>();
    }

    private AnimationStation GetAvailableStation(AnimationStationSet set, int index) {
        foreach (var station in set.GetAnimationStations()) {
            if (station.info.user == null) {
                if (index == 0) {
                    return station;
                } else {
                    index--;
                }
            } 
        }
        return null;
    }

    private AnimationStation GetAvailableStation(Kobold kobold, int index) {
        if (animator.TryGetAnimationStationSet(out AnimationStationSet testSet)) {
            // Already animating!
            foreach (var station in testSet.GetAnimationStations()) {
                if (station.info.user == kobold) {
                    return station;
                }
            }

            AnimationStation test = GetAvailableStation(testSet, index);
            if (test != null) {
                return test;
            }
        }
        
        int hits = Physics.OverlapSphereNonAlloc(transform.position, 5f, colliders, mask, QueryTriggerInteraction.Collide);
        AnimationStation bestStation = null;
        float bestSetScore = float.MaxValue;
        for (int i = 0; i < hits; i++) {
            AnimationStationSet targetSet = colliders[i].GetComponentInParent<AnimationStationSet>();
            AnimationStation testStation = GetAvailableStation(targetSet, index);
            float distance = Vector3.Distance(targetSet.transform.position, transform.position);
            if (testStation != null && distance < bestSetScore) {
                bestStation = testStation;
                bestSetScore = distance;
            }
        }

        return bestStation;
    }

    public override bool CanUse(Kobold k) {
        AnimationStation aStation = GetAvailableStation(selfKobold, 0);
        AnimationStation bStation = GetAvailableStation(k, 1);
        return aStation != null && bStation != null;
    }

    public override void LocalUse(Kobold k) {
        selfKobold.photonView.RequestOwnership();
        AnimationStation aStation = GetAvailableStation(selfKobold, 0);
        if (aStation != null) {
            AnimationStationSet aSet = aStation.GetComponentInParent<AnimationStationSet>();
            photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All,
                new object[] { aSet.photonView.ViewID, aSet.GetAnimationStations().IndexOf(aStation) });
        }

        AnimationStation bStation = GetAvailableStation(k, 1);
        if (bStation != null) {
            AnimationStationSet bSet = bStation.GetComponentInParent<AnimationStationSet>();
            k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All,
                new object[] { bSet.photonView.ViewID, bSet.GetAnimationStations().IndexOf(bStation) });
        }
    }
}
