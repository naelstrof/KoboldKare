using System;
using System.Threading.Tasks;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using SimpleJSON;
using UnityEngine;

public class NetworkedEntity : GeneHolder {
    public struct AssetNamePair {
        public string groupName;
        public string assetName;
    }
    private readonly SyncVar<AssetNamePair> assetPair = new SyncVar<AssetNamePair>();

    private GameObject entityInstance;
    private AssetGroup.AssetLocation.AssetHandle<GameObject> entityHandle;
    
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
        
        if (entityHandle != null) {
            entityHandle.Release();
        }
        
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
