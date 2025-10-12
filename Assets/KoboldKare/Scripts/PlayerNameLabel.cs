using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerNameLabel : MonoBehaviour {
    private void OnEnable() {
        GetComponent<TMP_Text>().text = $"{PhotonNetwork.NickName}:";
    }
}
