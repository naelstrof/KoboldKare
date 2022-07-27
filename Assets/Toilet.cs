using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class Toilet : GenericUsable, IAnimationStationSet {
    [SerializeField] private Sprite useSprite;
    [SerializeField] private AnimationStation station;
    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    
    public override Sprite GetSprite(Kobold k) {
        return useSprite;
    }

    public override bool CanUse(Kobold k) {
        return station.info.user == null;
    }

    public override void LocalUse(Kobold k) {
        base.LocalUse(k);
        k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All, photonView.ViewID, 0);
    }

    public override void Use() {
        base.Use();
        StopAllCoroutines();
        StartCoroutine(ToiletRoutine());
    }

    private IEnumerator ToiletRoutine() {
        yield return new WaitForSeconds(5f);
        Kobold k = station.info.user;
        if (k != null) {
            k.bellyContainer.Spill(k.bellyContainer.volume);
        }
    }

    private void Start() {
        List<AnimationStation> stations = new List<AnimationStation>();
        stations.Add(station);
        readOnlyStations = stations.AsReadOnly();
    }
    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
