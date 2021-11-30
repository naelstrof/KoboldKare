using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using UnityEngine.Analytics;
using Photon.Pun;

public class DeleteVolume : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        PhotonView view = other.GetComponentInParent<PhotonView>();
        if (view != null && !view.IsMine) {
            return;
        }
        if (other.transform.root.GetComponentInChildren<GenericDamagable>() != null) {
            other.transform.root.GetComponentInChildren<GenericDamagable>().Damage(9999999999999999);
            if (view) {
                PhotonNetwork.Destroy(other.transform.root.gameObject);
            } else {
                Destroy(other.transform.root.gameObject);
            }
        } else {
            if (view) {
                PhotonNetwork.Destroy(other.transform.root.gameObject);
            } else {
                Destroy(other.transform.root.gameObject);
            }
        }
    }
}
