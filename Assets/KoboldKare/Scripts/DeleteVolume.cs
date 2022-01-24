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
        if(other.GetComponentInParent<GenericGrabbable>() == null || view == null){
            return;
        }
        
        //Handle networked object
        PhotonNetwork.Destroy(view);
    }
}
