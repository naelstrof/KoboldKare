using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class DisableUntilNetworkReady : MonoBehaviour {
    private Selectable selectable;

    private void Awake() {
        selectable = GetComponent<Selectable>();
    }

    // FIXME FISHNET
    /*
    public override void OnEnable() {
        base.OnEnable();
        selectable.interactable = PhotonNetwork.IsConnected;
    }

    public override void OnJoinedLobby() {
        base.OnJoinedLobby();
        selectable.interactable = true;
    }

    public override void OnLeftLobby() {
        base.OnLeftLobby();
        selectable.interactable = false;
    }*/
}
