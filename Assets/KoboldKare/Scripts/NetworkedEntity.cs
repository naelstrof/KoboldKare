using System;
using System.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
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
    protected readonly SyncVar<AssetNamePair> assetPair = new SyncVar<AssetNamePair>();
    public readonly SyncVar<bool> frozen = new SyncVar<bool>();
    public readonly SyncVar<float> health = new SyncVar<float>(100f);
    
    public const string PHOTONVIEW_ID_GROUP = "PHOTONVIEW_ID_GROUP";

    private GameObject entityInstance;
    private GameObject mapInstance;
    
    private AssetGroup.AssetLocation.AssetHandle<GameObject> entityHandle;
    
    private NetworkManager networkManager;

    private Sprite sprite;

    public void SetSceneAsset(PhotonView photonView) {
        assetPair.Value = new AssetNamePair { groupName = PHOTONVIEW_ID_GROUP, assetName = $"{photonView.sceneViewId}"};
    }

    [ServerRpc]
    public void SetFrozen(bool setFrozen) {
        frozen.Value = setFrozen;
    }
    
    public void InitializeAsset(string groupName, string assetName) {
        assetPair.Value = new AssetNamePair { groupName = groupName, assetName = assetName };
    }
    
    [ServerRpc]
    public void SetAsset(string groupName, string assetName) {
        assetPair.Value = new AssetNamePair { groupName = groupName, assetName = assetName };
    }
    
    protected virtual void Awake() {
        assetPair.OnChange += OnAssetChanged;
        RandomizeGenes();
    }

    private void Start() {
        networkManager = InstanceFinder.NetworkManager;
    }

    private void OnAssetChanged(AssetNamePair prev, AssetNamePair next, bool asServer) {
        if (!asServer && InstanceFinder.ServerManager.Started) {
            return;
        }
        _ = AssetChangedAsync(prev, next, asServer);
    }

    protected virtual async Task AssetChangedAsync(AssetNamePair prev, AssetNamePair next, bool asServer) {
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
    
    public delegate void KoboldAction(NetworkedKobold by);
    public delegate void KoboldReleaseAction(NetworkedKobold by, Vector3 velocity);
    public delegate bool KoboldRequestAction(NetworkedKobold by);
    
    public event KoboldAction grabbed;
    public event KoboldReleaseAction released;
    public event KoboldRequestAction grabRequested;
    
    public event KoboldAction used;
    public event KoboldRequestAction useRequested;

    private Transform grabTransform;
    public void SetGrabTransform(Transform grabTransform) {
        this.grabTransform = grabTransform;
    }

    public bool CanGrab(NetworkedKobold kobold) {
        if (grabRequested != null) {
            return grabRequested.Invoke(kobold);
        }
        return false;
    }
    
    public bool CanUse(NetworkedKobold kobold) {
        if (useRequested != null) {
            return useRequested.Invoke(kobold);
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryUse(NetworkConnection conn = null) {
        if (KoboldPlayerSpawner.TryGetPlayerKobold(conn, out var kobold)) {
            OnUse(kobold);
        }
    }

    [ObserversRpc]
    private void OnUse(NetworkObject kobold) {
        var kob = kobold.GetComponentInChildren<NetworkedKobold>();
        if (kob) {
            used?.Invoke(kob);
        }
    }

    public void SetSprite(Sprite newSprite) {
        sprite = newSprite;
    }

    public Sprite GetSprite() {
        if (sprite) {
            return sprite;
        }
        return GameManager.GetErrorSprite();
    }
    
    [ServerRpc]
    public void OnGrab(NetworkedKobold kobold) {
        grabbed?.Invoke(kobold);
    }
    
    [ServerRpc]
    public void OnRelease(NetworkedKobold kobold, Vector3 velocity) {
        released?.Invoke(kobold, velocity);
    }
    public Transform GrabTransform() {
        if (grabTransform) {
            return grabTransform;
        } else {
            return transform;
        }
    }
    
    [ServerRpc(RequireOwnership = true)]
    public void Equip(NetworkedKobold k, string representedEquipment) {
        if (!k || !k.TryGetKobold(out var kobold)) {
            return;
        }
        // Only successfully equip if we own both the equipment, and the kobold. Otherwise, wait for ownership to successfully transfer
        KoboldInventory inventory = kobold.GetComponent<KoboldInventory>();
        _ = inventory.PickupEquipment(representedEquipment, gameObject);
        InstanceFinder.ServerManager.Despawn(GetComponent<NetworkObject>());
    }
    
}
