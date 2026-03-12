using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Broadcast;
using UnityEngine;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;

public class KoboldEntitySpawner : MonoBehaviour {
    private static Dictionary<int, NetworkObject> playerKobolds;
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
    [SerializeField] private NetworkObject entityPrefab;
    
    [SerializeField] private NetworkObject playerPrefab;
    
    [SerializeField] private PrefabSelectSingleSetting playerSetting; 
    [SerializeField] private PrefabDatabase playerPrefabDatabase;

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
    #endregion

    private void Awake() {
        InitializeOnce();
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
        _networkManager.ServerManager.RegisterBroadcast<NetworkedEntityInstantiationData>(OnSpawnBroadcast);
        playerKobolds = new ();
    }

    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer) {
        if (asServer) {
            return;
        }

        if (!GameManager.InLevel()) {
            return;
        }

        if (!playerPrefab) {
            _networkManager.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }
        
        _networkManager.ClientManager.Broadcast(PlayerKoboldLoader.GetPlayerInstantiationData()); 
    }

    public struct NetworkedEntityInstantiationData : IBroadcast {
        public string groupName;
        public string assetName;
        public Vector3 position;
        public Vector3 velocity;
        public Quaternion rotation;

        public string species;
        public float maxEnergy;
        public float baseSize;
        public float fatSize;
        public float ballSize;
        public float dickSize;
        public float breastSize;
        public float bellySize;
        public float metabolizeCapacitySize;
        public float dickThickness;
        public byte hue;
        public byte clothingHue;
        public byte brightness;
        public byte saturation;
        public string dickEquip;
        public byte grabCount;

        public ReagentContents reagentContents;

        public void CopyGenesFrom(GeneHolder holder) {
            maxEnergy = holder.maxEnergy.Value;
            baseSize = holder.baseSize.Value;
            fatSize = holder.fatSize.Value;
            ballSize = holder.ballSize.Value;
            dickSize = holder.dickSize.Value;
            breastSize = holder.breastSize.Value;
            bellySize = holder.bellySize.Value;
            metabolizeCapacitySize = holder.metabolizeCapacitySize.Value;
            dickThickness = holder.dickThickness.Value;
            hue = holder.hue.Value;
            clothingHue = holder.clothingHue.Value;
            brightness = holder.brightness.Value;
            saturation = holder.saturation.Value;
            dickEquip = holder.dickEquip.Value;
            grabCount = holder.grabCount.Value;
            species = holder.species.Value;
        }
        
        public static NetworkedEntityInstantiationData Default() {
            return new NetworkedEntityInstantiationData() {
                groupName = "Error",
                assetName = "Error",
                velocity = Vector3.zero,
                species = "Kobold",
                position = Vector3.zero,
                rotation = Quaternion.identity,
                maxEnergy = 5f,
                baseSize = 20f,
                fatSize = 0f,
                ballSize = 0f,
                dickSize = 0f,
                breastSize = 0f,
                bellySize = 20f,
                metabolizeCapacitySize = 20f,
                dickThickness = 0f,
                hue = 0,
                clothingHue = 0,
                brightness = 128,
                saturation = 128,
                dickEquip = "None",
                grabCount = 1,
                reagentContents = new ReagentContents(20f)
            };
        }
    }

    public NetworkObject SpawnAsServer(NetworkedEntityInstantiationData data, bool randomizedGenes, NetworkConnection conn = null) {
        if (!GameManager.InLevel()) {
            return null;
        }

        if (!entityPrefab) {
            _networkManager.LogWarning($"Entity prefab is empty and cannot be spawned for connection.");
            return null;
        }

        NetworkObject nob = _networkManager.GetPooledInstantiated(data.groupName == "PlayableCharacter" ? playerPrefab : entityPrefab, data.position, data.rotation, true);
        var geneHolder = nob.GetComponentInChildren<GeneHolder>();
        geneHolder.SetInstantiationData(data);
        if (randomizedGenes) {
            geneHolder.RandomizeGenes();
        }
        _networkManager.ServerManager.Spawn(nob, conn);

        OnSpawned?.Invoke(nob);
        return nob;
    }
    
    private void OnSpawnBroadcast(NetworkConnection conn, NetworkedEntityInstantiationData data, Channel channel) {
        if (!GameManager.InLevel()) {
            return;
        }

        if (!entityPrefab) {
            _networkManager.LogWarning($"Entity prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }
        var isPlayer = data.groupName == "PlayableCharacter";
        SceneDescriptor.GetSpawnLocationAndRotation(out var pos, out Quaternion quat);

        NetworkObject nob = _networkManager.GetPooledInstantiated(isPlayer ? playerPrefab : entityPrefab, isPlayer ? pos : data.position, isPlayer ? quat : data.rotation, true);
        var geneHolder = nob.GetComponentInChildren<GeneHolder>();
        geneHolder.SetInstantiationData(data);
        _networkManager.ServerManager.Spawn(nob, conn);
        
        if (data.groupName == "PlayableCharacter") {
            playerKobolds.TryAdd(conn.ClientId, nob);
        }

        // If there are no global scenes 
        if (_addToDefaultScene) {
            _networkManager.SceneManager.AddOwnerToDefaultScene(nob);
        }

        OnSpawned?.Invoke(nob);
    }

    public static bool GetIsPlayerKobold(NetworkedKobold o) {
        if (playerKobolds.TryGetValue(o.OwnerId, out var nob)) {
            return (nob.TryGetComponent(out NetworkedKobold test) && test == o);
        }
        return false;
    }

    public static bool TryGetPlayerKobold(NetworkConnection conn, out NetworkedKobold o) {
        if (playerKobolds.TryGetValue(conn.ClientId, out var nob)) {
            return nob.TryGetComponent(out o);
        }
        o = null;
        return false;
    }
}
