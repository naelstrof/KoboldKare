using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityScriptableSettings;

public class PlayableMapPostProcessor : ModPostProcessor {
    private List<PlayableMap> addedPlayableMaps;

    public override void Awake() {
        base.Awake();
        addedPlayableMaps = new List<PlayableMap>();
    }

    public override async Task LoadAllAssets(IList<IResourceLocation> locations) {
        addedPlayableMaps.Clear();
        var opHandle = Addressables.LoadAssetsAsync<PlayableMap>(locations, LoadPlayableMap);
        await opHandle.Task;
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

    public override void UnloadAllAssets() {
        foreach (var playableMap in addedPlayableMaps) {
            PlayableMapDatabase.RemovePlayableMap(playableMap);
        }
    }
}
