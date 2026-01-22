using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

[RequireComponent(typeof(CharacterControllerAnimator))]
public class KoboldAnimationUsable : MonoBehaviour {
    private NetworkedKobold selfKobold;
    private CharacterControllerAnimator animator;
    private static Collider[] colliders = new Collider[32];
    private LayerMask mask;
    private List<NetworkedKobold> koboldCache;
    
    private NetworkedEntity networkedEntity;
    
    [SerializeField] private Sprite sprite;

    void Start() {
        koboldCache = new List<NetworkedKobold>();
        mask = LayerMask.GetMask("AnimationSet");
        selfKobold = GetComponentInParent<NetworkedKobold>();
        animator = GetComponent<CharacterControllerAnimator>();
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(sprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
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

            // FIXME FISHNET
            /*float distance = Vector3.Distance(position, targetSet.photonView.transform.position);
            if (distance < bestStationDistance) {
                bestStationSet = targetSet;
                bestStationDistance = distance;
            }*/
        }
        return bestStationSet;
    }

    private bool OnUseRequested(NetworkedKobold k) {
        if (k.energy.Value == 0 || selfKobold.energy.Value == 0) {
            return false;
        }
        koboldCache.Clear();
        koboldCache.Add(selfKobold);
        koboldCache.Add(k);
        if (animator.TryGetAnimationStationSet(out IAnimationStationSet testSet)) {
            foreach (AnimationStation station in testSet.GetAnimationStations()) {
                if (station.info.user != null && station.info.user != selfKobold && station.info.user != k && station.info.user.energy.Value > 0) {
                    koboldCache.Add(station.info.user);
                }
            }
        }
        IAnimationStationSet targetSet = GetAnimationStationSet(transform.position, koboldCache.Count);
        return targetSet != null;
    }

    private void OnUse(NetworkedKobold k) {
        // FIXME FISHNET
        //selfKobold.photonView.RequestOwnership();
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
                koboldCache[i].BeginAnimation(GetComponentInParent<NetworkObject>(), i);
            }
        }
    }
}
