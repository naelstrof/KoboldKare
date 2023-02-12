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
    }

    public static void RemovePlayableMap(PlayableMap playableMap) {
        instance.playableMaps.Remove(playableMap);
    }

    public static ReadOnlyCollection<PlayableMap> GetPlayableMaps() {
        return instance.readOnlyPlayableMaps;
    }
}
