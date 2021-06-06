using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Pauser : MonoBehaviour {
    public void Start() {
        Time.timeScale = 1.0f;
    }
    public void SetPaused(bool paused) {
        if (!PhotonNetwork.OfflineMode) {
            return;
        }
        Time.timeScale = paused ? 0.0f : 1.0f;
    }
    public void TogglePause() {
        if (!PhotonNetwork.OfflineMode) {
            return;
        }
        Time.timeScale = Mathf.Approximately(Time.timeScale, 0.0f) ? 1.0f : 0.0f;
    }
}
