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

    public void SetPlanted(Plant plant) {
        if (planted != null) {
            PhotonNetwork.Destroy(planted.gameObject);
        }
        
        planted = plant;
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
            if (planted == null) {
                stream.SendNext(-1);
            } else {
                stream.SendNext(planted.photonView.ViewID);
            }
        } else {
            SetDebris((bool)stream.ReceiveNext());
            int viewID = (int)stream.ReceiveNext();
            SetPlanted(viewID == -1 ? null : PhotonNetwork.GetPhotonView(viewID).GetComponent<Plant>());
        }
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
        SetPlanted(viewID == -1 ? null : PhotonNetwork.GetPhotonView(viewID).GetComponent<Plant>());
    }
}
