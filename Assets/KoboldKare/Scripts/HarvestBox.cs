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
        if (other.isTrigger) {
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

        //Debug.Log("Beginning value determination...");
        foreach(IValuedGood v in other.GetAllComponents<IValuedGood>()) {
            if (v != null) {
                if(grabbable != null && grabbable.grabbableType == GrabbableType.Fruit){                    
                    totalWorth += Mathf.RoundToInt(v.GetWorth()*returnedValueFruit); //Don't provide full value of the items
                }
                else if(other.transform.root.GetComponent<Kobold>() != null){
                    if(other.transform.root.GetComponent<PhotonView>().IsMine){ // Give 10% back if player is selling self
                        totalWorth += Mathf.RoundToInt(v.GetWorth()*returnedValueSelfSale);
                    }
                    else{
                        totalWorth += Mathf.RoundToInt(v.GetWorth()*returnedValue);
                    }
                }
                else{
                    totalWorth += Mathf.RoundToInt(v.GetWorth()*returnedValue);
                }
            }
        }

        if (totalWorth > 0f) {
            totalWorth = Mathf.Min(totalWorth,maxSaleValue);
            Debug.Log("Giving player $"+totalWorth);
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
