using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioReverbArea : MonoBehaviour {
    public AudioReverbPreset preset;
    [Range(0.01f,10f)]
    public float fadeDistance = 1f;
    public Collider shape;
    [HideInInspector]
    public AudioReverbData data;
    public int priority = 0;
    private void Start() {
        AudioReverbZone zone = gameObject.AddComponent<AudioReverbZone>();
        zone.reverbPreset = preset;
        zone.minDistance = 0f;
        zone.maxDistance = 0f;
        data = new AudioReverbData(zone);
        data.priority = priority;
        data.shape = shape;
        data.fadeDistance = fadeDistance;
        Destroy(zone);
    }
}
