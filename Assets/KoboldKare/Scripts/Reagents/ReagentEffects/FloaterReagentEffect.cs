using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[System.Serializable]
public class FloaterReagentEffect : ReagentEffect
{
    [SerializeField]
    private PhotonGameObjectReference floaterInfoPrefab;
    [SerializeField]
    private float Duration = 5;

    public override void Apply(Kobold k, float usedAmount, ref KoboldGenes genes, ref ReagentContents addBack, ref float energy)
    {
        GameObject obj = PhotonNetwork.Instantiate(floaterInfoPrefab.photonName, k.transform.position + Vector3.up * 0.5f, Quaternion.identity);
        obj.GetPhotonView().StartCoroutine(DestroyInSeconds(obj));
    }

    private IEnumerator DestroyInSeconds(GameObject obj)
    {
        yield return new WaitForSeconds(Duration);
        PhotonNetwork.Destroy(obj);
    }

    public override void OnValidate()
    {
        base.OnValidate();
        if (Duration <= 1)
        {
            Duration = 5;
        }
        floaterInfoPrefab.OnValidate();
    }
}
