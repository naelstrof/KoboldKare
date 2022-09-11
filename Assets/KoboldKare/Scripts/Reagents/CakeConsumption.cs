using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[System.Serializable]
public class CakeConsumption : ConsumptionDiscreteTrigger {
    [SerializeField] private PhotonGameObjectReference floaterInfoPrefab;
    protected override void OnTrigger(Kobold k, ScriptableReagent scriptableReagent, ref float amountProcessed,
        ref ReagentContents reagentMemory, ref ReagentContents addBack, ref KoboldGenes genes, ref float energy) {
        genes.maxEnergy = (byte)Mathf.Min(genes.maxEnergy+1, 255);
        GameObject obj = PhotonNetwork.Instantiate(floaterInfoPrefab.photonName, k.transform.position + Vector3.up, Quaternion.identity);
        obj.GetPhotonView().StartCoroutine(DestroyInSeconds(obj));
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
