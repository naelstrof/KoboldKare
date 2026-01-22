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
        container = gameObject.AddComponent<GenericReagentContainer>();
        container.type = GenericReagentContainer.ContainerType.Mouth;
        // FIXME FISHNET
        //photonView.ObservedComponents.Add(container);
        container.OnChange += OnReagentContentsChanged;
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
        }
    }

    private void OnDestroy() {
        container.OnChange -= OnReagentContentsChanged;
    }

    private void OnReagentContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        // FIXME FISHNET
        //photonView.RPC(nameof(FireStream), RpcTarget.All);
    }

    // FIXME FISHNET
    //[PunRPC]
    private void FireStream() {
        stream.OnFire(container);
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
