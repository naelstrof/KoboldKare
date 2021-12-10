using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class MusicManager : MonoBehaviour {
    [SerializeField]
    private GameEventGeneric sleepEvent;
    private AudioSource musicSource;
    public List<AudioClip> dayMusic = new List<AudioClip>();
    public List<AudioClip> nightMusic = new List<AudioClip>();
    private bool waiting = false;
    public IEnumerator FadeOutAndStartOver() {
        float fadeoutTime = Time.time + 1f;
        while(Time.time<fadeoutTime) {
            musicSource.volume = fadeoutTime-Time.time;
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = 0.7f;
        waiting = false;
    }
    public void Interrupt() {
        StopAllCoroutines();
        waiting = true;
        StartCoroutine(FadeOutAndStartOver());
    }
    public IEnumerator WaitAndPlay(float time) {
        yield return new WaitForSecondsRealtime(time);
        AudioClip p = null;
        if ( DayNightCycle.instance.daylight > 0f ) {
            p = dayMusic[Random.Range(0,dayMusic.Count)];
        } else {
            p = nightMusic[Random.Range(0,nightMusic.Count)];
        }
        musicSource.clip = p;
        musicSource.Play();
        waiting = false;
    }
    void Start() {
        musicSource = GetComponent<AudioSource>();
        sleepEvent.AddListener(OnSleep);
    }

    void OnDestroy() {
        sleepEvent.RemoveListener(OnSleep);
    }
    void OnSleep(object nothing) {
        Interrupt();
    }

    void Update() {
        if (!musicSource.isPlaying && !waiting) {
            waiting = true;
            StartCoroutine(WaitAndPlay(UnityEngine.Random.Range(60, 220)));
        }
    }
}
