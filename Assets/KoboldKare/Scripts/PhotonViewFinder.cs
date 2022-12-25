using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

public class PhotonViewFinder : ScriptableWizard {
    public int findID = 0;
    [MenuItem("Tools/KoboldKare/Find Photon View")]
    static void CreateWizard() {
        DisplayWizard<PhotonViewFinder>("Find Photon View", "OK");
    }
    private void OnWizardCreate() {
        foreach (var photonView in FindObjectsOfType<PhotonView>(true)) {
            if (photonView.sceneViewId == findID) {
                Selection.activeObject = photonView.gameObject;
                return;
            }
        }
        Debug.Log("Couldn't find photonview with id " + findID);
    }
}

#endif