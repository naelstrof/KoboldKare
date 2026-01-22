using System.Collections.Generic;
using System.Collections.ObjectModel;
using FishNet.Object;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class BreedingStation : UsableMachine, IAnimationStationSet {
    [SerializeField]
    private Sprite breedingSprite;
    [SerializeField]
    private List<AnimationStation> animationStations;
    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    
    private NetworkedEntity networkedEntity;

    void Awake() {
        readOnlyStations = animationStations.AsReadOnly();
    }

    protected override void Start() {
        base.Start();
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(breedingSprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    private bool OnUseRequested(NetworkedKobold k) {
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

    private void OnUse(NetworkedKobold k) {
        for (int i = 0; i < animationStations.Count; i++) {
            if (animationStations[i].info.user == null) {
                k.BeginAnimation(GetComponentInParent<NetworkObject>(), i);
                break;
            }
        }
    }
    
    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
