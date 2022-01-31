using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitFromMenu : MonoBehaviour{
    public void Fire(){
        GameManager.instance.Quit();
    }
}

