using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon.StructWrapping;
using NetStack.Serialization;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;

public class SoilTile : MonoBehaviourPun, IPunObservable, ISavable {
    private Plant planted;
    [SerializeField]
    private bool hasDebris = false;
    [SerializeField]
    private List<GameObject> debris;
    [SerializeField]
    private PhotonGameObjectReference plantPrefab;

    public delegate void FarmTileClearedAction(SoilTile tile);
    public static event FarmTileClearedAction tileCleared;
    
    [PunRPC]
    public void SetDebris(bool newHasDebris) {
        PhotonProfiler.LogReceive(sizeof(bool));
        hasDebris = newHasDebris;
        foreach (GameObject obj in debris) {
            obj.SetActive(hasDebris);
        }

        if (hasDebris == false) {
            tileCleared?.Invoke(this);
        }
    }

    public bool GetPlantable() {
        return (planted == null || planted.plant.possibleNextGenerations.Length == 0) && !hasDebris;
    }

    [PunRPC]
    public void SetPlantedRPC(int viewID) {
        if (viewID == -1) {
            planted = null;
        }

        PhotonView view = PhotonNetwork.GetPhotonView(viewID);
        if (view == null) {
            return;
        }

        if (view.TryGetComponent(out Plant plant)) {
            if (planted != null) {
                if (planted.photonView.IsMine) {
                    PhotonNetwork.Destroy(planted.gameObject);
                }
            }
            planted = plant;
        }
    }

    public Vector3 GetPlantPosition() {
        return transform.position + Vector3.up * 0.35f;
    }

    private void Awake() {
        SetDebris(hasDebris);
    }

    public bool GetDebris() {
        return hasDebris;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(hasDebris);
        } else {
            SetDebris((bool)stream.ReceiveNext());
            PhotonProfiler.LogReceive(sizeof(bool));
        }
    }
    
    [PunRPC]
    public void PlantRPC(int seed, BitBuffer spawnData) {
        PhotonView seedView = PhotonNetwork.GetPhotonView(seed);
        if (seedView != null && seedView.IsMine) {
            PhotonNetwork.Destroy(seedView);
        }
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }

        //BitBuffer spawnData = new BitBuffer(16);
        //spawnData.AddShort(plantID);
        //spawnData.AddKoboldGenes(myGenes);
        GameObject obj = PhotonNetwork.InstantiateRoomObject(plantPrefab.photonName, GetPlantPosition(),
            Quaternion.LookRotation(Vector3.forward, Vector3.up), 0,
            new object[] { spawnData });
        photonView.RPC(nameof(SoilTile.SetPlantedRPC), RpcTarget.All,
            obj.GetComponent<Plant>().photonView.ViewID);
    }


    public void Save(JSONNode node) {
        node["hasDebris"] = hasDebris;
        if (planted == null) {
            node["planted"] = -1;
        } else {
            node["planted"] = planted.photonView.ViewID;
        }
    }

    public void Load(JSONNode node) {
        SetDebris(node["hasDebris"]);
        int viewID = node["planted"];
        SetPlantedRPC(viewID);
    }

    private void OnValidate() {
        plantPrefab.OnValidate();
    }
}
