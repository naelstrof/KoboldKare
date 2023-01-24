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
        selectable = GetComponent<Button>();
    }

    public override void OnEnable() {
        base.OnEnable();
        selectable.interactable = PhotonNetwork.IsConnected;
    }

    public override void OnConnected() {
        base.OnConnected();
        selectable.interactable = true;
    }

    public override void OnDisconnected(DisconnectCause cause) {
        base.OnDisconnected(cause);
        selectable.interactable = false;
    }
}
