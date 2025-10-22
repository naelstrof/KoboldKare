using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        // This can happen if multiple maps try to load in a row.
        if (playableMap == null) {
            return;
        }
        for (int i = 0; i < addedPlayableMaps.Count; i++) {
            if (addedPlayableMaps[i].unityScene.RuntimeKey != playableMap.unityScene.RuntimeKey) continue;
            PlayableMapDatabase.RemovePlayableMap(playableMap);
            addedPlayableMaps.RemoveAt(i);
            break;
        }
        PlayableMapDatabase.AddPlayableMap(playableMap);
        addedPlayableMaps.Add(playableMap);
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
