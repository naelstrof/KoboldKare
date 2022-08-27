using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using KoboldKare;

public class FreezerDoor : GenericDoor, IPunObservable, ISavable {
    public bool shouldSpawnIceCube {get; set;}
    public PhotonGameObjectReference iceCube;
    public GameEventGeneric midnight;
    private bool iceCubeSpawned = false;
    public override void Start() {
        base.Start();
    }
    private void MidnightEvent() {
        if (photonView.IsMine) {
            iceCubeSpawned = false;
        }
    }
    public override void Use() {
        base.Use();
        if (photonView.IsMine && shouldSpawnIceCube && !iceCubeSpawned) {
            iceCubeSpawned = true;
            PhotonNetwork.Instantiate(iceCube.photonName, transform.position, Quaternion.identity);
        }
    }
    public override void Load(BinaryReader reader) {
        base.Load(reader);
        shouldSpawnIceCube = reader.ReadBoolean();
        iceCubeSpawned = reader.ReadBoolean();
    }
    public override void Save(BinaryWriter writer) {
        base.Save(writer);
        writer.Write(shouldSpawnIceCube);
        writer.Write(iceCubeSpawned);
    }
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(shouldSpawnIceCube);
            stream.SendNext(iceCubeSpawned);
        } else {
            shouldSpawnIceCube = (bool)stream.ReceiveNext();
            iceCubeSpawned = (bool)stream.ReceiveNext();
        }
    }
    void OnValidate() {
        iceCube.OnValidate();
    }
}
