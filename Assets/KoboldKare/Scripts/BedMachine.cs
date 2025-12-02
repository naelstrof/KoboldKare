using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Naelstrof.Mozzarella;
using Photon.Pun;
using SkinnedMeshDecals;
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
    void Awake() {
        readOnlyStations = stations.AsReadOnly();
        energyGrantPeriod = new WaitForSeconds(1f);
    }
    public override Sprite GetSprite(Kobold k) {
        return sleepingSprite;
    }
    public override bool CanUse(Kobold k) {
        if (k.GetEnergy() >= maxEnergy) {
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
                // FIXME FISHNET
                // k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All, photonView.ViewID, i);
                break;
            }
        }

        // FIXME FISHNET
        //photonView.RPC(nameof(Sleep), RpcTarget.All, k.photonView.ViewID);
    }
    
    // FIXME FISHNET
    //[PunRPC]
    private void Sleep(int targetID) {
        // FIXME FISHNET
        /*PhotonView view = PhotonNetwork.GetPhotonView(targetID);
        if (view != null && view.TryGetComponent(out Kobold kobold)) {
            StartCoroutine(SleepRoutine(kobold));
        }
        PhotonProfiler.LogReceive(sizeof(int));*/
    }

    private IEnumerator SleepRoutine(Kobold k) {
        // FIXME FISHNET
        /*
        bool stillSleeping = true;
        float startStimulation = k.stimulation;
        while (k != null && stillSleeping && k.GetEnergy() < maxEnergy) {
            yield return energyGrantPeriod;
            if (k.photonView.IsMine) {
                k.SetEnergyRPC(Mathf.Min(k.GetEnergy() + 0.2f, maxEnergy));
                k.stimulation = Mathf.MoveTowards(startStimulation, 0f, 1f);
            }
            stillSleeping = false;
            foreach (var station in GetAnimationStations()) {
                if (station.info.user == k) {
                    stillSleeping = true;
                }
            }
        }
        if (stillSleeping && k.photonView.IsMine) {
            k.photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
        }*/
        yield break;
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
