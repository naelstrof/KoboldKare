using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using System.IO;

public class HarvestBox : MonoBehaviourPun, IPunObservable, ISavable {
    public LayerMask layerMask;
    public ScriptableFloat money;
    public AudioSource moneyBlips;
    public List<AudioClip> moneyBlipClips = new List<AudioClip>();
    public float maxMoneyBlip = 100f;
    public BoxCollider inside;
    public Animator targetAnimator;
    public PhotonGameObjectReference moneyPile;
    private float payout;
    [SerializeField]
    private Transform payoutLocation;
    public GameEventGeneric midnight;
    private void OnMidnight(object ignore) {
        if (!photonView.IsMine) {
            return;
        }
        while(payout > 0f) {
            float currentPayout = payout*0.25f+5f;
            currentPayout = Mathf.Min(payout, currentPayout);
            payout -= currentPayout;
            Debug.Log("Instantiating "+currentPayout + " money");
            PhotonNetwork.Instantiate(moneyPile.photonName, payoutLocation.position, payoutLocation.rotation, 0, new object[]{currentPayout});
        }
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
        if (other.isTrigger || !other.GetComponentInParent<PhotonView>().IsMine) { //Ensure only one player is running this at a time
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
        payout += amount;
    }

    private void OnTriggerEnter(Collider other) {
        Check(other);
    }
    private void OnTriggerStay(Collider other) {
        Check(other);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(payout);
        } else {
            payout = (float)stream.ReceiveNext();
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(payout);
    }

    public void Load(BinaryReader reader, string version) {
        payout = reader.ReadSingle();
    }
}
