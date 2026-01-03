using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using Photon.Pun;
using UnityEngine;

public class FishnetBackwardCompatibility : MonoBehaviour {
    [SerializeField] private GameObject networkedEntityPrefab;
    
    private NetworkManager _networkManager;
    private static Dictionary<int, PhotonView> photonViews = new();
    
    private void Awake() {
        PhotonView.OnPhotonViewAdd += OnPhotonViewAdd;
        PhotonView.OnPhotonViewAdd += OnPhotonViewRemove;
        InitializeOnce();
    }

    private void OnDestroy() {
        PhotonView.OnPhotonViewAdd -= OnPhotonViewAdd;
        PhotonView.OnPhotonViewAdd -= OnPhotonViewRemove;
    }

    private void InitializeOnce() {
        _networkManager = GetComponentInParent<NetworkManager>();
        if (_networkManager == null) {
            _networkManager = InstanceFinder.NetworkManager;
        }

        if (_networkManager == null) {
            _networkManager.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
        }
    }

    private void OnPhotonViewRemove(PhotonView obj) {
        if (photonViews.ContainsKey(obj.GetInstanceID())) {
            photonViews.Remove(obj.GetInstanceID());
        }
    }

    private void OnPhotonViewAdd(PhotonView obj) {
        if (photonViews.ContainsKey(obj.GetInstanceID())) {
            return;
        }
        if (obj.transform.parent !=null && obj.transform.parent.GetComponentInParent<PhotonView>() != null) {
            return;
        }

        if (obj.GetComponentInParent<GeneHolder>() != null || obj.GetComponent<GeneHolder>() != null) {
            return;
        }

        // Not a scene view
        if (obj.sceneViewId == 0) {
            return;
        }
        
        photonViews.Add(obj.GetInstanceID(), obj);
        if (_networkManager.IsServerStarted) {
            NetworkObject nob = _networkManager.GetPooledInstantiated(networkedEntityPrefab, obj.transform.position, obj.transform.rotation, true);
            if (!nob.TryGetComponent(out NetworkedEntity networkedEntity)) {
                Debug.LogError("Networked entity prefab does not have NetworkedEntity component!");
                return;
            }
            networkedEntity.SetSceneAsset(obj);
            _networkManager.ServerManager.Spawn(nob);
        } else {
            obj.gameObject.SetActive(false);
        }
    }
}
