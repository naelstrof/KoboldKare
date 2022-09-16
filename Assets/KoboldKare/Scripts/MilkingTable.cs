using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Naelstrof.Mozzarella;
using Photon.Pun;
using SkinnedMeshDecals;
using UnityEngine;
using Vilar.AnimationStation;

public class MilkingTable : UsableMachine, IAnimationStationSet {
    [SerializeField]
    private Sprite milkingSprite;
    [SerializeField]
    private List<AnimationStation> stations;
    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    [SerializeField]
    private FluidStream stream;

    private GenericReagentContainer container;
    void Awake() {
        readOnlyStations = stations.AsReadOnly();
        container = gameObject.AddComponent<GenericReagentContainer>();
        container.type = GenericReagentContainer.ContainerType.Mouth;
        container.OnChange.AddListener(OnReagentContainerChangedEvent);
        photonView.ObservedComponents.Add(container);
    }
    private void OnReagentContainerChangedEvent(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        stream.OnFire(container);
    }
    public override Sprite GetSprite(Kobold k) {
        return milkingSprite;
    }
    public override bool CanUse(Kobold k) {
        if (k.GetEnergy() < 1f || !constructed) {
            return false;
        }
        foreach (var station in stations) {
            if (station.info.user == null) {
                return true;
            }
        }
        return false;
    }

    public override void LocalUse(Kobold k) {
        for (int i = 0; i < stations.Count; i++) {
            if (stations[i].info.user == null) {
                k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All, photonView.ViewID, i);
                break;
            }
        }
        base.LocalUse(k);
    }
    public override void Use() {
        StopAllCoroutines();
        StartCoroutine(WaitThenMilk());
    }
    private IEnumerator WaitThenMilk() {
        yield return new WaitForSeconds(6f);
        if (!photonView.IsMine) {
            yield break;
        }
        // Validate that we have two characters with energy that have been animating for 5 seconds
        for (int i = 0; i < stations.Count; i++) {
            if (stations[i].info.user == null || stations[i].info.user.GetEnergy() <= 0) {
                yield break;
            }
        }
        // Consume their energy!
        for (int i = 0; i < stations.Count; i++) {
            if (!stations[i].info.user.TryConsumeEnergy(1)) {
                yield break;
            }
        }
        stations[0].info.user.photonView.RPC(nameof(Kobold.MilkRoutine), RpcTarget.All);
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
