using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FishNet.Object;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class BreedingMount : UsableMachine, IAnimationStationSet {
    [SerializeField] private Sprite useSprite;
    [SerializeField] private AnimationStation station;
    [SerializeField] private FluidStream stream;
    private ReadOnlyCollection<AnimationStation> stations;
    private GenericReagentContainer container;
    
    private NetworkedEntity networkedEntity;
    private void Awake() {
        // FIXME FISHNET
        //photonView.ObservedComponents.Add(container);
        List<AnimationStation> tempList = new List<AnimationStation>();
        tempList.Add(station);
        stations = tempList.AsReadOnly();
    }

    protected override void Start() {
        base.Start();
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(useSprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
            networkedEntity.type = GeneHolder.ContainerType.Mouth;
            networkedEntity.reagentContents.OnChange += OnReagentContentsChanged;
        }
    }

    private void OnDestroy() {
        if (networkedEntity) {
            networkedEntity.reagentContents.OnChange += OnReagentContentsChanged;
        }
    }

    private void OnReagentContentsChanged(ReagentContents contents, ReagentContents next, bool asServer) {
        // FIXME FISHNET
        //photonView.RPC(nameof(FireStream), RpcTarget.All);
    }

    // FIXME FISHNET
    //[PunRPC]
    private void FireStream() {
        stream.OnFire(networkedEntity);
    }

    private bool OnUseRequested(NetworkedKobold k) {
        return constructed && k.energy.Value > 0 && station.info.user == null;
    }
    private void OnUse(NetworkedKobold k) {
        k.BeginAnimation(GetComponentInParent<NetworkObject>(), 0);
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return stations;
    }
}
