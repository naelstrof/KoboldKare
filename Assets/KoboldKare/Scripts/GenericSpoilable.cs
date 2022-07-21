using System;
using Photon.Pun;

public class GenericSpoilable : MonoBehaviourPun, ISpoilable {
    void Start() {
        SpoilableHandler.AddSpoilable(this);
    }

    private void OnDestroy() {
        SpoilableHandler.RemoveSpoilable(this);
    }

    public void OnSpoil() {
        if (photonView.IsMine) {
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }
}
