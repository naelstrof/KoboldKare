using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class DisableUntilNetworkReady : MonoBehaviourPunCallbacks {
    private Selectable selectable;

    private void Awake() {
        selectable = GetComponent<Selectable>();
    }

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
    }
}
