using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class HarvestBox : MonoBehaviourPun {
    public LayerMask layerMask;
    public ScriptableFloat money;
    public AudioSource moneyBlips;
    public List<AudioClip> moneyBlipClips = new List<AudioClip>();
    public float maxMoneyBlip = 100f;
    public BoxCollider inside;

    [PunRPC]
    public void RPCSetMoney(float moneyValue) {
        money.set(moneyValue);
    }

    public void OnDestroy() {
        PhotonNetwork.CleanRpcBufferIfMine(photonView);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.isTrigger || other.transform.root.CompareTag("Player") || !other.GetComponentInParent<PhotonView>().IsMine) {
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
        foreach( IValuedGood v in other.GetAllComponents<IValuedGood>()) {
            if (v != null) {
                totalWorth += v.GetWorth();
            }
        }
        if (totalWorth > 0f) {
            AudioClip playback = moneyBlipClips[Mathf.RoundToInt(Mathf.Clamp01(totalWorth / maxMoneyBlip) * (moneyBlipClips.Count - 1))];
            moneyBlips.clip = playback;
            moneyBlips.Play();
            SaveManager.RPC(photonView, "RPCSetMoney", RpcTarget.AllBuffered, new object[] { money.value + totalWorth });
        }
        SaveManager.Destroy(other.transform.root.gameObject);
    }
}
