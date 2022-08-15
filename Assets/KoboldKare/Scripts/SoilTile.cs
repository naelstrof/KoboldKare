using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;

public class SoilTile : MonoBehaviourPun, IPunObservable, ISavable {
    private Plant planted;
    [SerializeField]
    private bool hasDebris = false;
    [SerializeField]
    private List<GameObject> debris;
    [SerializeField]
    private PhotonGameObjectReference plantPrefab;
    [PunRPC]
    public void SetDebris(bool newHasDebris) {
        hasDebris = newHasDebris;
        foreach (GameObject obj in debris) {
            obj.SetActive(hasDebris);
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
        }
    }
    
    [PunRPC]
    public void PlantRPC(int seed, short plantID, KoboldGenes myGenes) {
        PhotonView seedView = PhotonNetwork.GetPhotonView(seed);
        if (seedView != null && seedView.IsMine) {
            PhotonNetwork.Destroy(seedView);
        }
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }

        GameObject obj = PhotonNetwork.InstantiateRoomObject(plantPrefab.photonName, GetPlantPosition(),
            Quaternion.LookRotation(Vector3.forward, Vector3.up), 0,
            new object[] { plantID, myGenes });
        photonView.RPC(nameof(SoilTile.SetPlantedRPC), RpcTarget.All,
            obj.GetComponent<Plant>().photonView.ViewID);
    }


    public void Save(BinaryWriter writer, string version) {
        writer.Write(hasDebris);
        if (planted == null) {
            writer.Write((int)-1);
        } else {
            writer.Write(planted.photonView.ViewID);
        }
    }

    public void Load(BinaryReader reader, string version) {
        SetDebris(reader.ReadBoolean());
        int viewID = reader.ReadInt32();
        SetPlantedRPC(viewID);
    }

    private void OnValidate() {
        plantPrefab.OnValidate();
    }
}
