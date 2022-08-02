using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class UsableMachine : GenericUsable {
    [SerializeField] protected bool constructed;
    [SerializeField]
    private GameObject[] trackedConstructedObjects;
    public void SetConstructed(bool isConstructed) {
        constructed = isConstructed;
        foreach (GameObject obj in trackedConstructedObjects) {
            obj.SetActive(isConstructed);
        }
    }

    protected virtual void Start() {
        SetConstructed(constructed);
    }

    public override void Save(BinaryWriter writer, string version) {
        base.Save(writer, version);
        writer.Write(constructed);
    }

    public override void Load(BinaryReader reader, string version) {
        base.Load(reader, version);
        SetConstructed(reader.ReadBoolean());
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(constructed);
        } else {
            SetConstructed((bool)stream.ReceiveNext());
        }
    }
}
