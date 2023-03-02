using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceAudio : MonoBehaviour {
    private AudioSource targetSource;

    [SerializeField]
    private bool shouldInterruptMusic = false;

    private int samples;
    private void Awake() {
        targetSource = GetComponent<AudioSource>();
    }

    private void Update() {
        var camPosition = OrbitCamera.GetPlayerIntendedPosition();
        float dist = Vector3.Distance(camPosition, targetSource.transform.position);
        bool shouldEnable = dist < targetSource.maxDistance + 1f;
        if (!shouldEnable && targetSource.enabled) {
            samples = targetSource.timeSamples;
        }
        if (shouldEnable && !targetSource.enabled) {
            targetSource.enabled = shouldEnable;
            targetSource.timeSamples = samples;
        } else {
            targetSource.enabled = shouldEnable;
        }

        if (shouldInterruptMusic && targetSource.enabled) {
            MusicManager.InterruptStatic();
        }
    }
}
