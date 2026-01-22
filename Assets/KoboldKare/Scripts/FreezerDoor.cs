using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using KoboldKare;
using SimpleJSON;

public class FreezerDoor : GenericDoor {
    public bool shouldSpawnIceCube {get; set;}
    public PhotonGameObjectReference iceCube;
    public GameEventGeneric midnight;
    private NetworkedEntity networkedEntity;
    private bool iceCubeSpawned = false;
    public override void Start() {
        base.Start();
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    private bool OnUseRequested(NetworkedKobold by) {
        return true;
    }

    private void MidnightEvent() {
        // FIXME FISHNET
        /*
        if (photonView.IsMine) {
            iceCubeSpawned = false;
        }*/
    }
    private void OnUse(NetworkedKobold by) {
        // FIXME FISHNET
        /*
        if (photonView.IsMine && shouldSpawnIceCube && !iceCubeSpawned) {
            iceCubeSpawned = true;
            PhotonNetwork.Instantiate(iceCube.photonName, transform.position, Quaternion.identity);
        }*/
    }
    // FIXME FISHNET
    /*
    public override Task Load(JSONNode node) {
        base.Load(node);
        shouldSpawnIceCube = node["shouldSpawnIceCube"];
        iceCubeSpawned = node["iceCubeSpawned"];
        return Task.CompletedTask;
    }
    public override void Save(JSONNode node) {
        base.Save(node);
        node["shouldSpawnIceCube"] = shouldSpawnIceCube;
        node["iceCubeSpawned"] = iceCubeSpawned;
    }*/
    // FIXME FISHNET
    /*
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(shouldSpawnIceCube);
            stream.SendNext(iceCubeSpawned);
        } else {
            shouldSpawnIceCube = (bool)stream.ReceiveNext();
            iceCubeSpawned = (bool)stream.ReceiveNext();
            PhotonProfiler.LogReceive(sizeof(bool) * 2);
        }
    }*/
}
