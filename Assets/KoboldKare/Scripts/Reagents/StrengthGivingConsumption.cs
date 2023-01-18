using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[System.Serializable]
public class StrengthGivingConsumption : ConsumptionDiscreteTrigger {

    [SerializeField] private PhotonGameObjectReference floaterInfoPrefab;
    protected override void OnTrigger(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
        genes.grabCount = (byte)Mathf.Min(genes.grabCount+1, 255);
        genes.maxGrab = (byte)Mathf.Min(genes.maxGrab+1, 255);
        GameObject obj = PhotonNetwork.Instantiate(floaterInfoPrefab.photonName, k.transform.position + Vector3.up*0.5f, Quaternion.identity);
        obj.GetPhotonView().StartCoroutine(DestroyInSeconds(obj));
        base.OnTrigger(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack, ref genes, ref energy);
    }
    private IEnumerator DestroyInSeconds(GameObject obj) {
        yield return new WaitForSeconds(5f);
        PhotonNetwork.Destroy(obj);
    }
    public override void OnValidate() {
        base.OnValidate();
        floaterInfoPrefab.OnValidate();
    }
}
