using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Steamworks;
using UnityEngine;

[Serializable]
struct AssetInfo {
    public ulong modID;
    public string assetKey;
}

public class NetworkedKobold : NetworkBehaviour {
    private readonly SyncVar<AssetInfo> assetInfo = new SyncVar<AssetInfo>(new AssetInfo() {
        modID = (ulong)PublishedFileId_t.Invalid,
        assetKey = ""
    });

    private void Awake() {
        assetInfo.OnChange += OnAssetInfoChanged;
    }

    private void OnAssetInfoChanged(AssetInfo prev, AssetInfo next, bool asServer) {
        //ModManager.ModStub? stub = ModManager.GetModStub(next.modID);
    }
}
