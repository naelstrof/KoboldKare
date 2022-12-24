using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class BreedingStation : UsableMachine, IAnimationStationSet {
    [SerializeField]
    private Sprite breedingSprite;
    [SerializeField]
    private List<AnimationStation> animationStations;
    private ReadOnlyCollection<AnimationStation> readOnlyStations;

    void Awake() {
        readOnlyStations = animationStations.AsReadOnly();
    }

    public override Sprite GetSprite(Kobold k) {
        return breedingSprite;
    }

    public override bool CanUse(Kobold k) {
        if (!constructed) {
            return false;
        }

        foreach (AnimationStation station in animationStations) {
            if (station.info.user == null) {
                return true;
            }
        }
        return false;
    }

    public override void LocalUse(Kobold k) {
        photonView.RequestOwnership();
        for (int i = 0; i < animationStations.Count; i++) {
            if (animationStations[i].info.user == null) {
                k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All,
                    photonView.ViewID, i);
                break;
            }
        }

    }
    
    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
