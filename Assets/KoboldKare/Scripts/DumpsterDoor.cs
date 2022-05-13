using System.Collections;
using System.Collections.Generic;
using System.IO;
using KoboldKare;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(AudioSource)), RequireComponent(typeof(Photon.Pun.PhotonView))]
public class DumpsterDoor : GenericDoor, IPunObservable, ISavable {
    [SerializeField]
    private GameObject dumpsterTrigger;
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private AudioSource moneyBlips;
    [SerializeField]
    private List<AudioClip> moneyBlipClips = new List<AudioClip>();
    [SerializeField]
    private float maxMoneyBlip = 100f;
    [SerializeField]
    private BoxCollider inside;
    [SerializeField]
    private Animator targetAnimator;
    [SerializeField]
    private PhotonGameObjectReference moneyPile;
    private float payout;
    [SerializeField]
    private Transform payoutLocation;
    [SerializeField]
    private GameEventGeneric midnight;
    protected override void Open(){
        base.Open();
        dumpsterTrigger.SetActive(true);
    }
    protected override void Close(){
        base.Close();
        dumpsterTrigger.SetActive(false);
    }
    private float FloorNearestPower(float baseNum, float target) {
        float f = baseNum;
        for(;f<=target;f*=baseNum) {}
        return f/baseNum;
    }
    private void OnMidnight(object ignore) {
        if (!photonView.IsMine) {
            return;
        }
        int i = 0;
        while(payout > 0f) {
            float currentPayout = FloorNearestPower(5f,payout);
            //currentPayout = Mathf.Min(payout, currentPayout);
            payout -= currentPayout;
            payout = Mathf.Max(payout,0f);
            float up = Mathf.Floor((float)i/4f)*0.2f;
            PhotonNetwork.Instantiate(moneyPile.photonName, payoutLocation.position + payoutLocation.forward*(i%4)*0.25f + payoutLocation.up*up, payoutLocation.rotation, 0, new object[]{currentPayout});
            i++;
        }
        targetAnimator.SetBool("Ready", false);
    }
    private void Awake() {
        midnight.AddListener(OnMidnight);
    }
    private void OnDestroy() {
        midnight.RemoveListener(OnMidnight);
    }
    private void Check(Collider other) {
        //Debug.Log("Running dumpster check");
        //if (other.isTrigger || other.transform.root.CompareTag("Player") || !other.GetComponentInParent<PhotonView>().IsMine) {
        if (!layerMask.Contains(other.gameObject.layer)) {
            return;
        }
        if (other.isTrigger || other.GetComponentInParent<PhotonView>() == null || !other.GetComponentInParent<PhotonView>().IsMine) { //Ensure only one player is running this at a time
            return;
        }
        Vector3 origin = other.transform.root.position;
        GenericGrabbable grabbable = other.GetComponentInParent<GenericGrabbable>();
        if (grabbable != null) {
            origin = grabbable.center.position;
        }
        // Make sure we're fully inside
        if (Vector3.Distance(inside.ClosestPoint(origin),origin)>0.01f) {
            return;
        }
        float totalWorth = 0f;
        foreach(IValuedGood v in other.GetAllComponents<IValuedGood>()) {
            if (v != null) {
                totalWorth += v.GetWorth();
            }
        }

        if (totalWorth > 0f) {
            //totalWorth = Mathf.Min(totalWorth,maxSaleValue);
            //Debug.Log("Giving player $"+totalWorth+" from a market value of "+totalWorthUnmod);
            //Debug.Log("Giving player $"+totalWorth);
            //payout += totalWorth;
            AudioClip playback = moneyBlipClips[Mathf.RoundToInt(Mathf.Clamp01(totalWorth / maxMoneyBlip) * (moneyBlipClips.Count - 1))];
            moneyBlips.clip = playback;
            moneyBlips.Play();
            //MoneySyncHack.view.RPC("RPCGiveMoney", RpcTarget.All, new object[]{totalWorth});
            photonView.RPC("RPCAddMoney", RpcTarget.All, new object[]{totalWorth});
        }
        PhotonNetwork.Destroy(other.GetComponentInParent<PhotonView>().gameObject);
    }

    [PunRPC]
    void RPCAddMoney(float amount) {
        targetAnimator.SetBool("Ready", true);
        payout += amount;
    }

    private void OnTriggerEnter(Collider other) {
        if (!opened) {
            return;
        }
        Check(other);
    }
    private void OnTriggerStay(Collider other) {
        if (!opened) {
            return;
        }
        Check(other);
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(payout);
        } else {
            payout = (float)stream.ReceiveNext();
        }
    }
    public override void Save(BinaryWriter writer, string version) {
        writer.Write(payout);
    }
    public override void Load(BinaryReader reader, string version) {
        payout = reader.ReadSingle();
    }
}
