using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour {
    private static MusicManager instance;
    
    private AudioSource musicSource;
    [SerializeField]
    private AudioPack music;
    private bool waiting;
    [SerializeField]
    private float minWaitTime = 60f;
    [SerializeField]
    private float maxWaitTime = 220f;

    private IEnumerator FadeOutAndStartOver() {
        float fadeoutTime = Time.time + 1f;
        while(Time.time<fadeoutTime) {
            musicSource.volume = fadeoutTime-Time.time;
            yield return null;
        }
        Debug.Log("PAUSED MUSIC");
        musicSource.Stop();
        musicSource.volume = 0.7f;
        waiting = false;
    }
    public void Interrupt() {
        if (waiting) return;
        waiting = true;
        StopAllCoroutines();
        StartCoroutine(FadeOutAndStartOver());
    }

    public static void InterruptStatic() {
        instance.Interrupt();
    }

    private IEnumerator WaitAndPlay(float time) {
        yield return new WaitForSecondsRealtime(time);
        if (music != null) {
            music.Play(musicSource);
            musicSource.outputAudioMixerGroup = GameManager.GetMusicMixer();
        }
        waiting = false;
    }

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start() {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.bypassEffects = true;
        musicSource.bypassListenerEffects = true;
        musicSource.bypassReverbZones = true;
    }
    void Update() {
        if (!musicSource.isPlaying && !waiting) {
            waiting = true;
            StartCoroutine(WaitAndPlay(Random.Range(minWaitTime, maxWaitTime)));
        }
    }
}
