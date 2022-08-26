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

public class MailMachine : UsableMachine, IAnimationStationSet {
    [SerializeField]
    private Sprite mailSprite;
    [SerializeField]
    private List<AnimationStation> stations;
    [SerializeField]
    private Animator mailAnimator;
    [SerializeField]
    private PhotonGameObjectReference moneyPile;
    [SerializeField]
    private Transform suckLocation;
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
    private HashSet<Rigidbody> trackedRigidbodies;
    private bool sucking;
    private WaitForFixedUpdate waitForFixedUpdate;
    void Awake() {
        trackedRigidbodies = new HashSet<Rigidbody>();
        waitForFixedUpdate = new WaitForFixedUpdate();
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
            photonView.RPC(nameof(SellObject), RpcTarget.All, station.info.user.photonView.ViewID);
        }
    }

    private float FloorNearestPower(float baseNum, float target) {
        float f = baseNum;
        for(;f<=target;f*=baseNum) {}
        return f/baseNum;
    }
    
    [PunRPC]
    void SellObject(int viewID) {
        PhotonView view = PhotonNetwork.GetPhotonView(viewID);
        float totalWorth = 0f;
        foreach(IValuedGood v in view.GetComponentsInChildren<IValuedGood>()) {
            if (v != null) {
                totalWorth += v.GetWorth();
            }
        }
        soldGameEvent.Raise(view);
        poof.SendEvent("TriggerPoof");
        sellPack.PlayOneShot(sellSource);
        
        if (view.IsMine) {
            PhotonNetwork.Destroy(view.gameObject);
        }
        
        int i = 0;
        while(totalWorth > 0f) {
            float currentPayout = FloorNearestPower(5f,totalWorth);
            //currentPayout = Mathf.Min(payout, currentPayout);
            totalWorth -= currentPayout;
            totalWorth = Mathf.Max(totalWorth,0f);
            float up = Mathf.Floor((float)i/4f)*0.2f;
            PhotonNetwork.Instantiate(moneyPile.photonName, payoutLocation.position + payoutLocation.forward * ((i%4) * 0.25f) + payoutLocation.up*up, payoutLocation.rotation, 0, new object[]{currentPayout});
            i++;
        }
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }

    private void OnValidate() {
        moneyPile.OnValidate();
    }

    private bool ShouldStopTracking(Rigidbody body) {
        if (body == null) {
            return true;
        }

        float distance = Vector3.Distance(body.transform.TransformPoint(body.centerOfMass), suckLocation.position);
        if (distance > 4f) {
            return true;
        }
        if (Vector3.Distance(body.ClosestPointOnBounds(suckLocation.position), suckLocation.position) < 0.1f) {
            PhotonView view = body.gameObject.GetComponentInParent<PhotonView>();
            if (view != null && view.IsMine) {
                photonView.RPC(nameof(SellObject), RpcTarget.All, view.ViewID);
            }
            mailAnimator.SetTrigger("Mail");
            return true;
        }
        return false;
    }

    IEnumerator SuckAndSell() {
        sucking = true;
        while (isActiveAndEnabled && trackedRigidbodies.Count > 0) {
            trackedRigidbodies.RemoveWhere(ShouldStopTracking);
            foreach (var body in trackedRigidbodies) {
                body.velocity = Vector3.MoveTowards(body.velocity, Vector3.zero, body.velocity.magnitude*Time.deltaTime * 10f);
                body.AddForce((suckLocation.position-body.transform.TransformPoint(body.centerOfMass))*30f, ForceMode.Acceleration);
            }
            yield return waitForFixedUpdate;
        }
        sucking = false;
    }

    private void OnTriggerEnter(Collider other) {
        Kobold targetKobold = other.GetComponentInParent<Kobold>();
        if (targetKobold != null) {
            foreach (var player in PhotonNetwork.PlayerList) {
                if ((Kobold)player.TagObject == targetKobold) {
                    return;
                }
            }

            if (targetKobold.grabbed || !targetKobold.GetComponent<Ragdoller>().ragdolled) {
                return;
            }

            LocalUse(targetKobold);
            return;
        }

        Rigidbody body = other.GetComponentInParent<Rigidbody>();
        if (body != null && body.gameObject.GetComponent<MoneyPile>() == null) {
            trackedRigidbodies.Add(body);
            if (!sucking) {
                sucking = true;
                StartCoroutine(SuckAndSell());
            }
        }
        
    }
}
