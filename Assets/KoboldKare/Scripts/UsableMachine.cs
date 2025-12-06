using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;

public class UsableMachine : GenericUsable {
    [SerializeField] protected bool constructed;
    [SerializeField]
    private GameObject[] trackedConstructedObjects;
    public virtual void SetConstructed(bool isConstructed) {
        constructed = isConstructed;
        foreach (GameObject obj in trackedConstructedObjects) {
            obj.SetActive(isConstructed);
        }
    }

    protected virtual void Start() {
        SetConstructed(constructed);
    }

    public override void Save(JSONNode node) {
        base.Save(node);
        node["constructed"] = constructed;
    }

    public override Task Load(JSONNode node) {
        base.Load(node);
        if (node.HasKey("constructed")) {
            SetConstructed(node["constructed"]);
        }
        return Task.CompletedTask;
    }

    /*public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(constructed);
        } else {
            SetConstructed((bool)stream.ReceiveNext());
            PhotonProfiler.LogReceive(sizeof(bool));
        }
    }*/
}
