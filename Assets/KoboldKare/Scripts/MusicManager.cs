using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
    private AudioSource musicSource;
    public List<AudioClip> dayMusic = new List<AudioClip>();
    public List<AudioClip> nightMusic = new List<AudioClip>();
    private bool waiting = false;
    public IEnumerator FadeOutAndStartOver() {
        while(musicSource.volume != 0f) {
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, 0f, Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        musicSource.Stop();
        musicSource.volume = 0.7f;
        waiting = false;
    }
    public void Interrupt() {
        StopAllCoroutines();
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
    }

    void Update() {
        if (!musicSource.isPlaying && !waiting) {
            waiting = true;
            StartCoroutine(WaitAndPlay(UnityEngine.Random.Range(60, 220)));
        }
    }
}
