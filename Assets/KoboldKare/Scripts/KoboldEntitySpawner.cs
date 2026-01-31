using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Broadcast;
using UnityEngine;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;

public class KoboldEntitySpawner : MonoBehaviour {
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
    private NetworkObject entityPrefab;

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
        _networkManager.ServerManager.RegisterBroadcast<NetworkedEntityInstantiationData>(OnSpawnBroadcast);
    }

    public struct NetworkedEntityInstantiationData : IBroadcast {
        public string groupName;
        public string assetName;
        public Vector3 position;
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
                grabCount = 1
            };
        }
    }

    public NetworkObject SpawnAsServer(NetworkedEntityInstantiationData data, bool randomizedGenes) {
        if (!GameManager.InLevel()) {
            return null;
        }

        if (!entityPrefab) {
            _networkManager.LogWarning($"Entity prefab is empty and cannot be spawned for connection.");
            return null;
        }

        NetworkObject nob = _networkManager.GetPooledInstantiated(entityPrefab, data.position, data.rotation, true);
        nob.GetComponent<NetworkedEntity>().SetInstantiationAsset(data.groupName, data.assetName);
        if (randomizedGenes) {
            var geneHolder = nob.GetComponentInChildren<GeneHolder>();
            geneHolder.RandomizeGenes();
        }
        _networkManager.ServerManager.Spawn(nob);

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

        NetworkObject nob = _networkManager.GetPooledInstantiated(entityPrefab, data.position, data.rotation, true);
        nob.GetComponent<NetworkedEntity>().SetInstantiationAsset(data.groupName, data.assetName);
        _networkManager.ServerManager.Spawn(nob, conn);

        // If there are no global scenes 
        if (_addToDefaultScene) {
            _networkManager.SceneManager.AddOwnerToDefaultScene(nob);
        }

        OnSpawned?.Invoke(nob);
    }
}
