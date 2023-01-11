using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using KoboldKare;

[RequireComponent(typeof(Photon.Pun.PhotonView))]
public class IceSpawnerUsable : GenericUsable {
    [SerializeField]
    private float cost = 20f;
    [SerializeField]
    private MoneyFloater floater;
    [SerializeField]
    private Sprite buySprite;
    [SerializeField]
    private Transform spawnLocation;
    public PhotonGameObjectReference prefabSpawn;
    void Start() {
        Bounds newBounds = new Bounds(transform.position, Vector3.zero);
        foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
            newBounds.Encapsulate(r.bounds);
        }
        floater.SetBounds(newBounds);
        floater.SetText(cost.ToString());
    }
    public override bool CanUse(Kobold k) {
        return k.GetComponent<MoneyHolder>().HasMoney(cost);
    }
    public override Sprite GetSprite(Kobold k) {
        return buySprite;
    }
    public override void LocalUse(Kobold k) {
        k.GetComponent<MoneyHolder>().ChargeMoney(cost);
        photonView.RPC("RPCUse", RpcTarget.All);
    }
    [PunRPC]
    public override void Use() {
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.InstantiateRoomObject(prefabSpawn.photonName, spawnLocation.position, spawnLocation.rotation);
        }
        PhotonProfiler.LogReceive(1);
    }
    void OnValidate() {
        prefabSpawn.OnValidate();
    }
}
