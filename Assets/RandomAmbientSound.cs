using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAmbientSound : MonoBehaviour{
    public List<AudioClip> sounds = new List<AudioClip>();
    public float delayStart, delayBetween, delayUpToRandom, randomDelayStartAdd;
    public AudioSource audioSource;
    public bool play, startPlaying;
    Coroutine audioplayer;

    void Start(){
        if(startPlaying) StartPlayingSounds();
    }
    
    //TODO: Don't play sounds all the time; only when player is in range of being able to hear them
    void OnTriggerEnter(Collider other){}

    //TODO: Don't play sounds all the time; only when player is in range of being able to hear them
    void OnTriggerExit(Collider other){}

    public void StartPlayingSounds(){        
        play = true;
        audioplayer = StartCoroutine(ShuffleAndPlaySounds());
    }

    IEnumerator ShuffleAndPlaySounds(){
        while(play){
            yield return new WaitForSeconds(delayStart+Random.Range(0,randomDelayStartAdd));
            audioSource.PlayOneShot(sounds[Random.Range(0,sounds.Count-1)]);
            yield return new WaitForSeconds(delayBetween+Random.Range(0,delayUpToRandom));
        }

    }
}
