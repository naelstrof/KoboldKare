using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class MusicEventHandler : MonoBehaviour{
    [SerializeField]
    private GameEventGeneric midnightEvent, nightEvent;
    public float timeToFadeAudio, delayFadeAudio, loudness;
    public static MusicEventHandler instance;
    public AudioSource audio;
    public AudioClip midnightStinger;

    void Awake(){
        if(instance == null)
            instance = this;

        else{
            Destroy(instance);
            instance = this;
        }
    }

    void Start(){
        midnightEvent.AddListener(OnMidnight);
        nightEvent.AddListener(OnNight);
    }

    void OnDestroy(){
        midnightEvent.RemoveListener(OnMidnight);
        nightEvent.RemoveListener(OnNight);
    }

    void OnNight(object nothing){
        audio.Play();
        StartCoroutine(smoothlyFadeInAudio());
    }

    void OnMidnight(object nothing){
        audio.Stop();
        audio.PlayOneShot(midnightStinger);
    }

    IEnumerator smoothlyFadeInAudio(){
        //Debug.Log("Starting wait for smoothly fade in");
        audio.volume = 0f;
        var t = 0f;
        yield return new WaitForSecondsRealtime(delayFadeAudio);
        //Debug.Log("Finished waiting");

        while(t < timeToFadeAudio){
            t += Time.deltaTime;
            audio.volume = (t/timeToFadeAudio)*loudness;
            //Debug.Log("Fading audio");
            yield return new WaitForSecondsRealtime(Time.deltaTime);
        }
    }
}
