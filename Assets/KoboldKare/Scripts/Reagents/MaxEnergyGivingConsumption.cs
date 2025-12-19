using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[System.Serializable]
public class MaxEnergyGivingConsumption : ConsumptionDiscreteTrigger {
    [SerializeField] private PhotonGameObjectReference floaterInfoPrefab;
    protected override void OnTrigger(NetworkedKobold k, ScriptableReagent scriptableReagent, ref float amountProcessed, ref ReagentContents reagentMemory, ref ReagentContents addBack, ref float energy) {
        k.SetMaxEnergy((byte)Mathf.Min(k.maxEnergy.Value+1, 255));
        // FIXME FISHNET
        /*GameObject obj = PhotonNetwork.Instantiate(floaterInfoPrefab.photonName, k.transform.position + Vector3.up*0.5f, Quaternion.identity);
        obj.GetPhotonView().StartCoroutine(DestroyInSeconds(obj));*/
        base.OnTrigger(k, scriptableReagent, ref amountProcessed, ref reagentMemory, ref addBack,  ref energy);
    }
    private IEnumerator DestroyInSeconds(GameObject obj) {
        yield return new WaitForSeconds(5f);
        // FIXME FISHNET
        //PhotonNetwork.Destroy(obj);
    }
}
