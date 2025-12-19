using UnityEngine;

public class Seed : GenericUsable, IValuedGood {
    //public List<GameObject> _plantPrefabs;
    [SerializeField]
    private float worth = 5f;
    [SerializeField]
    private Sprite displaySprite;
    public float _spacing = 1f;
    public ScriptablePlant plant;
    private Collider[] hitColliders = new Collider[16];
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
            // FIXME FISHNET
            //BitBuffer spawnData = new BitBuffer(16);
            //spawnData.AddKoboldGenes(genes);
            //spawnData.AddShort((short)KoboldKareObjectPostProcessor.GetAssetID("Plant", plant.name));
            //bestTile.photonView.RPC(nameof(SoilTile.PlantRPC), RpcTarget.All, photonView.ViewID, spawnData);
        }

    }

    void Start() {
        // FIXME FISHNET
        //PlayAreaEnforcer.AddTrackedObject(photonView);
    }

    private void OnDestroy() {
        // FIXME FISHNET
        //PlayAreaEnforcer.RemoveTrackedObject(photonView);
    }
    public float GetWorth() {
        return worth;
    }

    
    // FIXME FISHNET
    /*public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData != null && info.photonView.InstantiationData[0] is BitBuffer) {
            BitBuffer buffer = (BitBuffer)info.photonView.InstantiationData[0];
            genes = buffer.ReadKoboldGenes();
            PhotonProfiler.LogReceive(buffer.Length);
        } else {
            genes = new KoboldGenes().Randomize();
        }
    }*/
}
