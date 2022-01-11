using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestBox : MonoBehaviourPun {
    public LayerMask layerMask;
    public ScriptableFloat money;
    public AudioSource moneyBlips;
    public List<AudioClip> moneyBlipClips = new List<AudioClip>();
    public float maxMoneyBlip = 100f;
    public float returnedValue = 0.3f;
    public float returnedValueFruit = 0.5f;
    public float returnedValueSelfSale = 0.1f;
    public float maxSaleValue = 6000f;
    public BoxCollider inside;
    public Animator targetAnimator;
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
        //float totalWorthUnmod = 0f;

        //Debug.Log("Beginning value determination...");
        foreach(IValuedGood v in other.GetAllComponents<IValuedGood>()) {
            if (v != null) {
                totalWorth += v.GetWorth();
            }
        }

        if (totalWorth > 0f) {
            totalWorth = Mathf.Min(totalWorth,maxSaleValue);
            //Debug.Log("Giving player $"+totalWorth+" from a market value of "+totalWorthUnmod);
            Debug.Log("Giving player $"+totalWorth+" with a maximum value of "+maxSaleValue);
            AudioClip playback = moneyBlipClips[Mathf.RoundToInt(Mathf.Clamp01(totalWorth / maxMoneyBlip) * (moneyBlipClips.Count - 1))];
            moneyBlips.clip = playback;
            moneyBlips.Play();
            MoneySyncHack.view.RPC("RPCGiveMoney", RpcTarget.All, new object[]{totalWorth});
        }
        PhotonNetwork.Destroy(other.GetComponentInParent<PhotonView>().gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        Check(other);
    }
    private void OnTriggerStay(Collider other) {
        Check(other);
    }
}
