using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class LayLargeEggObjective : BreedKoboldObjective {
    protected override void OnOviposit(int koboldID, int eggID) {
        PhotonView view = PhotonNetwork.GetPhotonView(eggID);
        if (view!=null && view.GetComponent<GenericReagentContainer>().volume > 60f) {
            ObjectiveManager.NetworkAdvance(view.transform.position, $"{koboldID.ToString()}{eggID.ToString()}");
        }
    }
}
