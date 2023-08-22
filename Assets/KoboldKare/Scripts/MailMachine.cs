using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KoboldKare;
using Naelstrof.Mozzarella;
using Photon.Pun;
using SkinnedMeshDecals;
using UnityEngine;
using UnityEngine.VFX;
using Vilar.AnimationStation;

public class MailMachine : SuckingMachine, IAnimationStationSet {
    [SerializeField]
    private Sprite mailSprite;
    [SerializeField]
    private List<AnimationStation> stations;
    [SerializeField]
    private Animator mailAnimator;
    [SerializeField]
    private PhotonGameObjectReference moneyPile;
    [SerializeField]
    private AudioPack sellPack;
    private AudioSource sellSource;
    
    [SerializeField] private VisualEffect poof;

    [SerializeField]
    private GameEventPhotonView soldGameEvent;
    
    [SerializeField]
    Transform payoutLocation;
    
    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    private WaitForSeconds wait;
    private List<AnimationStation> availableStations;
    protected override void Awake() {
        base.Awake();
        readOnlyStations = stations.AsReadOnly();
        wait = new WaitForSeconds(2f);
        availableStations = new List<AnimationStation>();
        if (sellSource == null) {
            sellSource = gameObject.AddComponent<AudioSource>();
            sellSource.playOnAwake = false;
            sellSource.maxDistance = 10f;
            sellSource.minDistance = 0.2f;
            sellSource.rolloffMode = AudioRolloffMode.Linear;
            sellSource.spatialBlend = 1f;
            sellSource.loop = false;
        }
    }
    public override Sprite GetSprite(Kobold k) {
        return mailSprite;
    }
    public override bool CanUse(Kobold k) {
        if (!constructed) {
            return false;
        }

        foreach (var player in PhotonNetwork.PlayerList) {
            if ((Kobold)player.TagObject == k) {
                return false;
            }
        }

        foreach (var station in stations) {
            if (station.info.user == null) {
                return true;
            }
        }
        return true;
    }

    public override void LocalUse(Kobold k) {
        availableStations.Clear();
        foreach (var station in stations) {
            if (station.info.user == null) {
                availableStations.Add(station);
            }
        }
        if (availableStations.Count <= 0) {
            return;
        }
        int randomStation = UnityEngine.Random.Range(0, availableStations.Count);
        k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All, photonView.ViewID, stations.IndexOf(availableStations[randomStation]));
        base.LocalUse(k);
    }
    public override void Use() {
        StopAllCoroutines();
        StartCoroutine(WaitThenVoreKobold());
    }
    private IEnumerator WaitThenVoreKobold() {
        yield return wait;
        mailAnimator.SetTrigger("Mail");
        yield return wait;
        foreach (var station in stations) {
            if (station.info.user == null || !station.info.user.photonView.IsMine) {
                continue;
            }
            photonView.RPC(nameof(OnSwallowed), RpcTarget.All, station.info.user.photonView.ViewID);
        }
    }
    
    [PunRPC]
    protected override IEnumerator OnSwallowed(int viewID) {
        PhotonProfiler.LogReceive(sizeof(int));
         if(suckingIDs.Contains(viewID)){
            yield break;
        }
        suckingIDs.Add(viewID);
        PhotonView view = PhotonNetwork.GetPhotonView(viewID);
        float totalWorth = 0f;
        foreach(IValuedGood v in view.GetComponentsInChildren<IValuedGood>()) {
            if (v != null) {
                float add = Mathf.Min(v.GetWorth(), 1953125f);
                totalWorth = Mathf.Min(totalWorth+add,1953125f);
            }
        }
        soldGameEvent.Raise(view);
        poof.SendEvent("TriggerPoof");
        // Kobolds can only be sold if they've already played the mail animation.
        if (view.GetComponent<Kobold>() != null) {
            mailAnimator.SetTrigger("Mail");
        }
        sellPack.PlayOneShot(sellSource);

        if (!view.IsMine) {
            yield break;
        }

        // Just wait a very short while so that we don't shuffle the order of our commands (sell -> delete)
        yield return new WaitForSeconds(0.1f);
        
        // It is technically possible for it to be destroyed at this point already.
        if (view != null) {
            PhotonNetwork.Destroy(view.gameObject);
        }

        totalWorth = Mathf.Min(totalWorth, 1953125f);
        PhotonNetwork.Instantiate(moneyPile.photonName, payoutLocation.position, payoutLocation.rotation, 0, new object[]{totalWorth});
        suckingIDs.Remove(viewID);
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }

    private void OnValidate() {
        moneyPile.OnValidate();
    }
}
