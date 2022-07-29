using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;
using Vilar.AnimationStation;

public class Toilet : GenericUsable, IAnimationStationSet {
    [SerializeField] private Sprite useSprite;
    [SerializeField] private AnimationStation station;
    
    [SerializeField] private AudioPack sparkles;
    [SerializeField] private AudioPack flush;
    [SerializeField] private VisualEffect effect;
    
    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    private AudioSource source;
    
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
        yield return new WaitForSeconds(4f);
        source.enabled = true;
        sparkles.Play(source);
        effect.gameObject.SetActive(true);
        yield return new WaitForSeconds(6f);
        source.Pause();
        flush.PlayOneShot(source);
        Kobold k = station.info.user;
        if (k != null) {
            k.bellyContainer.Spill(k.bellyContainer.volume);
            k.photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
        }
        effect.gameObject.SetActive(false);
        yield return new WaitForSeconds(4f);
        source.Stop();
        source.enabled = false;
    }

    private void Start() {
        List<AnimationStation> stations = new List<AnimationStation>();
        stations.Add(station);
        readOnlyStations = stations.AsReadOnly();
        if (source == null) {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.maxDistance = 10f;
            source.minDistance = 0.2f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.spatialBlend = 1f;
            source.loop = true;
        }

        source.enabled = false;
    }
    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
