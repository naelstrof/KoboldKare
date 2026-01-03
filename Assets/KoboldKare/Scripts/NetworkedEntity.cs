using System;
using System.Threading.Tasks;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;

public class NetworkedEntity : GeneHolder {
    public struct AssetNamePair {
        public string groupName;
        public string assetName;
    }
    private readonly SyncVar<AssetNamePair> assetPair = new SyncVar<AssetNamePair>();
    
    public const string PHOTONVIEW_ID_GROUP = "PHOTONVIEW_ID_GROUP";

    private GameObject entityInstance;
    private GameObject mapInstance;
    
    private AssetGroup.AssetLocation.AssetHandle<GameObject> entityHandle;

    public void SetSceneAsset(PhotonView photonView) {
        assetPair.Value = new AssetNamePair { groupName = PHOTONVIEW_ID_GROUP, assetName = $"{photonView.sceneViewId}"};
    }
    
    public void InitializeAsset(string groupName, string assetName) {
        assetPair.Value = new AssetNamePair { groupName = groupName, assetName = assetName };
    }
    
    [ServerRpc]
    public void SetAsset(string groupName, string assetName) {
        assetPair.Value = new AssetNamePair { groupName = groupName, assetName = assetName };
    }
    
    private void Awake() {
        assetPair.OnChange += OnAssetChanged;
        RandomizeGenes();
    }

    private void OnAssetChanged(AssetNamePair prev, AssetNamePair next, bool asServer) {
        if (!asServer && InstanceFinder.ServerManager.Started) {
            return;
        }
        _ = AssetChangedAsync(prev, next, asServer);
    }
    
    private async Task AssetChangedAsync(AssetNamePair prev, AssetNamePair next, bool asServer) {
        if (entityInstance) {
            Destroy(entityInstance);
        }

        if (mapInstance) {
            mapInstance.transform.SetParent(null);
        }
        
        if (entityHandle != null) {
            entityHandle.Release();
        }

        if (next.groupName != PHOTONVIEW_ID_GROUP) {
            entityHandle = await KoboldKareObjectPostProcessor.GetAssetAsync(next.groupName, next.assetName, GameManager.GetErrorGeneric());
            entityInstance = Instantiate(entityHandle.asset, transform);

            try {
                await TryInitializeEntity(entityInstance);
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError($"Failed to initialize entity with asset id {next.groupName}:{next.assetName}, loading error instead.");
                Destroy(entityInstance);
                entityInstance = Instantiate(GameManager.GetErrorGeneric(), transform);
                await TryInitializeEntity(entityInstance);
            }
        } else {
            if (!int.TryParse(next.assetName, out int photonViewID)) {
                Debug.LogError($"Failed to parse photonViewID from asset name {next.groupName}:{next.assetName}");
                entityInstance = Instantiate(GameManager.GetErrorGeneric(), transform);
                return;
            }

            if (!PhotonView.TryFind(out var view, photonViewID)) {
                Debug.LogError($"Failed to find PhotonView with ID {photonViewID}");
                entityInstance = Instantiate(GameManager.GetErrorGeneric(), transform);
                return;
            }

            mapInstance = view.gameObject;
            transform.SetParent(mapInstance.transform.parent);
            view.gameObject.transform.GetLocalPositionAndRotation(out var pos, out var rot);
            transform.SetLocalPositionAndRotation(pos, rot);
            mapInstance.transform.SetParent(transform, true);
            
            mapInstance.gameObject.SetActive(true);
        }
    }

    private async Task TryInitializeEntity(GameObject instance) {
    }
    
    public void Save(JSONNode node) {
        SaveGenes(node, "genes");
    }

    public Task Load(JSONNode node) {
        LoadGenes(node, "genes");
        return Task.CompletedTask;
    }
}
