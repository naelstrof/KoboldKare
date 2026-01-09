using System;
using System.Collections;
using System.Threading.Tasks;
using FishNet;
using UnityEngine;
using UnityEngine.AddressableAssets;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine.SceneManagement;

public class KoboldPlayerSpawner : MonoBehaviour {
    [SerializeField] private PrefabSelectSingleSetting playerSetting; 
    [SerializeField] private PrefabDatabase playerPrefabDatabase;
    
    #region Public.
    /// <summary>
    /// Called on the server when a player is spawned.
    /// </summary>
    public event Action<NetworkObject> OnSpawned;
    #endregion

    #region Serialized.
    /// <summary>
    /// Prefab to spawn for the player.
    /// </summary>
    [Tooltip("Prefab to spawn for the player.")]
    [SerializeField]
    private NetworkObject _playerPrefab;

    /// <summary>
    /// Sets the PlayerPrefab to use.
    /// </summary>
    /// <param name = "nob"></param>
    public void SetPlayerPrefab(NetworkObject nob) => _playerPrefab = nob;

    /// <summary>
    /// True to add player to the active scene when no global scenes are specified through the SceneManager.
    /// </summary>
    [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
    [SerializeField]
    private bool _addToDefaultScene = true;
    #endregion

    #region Private.
    /// <summary>
    /// First instance of the NetworkManager found. This will be either the NetworkManager on or above this object, or InstanceFinder.NetworkManager.
    /// </summary>
    private NetworkManager _networkManager;
    /// <summary>
    /// Next spawns to use.
    /// </summary>
    private int _nextSpawn;
    #endregion

    private void OnDestroy() {
        OnSpawned -= OnPlayerSpawn;
        if (_networkManager) {
            _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        }
    }

    private void Awake() {
        InitializeOnce();
        OnSpawned += OnPlayerSpawn;
    }

    private void OnPlayerSpawn(NetworkObject obj) {
        if (obj.IsOwner) {
            var networkedKobold = obj.GetComponent<NetworkedKobold>();
            if (playerSetting.TryGetPrefab(out string playerPrefabName)) {
                networkedKobold.SetAsset("PlayableCharacter", playerPrefabName);
            } else {
                networkedKobold.SetAsset("PlayableCharacter", "Kobold");
            }
        }
    }

    /// <summary>
    /// Initializes this script for use.
    /// </summary>
    private void InitializeOnce() {
        _networkManager = GetComponentInParent<NetworkManager>();
        if (_networkManager == null) {
            _networkManager = InstanceFinder.NetworkManager;
        }

        if (_networkManager == null) {
            _networkManager.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }
        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        _networkManager.ServerManager.RegisterBroadcast<NetworkedKobold.NetworkedKoboldInstantiationData>(OnSpawnBroadcast);
    }

    
    private void OnSpawnBroadcast(NetworkConnection conn, NetworkedKobold.NetworkedKoboldInstantiationData data, Channel channel) {
        if (!GameManager.InLevel()) {
            return;
        }

        if (!_playerPrefab) {
            _networkManager.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }
        
        // FIXME fishnet: needs validation to prevent spam
        Vector3 position;
        Quaternion rotation;
        SceneDescriptor.GetSpawnLocationAndRotation(out position, out rotation);

        data.controlType = NetworkedKobold.ControlType.NetworkedPlayer;

        NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, position, rotation, true);
        nob.GetComponent<NetworkedKobold>().SetInstantiationData(data);
        _networkManager.ServerManager.Spawn(nob, conn);

        // If there are no global scenes 
        if (_addToDefaultScene) {
            _networkManager.SceneManager.AddOwnerToDefaultScene(nob);
        }

        OnSpawned?.Invoke(nob);
    }

    /// <summary>
    /// Called when a client loads initial scenes after connecting.
    /// </summary>
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer) {
        if (asServer) {
            return;
        }

        if (!GameManager.InLevel()) {
            return;
        }

        if (!_playerPrefab) {
            _networkManager.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }
        
        _networkManager.ClientManager.Broadcast(PlayerKoboldLoader.GetPlayerInstantiationData());
    }
}
