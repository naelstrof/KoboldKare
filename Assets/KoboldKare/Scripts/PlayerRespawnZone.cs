using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerRespawnZone : MonoBehaviour {
    [SerializeField]
    private Transform[] spawnLocations;

    [SerializeField] private bool resetVelocity = true;
    private void Awake() {
        gameObject.layer = LayerMask.NameToLayer("UserGrabber");
    }
    private void OnTriggerEnter(Collider other) {
        Kobold k = other.GetComponentInParent<Kobold>();
        if (k == null) {
            PhotonView view = other.GetComponentInParent<PhotonView>();
            if (view != null && view.IsMine) {
                PhotonNetwork.Destroy(view.gameObject);
            }
            return;
        }

        if (!k.photonView.IsMine) {
            return;
        }

        k.GetComponent<Ragdoller>().PopRagdoll();
        GetSpawnLocationAndRotation(out Vector3 pos, out Quaternion rot);
        k.transform.SetPositionAndRotation(pos, Quaternion.identity);
        k.GetComponent<CharacterDescriptor>().SetEyeDir(rot*Vector3.forward);
        if (resetVelocity) {
            k.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
    private void GetSpawnLocationAndRotation(out Vector3 position, out Quaternion rotation) {
        if (spawnLocations == null || spawnLocations.Length == 0) {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return;
        }
        var t = spawnLocations[UnityEngine.Random.Range(0, spawnLocations.Length)];
        Vector3 flattenedForward = t.forward.With(y:0);
        if (flattenedForward.magnitude == 0) {
            flattenedForward = Vector3.forward;
        }
        rotation = Quaternion.FromToRotation(Vector3.forward,flattenedForward.normalized); 
        position = t.position;
    }
}
