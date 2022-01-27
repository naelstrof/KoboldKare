using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class QuitFromIngame : MonoBehaviour{
    public void QuitToMenu(){
        GameManager.instance.StartCoroutine(QuitToMenuRoutine());
    }

    public IEnumerator QuitToMenuRoutine() {
        PhotonNetwork.Disconnect();
        yield return new WaitUntil(()=>!PhotonNetwork.IsConnected);
        yield return LevelLoader.instance.LoadLevel("MainMenu");
        PhotonNetwork.OfflineMode = false;
    }

}
