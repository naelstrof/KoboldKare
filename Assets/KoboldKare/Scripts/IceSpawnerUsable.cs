using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using KoboldKare;

[RequireComponent(typeof(Photon.Pun.PhotonView))]
public class IceSpawnerUsable : MonoBehaviour {
    [SerializeField]
    private float cost = 20f;
    [SerializeField]
    private MoneyFloater floater;
    [SerializeField]
    private Sprite buySprite;
    [SerializeField]
    private Transform spawnLocation;
    public PhotonGameObjectReference prefabSpawn;
    private NetworkedEntity networkedEntity;
    void Start() {
        Bounds newBounds = new Bounds(transform.position, Vector3.zero);
        foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
            newBounds.Encapsulate(r.bounds);
        }
        floater.SetBounds(newBounds);
        floater.SetText(cost.ToString());
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(buySprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }
    private bool OnUseRequested(NetworkedKobold k) {
        return k.TryGetKobold(out var kobold) && kobold.GetComponent<MoneyHolder>().HasMoney(cost);
    }
    private void OnUse(NetworkedKobold k) {
        if (k.TryGetKobold(out var kobold) && kobold.GetComponent<MoneyHolder>().ChargeMoney(cost)) {
            // FIXME FISHNET
            //PhotonNetwork.InstantiateRoomObject(prefabSpawn.photonName, spawnLocation.position, spawnLocation.rotation);
        }
    }
}
