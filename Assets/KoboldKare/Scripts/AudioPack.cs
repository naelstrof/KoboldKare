using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewAudioPack", menuName = "Data/AudioPack")]
public class AudioPack : ScriptableObject {
    [SerializeField]
    private AudioClip[] clips;
    [SerializeField]
    private float volume = 1f;
    [SerializeField]
    private AudioMixerGroup group;
    //public AudioClip GetRandomClip() {
        //return clips[Random.Range(0, clips.Length)];
    //}
    public void Play(AudioSource source) {
        source.outputAudioMixerGroup = group;
        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = volume;
        source.pitch = Random.Range(0.7f,1.3f);
        source.Play();
    }
    public void PlayOneShot(AudioSource source) {
        source.outputAudioMixerGroup = group;
        source.pitch = Random.Range(0.7f,1.3f);
        source.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }
}
