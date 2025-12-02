using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ServerNameLabel : MonoBehaviour {
    private void OnEnable() {
        // FIXME FISHNET
        /*
        if (PhotonNetwork.InRoom) {
            GetComponent<TMPro.TMP_Text>().text = PhotonNetwork.CurrentRoom.Name;
        } else {
            GetComponent<TMPro.TMP_Text>().text = "";
        }*/
    }
}
