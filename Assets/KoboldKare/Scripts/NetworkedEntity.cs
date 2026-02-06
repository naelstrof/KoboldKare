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
    public readonly SyncVar<bool> planted = new SyncVar<bool>();
    
    public const string PHOTONVIEW_ID_GROUP = "PHOTONVIEW_ID_GROUP";

    private GameObject entityInstance;
    private GameObject mapInstance;
    
    private AssetGroup.AssetLocation.AssetHandle<GameObject> entityHandle;
    private AssetGroup.AssetLocation.AssetHandle<ScriptablePlant> plantHandle;
    
    private NetworkManager networkManager;

    private Sprite sprite;
    
    public event System.Action<NetworkedKobold> weaponFireStart;
    public event System.Action<NetworkedKobold> weaponFireEnd;

    public void SetSceneAsset(PhotonView photonView) {
        assetPair.Value = new AssetNamePair { groupName = PHOTONVIEW_ID_GROUP, assetName = $"{photonView.sceneViewId}"};
    }

    [ObserversRpc]
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

    public override void SetInstantiationData(KoboldEntitySpawner.NetworkedEntityInstantiationData data) {
        base.SetInstantiationData(data);
        assetPair.Value = new AssetNamePair { groupName = data.groupName, assetName = data.assetName };
    }
    
    protected override void Awake() {
        base.Awake();
        assetPair.OnChange += OnAssetChanged;
        RandomizeGenes();
    }

    protected override void Start() {
        base.Awake();
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
            entityHandle = null;
        }

        if (plantHandle != null) {
            plantHandle.Release();
            plantHandle = null;
        }

        try {
            await TryInitializeEntity(next.groupName, next.assetName);
        } catch (Exception e) {
            Debug.LogException(e);
            Debug.LogError($"Failed to initialize entity with asset id {next.groupName}:{next.assetName}.");
            if (entityInstance) {
                Destroy(entityInstance);
                entityInstance = null;
            }
            entityInstance = Instantiate(GameManager.GetErrorGeneric(), transform);
        }
    }

    private async Task TryInitializeEntity(string groupName, string assetName) {
        switch (groupName) {
            default:
                try {
                    entityHandle = await KoboldKareObjectPostProcessor.GetAssetAsync(groupName, assetName, GameManager.GetErrorGeneric());
                    entityInstance = Instantiate(entityHandle.asset, transform);
                } catch (Exception e) {
                    if (entityInstance) {
                        Destroy(entityInstance);
                        entityInstance = null;
                    }
                    Debug.LogException(e);
                    Debug.LogError($"Failed to initialize entity with asset id {groupName}:{assetName}, loading error instead.");
                    entityInstance = Instantiate(GameManager.GetErrorGeneric(), transform);
                }
                entityInstance.transform.localPosition = Vector3.zero;
                entityInstance.transform.localRotation = Quaternion.identity;
                break;
            case "Plant":
                try {
                    plantHandle = await KoboldKareObjectPostProcessor.GetAssetAsync(groupName, assetName, GameManager.GetErrorPlant());
                    if (plantHandle.asset.display) {
                        entityInstance = Instantiate(plantHandle.asset.display, transform);
                    } else {
                        entityInstance = Instantiate(GameManager.GetErrorGeneric(), transform);
                        Debug.LogError("Failed to initialize plant display, not found.");
                    }
                } catch (Exception e) {
                    if (entityInstance) {
                        Destroy(entityInstance);
                        entityInstance = null;
                    }
                    Debug.LogException(e);
                    Debug.LogError($"Failed to initialize entity with asset id {groupName}:{assetName}, loading error instead.");
                    entityInstance = Instantiate(GameManager.GetErrorGeneric(), transform);
                }
                entityInstance.transform.localPosition = Vector3.zero;
                entityInstance.transform.localRotation = Quaternion.identity;
                break;
            case PHOTONVIEW_ID_GROUP:
                if (!int.TryParse(assetName, out int photonViewID)) {
                    Debug.LogError($"Failed to parse photonViewID from asset name {groupName}:{assetName}");
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
                
                foreach (var reagentContainer in mapInstance.GetComponentsInChildren<GenericReagentContainer>()) {
                    foreach (var reagent in reagentContainer.startingReagents) {
                        AddMix(reagent.reagent.GetReagent(reagent.volume), InjectType.Inject);
                    }
                }
                break;
        }
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
        if (KoboldEntitySpawner.TryGetPlayerKobold(conn, out var kobold)) {
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
    
    [ObserversRpc]
    public void PlantRPC(NetworkObject seed, string plantName) {
        if (!networkManager.ServerManager.Started) {
            return;
        }

        SoilTile tile = GetComponentInChildren<SoilTile>();
        if (tile == null) {
            return;
        }

        if (seed == null) {
            return;
        }
        
        
        var data = KoboldEntitySpawner.NetworkedEntityInstantiationData.Default();
        data.groupName = "Plant";
        data.assetName = plantName;
        data.position = tile.GetPlantPosition();
        data.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        var geneHolder = seed.GetComponentInChildren<GeneHolder>();
        if (geneHolder == null) {
            Debug.LogError("Failed to find gene holder for seed", seed.gameObject);
            return;
        }
        data.CopyGenesFrom(geneHolder);
        var nob = networkManager.GetComponent<KoboldEntitySpawner>().SpawnAsServer(data, false);
        networkManager.ServerManager.Despawn(seed);

        planted.Value = true;
    }

    [ServerRpc]
    public void Destroy() {
        networkManager.ServerManager.Despawn(GetComponent<NetworkObject>());
    }

    [ObserversRpc]
    public void OnFire(NetworkedKobold kobold) {
        weaponFireStart?.Invoke(kobold);
    }
    
    [ObserversRpc]
    public void OnEndFire(NetworkedKobold kobold) {
        weaponFireEnd?.Invoke(kobold);
    }
    
}
