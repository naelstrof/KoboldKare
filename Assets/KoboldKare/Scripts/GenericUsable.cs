using KoboldKare;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

public class GenericUsable : MonoBehaviourPun, ISavable, IPunObservable {
    public virtual Sprite GetSprite(Kobold k) { return null; }
    public virtual bool CanUse(Kobold k) { return true; }

    // Called only by us locally when the player tries to use an object. By default we try to inform everyone that we used it.
    public virtual void LocalUse(Kobold k) {
        photonView.RPC(nameof(GenericUsable.RPCUse), RpcTarget.All);
    }
    // Called globally by all clients, synced.
    public virtual void Use() { }

    private void Start() {
        if (photonView == null) {
            gameObject.AddComponent<PhotonView>();
        }
    }

    // A passthrough to call from RPC
    [PunRPC]
    public void RPCUse() {
        PhotonProfiler.LogReceive(1);
        Use();
    }
    public virtual void Save(JSONNode node) { }
    public virtual void Load(JSONNode node) { }
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    }
}
