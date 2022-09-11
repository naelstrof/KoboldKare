using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPackPlayer : MonoBehaviour {
    [SerializeField]
    private AudioPack pack;
    private AudioSource source;
    void OnEnable() {
        if (source == null) {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.maxDistance = 10f;
            source.minDistance = 0.2f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.spatialBlend = 1f;
            source.loop = false;
        }
        pack.Play(source);
    }
}
