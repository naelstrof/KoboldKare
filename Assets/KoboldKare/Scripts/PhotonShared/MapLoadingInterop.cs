using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapLoadingInterop {
    public delegate BoxedSceneLoad MapLoaderDelegate(string mapName);
    public static event MapLoaderDelegate OnMapRequest;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init() {
        OnMapRequest = null;
    }
    public static BoxedSceneLoad RequestMapLoad(string mapName) {
        if (OnMapRequest != null) {
            return OnMapRequest.Invoke(mapName);
        }
        return null;
    }
}
