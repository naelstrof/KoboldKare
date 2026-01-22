using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FishNet.Object;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;
using Vilar.AnimationStation;

public class Toilet : MonoBehaviour, IAnimationStationSet {
    [SerializeField] private Sprite useSprite;
    [SerializeField] private AnimationStation station;
    
    [SerializeField] private AudioPack sparkles;
    [SerializeField] private AudioPack flush;
    [SerializeField] private VisualEffect effect;
    
    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    private AudioSource source;
    private NetworkedEntity networkedEntity;
    
    private bool OnUseRequested(NetworkedKobold k) {
        return station.info.user == null;
    }

    private void OnUse(NetworkedKobold k) {
        // FIXME FISHNET
        k.BeginAnimation(GetComponentInParent<NetworkObject>(), 0);
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
        NetworkedKobold k = station.info.user;
        if (k != null && k.TryGetKobold(out var kobold)) {
            kobold.bellyContainer.Spill(kobold.bellyContainer.volume);
            k.StopAnimation();
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
        networkedEntity = GetComponentInParent<NetworkedKobold>();
        if (networkedEntity) {
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
            networkedEntity.SetSprite(useSprite);
        }
    }
    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
