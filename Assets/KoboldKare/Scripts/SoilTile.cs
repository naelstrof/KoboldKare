using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FishNet;
using FishNet.Object;
using NetStack.Serialization;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;

public class SoilTile : MonoBehaviour, ISavable {
    [SerializeField]
    private bool hasDebris = false;
    [SerializeField]
    private List<GameObject> debris;

    private NetworkedEntity networkedEntity;

    public delegate void FarmTileClearedAction(SoilTile tile);
    public static event FarmTileClearedAction tileCleared;
    
    // FIXME FISHNET
    //[PunRPC]
    public void SetDebris(bool newHasDebris) {
        hasDebris = newHasDebris;
        foreach (GameObject obj in debris) {
            obj.SetActive(hasDebris);
        }

        if (hasDebris == false) {
            tileCleared?.Invoke(this);
        }
    }

    public bool GetPlantable() {
        return networkedEntity.planted.Value == false;
    }

    public Vector3 GetPlantPosition() {
        return transform.position + Vector3.up * 0.35f;
    }

    private void Awake() {
        SetDebris(hasDebris);
    }

    void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
    }

    public bool GetDebris() {
        return hasDebris;
    }

    
    // FIXME FISHNET
    /*
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(hasDebris);
        } else {
            SetDebris((bool)stream.ReceiveNext());
            PhotonProfiler.LogReceive(sizeof(bool));
        }
    }
    */
    
    
    // FIXME FISHNET
    //[PunRPC]

    // FIXME FISHNET
    public void Save(JSONNode node) {
        /*node["hasDebris"] = hasDebris;
        if (planted == null) {
            node["planted"] = -1;
        } else {
            //node["planted"] = planted.photonView.ViewID;
        }*/
    }

    public Task Load(JSONNode node) {
        /*SetDebris(node["hasDebris"]);
        int viewID = node["planted"];
        SetPlantedRPC(viewID);*/
        return Task.CompletedTask;
    }
}
