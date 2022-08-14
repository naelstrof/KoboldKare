using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Seed : GenericUsable, IValuedGood, IPunInstantiateMagicCallback {
    //public List<GameObject> _plantPrefabs;
    [SerializeField]
    private float worth = 5f;
    [SerializeField]
    private Sprite displaySprite;
    public PhotonGameObjectReference plantPrefab;
    public float _spacing = 1f;
    public ScriptablePlant plant;
    private Collider[] hitColliders = new Collider[16];
    private KoboldGenes genes;
    private bool waitingOnPlant = false;

    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }
    public override bool CanUse(Kobold k) {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _spacing, hitColliders, GameManager.instance.plantHitMask, QueryTriggerInteraction.Ignore);
        for(int i=0;i<hitCount;i++) {
            SoilTile tile = hitColliders[i].GetComponentInParent<SoilTile>();
            if (tile != null && tile.GetPlantable()) {
                return true && !waitingOnPlant;
            }
        }
        return false;
    }

    public override void LocalUse(Kobold k) {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _spacing, hitColliders, GameManager.instance.plantHitMask, QueryTriggerInteraction.Ignore);
        SoilTile bestTile = null;
        float bestTileDistance = float.MaxValue;
        for(int i=0;i<hitCount;i++) {
            SoilTile tile = hitColliders[i].GetComponentInParent<SoilTile>();
            if (tile != null && tile.GetPlantable()) {
                float distance = Vector3.Distance(tile.transform.position, transform.position);
                if (distance < bestTileDistance) {
                    bestTile = tile;
                    bestTileDistance = distance;
                }
            }
        }

        if (bestTile != null && bestTile.GetPlantable()) {
            base.LocalUse(k);
            genes ??= new KoboldGenes().Randomize();
            
            GameObject obj = PhotonNetwork.Instantiate(plantPrefab.photonName, bestTile.GetPlantPosition(), Quaternion.LookRotation(Vector3.forward, Vector3.up), 0, new object[] {PlantDatabase.GetID(plant), genes} );
            bestTile.photonView.RPC(nameof(SoilTile.SetPlantedRPC), RpcTarget.All, obj.GetComponent<Plant>().photonView.ViewID);
            
            if (photonView.IsMine) {
                PhotonNetwork.Destroy(photonView.gameObject);
            } else {
                StartCoroutine(WaitOnPlant());
            }
            waitingOnPlant = true;
        }

    }

    void Start() {
        PlayAreaEnforcer.AddTrackedObject(photonView);
    }

    private void OnDestroy() {
        PlayAreaEnforcer.RemoveTrackedObject(photonView);
    }

    IEnumerator WaitOnPlant() {
        waitingOnPlant = true;
        while (true) {
            photonView.RPC(nameof(Use), RpcTarget.All);
            yield return new WaitForSeconds(1f);
        }
    }

    [PunRPC]
    public override void Use() {
        if (photonView.IsMine) {
            PhotonNetwork.Destroy(gameObject);
        }
    }
    public float GetWorth() {
        return worth;
    }
    public void OnValidate() {
        plantPrefab.OnValidate();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData != null) {
            genes = (KoboldGenes)info.photonView.InstantiationData[0];
        } else {
            genes = new KoboldGenes().Randomize();
        }
    }
}
