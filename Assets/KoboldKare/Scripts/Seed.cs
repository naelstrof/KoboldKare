using FishNet;
using FishNet.Object;
using UnityEngine;

public class Seed : MonoBehaviour, IValuedGood {
    //public List<GameObject> _plantPrefabs;
    [SerializeField]
    private float worth = 5f;
    [SerializeField]
    private Sprite displaySprite;
    public float _spacing = 1f;
    public ScriptablePlant plant;
    private Collider[] hitColliders = new Collider[16];
    private NetworkedEntity networkedEntity;

    private bool OnUseRequested(NetworkedKobold k) {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _spacing, hitColliders, GameManager.instance.plantHitMask, QueryTriggerInteraction.Ignore);
        for(int i=0;i<hitCount;i++) {
            SoilTile tile = hitColliders[i].GetComponentInParent<SoilTile>();
            if (tile != null && tile.GetPlantable()) {
                return true;
            }
        }
        return false;
    }

    private void OnUse(NetworkedKobold k) {
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
            bestTile.GetComponentInParent<NetworkedEntity>().PlantRPC(GetComponentInParent<NetworkObject>(), plant.name);
        }

    }

    void Start() {
        // FIXME FISHNET
        //PlayAreaEnforcer.AddTrackedObject(photonView);
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(displaySprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
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
