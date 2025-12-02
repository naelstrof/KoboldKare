using System;
using Photon.Pun;
using UnityEngine;

public class GenericSpoilable : MonoBehaviour, ISpoilable {
    void Start() {
        SpoilableHandler.AddSpoilable(this);
    }

    private void OnDestroy() {
        SpoilableHandler.RemoveSpoilable(this);
    }

    public void OnSpoil() {
        // FIXME FISHNET
        /*if (photonView.IsMine) {
            PhotonNetwork.Destroy(photonView.gameObject);
        }*/
    }
}
