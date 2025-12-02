using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class BreedingMount : UsableMachine, IAnimationStationSet {
    [SerializeField] private Sprite useSprite;
    [SerializeField] private AnimationStation station;
    [SerializeField] private FluidStream stream;
    private ReadOnlyCollection<AnimationStation> stations;
    private GenericReagentContainer container;
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

    public override bool CanUse(Kobold k) {
        return constructed && k.GetEnergy() > 0 && station.info.user == null;
    }
    public override Sprite GetSprite(Kobold k) {
        return useSprite;
    }
    public override void LocalUse(Kobold k) {
        // FIXME FISHNET
        //k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All, photonView.ViewID, 0);
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return stations;
    }
}
