using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapLoadingInterop {
    public delegate BoxedSceneLoad MapLoaderDelegate(string mapName);
    public static event MapLoaderDelegate OnMapRequest;
    
    public static event System.Action<BoxedSceneLoad> OnMapStartLoad;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init() {
        OnMapRequest = null;
    }
    public static BoxedSceneLoad RequestMapLoad(string mapName) {
        if (OnMapRequest != null) {
            var load = OnMapRequest.Invoke(mapName);
            OnMapStartLoad?.Invoke(load);
            return load;
        }
        return null;
    }
}
