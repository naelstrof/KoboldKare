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
                k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All,
                    photonView.ViewID, i);
                break;
            }
        }

        photonView.RPC(nameof(Sleep), RpcTarget.All, k.photonView.ViewID);
        //base.LocalUse(k);
    }
    //public override void Use() {
        //StopAllCoroutines();
        //StartCoroutine(WaitThenMilk());
    //}
    [PunRPC]
    private void Sleep(int targetID) {
        PhotonView view = PhotonNetwork.GetPhotonView(targetID);
        if (view != null && view.TryGetComponent(out Kobold kobold)) {
            StartCoroutine(SleepRoutine(kobold));
        }
        PhotonProfiler.LogReceive(sizeof(int));
    }

    private IEnumerator SleepRoutine(Kobold k) {
        bool stillSleeping = true;
        float startStimulation = k.stimulation;
        while (k != null && stillSleeping && k.GetEnergy() < maxEnergy) {
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
            yield return energyGrantPeriod;
        }
        if (stillSleeping && k.photonView.IsMine) {
            k.photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
        }
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
