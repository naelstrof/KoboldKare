using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;

public class SoilTile : MonoBehaviourPun, IPunObservable, ISavable {
    [SerializeField]
    private bool plantable = false;
    [SerializeField]
    private List<GameObject> debris;
    private void SetPlantable(bool newPlantable) {
        plantable = newPlantable;
        foreach (GameObject obj in debris) {
            obj.SetActive(!plantable);
        }
    }

    public Vector3 GetPlantPosition() {
        return transform.position + Vector3.up * 0.35f;
    }

    private void Awake() {
        SetPlantable(plantable);
    }

    public bool GetPlantable() {
        return plantable;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(plantable);
        } else {
            SetPlantable((bool)stream.ReceiveNext());
        }
    }

    public void Save(BinaryWriter writer, string version) {
        writer.Write(plantable);
    }

    public void Load(BinaryReader reader, string version) {
        SetPlantable(reader.ReadBoolean());
    }
}
