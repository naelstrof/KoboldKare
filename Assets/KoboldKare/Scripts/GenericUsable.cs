using KoboldKare;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class GenericUsable : MonoBehaviourPun, ISavable, IPunObservable {
    public virtual Sprite GetSprite(Kobold k) { return null; }
    public virtual bool CanUse(Kobold k) { return true; }

    // Called only by us locally when the player tries to use an object. By default we try to inform everyone that we used it.
    public virtual void LocalUse(Kobold k) {
        photonView.RPC("RPCUse", RpcTarget.AllBufferedViaServer, new object[]{});
    }
    // Called globally by all clients, synced.
    public virtual void Use() { }

    // A passthrough to call from RPC
    [PunRPC]
    public void RPCUse() {
        Use();
    }
    public virtual void Save(BinaryWriter writer, string version) { }
    public virtual void Load(BinaryReader reader, string version) { }
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    }
}
