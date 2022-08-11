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
    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    void Awake() {
        readOnlyStations = stations.AsReadOnly();
    }
    public override Sprite GetSprite(Kobold k) {
        return sleepingSprite;
    }
    public override bool CanUse(Kobold k) {
        if (k.GetEnergy() > 0) {
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
        if (!photonView.IsMine) {
            return;
        }
        PhotonView view = PhotonNetwork.GetPhotonView(targetID);
        if (view != null && view.TryGetComponent(out Kobold kobold)) {
            StartCoroutine(WaitThenSleep(kobold));
        }
    }

    private IEnumerator WaitThenSleep(Kobold k) {
        yield return new WaitForSeconds(10f);
        if (k == null) {
            yield break;
        }

        bool stillSleeping = false;
        foreach (var station in GetAnimationStations()) {
            if (station.info.user == k) {
                stillSleeping = true;
            }
        }

        if (stillSleeping) {
            k.photonView.RPC(nameof(Kobold.Rest), RpcTarget.All);
            k.photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
        }
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
