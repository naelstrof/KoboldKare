using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    //private float payout;
    [SerializeField]
    private Transform payoutLocation;

    private static readonly int Ready = Animator.StringToHash("Ready");
    public delegate void SellObjectAction(GameObject obj, float moneyGained);
    public static event SellObjectAction soldObject;
    
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
    /*private void OnMidnight(object ignore) {
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
            PhotonNetwork.Instantiate(moneyPile.photonName, payoutLocation.position + payoutLocation.forward * ((i%4) * 0.25f) + payoutLocation.up*up, payoutLocation.rotation, 0, new object[]{currentPayout});
            i++;
        }
        targetAnimator.SetBool(Ready, false);
    }*/

    private IEnumerator WaitAndPay(float newPayout) {
        yield return new WaitForSeconds(30f);
        if (!photonView.IsMine) {
            yield break;
        }
        int i = 0;
        while(newPayout > 0f) {
            float currentPayout = FloorNearestPower(5f,newPayout);
            //currentPayout = Mathf.Min(payout, currentPayout);
            newPayout -= currentPayout;
            newPayout = Mathf.Max(newPayout,0f);
            float up = Mathf.Floor((float)i/4f)*0.2f;
            PhotonNetwork.Instantiate(moneyPile.photonName, payoutLocation.position + payoutLocation.forward * ((i%4) * 0.25f) + payoutLocation.up*up, payoutLocation.rotation, 0, new object[]{currentPayout});
            i++;
        }
        targetAnimator.SetBool(Ready, false);
    }

    private void Check(Collider other) {
        if (!layerMask.Contains(other.gameObject.layer)) {
            return;
        }
        
        PhotonView otherView = other.GetComponentInParent<PhotonView>();
        if (other.isTrigger || otherView == null) {
            return;
        }

        if (!otherView.IsMine) { //Ensure only one player is running this at a time
            return;
        }
        
        Vector3 origin = other.transform.root.position;
        GenericGrabbable grabbable = other.GetComponentInParent<GenericGrabbable>();
        if (grabbable != null) {
            origin = grabbable.center.position;
        }
        if (Vector3.Distance(inside.ClosestPoint(origin),origin)>0.01f) {
            return;
        }

        photonView.RPC(nameof(RPCSellObject), RpcTarget.All, otherView.ViewID);
    }
    
    [PunRPC]
    void RPCSellObject(int photonViewID) {
        PhotonView view = PhotonNetwork.GetPhotonView(photonViewID);
        
        float totalWorth = 0f;
        foreach(IValuedGood v in view.GetComponentsInChildren<IValuedGood>()) {
            if (v != null) {
                totalWorth += v.GetWorth();
            }
        }
        if (totalWorth > 0f) {
            AudioClip playback = moneyBlipClips[Mathf.RoundToInt(Mathf.Clamp01(totalWorth / maxMoneyBlip) * (moneyBlipClips.Count - 1))];
            moneyBlips.clip = playback;
            moneyBlips.Play();
            targetAnimator.SetBool(Ready, true);
            StartCoroutine(WaitAndPay(totalWorth));
        }
        soldObject?.Invoke(view.gameObject, totalWorth);
        if (view.IsMine) {
            PhotonNetwork.Destroy(view.gameObject);
        }
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
    }
    public override void Save(BinaryWriter writer, string version) {
    }
    public override void Load(BinaryReader reader, string version) {
    }
}
