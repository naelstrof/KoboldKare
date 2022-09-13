using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class KoboldCreateObjective : DragonMailObjective {
    [SerializeField]
    private int maxKobolds = 1;

    [SerializeField]
    private LocalizedString description;
    
    private int kobolds = 0;
    public override void Register() {
        base.Register();
        FarmSpawnEventHandler.AddListener(OnEntitySpawn);
    }

    public override void Unregister() {
        base.Unregister();
        FarmSpawnEventHandler.RemoveListener(OnEntitySpawn);
    }

    private void OnEntitySpawn(GameObject obj) {
        if (obj.GetComponentInChildren<Kobold>() != null) {
            Advance(obj.transform.position);
        }
    }

    protected override void Advance(Vector3 position) {
        base.Advance(position);
        kobolds++;
        TriggerUpdate();
        if (kobolds >= maxKobolds) {
            TriggerComplete();
        }
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} {kobolds.ToString()}/{maxKobolds.ToString()}";
    }

    public override void Save(BinaryWriter writer) {
        writer.Write(kobolds);
    }

    public override void Load(BinaryReader reader) {
        kobolds = reader.ReadInt32();
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(kobolds);
        } else {
            kobolds = (int)stream.ReceiveNext();
        }
    }
}
