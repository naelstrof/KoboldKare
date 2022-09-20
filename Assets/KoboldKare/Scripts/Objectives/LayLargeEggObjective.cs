using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class LayLargeEggObjective : BreedKoboldObjective {
    protected override void OnOviposit(GameObject egg) {
        if (egg.GetComponent<GenericReagentContainer>().volume > 60f) {
            Advance(egg.transform.position);
        }
    }
}
