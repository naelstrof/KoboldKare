using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ObjectiveManager))]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PlayAreaEnforcer))]
[RequireComponent(typeof(OcclusionArea))]
[RequireComponent(typeof(MusicManager))]
[RequireComponent(typeof(DayNightCycle))]
public class SceneDescriptor : MonoBehaviour {
    private static SceneDescriptor instance;
    
    [SerializeField] private Transform[] spawnLocations;
    [SerializeField] private bool canGrabFly = true;
    private AudioListener audioListener;

    private void Awake() {
        instance = this;
        var obj = new GameObject("AutoAudioListener", typeof(AudioListenerAutoPlacement), typeof(AudioListener));
        audioListener = obj.GetComponent<AudioListener>();
    }

    public static void GetSpawnLocationAndRotation(out Vector3 position, out Quaternion rotation) {
        if (instance == null || instance.spawnLocations == null || instance.spawnLocations.Length == 0) {
            Debug.Log(instance);
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return;
        }
        var t = instance.spawnLocations[Random.Range(0, instance.spawnLocations.Length)];
        Vector3 flattenedForward = t.forward.With(y:0);
        if (flattenedForward.magnitude == 0) {
            flattenedForward = Vector3.forward;
        }
        rotation = Quaternion.FromToRotation(Vector3.forward,flattenedForward.normalized); 
        position = t.position;
    }
    public static bool CanGrabFly() {
        return instance == null || instance.canGrabFly;
    }
}
