using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayableMapDatabase : MonoBehaviour {
    private static PlayableMapDatabase instance;

    private List<PlayableMap> playableMaps;
    private ReadOnlyCollection<PlayableMap> readOnlyPlayableMaps;
    private void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }

        instance = this;
        playableMaps = new List<PlayableMap>();
        readOnlyPlayableMaps = playableMaps.AsReadOnly();
        MapLoadingInterop.OnMapRequest += OnMapRequest;
    }

    private BoxedSceneLoad OnMapRequest(string mapName) {
        if (mapName == "MainMenu" || mapName == "ErrorScene") {
            return BoxedSceneLoad.FromAddressables(Addressables.LoadSceneAsync(mapName));
        }
        if (TryGetPlayableMap(mapName, out var playableMap)) {
            return playableMap.LoadAsync();
        }
        Debug.LogError("Could not find map: " + mapName);
        return BoxedSceneLoad.FromAddressables(Addressables.LoadSceneAsync("ErrorScene"));
    }

    public static void AddPlayableMap(PlayableMap playableMap) {
        instance.playableMaps.Add(playableMap);
        instance.playableMaps.Sort((a,b)=>String.Compare(a.GetSceneName(), b.GetSceneName(), StringComparison.InvariantCulture));
    }

    public static bool TryGetPlayableMap(string key, out PlayableMap match) {
        foreach(var map in instance.playableMaps) {
            if (map.GetRepresentedByKey(key)) {
                match = map;
                return true;
            }
        }
        match = null;
        return false;
    }

    public static void RemovePlayableMap(PlayableMap playableMap) {
        instance.playableMaps.Remove(playableMap);
        instance.playableMaps.Sort((a,b)=>String.Compare(a.GetSceneName(), b.GetSceneName(), StringComparison.InvariantCulture));
    }

    public static ReadOnlyCollection<PlayableMap> GetPlayableMaps() {
        return instance.readOnlyPlayableMaps;
    }
}
