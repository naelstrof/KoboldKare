using System;
using System.Collections;
using System.Threading.Tasks;
using FishNet;
using UnityEngine;
using UnityEngine.AddressableAssets;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;

public class KoboldCustomizerSpawner : MonoBehaviour {
    [SerializeField] private PrefabSelectSingleSetting playerSetting; 
    [SerializeField] private PrefabDatabase playerPrefabDatabase;
    private OrbitCameraConfigurationBlend cameraConfiguration;

    private GameObject player;
    private OrbitCameraLockedLerpTrackPivot shoulderPivot;
    private OrbitCameraLockedLerpTrackPivot buttPivot;
    
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
    /// <summary>
    /// Areas in which players may spawn.
    /// </summary>
    [Tooltip("Areas in which players may spawn.")]
    public Transform[] Spawns = new Transform[0];
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
        if (player) {
            _networkManager.ServerManager.Despawn(player);
            Destroy(player);
        }
        if (playerSetting) {
            playerSetting.changed -= OnChangedPlayer;
        }

        if (_networkManager) {
            _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        }
    }

    private void OnDisable() {
        OrbitCamera.RemoveConfiguration(cameraConfiguration);
    }

    private void Awake() {
        InitializeOnce();
        OnSpawned += OnPlayerSpawn;
        playerSetting.changed += OnChangedPlayer;
        shoulderPivot = new GameObject("ShoulderCamPivot", typeof(OrbitCameraLockedLerpTrackPivot)).GetComponent<OrbitCameraLockedLerpTrackPivot>();
        buttPivot = new GameObject("ButtPivot", typeof(OrbitCameraLockedLerpTrackPivot)).GetComponent<OrbitCameraLockedLerpTrackPivot>();
        shoulderPivot.gameObject.SetActive(false);
        buttPivot.gameObject.SetActive(false);
        cameraConfiguration = new OrbitCameraConfigurationBlend();
        cameraConfiguration.SetPivots(shoulderPivot, buttPivot, 0.5f);
        OrbitCamera.AddConfiguration(cameraConfiguration);
    }

    private void OnChangedPlayer(int newValue) {
        if (!player) {
            return;
        }
        shoulderPivot.transform.SetParent(null);
        buttPivot.transform.SetParent(null);
        shoulderPivot.gameObject.SetActive(false);
        buttPivot.gameObject.SetActive(false);
        
        if (player.TryGetComponent<NetworkedKobold>(out var networkedKobold)) {
            if (playerSetting.TryGetPrefab(out string playerPrefabName)) {
                networkedKobold.SetKoboldAssetName(playerPrefabName);
            } else {
                networkedKobold.SetKoboldAssetName("Kobold");
            }
        }
    }


    private void OnPlayerSpawn(NetworkObject obj) {
        player = obj.gameObject;
        var networkedKobold = player.GetComponent<NetworkedKobold>();
        if (playerSetting.TryGetPrefab(out string playerPrefabName)) {
            networkedKobold.SetKoboldAssetName(playerPrefabName);
        } else {
            networkedKobold.SetKoboldAssetName("Kobold");
        }
        networkedKobold.koboldFinishedLoading += (kobold) => {
            var characterDescriptor = kobold.GetComponent<CharacterDescriptor>();
            player.AddComponent<PlayerKoboldLoader>();
            shoulderPivot.SetInfo(new Vector2(0.666f, 0.666f), 2f);
            shoulderPivot.Initialize(characterDescriptor.GetDisplayAnimator(), HumanBodyBones.Head, 1f);

            buttPivot.SetInfo(new Vector2(0.666f, 0.333f), 2f);
            buttPivot.Initialize(characterDescriptor.GetDisplayAnimator(), HumanBodyBones.Hips, 1f);
            shoulderPivot.gameObject.SetActive(true);
            buttPivot.gameObject.SetActive(true);

            characterDescriptor.GetDisplayAnimator().gameObject.AddComponent<LookAtCursor>();
        };
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
    }

    /// <summary>
    /// Called when a client loads initial scenes after connecting.
    /// </summary>
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer) {
        if (!asServer) {
            return;
        }

        if (!_playerPrefab) {
            _networkManager.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }

        Vector3 position;
        Quaternion rotation;
        SetSpawn(_playerPrefab.transform, out position, out rotation);

        NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, position, rotation, true);
        _networkManager.ServerManager.Spawn(nob, conn);

        // If there are no global scenes 
        if (_addToDefaultScene) {
            _networkManager.SceneManager.AddOwnerToDefaultScene(nob);
        }

        OnSpawned?.Invoke(nob);
    }

    /// <summary>
    /// Sets a spawn position and rotation.
    /// </summary>
    /// <param name = "pos"></param>
    /// <param name = "rot"></param>
    private void SetSpawn(Transform prefab, out Vector3 pos, out Quaternion rot) {
        // No spawns specified.
        if (Spawns.Length == 0) {
            SetSpawnUsingPrefab(prefab, out pos, out rot);
            return;
        }

        Transform result = Spawns[_nextSpawn];
        if (!result) {
            SetSpawnUsingPrefab(prefab, out pos, out rot);
        } else {
            pos = result.position;
            rot = result.rotation;
        }

        // Increase next spawn and reset if needed.
        _nextSpawn++;
        if (_nextSpawn >= Spawns.Length) {
            _nextSpawn = 0;
        }
    }

    /// <summary>
    /// Sets spawn using values from prefab.
    /// </summary>
    /// <param name = "prefab"></param>
    /// <param name = "pos"></param>
    /// <param name = "rot"></param>
    private void SetSpawnUsingPrefab(Transform prefab, out Vector3 pos, out Quaternion rot) {
        pos = prefab.position;
        rot = prefab.rotation;
    }
}
