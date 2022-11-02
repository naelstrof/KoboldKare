using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class QuitFromIngame : MonoBehaviour{
    public void QuitToMenu(){
        GameManager.instance.QuitToMenu();
    }

}
