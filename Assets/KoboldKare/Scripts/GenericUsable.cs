using KoboldKare;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class GenericUsable : MonoBehaviourPun, IPunObservable, ISavable {
    protected int usedCount;
    public virtual Sprite GetSprite(Kobold k) {
        return null;
    }
    public virtual bool CanUse(Kobold k) {
        return true;
    }
    public virtual void Use(Kobold k) {
        usedCount++;
    }
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(usedCount);
        } else {
            int otherUseCount = (int)stream.ReceiveNext();
            for (int i=usedCount;i<otherUseCount;i++) {
                Use(null);
            }
        }
    }
    public virtual void Save(BinaryWriter writer, string version) {
        writer.Write(usedCount);
    }
    public virtual void Load(BinaryReader reader, string version) {
        usedCount = 0;
        int otherUseCount = reader.ReadInt32();
        for (int i=usedCount;i<otherUseCount;i++) {
            Use(null);
        }
        usedCount = otherUseCount;
    }
}
