using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

public class PlayableCharacterPrefabPostProcessor : ModPostProcessor {
    public override void LoadAsset(IResourceLocation location, object asset) {
        base.LoadAsset(location, asset);
        if (asset is not GameObject obj) {
            throw new UnityException("Asset marked as networked object is not required type `GameObject`.");
        }
        PlayableCharacterDatabase.AddPlayableCharacter(location.PrimaryKey);
    }

    public override void UnloadAsset(IResourceLocation location, object asset) {
        base.UnloadAsset(location, asset);
        PlayableCharacterDatabase.RemovePlayableCharacter(location.PrimaryKey);
    }
}
