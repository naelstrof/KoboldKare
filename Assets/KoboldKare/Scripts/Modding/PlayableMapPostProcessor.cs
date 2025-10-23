using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayableMapPostProcessor : ModPostProcessor {
    AsyncOperationHandle opHandle;
    private List<PlayableMap> addedPlayableMaps;

    public override void Awake() {
        base.Awake();
        addedPlayableMaps = new List<PlayableMap>();
    }

    public override async Task LoadAllAssets() {
        addedPlayableMaps.Clear();
        var assetsHandle = Addressables.LoadResourceLocationsAsync(searchLabel.RuntimeKey);
        await assetsHandle.Task;
        opHandle = Addressables.LoadAssetsAsync<PlayableMap>(assetsHandle.Result, LoadPlayableMap);
        await opHandle.Task;
        Addressables.Release(assetsHandle);
    }

    private void LoadPlayableMap(PlayableMap playableMap) {
        PlayableMapDatabase.AddPlayableMap(playableMap);
        addedPlayableMaps.Add(playableMap);
    }

    public override async Task HandleAssetBundleMod(ModManager.ModAssetBundle mod) {
        var node = mod.info.assets;
        if (node.HasKey("Scene")) {
            PlayableMap playableMap = ScriptableObject.CreateInstance<PlayableMap>();
            var icon = await mod.bundle.LoadAssetAsync<Object>(node["SceneIcon"]).AsSingleAssetTask();
            string sceneTitle = node.HasKey("SceneTitle") ? node["SceneTitle"] : "Unknown Map";
            string sceneDescription = node.HasKey("SceneDescription") ? node["SceneDescription"] : "No description provided.";
            playableMap.SetFromBundle(mod.GetSceneBundleLocation(), node["Scene"], sceneTitle, icon as Sprite, sceneDescription);
            PlayableMapDatabase.AddPlayableMap(playableMap);
            addedPlayableMaps.Add(playableMap);
        }
    }

    public override Task UnloadAllAssets() {
        foreach (var playableMap in addedPlayableMaps) {
            PlayableMapDatabase.RemovePlayableMap(playableMap);
        }
        
        if (opHandle.IsValid()) {
            Addressables.Release(opHandle);
        }
        return Task.CompletedTask;
    }
}
