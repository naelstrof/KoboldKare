using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FishNet.Object;
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

    private NetworkedEntity networkedEntity;

    void Awake() {
        readOnlyStations = stations.AsReadOnly();
        
        
        // FIXME FISHNET
        /*photonView.ObservedComponents.Add(container);*/
    }

    protected override void Start() {
        base.Start();
        networkedEntity = GetComponentInParent<NetworkedKobold>();
        if (networkedEntity) {
            networkedEntity.SetSprite(milkingSprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
            networkedEntity.type = GeneHolder.ContainerType.Mouth;
            networkedEntity.reagentContents.OnChange += OnReagentContainerChangedEvent;
        }
    }

    private void OnReagentContainerChangedEvent(ReagentContents contents, ReagentContents next, bool asServer) {
        stream.OnFire(networkedEntity);
    }
    
    private bool OnUseRequested(NetworkedKobold k) {
        if (k.energy.Value < 1f || !constructed) {
            return false;
        }
        foreach (var station in stations) {
            if (station.info.user == null) {
                return true;
            }
        }
        return false;
    }

    private void OnUse(NetworkedKobold k) {
        for (int i = 0; i < stations.Count; i++) {
            if (stations[i].info.user == null) {
                k.BeginAnimation(GetComponentInParent<NetworkObject>(), i);
                break;
            }
        }
        StopAllCoroutines();
        StartCoroutine(WaitThenMilk());
    }
    private IEnumerator WaitThenMilk() {
        yield return new WaitForSeconds(6f);
        // FIXME FISHNET
        /*if (!photonView.IsMine) {
            yield break;
        }*/
        // Validate that we have two characters with energy that have been animating for 5 seconds
        for (int i = 0; i < stations.Count; i++) {
            if (stations[i].info.user == null || stations[i].info.user.energy.Value <= 0) {
                yield break;
            }
        }
        // Consume their energy!
        for (int i = 0; i < stations.Count; i++) {
            if (stations[i].info.user.energy.Value < 1) { // TryConsume
                yield break;
            }
        }
        
        // FIXME FISHNET
        /* stations[0].info.user.photonView.RPC(nameof(Kobold.MilkRoutine), RpcTarget.All);*/
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
