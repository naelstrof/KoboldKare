using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class QuitFromIngame : MonoBehaviour{
    public string MainMenuDest;
    public void QuitToMenu(){
        PhotonNetwork.Disconnect(); // Must always D/C before returning to main menu
        SceneManager.LoadScene(MainMenuDest);
    }

}
