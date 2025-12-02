using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using UnityEngine.Analytics;
using Photon.Pun;

public class DeleteVolume : MonoBehaviour {
    [SerializeField]
    private Transform bucketRespawnPoint;
    private void OnTriggerEnter(Collider other) {
        
        // FIXME FISHNET
        /*
        PhotonView view = other.GetComponentInParent<PhotonView>();
        if (view == null) {
            return;
        }
        if (!view.IsMine) {
            return;
        }
        
        if (view.TryGetComponent(out BucketWeapon bucket)) {
            bucket.transform.position = bucketRespawnPoint.position;
            bucket.GetComponent<Rigidbody>().velocity = Vector3.zero;
        } else {
            if (view.TryGetComponent(out Rigidbody body)) {
                Debug.Log("Destroying view at " + view.transform.position + " going speed " + body.velocity.magnitude);
            }
            PhotonNetwork.Destroy(view);
        }
        */
    }
}
