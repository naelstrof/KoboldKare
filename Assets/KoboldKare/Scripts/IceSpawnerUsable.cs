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
    private ScriptableFloat money;
    [SerializeField]
    private float cost = 20f;
    [SerializeField]
    private MoneyFloater floater;
    [SerializeField]
    private GameEventGeneric refreshSpawnsEvent;
    [SerializeField]
    private Sprite buySprite;
    [SerializeField]
    private Transform spawnLocation;
    public PhotonGameObjectReference prefabSpawn;
    [SerializeField]
    private byte spawnsLeft = 3;
    private byte startingSpawnsLeft;
    void Start() {
        startingSpawnsLeft = spawnsLeft;
        refreshSpawnsEvent.AddListener(RefreshSpawns);
        Bounds newBounds = new Bounds(transform.position, Vector3.zero);
        foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
            newBounds.Encapsulate(r.bounds);
        }
        floater.SetBounds(newBounds);
        floater.SetText(cost.ToString());
    }
    void OnDestroy() {
        refreshSpawnsEvent.RemoveListener(RefreshSpawns);
    }
    public void RefreshSpawns(object nothing) {
        if (photonView.IsMine) {
            spawnsLeft = startingSpawnsLeft;
        }
    }
    public override bool CanUse(Kobold k) {
        return spawnsLeft > 0 && money.has(cost);
    }
    public override Sprite GetSprite(Kobold k) {
        return buySprite;
    }
    public override void Use(Kobold k) {
        base.Use(k);
        if (photonView.IsMine) {
            PhotonNetwork.Instantiate(prefabSpawn.photonName, spawnLocation.position, spawnLocation.rotation);
            spawnsLeft--;
        }
        money.charge(cost);
    }
    void OnValidate() {
        prefabSpawn.OnValidate();
    }
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream,info);
        if (stream.IsWriting) {
            stream.SendNext(spawnsLeft);
        } else {
            spawnsLeft = (byte)stream.ReceiveNext();
        }
    }
    public override void Save(BinaryWriter writer, string version) {
        base.Save(writer, version);
        writer.Write(spawnsLeft);
    }
    public override void Load(BinaryReader reader, string version) {
        base.Load(reader, version);
        spawnsLeft = reader.ReadByte();
    }
}
