using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FishNet.Object;
using UnityEngine;
using Vilar.AnimationStation;

public class BedMachine : UsableMachine, IAnimationStationSet {
    [SerializeField]
    private Sprite sleepingSprite;
    [SerializeField]
    private List<AnimationStation> stations;
    [SerializeField]
    private float maxEnergy = 3f;
    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    private WaitForSeconds energyGrantPeriod;
    private NetworkedEntity networkedEntity;
    void Awake() {
        readOnlyStations = stations.AsReadOnly();
        energyGrantPeriod = new WaitForSeconds(1f);
    }

    protected override void Start() {
        base.Start();
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        networkedEntity.SetSprite(sleepingSprite);
        networkedEntity.useRequested += OnUseRequested;
        networkedEntity.used += OnUse;
    }

    private bool OnUseRequested(NetworkedKobold k) {
        if (k.energy.Value >= maxEnergy) {
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
        if (!k.TryGetKobold(out var kobold)) {
            return;
        }
        for (int i = 0; i < stations.Count; i++) {
            if (stations[i].info.user == null) {
                k.BeginAnimation(GetComponent<NetworkObject>(), i);
                break;
            }
        }
        StartCoroutine(SleepRoutine(k));
    }

    private IEnumerator SleepRoutine(NetworkedKobold k) {
        bool stillSleeping = true;
        while (k != null && stillSleeping && k.energy.Value < maxEnergy) {
            yield return energyGrantPeriod;
            if (!k.IsOwner) {
                continue;
            }
            k.SetEnergy(Mathf.Min(k.energy.Value + 0.2f, maxEnergy));
            stillSleeping = false;
            foreach (var station in GetAnimationStations()) {
                if (station.info.user == k) {
                    stillSleeping = true;
                }
            }
        }
        if (stillSleeping && k.IsOwner) {
            k.StopAnimation();
        }
        yield break;
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
