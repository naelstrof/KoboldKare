using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

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
