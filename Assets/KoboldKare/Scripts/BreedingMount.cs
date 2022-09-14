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
        photonView.ObservedComponents.Add(container);
        container.OnChange.AddListener(OnReagentContentsChanged);
        List<AnimationStation> tempList = new List<AnimationStation>();
        tempList.Add(station);
        stations = tempList.AsReadOnly();
    }

    private void OnReagentContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        photonView.RPC(nameof(FireStream), RpcTarget.All);
    }

    [PunRPC]
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
        k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All, photonView.ViewID, 0);
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return stations;
    }
}
