using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

[RequireComponent(typeof(CharacterControllerAnimator))]
public class KoboldAnimationUsable : GenericUsable {
    private Kobold selfKobold;
    private CharacterControllerAnimator animator;
    private static Collider[] colliders = new Collider[32];
    private LayerMask mask;
    private List<Kobold> koboldCache;
    [SerializeField] private Sprite sprite;
    public override Sprite GetSprite(Kobold k) {
        return sprite;
    }

    void Start() {
        koboldCache = new List<Kobold>();
        mask = LayerMask.GetMask("AnimationSet");
        selfKobold = GetComponent<Kobold>();
        animator = GetComponent<CharacterControllerAnimator>();
    }
    
    private IAnimationStationSet GetAnimationStationSet(Vector3 position, int neededSlots) {
        //Already animating
        if (animator.TryGetAnimationStationSet(out IAnimationStationSet testSet) && testSet.GetAnimationStations().Count >= neededSlots ) {
            return testSet;
        }
        int hits = Physics.OverlapSphereNonAlloc(position, 5f, colliders, mask, QueryTriggerInteraction.Collide);
        IAnimationStationSet bestStationSet = null;
        float bestStationDistance = float.MaxValue;
        for (int i = 0; i < hits; i++) {
            IAnimationStationSet targetSet = colliders[i].GetComponentInParent<IAnimationStationSet>();
            if (targetSet.GetAnimationStations().Count < neededSlots) {
                continue;
            }

            float distance = Vector3.Distance(position, targetSet.photonView.transform.position);
            if (distance < bestStationDistance) {
                bestStationSet = targetSet;
                bestStationDistance = distance;
            }
        }
        return bestStationSet;
    }

    public override bool CanUse(Kobold k) {
        if (k.GetEnergy() == 0 || selfKobold.GetEnergy() == 0) {
            return false;
        }
        koboldCache.Clear();
        koboldCache.Add(selfKobold);
        koboldCache.Add(k);
        if (animator.TryGetAnimationStationSet(out IAnimationStationSet testSet)) {
            foreach (AnimationStation station in testSet.GetAnimationStations()) {
                if (station.info.user != null && station.info.user != selfKobold && station.info.user != k && station.info.user.GetEnergy() > 0) {
                    koboldCache.Add(station.info.user);
                }
            }
        }
        IAnimationStationSet targetSet = GetAnimationStationSet(transform.position, koboldCache.Count);
        return targetSet != null;
    }

    public override void LocalUse(Kobold k) {
        selfKobold.photonView.RequestOwnership();
        koboldCache.Clear();
        koboldCache.Add(selfKobold);
        koboldCache.Add(k);
        if (animator.TryGetAnimationStationSet(out IAnimationStationSet testSet)) {
            foreach (AnimationStation station in testSet.GetAnimationStations()) {
                if (station.info.user != null && station.info.user != selfKobold && station.info.user != k) {
                    koboldCache.Add(station.info.user);
                }
            }
        }
        
        IAnimationStationSet targetSet = GetAnimationStationSet(transform.position, koboldCache.Count);
        if (targetSet != null) {
            for (int i = 0; i < koboldCache.Count; i++) {
                koboldCache[i].photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All,
                    new object[] { targetSet.photonView.ViewID, i });
            }
        }
    }
}
