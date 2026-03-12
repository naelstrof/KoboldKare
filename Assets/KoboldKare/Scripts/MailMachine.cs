using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FishNet;
using FishNet.Object;
using KoboldKare;
using Photon.Pun;
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

    protected override void OnSwallowed(NetworkedEntity ent) {
        base.OnSwallowed(ent);
        
        float totalWorth = 0f;
        foreach(IValuedGood v in ent.GetComponentsInChildren<IValuedGood>()) {
            if (v != null) {
                float add = Mathf.Min(v.GetWorth(), 1953125f);
                totalWorth = Mathf.Min(totalWorth+add,1953125f);
            }
        }
        soldGameEvent.Raise(ent.GetComponentInChildren<PhotonView>());
        poof.SendEvent("TriggerPoof");
        
        sellPack.PlayOneShot(sellSource);

        if (!ent.IsOwner) {
            return;
        }

        totalWorth = Mathf.Min(totalWorth, 1953125f);
        if (moneyPile.TryGetAssetGroupAndKey(out var group, out var key)) {
            var data = KoboldEntitySpawner.NetworkedEntityInstantiationData.Default();
            data.groupName = group;
            data.assetName = key;
            data.position = payoutLocation.position;
            data.rotation = payoutLocation.rotation;
            data.moneyPileWorth = totalWorth;
            InstanceFinder.NetworkManager.GetComponent<KoboldEntitySpawner>().SpawnAsServer(data, false, ent.Owner);
        } else {
            Debug.LogError("Money pile prefab not found in database.");
            return;
        }
            
        // It is technically possible for it to be destroyed at this point already.
        if (ent != null) {
            InstanceFinder.ServerManager.Despawn(ent.NetworkObject);
        }
    }

    protected override void Start() {
        base.Start();
        if (networkedEntity) {
            networkedEntity.SetSprite(mailSprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    private bool OnUseRequested(NetworkedKobold k) {
        if (!constructed) {
            return false;
        }

        if (KoboldEntitySpawner.GetIsPlayerKobold(k)) {
            return false;
        }
        
        foreach (var station in stations) {
            if (station.info.user == k) {
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

    private void OnUse(NetworkedKobold k) {
        availableStations.Clear();
        foreach (var station in stations) {
            if (station.info.user == null) {
                availableStations.Add(station);
            }

            if (station.info.user == k) {
                return;
            }
        }
        if (availableStations.Count <= 0) {
            Debug.Log("All stations are full!");
            return;
        }

        if (k.IsAnimating()) {
            return;
        }
        
        int randomStation = Random.Range(0, availableStations.Count);
        
        k.BeginAnimation(GetComponentInParent<NetworkObject>(), stations.IndexOf(availableStations[randomStation]));
        StopAllCoroutines();
        StartCoroutine(WaitThenVoreKobold());
    }
    
    private IEnumerator WaitThenVoreKobold() {
        yield return wait;
        mailAnimator.SetTrigger("Mail");
        yield return wait;
        foreach (var station in stations) {
            if (station.info.user == null || !station.info.user.IsOwner) {
                continue;
            }
            OnSwallowed(station.info.user);
        }
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
